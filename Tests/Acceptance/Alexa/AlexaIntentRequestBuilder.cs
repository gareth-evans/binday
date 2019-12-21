using System.Net.Http;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;

namespace Tests.Acceptance.Alexa
{
    public class AlexaIntentRequestBuilder
    {
        private string _intentName;

        public AlexaIntentRequestBuilder WithIntent(string intentName)
        {
            _intentName = intentName;
            return this;
        }

        public HttpContent Build()
        {
            var skillRequest = new SkillRequest
            {
                Context = new Context(),
                Session = new Session(),
                Version = "1",
                Request = new IntentRequest { Type = "IntentRequest", Intent = new Intent { Name = _intentName } }
            };

            return skillRequest.ToStreamContent();
        }
    }
}