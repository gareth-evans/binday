using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using LightBDD.Framework.Scenarios;
using LightBDD.NUnit3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;

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
                var skillRequest = new SkillRequest
                {
                    Context = new Context(),
                    Session = new Session(),
                    Version = "1",
                    Request = new LaunchRequest { Type = "LaunchRequest" }
                };

               Response = await client.PostAsync("api/alexa", new ObjectContent(typeof(SkillRequest), skillRequest, new JsonMediaTypeFormatter()));
            }
        }

        private async Task Then_I_should_receive_the_response_EXPECTED(string expected)
        {
            var json = await Response.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<JObject>(json);
            var responseText = response["response"]["outputSpeech"]["text"].Value<string>();

            responseText.ShouldBe(expected);
        }
    }


}
