using System.Net.Http;
using BinDay;
using Moq;
using NUnit.Framework;

namespace Tests.Unit
{
    public class SouthBucksBinDayResolverTests
    {
        [Test]
        public void Should_resolve_bin_day()
        {
            var sut = new SouthBucksBinDayResolver(
                Mock.Of<IHttpClientFactory>(),
                Mock.Of<ITableStoragePostcodeResolver>());


        }
    }
}