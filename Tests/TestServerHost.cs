﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using BinDay;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using RunExtensions = Microsoft.AspNetCore.Builder.RunExtensions;

namespace Tests
{
    public class TestServerHost
    {
        [Test]
        public async Task Foo()
        {
            var applicationFactory = new TestWebApplicationFactory<Startup>();
            var client = applicationFactory.CreateServer().CreateClient();

            var skillRequest = new SkillRequest
            {
                Context = new Context(),
                Session =  new Session(),
                Version = "1",
                Request = new LaunchRequest() { Type = "LaunchRequest" }
            };
            var result = await client.PostAsync("api/alexa", new ObjectContent(typeof(SkillRequest), skillRequest, new JsonMediaTypeFormatter()));

            var json = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<JObject>(json);
            var responseText = response["response"]["outputSpeech"]["text"].Value<string>();

            responseText.ShouldBe("Welcome to Bin Day!");
        }
    }

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
                    var logger = context.RequestServices.GetRequiredService<ILogger>();

                    if (context.Request.Path.StartsWithSegments("/api/alexa"))
                    {
                        var function = context.RequestServices.GetRequiredService<AlexaSkill>();

                        var result = await function.Run(context.Request, logger);

                        await context.ExecuteResultAsync(result);

                        return;
                    }

                    throw new NotSupportedException();
                });
            });
        }

        public TestServer CreateServer() => new TestServer(_webHostBuilder);

    }

    public class TestFunctionsHostBuilder : IFunctionsHostBuilder
    {
        public TestFunctionsHostBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    public static class HttpContextExtensions
    {
        private static readonly RouteData EmptyRouteData = new RouteData();
        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public static  Task ExecuteResultAsync(this HttpContext context, IActionResult result)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (result == null) throw new ArgumentNullException(nameof(result));

            var routeData = context.GetRouteData() ?? EmptyRouteData;

            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

            return result.ExecuteResultAsync(actionContext);
        }
    }
}