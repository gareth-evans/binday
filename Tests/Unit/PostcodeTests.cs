using NUnit.Framework;
using Shared;
using Shouldly;

namespace Tests.Unit
{
    public class PostcodeTests
    {
        public static TestCaseData[] ParsingTestCases = new[]
        {
            new TestCaseData("HP9 2ET", "HP9", "2ET").SetDescription("Standard format"),
            new TestCaseData("HP92ET", "HP9", "2ET").SetDescription("No space between outward and inward codes"),
            new TestCaseData("Hp92eT", "HP9", "2ET").SetDescription("Mixed case"),
            new TestCaseData("EC1A 2BN", "EC1A", "2BN").SetDescription("London")
        };

        [TestCaseSource(nameof(ParsingTestCases))]
        public void ShouldParsePostcode(string input, string outwardCode, string inwardCode)
        {
            var result = Postcode.TryParse(input, out var postcode);

            result.ShouldBeTrue();
            postcode.Value.ShouldBe(input);
            postcode.OutwardCode.ShouldBe(outwardCode);
        }

        [TestCaseSource(nameof(ParsingTestCases))]
        public void ShouldCreatePostcode(string input, string outwardCode, string inwardCode)
        {
            var result = Postcode.TryParse(input, out var postcode);

            result.ShouldBeTrue();
            postcode.Value.ShouldBe(input);
            postcode.OutwardCode.ShouldBe(outwardCode);
        }
    }
}