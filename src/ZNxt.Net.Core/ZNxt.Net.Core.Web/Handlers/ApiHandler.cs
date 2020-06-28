using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Consts;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Helpers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
        private readonly IResponseBuilder _responseBuilder;
        private readonly IApiGatewayService _apiGatewayService;
        private readonly IInMemoryCacheService _inMemoryCacheService;
        private readonly IZNxtUserService _zNxtUserService;
        public ApiHandler(RequestDelegate next, ILogger logger, IDBService dbService, IRouting routing,
            IHttpContextProxy httpContextProxy, IAssemblyLoader assemblyLoader, IServiceResolver serviceResolver, IResponseBuilder responseBuilder,
            IApiGatewayService apiGatewayService, IInMemoryCacheService inMemoryCacheService, IZNxtUserService zNxtUserService)
        {
            _next = next;
            _routing = routing;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _assemblyLoader = assemblyLoader;
            _serviceResolver = serviceResolver;
            _logger = logger;
            _responseBuilder = responseBuilder;
            _apiGatewayService = apiGatewayService;
            _inMemoryCacheService = inMemoryCacheService;
            _zNxtUserService = zNxtUserService;
        }

        public async Task Invoke(HttpContext context, IAuthorizationService authorizationService)
        {
            var route = _routing.GetRoute(_httpContextProxy.GetHttpMethod(), _httpContextProxy.GetURIAbsolutePath());
            if (route != null && !string.IsNullOrEmpty(route.ExecultAssembly) && !string.IsNullOrEmpty(route.ExecuteType))
            {
                if (AuthorizedRoute(context, route, authorizationService))
                {
                    var type = _assemblyLoader.GetType(route.ExecultAssembly, route.ExecuteType);
                    if (type != null)
                    {
                        context.Response.Headers[CommonConst.CommonField.MODULE_NAME] = route.module;
                        context.Response.Headers[CommonConst.CommonField.ROUTE] = route.Route;
                        
                        _logger.Debug(string.Format("Executing route:{0}", route.ToString()));
                        var controller = _serviceResolver.Resolve(type);
                        var method = controller.GetType().GetMethods().FirstOrDefault(f => f.Name == route.ExecuteMethod);
                        if (method != null)
                        {
                            context.Response.ContentType = route.ContentType;
                            object response = null;
                            if (method.ReturnType.BaseType == typeof(Task))
                            {
                                response = await (dynamic)method.Invoke(controller, null);
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
                                else if (method.ReturnType == typeof(byte[]))
                                {
                                    var byteResponse = response as byte[];
                                    await context.Response.Body.WriteAsync(byteResponse, 0, byteResponse.Length);
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
                        else
                        {
                            _logger.Error($"Method not found for route : {route.ToString()}");
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            await context.Response.WriteAsync(_responseBuilder.NotFound().ToString());
                        }
                    }
                    else
                    {

                        _logger.Error($"Type not found for route : {route.ToString()}");
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await context.Response.WriteAsync(_responseBuilder.NotFound().ToString());
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync(_responseBuilder.Unauthorized().ToString());
                }
            }
            else
            {
                try
                {

                    route = await _apiGatewayService.GetRouteAsync(_httpContextProxy.GetHttpMethod(), _httpContextProxy.GetURIAbsolutePath());
                    if (route != null)
                    {
                        try
                        {
                            _logger.Debug(string.Format("Executing remote route:{0}:{1}", route.Method, route.Route));
                            context.Response.ContentType = route.ContentType;
                            var response = await _apiGatewayService.CallAsync(_httpContextProxy.GetHttpMethod(), _httpContextProxy.GetURIAbsolutePath());
                            if (response != null)
                            {
                                if (response["content_type"] != null && response["data"] != null)
                                {
                                    await context.Response.WriteAsync(response["data"].ToString());
                                }
                                else
                                {
                                    await context.Response.WriteAsync(response.ToString());
                                }
                            }
                            else
                            {
                                await _next(context);
                            }
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            _logger.Error($"UnauthorizedAccessException remote route. {route.ToString() } . {ex.Message}", ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error Executing remote route. {route.ToString() } . {ex.Message}", ex);
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        
                    }
                    else
                    {
                        await _next(context);
                    }
                }
                
                catch (Exception ex)
                {
                    _logger.Error($"Error Executing remote route. {route.ToString() } . {ex.Message}", ex);
                    await _next(context);
                }

            }
        }

        private bool AuthorizedRoute(HttpContext context, RoutingModel route, IAuthorizationService authorizationService)
        {

            if (!route.auth_users.Where(f => f == CommonConst.CommonValue.ACCESS_ALL).Any())
            {
                try
                {
                    UserModel userModel = null;
                    var accessToken = _httpContextProxy.GetAccessTokenAync().GetAwaiter().GetResult();
                    var orgkey = string.Empty;

                    if (!string.IsNullOrEmpty(accessToken))
                    {

                        orgkey = _httpContextProxy.GetHeader(CommonConst.CommonValue.ORG_KEY);
                        if (string.IsNullOrEmpty(orgkey))
                        {
                            orgkey = _httpContextProxy.GetQueryString(CommonConst.CommonValue.ORG_KEY);
                        }
                        var cackeKey = $"{accessToken}_{orgkey}";

                        userModel = _inMemoryCacheService.Get<UserModel>(cackeKey);
                        if (userModel == null)
                        {
                            
                            var response = _apiGatewayService.CallAsync(CommonConst.ActionMethods.GET, "~/user/userinfo", "", null, new Dictionary<string, string>() { [CommonConst.CommonValue.ORG_KEY] = orgkey } , ApplicationConfig.AppEndpoint).GetAwaiter().GetResult();
                            if (response["user"] != null)
                            {
                                userModel = JsonConvert.DeserializeObject<UserModel>(response["user"].ToString());
                                if(userModel.orgs == null || userModel.orgs.Count ==0)
                                {
                                    _zNxtUserService.SetUserOrgs(userModel);
                                }
                                
                                _inMemoryCacheService.Put<UserModel>(cackeKey, userModel);
                            }
                        }
                    }
                    else
                    {
                        userModel = _httpContextProxy.User;
                    }
                    if (userModel != null)
                    {
                        var identity = new ClaimsIdentity();

                        _logger.Debug("userModel", JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(userModel)));

                        foreach (var claim in userModel.claims)
                        {
                            if (claim.Key == "roles")
                            {
                                var roles = new List<string>();
                                roles.AddRange(userModel.roles);
                                if (userModel.orgs != null)
                                {
                                    var org = userModel.orgs.FirstOrDefault(f => f.org_key == orgkey);
                                    if (org != null)
                                    {
                                        //roles.Add(org.Groups.)
                                    }
                                }
                                identity.AddClaim(new System.Security.Claims.Claim("roles", Newtonsoft.Json.JsonConvert.SerializeObject(roles)));
                            }
                            else
                            {
                                identity.AddClaim(new System.Security.Claims.Claim(claim.Key, claim.Value));
                            }
                        }


                        context.User = new ClaimsPrincipal(identity);
                        var u = _httpContextProxy.User;
                        _logger.Debug($"Assign user id :{u.user_id} Claims:{string.Join(", ", u.claims.Select(f => $"{f.Key}:{f.Value}"))} OrgRoles: { string.Join("," ,userModel.roles)}");

                        var hasaccess = false;
                        if (route.auth_users.IndexOf(CommonConst.CommonField.API_AUTH_TOKEN) != -1)
                        {
                            /// Check for API  to API auth 
                            var api_access_key = _httpContextProxy.GetHeader(CommonConst.CommonField.API_AUTH_TOKEN);
                            hasaccess = api_access_key == CommonUtility.GetApiAuthKey();
                        }
                        else
                        {
                            hasaccess = userModel.roles.Where(f => route.auth_users.IndexOf(f) != -1).Any();
                            if (!hasaccess)
                            {
                                _logger.Debug($"Access :{hasaccess}:{route.ToString()}:{  string.Join(",", route.auth_users)}");
                            }
                        }
                        return hasaccess;
                    }
                    return false;

                }
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
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
