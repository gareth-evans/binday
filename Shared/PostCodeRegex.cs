using System.Net;
using System.Text.RegularExpressions;

namespace Shared
{
    public static class PostCodeRegex
    {
        private const string PostcodeRegexPattern =
            "([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\\s?[0-9][A-Za-z]{2})";
        private static readonly Regex Matcher = new Regex(PostcodeRegexPattern, RegexOptions.Compiled);

        public static Match Match(string input)
        {
            return Matcher.Match(input);
        }
    }
}