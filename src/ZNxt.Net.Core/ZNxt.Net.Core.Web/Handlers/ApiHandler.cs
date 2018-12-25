using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZNxt.Net.Core.Web.Handlers
{
    public class ApiHandler
    {
        private readonly RequestDelegate _next;
        public ApiHandler(RequestDelegate next)
        {
            _next = next;
            // This is an HTTP Handler, so no need to store next
        }

        public async Task Invoke(HttpContext context)
        {
            string response = GenerateResponse(context);

            context.Response.ContentType = GetContentType();
            await context.Response.WriteAsync(response);
            // await _next(context);
        }

        // ...

        private string GenerateResponse(HttpContext context)
        {
            return string.Format("Api Handler");
        }

        private string GetContentType()
        {
            return "text/plain";
        }
    }
    public static class ApiHandlerExtensions
    {
        public static IApplicationBuilder UseApiHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiHandler>();
        }
    }
}
