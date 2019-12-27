using System;
using System.IO;
using System.Threading.Tasks;
using Alexa.NET.Request;
using BinDay.Alexa;
using BinDay.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
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
                .AddTransient<IUserPostcodeRetriever<SkillRequest>, AlexaUserPostcodeRetriever>()
                .AddSingleton<IDateProvider, DateProvider>()
                .AddSingleton<IDateFormatter, DateFormatter>();

            services.AddHttpClient<IAlexaCustomerProfileClient, AlexaCustomerProfileClient>();
        }
    }


    public static class ValidateAlexaSignature
    {
        public static HttpRequest EnableRewind(this HttpRequest request, int bufferThreshold = DefaultBufferThreshold, long? bufferLimit = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var body = request.Body;
            if (!body.CanSeek)
            {
                var fileStream = new FileBufferingReadStream(body, bufferThreshold, bufferLimit, _getTempDirectory);
                request.Body = fileStream;
                request.HttpContext.Response.RegisterForDispose(fileStream);
            }
            return request;
        }

        private const int DefaultBufferThreshold = 1024 * 30;

        private readonly static Func<string> _getTempDirectory = () => TempDirectory;

        private static string _tempDirectory;

        public static string TempDirectory
        {
            get
            {
                if (_tempDirectory == null)
                {
                    // Look for folders in the following order.
                    var temp = Environment.GetEnvironmentVariable("ASPNETCORE_TEMP") ??     // ASPNETCORE_TEMP - User set temporary location.
                               Path.GetTempPath();                                      // Fall back.

                    if (!Directory.Exists(temp))
                    {
                        // TODO: ???
                        throw new DirectoryNotFoundException(temp);
                    }

                    _tempDirectory = temp;
                }

                return _tempDirectory;
            }
        }

        public static async Task EnsureValidAlexaSignatureAsync(this HttpRequest request)
        {
            request.EnableRewind();

            // Verify SignatureCertChainUrl is present
            request.Headers.TryGetValue("SignatureCertChainUrl", out var signatureChainUrl);

            if (string.IsNullOrWhiteSpace(signatureChainUrl))
            {
                throw new InvalidSignatureException();
            }

            Uri certUrl;

            try
            {
                certUrl = new Uri(signatureChainUrl);
            }
            catch
            {
                throw new InvalidSignatureException();
            }

            // Verify SignatureCertChainUrl is Signature
            request.Headers.TryGetValue("Signature", out var signature);

            if (string.IsNullOrWhiteSpace(signature))
            {
                throw new InvalidSignatureException();
            }

            var body = new StreamReader(request.Body).ReadToEnd();

            request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidSignatureException();
            }

            var valid = await RequestVerification.Verify(signature, certUrl, body);

            if (!valid)
            {
                throw new InvalidSignatureException();
            }
        }
    }
}