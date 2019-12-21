using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BinDay;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests
{
    public class TestWebApplicationFactory<TStartup>
        where TStartup : FunctionsStartup, new()
    {
        private readonly IWebHostBuilder _webHostBuilder;

        public TestWebApplicationFactory()
        {
            _webHostBuilder = new WebHostBuilder();

            var functionsHostBuilder = new TestFunctionsHostBuilder(new ServiceCollection());

            var startup = new TStartup();

            startup.Configure(functionsHostBuilder);

            var loggerMock = new Mock<ILogger>();

            _webHostBuilder.ConfigureServices(services =>
            {
                services
                    .AddMvcCore()
                    .AddJsonFormatters();

                services
                    .AddTransient<TestFunctionsHostBuilder>()
                    .AddTransient<AlexaSkill>()
                    .AddSingleton(loggerMock.Object);

                foreach (var service in functionsHostBuilder.Services)
                {
                    services.Add(service);
                }
            });

            _webHostBuilder.Configure(app =>
            {
                app.Run(async context =>
                {
                    //TODO: function discovery should be done once at start up
                    var functions = DiscoverFunctions();

                    foreach (var function in functions)
                    {
                        if (!context.Request.Path.StartsWithSegments($"/api/{function.name}")) continue;

                        var parameters = function.methodInfo.GetParameters();

                        var arguments = parameters.Select(parameter =>
                        {
                            if (parameter.ParameterType == typeof(HttpRequest)) return context.Request;
                            var argument = context.RequestServices.GetService(parameter.ParameterType);
                            if (argument == null) throw new ArgumentException($"Could not resolve type {parameter.ParameterType} for method {function.methodInfo.DeclaringType}.{function.methodInfo.Name}");
                            
                            return argument;
                        }).ToArray();

                        var target = context.RequestServices.GetService(function.methodInfo.DeclaringType);

                        var invocationTask = (Task<IActionResult>)function.methodInfo.Invoke(target, arguments); //TODO: assuming return type of Task<IActionResult> for now

                        var result = await invocationTask;

                        await context.ExecuteResultAsync(result);

                        return;
                    }

                    throw new NotSupportedException();
                });
            });
        }

        public TestServer CreateServer() => new TestServer(_webHostBuilder);

        private IEnumerable<(string name, MethodInfo methodInfo)> DiscoverFunctions()
        {
            var assembly = typeof(TStartup).Assembly; //TODO: we probably want to allow multiple assemblies to used for discovery
            var allTypes = assembly.GetTypes();

            return allTypes
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                .SelectMany(method => method.GetCustomAttributes<FunctionNameAttribute>()
                    .Select(functionName => (functionName.Name, method)));
        }

        private class TestFunctionsHostBuilder : IFunctionsHostBuilder
        {
            public TestFunctionsHostBuilder(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }
        }
    }
}