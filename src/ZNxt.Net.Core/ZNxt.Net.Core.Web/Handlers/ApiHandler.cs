using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Handlers
{

    public class ApiHandler
    {
        private readonly RequestDelegate _next;
        private readonly IRouting _routing;
        private readonly IDBService _dbService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IAssemblyLoader _assemblyLoader;
        private readonly IServiceResolver _serviceResolver;
        private readonly ILogger _logger;
        public ApiHandler(RequestDelegate next,ILogger logger,IDBService dbService, IRouting routing, IHttpContextProxy httpContextProxy, IAssemblyLoader assemblyLoader, IServiceResolver serviceResolver)
        {
            _next = next;
            _routing = routing;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _assemblyLoader = assemblyLoader;
            _serviceResolver = serviceResolver;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var route = _routing.GetRoute(_httpContextProxy.GetHttpMethod(), _httpContextProxy.GetURIAbsolutePath());
            if (route != null)
            {
                var type = _assemblyLoader.GetType(route.ExecultAssembly, route.ExecuteType);
                if (type != null)
                {
                    _logger.Debug(string.Format("Executing route:{0}", route.ToString()));
                    var controller = _serviceResolver.Resolve(type);
                    var method = controller.GetType().GetMethods().FirstOrDefault(f => f.Name == route.ExecuteMethod);
                    if (method != null)
                    {
                        context.Response.ContentType = route.ContentType;
                        object response = null;
                        if (method.ReturnType.BaseType == typeof(Task))
                        {
                            response = await(dynamic)method.Invoke(controller, null);
                        }
                        else
                        {
                            response = method.Invoke(controller, null);
                        }
                        if (response != null)
                        {
                            if (method.ReturnType == typeof(string))
                            {
                                await context.Response.WriteAsync((response as string));
                            }
                            //else if (method.ReturnType == typeof(JObject))
                            //{
                            //    await context.Response.WriteAsync((response.ToString() as string));
                            //}
                            else if(method.ReturnType == typeof(byte[]))
                            {
                                var byteResponse = response as byte[];
                                await context.Response.Body.WriteAsync(byteResponse,0, byteResponse.Length);
                            }
                            else
                            {
                                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                            }
                        }
                        else
                        {
                            await context.Response.Body.WriteAsync(new byte[] { });
                        }
                    }
                    return;
                }

            }
            await _next(context);

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
