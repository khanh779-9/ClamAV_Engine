using System;

namespace ClamAV_Engine.ClamLib.Helpers
{
    internal static class ExpressionEvaluator
    {
        public static bool Evaluate(string expression, int[] matchCounts)
        {
            if (string.IsNullOrWhiteSpace(expression) || matchCounts == null)
                return false;

            int pos = 0;
            return ParseOr(expression, ref pos, matchCounts);
        }

        private static bool ParseOr(string expr, ref int pos, int[] counts)
        {
            bool value = ParseAnd(expr, ref pos, counts);

            while (true)
            {
                SkipSpaces(expr, ref pos);
                if (pos < expr.Length && expr[pos] == '|')
                {
                    pos++;
                    bool right = ParseAnd(expr, ref pos, counts);
                    value = value || right;
                }
                else
                {
                    break;
                }
            }

            return value;
        }

        private static bool ParseAnd(string expr, ref int pos, int[] counts)
        {
            bool value = ParsePrimary(expr, ref pos, counts);

            while (true)
            {
                SkipSpaces(expr, ref pos);
                if (pos < expr.Length && expr[pos] == '&')
                {
                    pos++;
                    bool right = ParsePrimary(expr, ref pos, counts);
                    value = value && right;
                }
                else
                {
                    break;
                }
            }

            return value;
        }

        private static bool ParsePrimary(string expr, ref int pos, int[] counts)
        {
            SkipSpaces(expr, ref pos);

            if (pos < expr.Length && expr[pos] == '(')
            {
                pos++;
                bool inner = ParseOr(expr, ref pos, counts);
                SkipSpaces(expr, ref pos);
                if (pos < expr.Length && expr[pos] == ')')
                    pos++;
                return inner;
            }

            return ParseIndexTerm(expr, ref pos, counts);
        }

        private static bool ParseIndexTerm(string expr, ref int pos, int[] counts)
        {
            SkipSpaces(expr, ref pos);

            int index = ParseNumber(expr, ref pos);
            int c = (index >= 0 && index < counts.Length) ? counts[index] : 0;

            SkipSpaces(expr, ref pos);
            if (pos >= expr.Length)
                return c > 0;

            char op = expr[pos];
            if (op != '=' && op != '<' && op != '>')
                return c > 0;

            pos++;
            SkipSpaces(expr, ref pos);

            int a = ParseNumber(expr, ref pos);
            int? b = null;

            SkipSpaces(expr, ref pos);
            if (pos < expr.Length && expr[pos] == ',')
            {
                pos++;
                SkipSpaces(expr, ref pos);
                b = ParseNumber(expr, ref pos);
            }

            switch (op)
            {
                case '=':
                    if (!b.HasValue)
                        return c == a;
                    return c >= a && c <= b.Value;
                case '>':
                    if (!b.HasValue)
                        return c > a;
                    return c > a && c <= b.Value;
                case '<':
                    if (!b.HasValue)
                        return c < a;
                    return c >= a && c < b.Value;
                default:
                    return c > 0;
            }
        }

        private static int ParseNumber(string expr, ref int pos)
        {
            SkipSpaces(expr, ref pos);
            int start = pos;
            while (pos < expr.Length && char.IsDigit(expr[pos]))
                pos++;

            if (start == pos)
                return 0;

            var s = expr.Substring(start, pos - start);
            if (int.TryParse(s, out var val))
                return val;
            return 0;
        }

        private static void SkipSpaces(string expr, ref int pos)
        {
            while (pos < expr.Length && char.IsWhiteSpace(expr[pos]))
                pos++;
        }
    }
}
