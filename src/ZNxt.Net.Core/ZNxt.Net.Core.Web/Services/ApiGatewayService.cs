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
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public class ApiGatewayService : IApiGatewayService
    {
        private readonly IInMemoryCacheService _inMemoryCacheService;
        private readonly IHttpContextProxy _httpContextProxy;
        public ApiGatewayService(IInMemoryCacheService inMemoryCacheService,IHttpContextProxy httpContextProxy)
        {
            _inMemoryCacheService = inMemoryCacheService;
            _httpContextProxy = httpContextProxy;
        }
        public async Task<JObject> CallAsync(string method, string route, string querystring = "", JObject requestBody = null, Dictionary<string, string> headres = null, string baseUrl = "")
        {
           
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {

                    await BuildRequestUrl(request, method, route, querystring, baseUrl);

                    if (method == CommonConst.ActionMethods.POST || method == CommonConst.ActionMethods.PUT || method == CommonConst.ActionMethods.DELETE)
                    {
                        BuildRequestBody(requestBody, request);
                    }
                    BuildHeaders(headres, request);
                    var httpresponse = await client.SendAsync(request);

                    if (httpresponse.IsSuccessStatusCode)
                    {
                        var data = await httpresponse.Content.ReadAsStringAsync();
                        return JObject.Parse(data);
                    }
                    else
                    {
                        throw new Exception($"Error while calling route http code : {httpresponse.StatusCode}");
                    }
                }
            }
        }

        private async Task BuildRequestUrl(HttpRequestMessage request, string method, string route, string querystring , string baseUrl )
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = await GetBaseUrl(method, route);
            }
            request.Method = new HttpMethod(method);
            if (string.IsNullOrEmpty(querystring))
            {
                querystring = _httpContextProxy.GetQueryString();
            }
            request.RequestUri = new Uri($"{baseUrl}/api{route}?{querystring}");
        }
        private void BuildHeaders(Dictionary<string, string> headres, HttpRequestMessage request)
        {
            foreach (var header in _httpContextProxy.GetHeaders())
            {
               // request.Headers.Add(header.Key, header.Value);
            }
            if (headres != null)
            {
                foreach (var header in headres)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
        }

        private void BuildRequestBody(JObject requestBody, HttpRequestMessage request)
        {
            if (requestBody != null)
            {
                var stringContent = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
                request.Content = stringContent;
            }
            else
            {
                var requestBodyString = _httpContextProxy.GetRequestBody();
                if (string.IsNullOrEmpty(requestBodyString))
                {
                    var stringContent = new StringContent(requestBodyString, Encoding.UTF8, "application/json");
                    request.Content = stringContent;
                }
            }
        }

        public async  Task<RoutingModel> GetRouteAsync(string method, string route)
        {
            JToken routeData = await GetRouteData(method, route);

            if (routeData != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<RoutingModel>(routeData.ToString());
            }
            else
            {
                return null;
            }

        }
        private async Task<string> GetBaseUrl(string method, string route)
        {
            JToken routeData = await GetRouteData(method, route);
            if (routeData != null)
            {
                return routeData[CommonConst.CommonField.MODULE_ENDPOINT].ToString();
            }
            else
            {
                throw new KeyNotFoundException("Route not found");
            }

        }

        private async Task<JToken> GetRouteData(string method, string route)
        {
            JObject routes = await GetAllRoutes();

            var routeData = (routes[CommonConst.CommonField.DATA] as JArray).FirstOrDefault(f => f["Route"].ToString() == route && f["Method"].ToString() == method);
            return routeData;
        }

        private async Task<JObject> GetAllRoutes()
        {
            const string routeCacheKey = "GatewayRouteCache";
            var routes = _inMemoryCacheService.Get<JObject>(routeCacheKey);
            if (routes == null)
            {
                routes = await CallAsync(CommonConst.ActionMethods.GET, "/gateway/routes", "", null, null, ApplicationConfig.ApiGatewayEndpoint);
                _inMemoryCacheService.Put<JObject>(routeCacheKey, routes);
            }

            return routes;
        }
    }
}
