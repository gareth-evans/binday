using System;
using System.Text.RegularExpressions;

namespace Shared
{
    public sealed class Postcode
    {
        private const string PostcodeRegexPattern =
            "([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\\s?[0-9][A-Za-z]{2})";
        private static readonly Regex Matcher = new Regex(PostcodeRegexPattern, RegexOptions.Compiled);

        public Postcode(string input) : this(Parse(input))
        {
            if (!TryParse(input, out (string value, string outwardCode) parsedValues)) throw new ArgumentException($"Could not parse argument {nameof(input)} as postcode");

            Value = parsedValues.value;
            OutwardCode = parsedValues.outwardCode;
        }

        private Postcode((string value, string outwardCode) postcode)
        {
            Value = postcode.value;
            OutwardCode = postcode.outwardCode;
        }

        public static bool TryParse(string input, out Postcode postcode)
        {
            postcode = null;

            if (!TryParse(input, out (string value, string outwardCode) parsedValues)) return false;

            postcode = new Postcode(parsedValues);

            return true;
        }

        public string Value { get; }
        public string OutwardCode { get; }

        private static (string value, string outwardCode) Parse(string input)
        {
            if (!TryParse(input, out (string value, string outwardCode) parsedValues)) throw new ArgumentException($"Could not parse argument {nameof(input)} as postcode");

            return parsedValues;
        }

        private static bool TryParse(string input, out (string value, string outwardCode) parsedValues)
        {
            parsedValues = default((string, string));

            var match = Matcher.Match(input.ToUpper());

            if (!match.Success) return false;

            parsedValues = (input, match.Groups[3].Value);

            return true;
        }
    }
}