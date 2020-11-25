using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace BinDay
{
    public class KeepFunctionsWarm
    {
        private readonly ILogger<KeepFunctionsWarm> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string BinDayUrl = "https://binday-production.azurewebsites.net/api/alexa";

        public KeepFunctionsWarm(ILogger<KeepFunctionsWarm> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [FunctionName("KeepWarm")]
        public async Task Run([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(BinDayUrl, null);

            _logger.LogInformation($"Got {response.StatusCode} from {BinDayUrl}");
        }
    }
}