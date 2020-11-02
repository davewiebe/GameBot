using System.Text.RegularExpressions;

namespace PerudoBot.Extensions
{
    public static class StringExtensions
    {
        public static string StripSpecialCharacters(this string text)
        {
            return Regex.Replace(text, "[^0-9a-zA-Z ,.@<>$#:'\\\"!%\\-]+", "");
        }

        public static string WrapInCodeBlock(this string text)
        {
            return $"```{text}```";
        }
    }
}