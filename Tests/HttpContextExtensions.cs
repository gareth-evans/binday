using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace Tests
{
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