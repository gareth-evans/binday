using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace Tests.Acceptance.Alexa
{
    public static class ObjectExtensions
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public static StreamContent ToStreamContent(this object obj)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            var jsonWriter = new JsonTextWriter(writer);

            Serializer.Serialize(jsonWriter, obj);

            jsonWriter.Flush();

            stream.Position = 0;

            return new StreamContent(stream);
        }
    }
}