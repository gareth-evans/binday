using System.Threading.Tasks;
using LightBDD.Framework.Scenarios;
using LightBDD.NUnit3;

namespace Tests.Acceptance.Alexa
{
    public class Cancelling : AzureFunctionFixture
    {
        [Scenario(Description = "Saying cancel during session")]
        public async Task Cancelling_Session()
        {
            await Runner.RunScenarioAsync(
                _ => When_I_cancel_the_session(),
                _ => Then_I_should_receive_the_response_EXPECTED("OK, I'll just take myself out")
            );
        }

        [Scenario(Description = "Saying stop during session")]
        public async Task Stopping_Session()
        {
            await Runner.RunScenarioAsync(
                _ => When_I_stop_the_session(),
                _ => Then_I_should_receive_the_response_EXPECTED("OK, I'll just take myself out")
            );
        }

        private async Task When_I_stop_the_session()
        {
            await Cancel("AMAZON.StopIntent");
        }


        private async Task When_I_cancel_the_session()
        {
            await Cancel("AMAZON.CancelIntent");
        }

        private async Task Cancel(string intentName)
        {
            using (var client = CreateClient())
            {
                var skillRequest = new AlexaIntentRequestBuilder()
                    .WithIntent(intentName)
                    .Build();

                Response = await client.PostAsync("api/alexa", skillRequest);
            }
        }
    }
}