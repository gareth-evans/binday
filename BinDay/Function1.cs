using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinDay
{
    public static class Function1
    {
        private static BinDayProvider _binBinDayProvider = new BinDayProvider();
        private static SouthBucksBinDayResolver _binDayResolver = new SouthBucksBinDayResolver(new HttpClient());

        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            var requestType = skillRequest.GetRequestType();

            SkillResponse response = null;

            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to Bin Day!");
                response.Response.ShouldEndSession = false;
            }

            else if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;

                if (intentRequest.Intent.Name == "whatbinisit")
                {
                    var binColor = _binBinDayProvider.GetBinName(DateTime.Now);
                    var dayOfWeek = _binDayResolver.GetBinDayForPostcodeAsync("HP9 2ET");

                    response = ResponseBuilder.Tell($"It's {binColor} bin on {dayOfWeek}");
                    response.Response.ShouldEndSession = true;
                }
            }

            return new OkObjectResult(response);
        }
    }

    public class BinDayProvider
    {
        private static class Bins
        {
            public const string Grey = nameof(Grey);
            public const string Blue = nameof(Blue);
        }

        private string GreyBin = "grey";

        private IList<(DateTime weekStart, string binColor)> _binDays = new List<(DateTime weekStart, string binColor)>
        {
            (DateTime.Parse("2019-11-18"), Bins.Grey),
            (DateTime.Parse("2019-11-25"), Bins.Blue)
        };

        public string GetBinName(DateTime date)
        {
            var justDate = date.Date;

            foreach (var (weekStart, binColor) in _binDays)
            {
                if (justDate >= weekStart && justDate < weekStart.AddDays(5))
                {
                    return binColor;
                }
            }

            return null;
        }
    }
}
