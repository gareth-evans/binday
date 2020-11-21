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
using Newtonsoft.Json.Linq;

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
            _userPostcodeRetriever =
                userPostcodeRetriever ?? throw new ArgumentNullException(nameof(userPostcodeRetriever));
            _dateFormatter = dateFormatter;
        }

        [FunctionName("Alexa")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.Log(LogLevel.Information, "Received Alexa request");

            try
            {
                await req.EnsureValidAlexaSignatureAsync();

                var json = await req.ReadAsStringAsync();

                var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

                SkillResponse response = null;

                if (skillRequest.Request is LaunchRequest launchRequest)
                {
                    response = ResponseBuilder
                        .Tell("Welcome to Bin Day!");

                    response.Response.ShouldEndSession = false;
                }
                else if (skillRequest.Request is IntentRequest intentRequest)
                {
                    var intentName = intentRequest.Intent.Name;

                    if (intentName == "whatbinisit")
                    {
                        var userPostcode = await _userPostcodeRetriever.GetPostcodeAsync(skillRequest);

                        if (userPostcode == null)
                        {
                            response = ResponseBuilder.Tell($"Sorry I can't check your collection schedule without your postcode, and I don't seem to have permission to access it." +
                                                            $" Please check the permissions I have.");
                            response.Response.ShouldEndSession = true;
                        }
                        else
                        {
                            var dayOfWeek = await _binDayResolver.GetBinInfoAsync(userPostcode, DateTime.Now);

                            var friendlyDayDescription = _dateFormatter.CreateFriendlyDescription(dayOfWeek.Date);

                            response = ResponseBuilder.Tell($"It's {dayOfWeek.Description} bin {friendlyDayDescription}");
                            response.Response.ShouldEndSession = true;
                        }
                    }
                    else if (intentName == BuiltInIntent.Cancel || intentName == BuiltInIntent.Stop)
                    {
                        response = ResponseBuilder.Tell("OK, I'll just take myself out");
                        response.Response.ShouldEndSession = true;
                    }
                    else if (intentName == BuiltInIntent.Help)
                    {
                        response = ResponseBuilder.Tell("You can ask me which bin it is.");
                        response.Response.ShouldEndSession = false;
                    }
                }
                else if (skillRequest.Request is SessionEndedRequest sessionEndedRequest)
                {
                    response = ResponseBuilder.Tell("OK, I'll just take myself out");
                    response.Response.ShouldEndSession = true;
                }

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                log.LogCritical(ex.Message);
                throw;
            }
        }
    }
}
