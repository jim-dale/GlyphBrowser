using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Text;

namespace GlyphBrowser
{
    public static class Helpers
    {
        public static string GetFontFamilyName(StorageFile file)
        {
            var parts = GetFontParts(file);
            return parts[0];
        }

        public static FontStyle GetFontStyle(StorageFile file)
        {
            var result = FontStyle.Normal;

            var parts = GetFontParts(file);
            if (parts.Length ==3)
            {
                string part = parts[1];
                if (Enum.TryParse<FontStyle>(part, true, out FontStyle style))
                {
                    if (Enum.IsDefined(typeof(FontStyle), style))
                    {
                        result = style;
                    }
                }
            }
            return result;
        }

        public static FontWeight GetFontWeight(StorageFile file)
        {
            var result = FontWeights.Normal;

            var parts = GetFontParts(file);
            if (parts.Length == 3)
            {
                string part = parts[2].ToLowerInvariant();
                switch (part)
                {
                    case "light":
                        result = FontWeights.Light;
                        break;
                    case "extralight":
                        result = FontWeights.ExtraLight;
                        break;
                    case "semilight":
                        result = FontWeights.SemiLight;
                        break;
                    case "thin":
                        result = FontWeights.Thin;
                        break;
                    case "medium":
                        result = FontWeights.Medium;
                        break;
                    case "black":
                        result = FontWeights.Black;
                        break;
                    case "extrablack":
                        result = FontWeights.ExtraBlack;
                        break;
                    case "semibold":
                        result = FontWeights.SemiBold;
                        break;
                    case "bold":
                        result = FontWeights.Bold;
                        break;
                    case "extrabold":
                        result = FontWeights.ExtraBold;
                        break;
                    default:
                        result = GetCustomFontWeight(part);
                        break;
                }
            }
            return result;
        }

        private static FontWeight GetCustomFontWeight(string str)
        {
            var result = FontWeights.Normal;

            if (ushort.TryParse(str, out ushort value))
            {
                result = new FontWeight
                {
                    Weight = value
                };
            }
            return result;
        }

        private static string[] GetFontParts(StorageFile file)
        {
            string[] result = Array.Empty<string>();

            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            Regex rx = new Regex(@"^(.+)-(.+)-(.+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            var match = rx.Match(fileName);
            if (match.Success)
            {
                List<string> parts = new List<string>();
                for (int i = 1; i < match.Groups.Count; i++)
                {
                    parts.Add(match.Groups[i].Value);
                }

                result = parts.ToArray();
            }
            return result;
        }

        public static async Task<string> LoadUnicodeTextFromFile(StorageFile file)
        {
            string result = String.Empty;

            if (file.ContentType == "text/plain")
            {
                var text = await FileIO.ReadTextAsync(file);

                result = ParseUnicodeString(text);
            }
            return result;
        }

        public static string ParseUnicodeString(string str)
        {
            StringBuilder result = new StringBuilder();

            Regex rx = new Regex(@"\\?[uU]?\+?([0-9A-F]{4,8})", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            var matches = rx.Matches(str);
            foreach (Match match in matches)
            {
                string toParse = match.Groups[1].Value;

                int value = Convert.ToInt32(toParse, 16);
                if (value <= 0xFFFF)
                {
                    result.Append((Char)value);
                }
                else
                {
                    result.Append(Char.ConvertFromUtf32(value));
                }
            }
            return result.ToString();
        }
    }
}
