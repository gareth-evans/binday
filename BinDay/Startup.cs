using Alexa.NET.Request;
using BinDay.Alexa;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BinDay.Startup))]

namespace BinDay
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services
                .AddLogging()
                .AddHttpClient()
                .AddSingleton<SouthBucksBinDayResolver>()
                .AddSingleton<ITableStoragePostcodeResolver, TableStoragePostcodeResolver>()
                .AddTransient<IUserPostcodeRetriever<SkillRequest>, AlexaUserPostcodeRetriever>();

            services.AddHttpClient<IAlexaCustomerProfileClient, AlexaCustomerProfileClient>();
        }
    }
}