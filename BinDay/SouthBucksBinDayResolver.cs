using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BinDay
{
    public class SouthBucksBinDayResolver
    {
        private readonly HttpClient _httpClient;

        public SouthBucksBinDayResolver(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DayOfWeek> GetBinDayForPostcodeAsync(string postcode)
        {
            var cacheId = await GetCacheIdAsync(postcode);
            var locationId = await GetLocationIdAsync(postcode, cacheId);
            var binDayOfWeek = await GetBinDayOfWeekAsync(locationId);

            return binDayOfWeek;
        }

        /// <summary>
        /// This calls an endpoint that returns a list of addresses for the postcode. At the moment
        /// we are assuming that all addresses will have the same collection day, so we'll use index 0 in a later call.
        /// Here we only care about the cache id so we can pass it to the next API call
        /// </summary>
        /// <param name="postcode"></param>
        /// <returns></returns>
        private async Task<string> GetCacheIdAsync(string postcode)
        {
            var urlEncodedPostcode = WebUtility.UrlEncode(postcode);

            var url =
                $"https://isa.chiltern.gov.uk/LocalView/ServiceProxies/LocatorHub/Rest/Match/e48a1266-5185-453a-bf77-c7d2ddf15e1f/ADDRESS?Query={urlEncodedPostcode}&format=jsonp";

            using (var response = await _httpClient.GetAsync(url))
            {
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                return jObject["CacheIdentifier"]?.Value<string>();
            }
        }

        private async Task<string> GetLocationIdAsync(string postcode, string cacheId)
        {
            const int pickedItem = 0;
            var urlEncodedPostcode = WebUtility.UrlEncode(postcode);

            var url =
                $"https://isa.chiltern.gov.uk/LocalView/ServiceProxies/LocatorHub/Rest/Match/e48a1266-5185-453a-bf77-c7d2ddf15e1f/ADDRESS?Query={urlEncodedPostcode}&PickedItem={pickedItem}&CacheID={cacheId}&format=jsonp";

            using (var response = await _httpClient.GetAsync(url))
            {
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                return jObject["MatchedRecord"]["R"][0]?.Value<string>();
            }
        }

        private static Regex DayOfWeekRegex = new Regex("Monday|Tueday|Wednesday|Thursday|Friday", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private async Task<DayOfWeek> GetBinDayOfWeekAsync(string locationId)
        {
            var url =
                $"https://isa.chiltern.gov.uk/localview/ServiceProxies/Feed/Rest.svc/SBDC%20Waste%20Rounds%20Calendar%20Search/Location/{locationId}?sortByDistance=true&format=jsonp";

            using (var response = await _httpClient.GetAsync(url))
            {
                var json = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                var title = jObject["channel"]["item"][0]["title"]?.Value<string>();

                var match = DayOfWeekRegex.Match(title);
                var day = match.Groups[0].Value;

                return Enum.Parse<DayOfWeek>(day);
            }
        }
    }
}