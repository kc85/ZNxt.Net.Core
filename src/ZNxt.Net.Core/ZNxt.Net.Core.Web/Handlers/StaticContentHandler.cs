using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Handlers
{
    public class StaticContentHandler
    {
        private readonly RequestDelegate _next;
        private readonly IStaticContentHandler _staticContentHandler;
        private readonly IHttpContextProxy _httpProxy;
        public StaticContentHandler(RequestDelegate next, IStaticContentHandler staticContentHandler, IHttpContextProxy httpProxy)
        {
            _next = next;
            _staticContentHandler = staticContentHandler;
            _httpProxy = httpProxy;
        }

        public async Task Invoke(HttpContext context)
        {
            var paths = GetPages(context.Request.Path.Value);
            context.Response.ContentType = GetContentType(paths.First());

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            if (CommonUtility.IsTextConent(context.Response.ContentType))
            {
                string response = string.Empty;
                foreach (var path in paths)
                {
                    response = await _staticContentHandler.GetStringContentAsync(path);
                    if (response != null) break;
                }

                if (!string.IsNullOrEmpty(response))
                {
                    await context.Response.WriteAsync(response);

                    return;
                }
                else
                {
                    await _next(context);
                }
            }
            else
            {
                byte[] response = await _staticContentHandler.GetContentAsync(context.Request.Path.Value);
                if (response != null)
                {
                    await context.Response.Body.WriteAsync(response, 0, response.Length);
                    return;
                }
            }
            await _next(context);
        }

        public List<string> GetPages(string filePath)
        {
            return CommonUtility.IsDefaultPages(filePath);            
        }

        private string GetContentType(string filePath)
        {
            string contentType;

            if (CommonUtility.IsServerSidePage(filePath, true))
            {
                contentType = "text/html; charset=utf-8";
            }
            else
            {
                new FileExtensionContentTypeProvider().TryGetContentType(filePath, out contentType);
            }
             return contentType ?? "application/octet-stream";
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
