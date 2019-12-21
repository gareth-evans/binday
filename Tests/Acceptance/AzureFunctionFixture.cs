using System.Net.Http;
using BinDay;
using LightBDD.NUnit3;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Tests.Acceptance
{
    public class AzureFunctionFixture : FeatureFixture
    {
        private readonly TestServer _server;

        protected HttpResponseMessage Response;

        [SetUp]
        public void Setup()
        {
            Response = null;
        }


        protected AzureFunctionFixture()
        {
            var applicationFactory = new TestWebApplicationFactory<Startup>();
            _server = applicationFactory.CreateServer();
        }

        protected HttpClient CreateClient() => _server.CreateClient();
    }
}