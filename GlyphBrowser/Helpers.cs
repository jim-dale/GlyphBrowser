using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace GlyphBrowser
{
    public static class Helpers
    {
        public static string GetFontFamilyName(StorageFile file)
        {
            var fileName = file.Name;
            string result = fileName.Substring(0, fileName.IndexOf('-'));

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
