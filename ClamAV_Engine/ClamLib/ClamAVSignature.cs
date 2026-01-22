using System;
using System.Collections.Generic;
using System.Linq;

namespace ClamAV_Engine.ClamLib
{
    public class ClamAVSignature
    {
        public SignatureType Type { get; set; }
        public string Name { get; set; }
        public TargetType Target { get; set; }
        public bool IsWildcard { get; set; }
        public bool IsUnofficial { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public ClamAVSignature()
        {
            Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }
        
        public static ClamAVSignature ParseHdb(string line, SignatureType type)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                return null;

            var parts = line.Split(':');
            if (parts.Length < 3)
                return null;

            var hash = parts[0].Trim();
            var sizePart = parts[1].Trim();
            var name = parts[2].Trim();

            var sig = new ClamAVSignature
            {
                Type = type,
                Name = NormalizeName(name, out var unofficial),
                IsUnofficial = unofficial,
                Target = TargetType.Any
            };

            sig.Properties["Hash"] = hash;

            if (sizePart == "*")
            {
                sig.IsWildcard = true;
            }
            else if (long.TryParse(sizePart, out var size))
            {
                sig.Properties["FileSize"] = size;
            }

            return sig;
        }

        public static ClamAVSignature Parse(string line, SignatureType type)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                return null;

            switch (type)
            {
                case SignatureType.HDB:
                case SignatureType.HSB:
                case SignatureType.FP:
                    return ParseHdb(line, type);
                case SignatureType.MDB:
                    return ParseMdb(line);
                case SignatureType.NDB:
                    return ParseNdb(line);
                case SignatureType.LDB:
                case SignatureType.LDU:
                    return ParseLdb(line, type);
                case SignatureType.CDB:
                    return ParseCdb(line);
                default:
                    return null;
            }
        }

        public static ClamAVSignature ParseMdb(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                return null;

            var parts = line.Split(':');
            if (parts.Length < 3)
                return null;

            var sectionSizePart = parts[0].Trim();
            var hash = parts[1].Trim();
            var name = parts[2].Trim();

            var sig = new ClamAVSignature
            {
                Type = SignatureType.MDB,
                Name = NormalizeName(name, out var unofficial),
                IsUnofficial = unofficial,
                Target = TargetType.PE
            };

            sig.Properties["SectionHash"] = hash;

            if (long.TryParse(sectionSizePart, out var sectionSize))
            {
                sig.Properties["SectionSize"] = sectionSize;
            }

            return sig;
        }

        public static ClamAVSignature ParseNdb(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                return null;

            var parts = line.Split(':');
            if (parts.Length < 4)
                return null;

            var name = parts[0].Trim();
            var targetPart = parts[1].Trim();
            var offsetPart = parts[2].Trim();
            var pattern = parts[3].Trim();

            int? minFLevel = null;
            int? maxFLevel = null;

            if (parts.Length >= 5 && int.TryParse(parts[4].Trim(), out var minFl))
                minFLevel = minFl;
            if (parts.Length >= 6 && int.TryParse(parts[5].Trim(), out var maxFl))
                maxFLevel = maxFl;

            var sig = new ClamAVSignature
            {
                Type = SignatureType.NDB,
                Name = NormalizeName(name, out var unofficial),
                IsUnofficial = unofficial,
                Target = ParseTargetType(targetPart)
            };

            sig.Properties["Offset"] = offsetPart;
            sig.Properties["RawPattern"] = pattern;

            if (minFLevel.HasValue)
                sig.Properties["MinFLevel"] = minFLevel.Value;
            if (maxFLevel.HasValue)
                sig.Properties["MaxFLevel"] = maxFLevel.Value;

            if (offsetPart == "*")
                sig.IsWildcard = true;

            return sig;
        }

        public static ClamAVSignature ParseLdb(string line, SignatureType type)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                return null;

            var parts = line.Split(';');
            if (parts.Length < 4)
                return null;

            var name = parts[0].Trim();
            var targetBlock = parts[1].Trim();
            var logicalExpression = parts[2].Trim();
            var subsigs = parts.Skip(3).Select(p => p.Trim()).Where(p => p.Length > 0).ToList();

            var sig = new ClamAVSignature
            {
                Type = type,
                Name = NormalizeName(name, out var unofficial),
                IsUnofficial = unofficial,
                Target = TargetType.Any
            };

            sig.Properties["LogicalExpression"] = logicalExpression;
            sig.Properties["RawPatterns"] = subsigs;

            if (!string.IsNullOrEmpty(targetBlock))
                ParseTargetDescriptionBlock(targetBlock, sig);

            if (!sig.Properties.ContainsKey("FileSize") &&
                (sig.Properties.ContainsKey("FileSizeMin") || sig.Properties.ContainsKey("FileSizeMax")))
            {
                if (sig.Properties.TryGetValue("FileSizeMin", out var minObj) && minObj is long min &&
                    sig.Properties.TryGetValue("FileSizeMax", out var maxObj) && maxObj is long max &&
                    min == max)
                {
                    sig.Properties["FileSize"] = min;
                }
            }

            return sig;
        }

        public static ClamAVSignature ParseCdb(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                return null;

            var trimmed = line.TrimEnd(':');
            var parts = trimmed.Split(':');
            if (parts.Length < 4)
                return null;

            var name = parts[0].Trim();
            var containerType = parts[1].Trim();
            var containerSizePart = parts[2].Trim();
            var fileNameRegex = parts[3];

            var sig = new ClamAVSignature
            {
                Type = SignatureType.CDB,
                Name = NormalizeName(name, out var unofficial),
                IsUnofficial = unofficial,
                Target = TargetType.Any
            };

            sig.Properties["ContainerType"] = containerType;
            sig.Properties["FileNamePattern"] = fileNameRegex;

            ParseSizeField(containerSizePart, sig, "ContainerSize");

            if (parts.Length > 4)
                ParseSizeField(parts[4].Trim(), sig, "FileSizeInContainer");
            if (parts.Length > 5)
                ParseSizeField(parts[5].Trim(), sig, "FileSizeReal");

            return sig;
        }

        private static string NormalizeName(string name, out bool isUnofficial)
        {
            isUnofficial = false;
            if (string.IsNullOrEmpty(name))
                return name;

            const string suffix = ".UNOFFICIAL";
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                isUnofficial = true;
                return name.Substring(0, name.Length - suffix.Length);
            }

            return name;
        }

        private static TargetType ParseTargetType(string targetPart)
        {
            if (string.IsNullOrEmpty(targetPart))
                return TargetType.Any;

            if (int.TryParse(targetPart, out var tInt))
            {
                if (Enum.IsDefined(typeof(TargetType), tInt))
                    return (TargetType)tInt;
                return TargetType.Any;
            }

            if (Enum.TryParse<TargetType>(targetPart, true, out var tEnum))
                return tEnum;

            return TargetType.Any;
        }

        private static void ParseTargetDescriptionBlock(string block, ClamAVSignature sig)
        {
            var pairs = block.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split(new[] { ':' }, 2);
                if (kv.Length != 2)
                    continue;

                var key = kv[0].Trim();
                var value = kv[1].Trim();

                if (key.Equals("Target", StringComparison.OrdinalIgnoreCase))
                {
                    sig.Target = ParseTargetType(value);
                }
                else if (key.Equals("FileSize", StringComparison.OrdinalIgnoreCase))
                {
                    ParseSizeRange(value, sig, "FileSizeMin", "FileSizeMax");
                }
                else
                {
                    sig.Properties[$"Target_{key}"] = value;
                }
            }
        }

        private static void ParseSizeField(string value, ClamAVSignature sig, string propertyBaseName)
        {
            if (string.IsNullOrEmpty(value) || value == "*")
                return;

            if (value.Contains("-"))
            {
                ParseSizeRange(value, sig, propertyBaseName + "Min", propertyBaseName + "Max");
            }
            else if (long.TryParse(value, out var size))
            {
                sig.Properties[propertyBaseName] = size;
            }
        }

        private static void ParseSizeRange(string value, ClamAVSignature sig, string minKey, string maxKey)
        {
            if (string.IsNullOrEmpty(value))
                return;

            var parts = value.Split('-');
            if (parts.Length == 1)
            {
                if (long.TryParse(parts[0], out var exact))
                {
                    sig.Properties[minKey] = exact;
                    sig.Properties[maxKey] = exact;
                }
                return;
            }

            if (long.TryParse(parts[0], out var min))
                sig.Properties[minKey] = min;
            if (long.TryParse(parts[1], out var max))
                sig.Properties[maxKey] = max;
        }
    }
}
