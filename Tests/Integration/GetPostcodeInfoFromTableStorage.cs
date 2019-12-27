using System.Threading.Tasks;
using BinDay;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shared;
using Shouldly;

namespace Tests
{
    public class GetPostcodeInfoFromTableStorage
    {
        [Test]
        public void ShouldGetPostcodeInfo()
        {
            var logger = Mock.Of<ILogger<TableStoragePostcodeResolver>>();
            var sut = new TableStoragePostcodeResolver(logger);
            var postcode = new Postcode("HP92ET");
            var postcodeInfo = sut.GetPostcodeInfo(postcode);

            postcodeInfo.Postcode.ShouldBe("HP92ET");
            postcodeInfo.AdminDistrictCode.ShouldBe("E07000006");
            postcodeInfo.AdminDistrictName.ShouldBe("South Bucks District");
            postcodeInfo.Eastings.ShouldBe(494705);
            postcodeInfo.Northings.ShouldBe(190415);
        }
    }
}