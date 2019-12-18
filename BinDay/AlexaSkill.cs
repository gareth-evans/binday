using System;
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
    public class AlexaSkill
    {
        private readonly SouthBucksBinDayResolver _binDayResolver;
        private readonly IUserPostcodeRetriever<SkillRequest> _userPostcodeRetriever;
        private readonly IDateFormatter _dateFormatter;

        public AlexaSkill(
            SouthBucksBinDayResolver binDayResolver,
            IUserPostcodeRetriever<SkillRequest> userPostcodeRetriever,
            IDateFormatter dateFormatter)
        {
            _binDayResolver = binDayResolver ?? throw new ArgumentNullException(nameof(binDayResolver));
            _userPostcodeRetriever = userPostcodeRetriever ?? throw new ArgumentNullException(nameof(userPostcodeRetriever));
            _dateFormatter = dateFormatter;
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
                    var dayOfWeek = await _binDayResolver.GetBinInfoAsync(userPostcode, DateTime.Now);

                    var friendlyDayDescription = _dateFormatter.CreateFriendlyDescription(dayOfWeek.Date);

                    response = ResponseBuilder.Tell($"It's {dayOfWeek.Description} bin {friendlyDayDescription}");
                    response.Response.ShouldEndSession = true;
                }
            }

            return new OkObjectResult(response);
        }
    }
}
