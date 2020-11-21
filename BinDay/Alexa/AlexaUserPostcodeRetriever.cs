using System;
using System.Threading.Tasks;
using Alexa.NET.Request;

namespace BinDay.Alexa
{
    public class AlexaUserPostcodeRetriever : IUserPostcodeRetriever<SkillRequest>
    {
        private readonly IAlexaCustomerProfileClient _customerProfileClient;

        public AlexaUserPostcodeRetriever(IAlexaCustomerProfileClient customerProfileClient)
        {
            _customerProfileClient = customerProfileClient ?? throw new ArgumentNullException(nameof(customerProfileClient));
        }

        public async Task<string> GetPostcodeAsync(SkillRequest request)
        {
            var regionAndPostcode = await _customerProfileClient.GetRegionAndPostalCodeAsync(
                new Uri(request.Context.System.ApiEndpoint),
                request.Context.System.ApiAccessToken,
                request.Context.System.Device.DeviceID
            );

            return regionAndPostcode?.PostalCode;
        }
    }
}