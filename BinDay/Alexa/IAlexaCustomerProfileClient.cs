using System;
using System.Threading.Tasks;
using Alexa.NET.CustomerProfile;

namespace BinDay.Alexa
{
    public interface IAlexaCustomerProfileClient
    {
        Task<RegionAndPostalCode> GetRegionAndPostalCodeAsync(
            Uri apiAddress,
            string accessToken,
            string deviceId);
    }
}