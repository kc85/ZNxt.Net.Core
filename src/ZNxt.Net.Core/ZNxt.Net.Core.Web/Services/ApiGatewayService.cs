using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using System.Linq;

namespace ZNxt.Net.Core.Web.Services
{
    public class ApiGatewayService : IApiGatewayService
    {
        private readonly IInMemoryCacheService _inMemoryCacheService;
        public ApiGatewayService(IInMemoryCacheService inMemoryCacheService)
        {
            _inMemoryCacheService = inMemoryCacheService;
        }
        public async Task<JObject> CallAsync(string method, string route, string querystring = "", JObject requestBody = null, Dictionary<string, string> headres = null, string baseUrl = "")
        {
            if(string.IsNullOrEmpty( baseUrl))
            {
                baseUrl = await GetBaseUrl(method, route);
            }
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = new HttpMethod(method);
                    request.RequestUri = new Uri($"{baseUrl}/api{route}?{querystring}");
                    if (requestBody != null) {
                        var stringContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
                        request.Content = stringContent;
                    }
                    if (headres != null)
                    {
                        foreach (var header in headres)
                        {
                            request.Headers.Add(header.Key, header.Value);
                        }
                    }
                  var httpresponse =   await client.SendAsync(request);

                    if (httpresponse.IsSuccessStatusCode)
                    {
                        var data = await httpresponse.Content.ReadAsStringAsync();
                        return JObject.Parse(data);
                    }
                    else
                    {
                        throw new Exception("Error while calling route ");
                    }
                }
            }
        }

        private async Task<string> GetBaseUrl(string method, string route)
        {
            const string routeCacheKey = "GatewayRouteCache";
            var routes = _inMemoryCacheService.Get<JObject>(routeCacheKey);
            if (routes == null)
            {
                routes = await CallAsync(CommonConst.ActionMethods.GET, "/gateway/routes", "", null, null, ApplicationConfig.ApiGatewayEndpoint);
                _inMemoryCacheService.Put<JObject>(routeCacheKey, routes);
            }

            var routeData = (routes[CommonConst.CommonField.DATA] as JArray).FirstOrDefault(f => f["Route"].ToString() == route && f["Method"].ToString() == method);
            if (routeData != null)
            {
                return routeData[CommonConst.CommonField.MODULE_ENDPOINT].ToString();
            }
            else
            {
                throw new Exception("Route not found");
            }

        }
    }
}
