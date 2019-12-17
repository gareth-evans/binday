using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Alexa.NET.CustomerProfile;
using Newtonsoft.Json;

namespace BinDay.Alexa
{
    public class AlexaCustomerProfileClient : IAlexaCustomerProfileClient
    {
        private readonly HttpClient _httpClient;

        public AlexaCustomerProfileClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RegionAndPostalCode> GetRegionAndPostalCodeAsync(Uri apiAddress, string accessToken, string deviceId)
        {
            var uri = new Uri(apiAddress, $"/v1/devices/{deviceId}/settings/address/countryAndPostalCode");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue($"Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new NotSupportedException("We need to handle 403 response and throw anything else");
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<RegionAndPostalCode>(json);
        }
    }
}