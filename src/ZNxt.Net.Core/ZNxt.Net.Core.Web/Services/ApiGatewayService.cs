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
using ZNxt.Net.Core.Helpers;

namespace ZNxt.Net.Core.Web.Services
{
    public class ApiGatewayService : IApiGatewayService
    {
        private readonly IInMemoryCacheService _inMemoryCacheService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        public ApiGatewayService(IInMemoryCacheService inMemoryCacheService,IHttpContextProxy httpContextProxy,ILogger logger)
        {
            _inMemoryCacheService = inMemoryCacheService;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
        }
        public async Task<JObject> CallAsync(string method, string route, string querystring = "", JObject requestBody = null, Dictionary<string, string> headres = null, string baseUrl = "")
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    };
                using (var client = new HttpClient(handler))
                {
                    using (var request = new HttpRequestMessage())
                    {

                        await BuildRequestUrl(request, method, route, querystring, baseUrl);

                        if (method == CommonConst.ActionMethods.POST || method == CommonConst.ActionMethods.PUT || method == CommonConst.ActionMethods.DELETE)
                        {
                            BuildRequestBody(requestBody, request);
                        }
                        await BuildHeaders(headres, request);
                        _logger.Debug($"callng  {request.Method} : {request.RequestUri}");
                        var httpresponse = await client.SendAsync(request);


                        if (httpresponse.IsSuccessStatusCode)
                        {
                            var data = await httpresponse.Content.ReadAsStringAsync();
                            if (httpresponse.Content.Headers.ContentType.MediaType.IndexOf("application/json") !=-1)
                            {
                                return JObject.Parse(data);
                            }
                            else
                            {
                                var response = new JObject();
                                response["content_type"] = httpresponse.Content.Headers.ContentType.MediaType;
                                response["data"] = data;
                                return response;
                            }
                        }
                        else
                        {
                            _logger.Error($"Http Status:{httpresponse.StatusCode }, url : {request.RequestUri}, Headers:{string.Join(",", request.Headers.Select(f=> $"{f.Key}:{f.Value.First()}"))}. Body:{ requestBody}");
                            if (httpresponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                throw new UnauthorizedAccessException();
                            }
                            else
                            {
                                throw new Exception($"Error while calling route http code : {httpresponse.StatusCode}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error callng ApiGatewayService.CallAsync {method}:{route}?{querystring}. {ex.Message}" , ex);
                throw;
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
            if ( !string.IsNullOrEmpty(querystring) && querystring.IndexOf("?") != 0)
            {
                querystring = $"?{querystring}";
            }
            if (route.IndexOf("~/") == 0)
            {   
                request.RequestUri = new Uri($"{baseUrl}{route.Remove(0, 1)}{querystring}");
            }
            else
            {
                request.RequestUri = new Uri($"{baseUrl}/api{route}{querystring}");
            }
        }
        private async Task BuildHeaders(Dictionary<string, string> headres, HttpRequestMessage request)
        {
            foreach (var header in _httpContextProxy.GetHeaders())
            {
                if (header.Key == CommonConst.CommonField.OAUTH_CLIENT_ID || header.Key == CommonConst.CommonField.OAUTH_CLIENT_SECRET)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            if (headres != null)
            {
                foreach (var header in headres)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            if (!request.Headers.Contains("Authorization"))
            {
                var accessToken = await _httpContextProxy.GetAccessTokenAync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Add("Authorization", $"Bearer {accessToken}");
                }
            }
            if (!request.Headers.Contains(CommonConst.CommonField.TRANSACTION_ID))
            {
                request.Headers.Add(CommonConst.CommonField.TRANSACTION_ID, _logger.TransactionId);
            }
            if (!request.Headers.Contains(CommonConst.CommonField.API_AUTH_TOKEN))
            {
                request.Headers.Add(CommonConst.CommonField.API_AUTH_TOKEN, CommonUtility.GetApiAuthKey());
            }
            if (!request.Headers.Contains(CommonConst.CommonValue.TENANT_KEY))
            {
                var orgkey = _httpContextProxy.GetHeader(CommonConst.CommonValue.TENANT_KEY);
                if (string.IsNullOrEmpty(orgkey))
                {
                    orgkey = _httpContextProxy.GetQueryString(CommonConst.CommonValue.TENANT_KEY);
                }
                request.Headers.Add(CommonConst.CommonValue.TENANT_KEY, orgkey);
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
                if (!string.IsNullOrEmpty(requestBodyString))
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
                throw new KeyNotFoundException($"Route not found {method}:{route}");
            }

        }

        private async Task<JToken> GetRouteData(string method, string route)
        {
            JObject routes = await GetAllRoutes();
            if (routes != null)
            {
                var routeData = (routes[CommonConst.CommonField.DATA] as JArray).FirstOrDefault(f => f["Route"].ToString() == route && f["Method"].ToString() == method);
                return routeData;
            }
            else
            {
                return null;
            }
        }

        private async Task<JObject> GetAllRoutes()
        {
            const string routeCacheKey = "GatewayRouteCache";
            var routes = _inMemoryCacheService.Get<JObject>(routeCacheKey);
            if (routes == null)
            {
                if (!string.IsNullOrEmpty(ApplicationConfig.ApiGatewayEndpoint))
                {
                    routes = await CallAsync(CommonConst.ActionMethods.GET, "/gateway/routes", "", null, null, ApplicationConfig.ApiGatewayEndpoint);
                    _inMemoryCacheService.Put<JObject>(routeCacheKey, routes);
                }
            }

            return routes;
        }
    }
}
