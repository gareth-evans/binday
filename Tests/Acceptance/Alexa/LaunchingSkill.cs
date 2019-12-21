using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using LightBDD.Framework.Scenarios;
using LightBDD.NUnit3;
using Tests.Acceptance.Alexa;

namespace Tests.Acceptance
{
    public class LaunchingSkill : AzureFunctionFixture
    {
        [Scenario(Description = "")]
        public async Task Launching_Skill()
        {
            await Runner.RunScenarioAsync(
                _ => When_I_launch_bin_day(),
                _ => Then_I_should_receive_the_response_EXPECTED("Welcome to Bin Day!")
            );
        }

        private async Task When_I_launch_bin_day()
        {
            using (var client = CreateClient())
            {
                var skillRequest = new AlexaLaunchRequestBuilder().Create();

                Response = await client.PostAsync("api/alexa", skillRequest);
            }
        }
    }
}
