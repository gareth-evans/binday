﻿using System.Net.Http;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;

namespace Tests.Acceptance.Alexa
{
    public class AlexaLaunchRequestBuilder
    {
        public HttpContent Create()
        {
            var skillRequest = new SkillRequest
            {
                Context = new Context(),
                Session = new Session(),
                Version = "1",
                Request = new LaunchRequest { Type = "LaunchRequest" }
            };

            return skillRequest.ToStreamContent();
        }
    }
}