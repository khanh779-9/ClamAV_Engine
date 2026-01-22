using System;
using System.Collections.Generic;

namespace ClamAV_Engine.ClamLib.Helpers
{
    /// <summary>
    /// OPTIMIZED PatternMatcher
    /// - Cache parsed patterns to avoid re-parsing
    /// - Faster byte matching with inline checks
    /// </summary>
    internal static class PatternMatcher
    {
        // Cache for parsed patterns to avoid re-parsing
        private static readonly Dictionary<string, List<Element>> PatternCache = new Dictionary<string, List<Element>>();
        private const int MaxPatternCacheSize = 1000;

        private abstract class Element { }

        private sealed class ByteConstraint : Element
        {
            public int? HighNibble; // 0-15 or null (wildcard)
            public int? LowNibble;  // 0-15 or null (wildcard)
        }

        private sealed class GapConstraint : Element
        {
            public int Min;   // >= 0
            public int Max;   // -1 = unbounded
        }

        public static bool Match(byte[] data, string pattern, string offsetSpec)
        {
            if (data == null || data.Length == 0)
                return false;
            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            var elements = GetOrParsePattern(pattern);
            if (elements == null || elements.Count == 0)
                return false;

            ParseOffset(offsetSpec, data.Length, out var startMin, out var startMax);

            for (int start = startMin; start <= startMax && start < data.Length; start++)
            {
                if (MatchFrom(elements, 0, data, start))
                    return true;
            }

            return false;
        }

        public static int CountMatches(byte[] data, string pattern, string offsetSpec)
        {
            if (data == null || data.Length == 0)
                return 0;
            if (string.IsNullOrWhiteSpace(pattern))
                return 0;

            var elements = GetOrParsePattern(pattern);
            if (elements == null || elements.Count == 0)
                return 0;

            ParseOffset(offsetSpec, data.Length, out var startMin, out var startMax);

            int count = 0;
            for (int start = startMin; start <= startMax && start < data.Length; start++)
            {
                if (MatchFrom(elements, 0, data, start))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Get cached pattern or parse new one
        /// </summary>
        private static List<Element> GetOrParsePattern(string pattern)
        {
            lock (PatternCache)
            {
                if (PatternCache.TryGetValue(pattern, out var cached))
                    return cached;

                var parsed = ParsePattern(pattern);
                
                // Keep cache size reasonable
                if (PatternCache.Count < MaxPatternCacheSize)
                    PatternCache[pattern] = parsed;

                return parsed;
            }
        }

        private static List<Element> ParsePattern(string pattern)
        {
            var elements = new List<Element>(pattern.Length / 2);
            int i = 0;
            while (i < pattern.Length)
            {
                char c = pattern[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (c == '*')
                {
                    elements.Add(new GapConstraint { Min = 0, Max = -1 });
                    i++;
                    continue;
                }

                if (c == '{')
                {
                    int end = pattern.IndexOf('}', i + 1);
                    if (end < 0)
                        break;

                    var inner = pattern.Substring(i + 1, end - i - 1).Trim();
                    ParseRange(inner, out int min, out int max);
                    elements.Add(new GapConstraint { Min = min, Max = max });
                    i = end + 1;
                    continue;
                }

                // Hex byte or nibble wildcard pair
                if (IsHexOrWildcard(c))
                {
                    if (i + 1 >= pattern.Length)
                        break;

                    char c2 = pattern[i + 1];
                    if (!IsHexOrWildcard(c2))
                        break;

                    var bc = new ByteConstraint
                    {
                        HighNibble = CharToNibble(c),
                        LowNibble = CharToNibble(c2)
                    };
                    elements.Add(bc);
                    i += 2;
                    continue;
                }

                // Unsupported constructs (alternatives, classes, etc.) -> abort pattern
                return null;
            }

            return elements;
        }

        private static bool MatchFrom(List<Element> elements, int ei, byte[] data, int di)
        {
            if (ei == elements.Count)
                return true;

            if (di > data.Length)
                return false;

            var el = elements[ei];

            if (el is ByteConstraint bc)
            {
                if (di >= data.Length)
                    return false;

                var b = data[di];
                int hi = (b >> 4) & 0xF;
                int lo = b & 0xF;

                // Inline checks for performance
                if (bc.HighNibble.HasValue && bc.HighNibble.Value != hi)
                    return false;
                if (bc.LowNibble.HasValue && bc.LowNibble.Value != lo)
                    return false;

                return MatchFrom(elements, ei + 1, data, di + 1);
            }

            if (el is GapConstraint gap)
            {
                int min = gap.Min;
                int max = gap.Max < 0 ? data.Length - di : Math.Min(gap.Max, data.Length - di);

                if (min > max)
                    return false;

                for (int skip = min; skip <= max; skip++)
                {
                    if (MatchFrom(elements, ei + 1, data, di + skip))
                        return true;
                }

                return false;
            }

            return false;
        }

        private static void ParseOffset(string offsetSpec, int dataLength, out int min, out int max)
        {
            min = 0;
            max = dataLength - 1;

            if (string.IsNullOrWhiteSpace(offsetSpec) || offsetSpec == "*")
                return;

            offsetSpec = offsetSpec.Trim();

            if (offsetSpec.Contains("-"))
            {
                ParseRange(offsetSpec, out int rmin, out int rmax);
                min = Math.Max(0, rmin);
                max = rmax < 0 ? dataLength - 1 : Math.Min(rmax, dataLength - 1);
            }
            else if (int.TryParse(offsetSpec, out var pos))
            {
                if (pos < 0)
                    pos = 0;
                min = Math.Min(pos, dataLength - 1);
                max = min;
            }
        }

        private static void ParseRange(string text, out int min, out int max)
        {
            min = 0;
            max = -1; // unbounded

            if (string.IsNullOrWhiteSpace(text))
                return;

            text = text.Trim();

            if (text.Contains("-"))
            {
                var parts = text.Split('-');
                if (parts.Length != 2)
                    return;

                string a = parts[0].Trim();
                string b = parts[1].Trim();

                if (a.Length == 0 && int.TryParse(b, out var maxOnly))
                {
                    // {-n} -> 0..n
                    min = 0;
                    max = maxOnly;
                }
                else if (b.Length == 0 && int.TryParse(a, out var minOnly))
                {
                    // {n-} -> n..inf
                    min = minOnly;
                    max = -1;
                }
                else if (int.TryParse(a, out var rmin) && int.TryParse(b, out var rmax))
                {
                    min = rmin;
                    max = rmax;
                }
            }
            else if (int.TryParse(text, out var exact))
            {
                min = exact;
                max = exact;
            }
        }

        private static bool IsHexOrWildcard(char c)
        {
            return c == '?' || Uri.IsHexDigit(c);
        }

        private static int? CharToNibble(char c)
        {
            if (c == '?')
                return null;

            if (c >= '0' && c <= '9')
                return c - '0';

            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;

            return null;
        }
    }
}
