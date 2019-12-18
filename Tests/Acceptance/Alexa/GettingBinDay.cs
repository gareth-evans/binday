using System.Threading.Tasks;
using Alexa.NET.Request;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.NUnit3;
using NUnit.Framework;

namespace Tests.Acceptance.Alexa
{
    [TestFixture]
    [FeatureDescription(
        @"In order to find out bin information
As an user
I want to launch the Alexa skill")]
    public partial class GettingBinDay
    {
        [Scenario(Description = "")] 
        public async Task Launching_Skill()
        {
            await Runner.RunScenarioAsync(
                () => Task.CompletedTask
            );
        }
    }

    public partial class GettingBinDay : FeatureFixture
    {
        private SkillRequest skillRequest;

        [SetUp]
        public void Setup()
        {
            skillRequest = new SkillRequest();
        }


        private void Given_the_user_is_about_to_login()
        {
        }

        private void When_the_user_clicks_login_button()
        {
        }

        private void Then_the_login_operation_should_be_successful()
        {
        }
    }
}
