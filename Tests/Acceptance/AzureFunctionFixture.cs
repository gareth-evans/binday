using System.Net.Http;
using System.Threading.Tasks;
using BinDay;
using LightBDD.NUnit3;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;

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

        protected async Task Then_I_should_receive_the_response_EXPECTED(string expected)
        {
            var json = await Response.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<JObject>(json);
            var responseText = response["response"]["outputSpeech"]["text"].Value<string>();

            responseText.ShouldBe(expected);
        }
    }
}