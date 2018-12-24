using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZNxt.Net.Core.Web.ContentHandler
{
    public class StaticContentHandler
    {
        public StaticContentHandler(RequestDelegate next)
        {
            // This is an HTTP Handler, so no need to store next
        }

        public async Task Invoke(HttpContext context)
        {
            string response = GenerateResponse(context);

            context.Response.ContentType = GetContentType();
            await context.Response.WriteAsync(response);
        }

        // ...

        private string GenerateResponse(HttpContext context)
        {
            string title = context.Request.Query["title"];
            return string.Format("Title of the report: {0}", title);
        }

        private string GetContentType()
        {
            return "text/plain";
        }
    }
    public static class StaticContentHandlerExtensions
    {
        public static IApplicationBuilder UseStaticContentHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<StaticContentHandler>();
        }
    }
}
