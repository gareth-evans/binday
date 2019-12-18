using System.IO;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;

namespace Tests.Acceptance.Alexa
{
    public class AlexaLaunchRequestBuilder
    {
        public HttpRequest Create()
        {
            var skillRequest = new SkillRequest
            {
                Request = new LaunchRequest()
            };

            return HttpRequestBuilder.Create(skillRequest);
        }
    }

    public static class HttpRequestBuilder
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public static HttpRequest Create(SkillRequest skillRequest)
        {
            var json = JsonConvert.SerializeObject(skillRequest);
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var jsonWriter = new JsonTextWriter(writer);

            Serializer.Serialize(jsonWriter, json);

            stream.Position = 0;

            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = stream
            };

            return request;
        }
    }
}