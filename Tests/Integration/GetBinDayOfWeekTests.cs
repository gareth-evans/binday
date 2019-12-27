using System;
using System.Net.Http;
using System.Threading.Tasks;
using BinDay;
using NUnit.Framework;
using Shouldly;

namespace Tests
{
    public class GetBinDayOfWeekTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("HP9 2ET", DayOfWeek.Friday)]
        [TestCase("SL9 7EN", DayOfWeek.Wednesday)]
        public Task GetBinDayOfWeek(string postcode, DayOfWeek expectedDay)
        {
            throw new NotImplementedException();

/*            var httpClient = new HttpClient();

            var sut = new SouthBucksBinDayResolver(httpClient);

            var result = await sut.GetBinDayForPostcodeAsync(postcode);

            result.ShouldBe(expectedDay);*/
        }
    }
}