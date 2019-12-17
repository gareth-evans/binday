using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared;

namespace BinDay
{
    public class AlexaSkill
    {
        private readonly BinDayProvider _binBinDayProvider = new BinDayProvider();
        private readonly SouthBucksBinDayResolver _binDayResolver;
        private readonly IUserPostcodeRetriever<SkillRequest> _userPostcodeRetriever;

        public AlexaSkill(
            SouthBucksBinDayResolver binDayResolver,
            IUserPostcodeRetriever<SkillRequest> userPostcodeRetriever,
            ITableStoragePostcodeResolver tableStoragePostcodeResolver)
        {
            _binDayResolver = binDayResolver ?? throw new ArgumentNullException(nameof(binDayResolver));
            _userPostcodeRetriever = userPostcodeRetriever ?? throw new ArgumentNullException(nameof(userPostcodeRetriever));
        }

        [FunctionName("Alexa")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            var requestType = skillRequest.GetRequestType();

            SkillResponse response = null;

            if (skillRequest.Request is LaunchRequest launchRequest)
            {
                response = ResponseBuilder
                    .Tell("Welcome to Bin Day!");

                response.Response.ShouldEndSession = false;
            }
            else if (skillRequest.Request is IntentRequest intentRequest)
            {
                if (intentRequest.Intent.Name == "whatbinisit")
                {
                    var userPostcode = await _userPostcodeRetriever.GetPostcodeAsync(skillRequest);
                    
                    //var binColor = _binBinDayProvider.GetBinName(DateTime.Now);
                    var dayOfWeek = await _binDayResolver.GetBinInfoAsync(userPostcode, DateTime.Now);

                    response = ResponseBuilder.Tell($"It's {dayOfWeek.Description} bin on {dayOfWeek.Date}");
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
                if (justDate >= weekStart && justDate < weekStart.AddDays(10))
                {
                    return binColor;
                }
            }

            return null;
        }
    }
}
