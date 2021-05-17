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
using Microsoft.Extensions.Primitives;
using ZNxt.Net.Core.Web.Services.SSO;
using IdentityServer4.Models;
using IdentityModel;

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
        private readonly IOAuthClientService _oAuthClientService;
        public ApiHandler(RequestDelegate next, ILogger logger, IDBService dbService, IRouting routing,
            IHttpContextProxy httpContextProxy, IAssemblyLoader assemblyLoader, IServiceResolver serviceResolver, IResponseBuilder responseBuilder,
            IApiGatewayService apiGatewayService, IInMemoryCacheService inMemoryCacheService, IOAuthClientService oAuthClientService)
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
            _oAuthClientService = oAuthClientService;

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
                        context.Response.Headers[CommonConst.CommonField.MODULE_NAME] = route.module_name;
                        context.Response.Headers[CommonConst.CommonField.EXECUTE_TYPE] = route.ExecuteType;
                        context.Response.Headers[CommonConst.CommonField.ROUTE] = $"{route.Method}:{route.Route}";

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
                            RemoveHeaders(context);
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
                            RemoveHeaders(context);
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
                            var headers = new Dictionary<string, string>();
                           // headers[CommonConst.CommonField.API_AUTH_TOKEN] = "route-call";
                            var response = await _apiGatewayService.CallAsync(_httpContextProxy.GetHttpMethod(), _httpContextProxy.GetURIAbsolutePath(), _httpContextProxy.GetQueryString(), _httpContextProxy.GetRequestBody<JObject>(), headers);
                            if (response != null)
                            {
                                RemoveHeaders(context);
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

        private static void RemoveHeaders(HttpContext context)
        {
            if (context.Response.Headers.ContainsKey(CommonConst.CommonField.MODULE_NAME))
            {
                context.Response.Headers.Remove(CommonConst.CommonField.MODULE_NAME);
            }
            if (context.Response.Headers.ContainsKey(CommonConst.CommonField.EXECUTE_TYPE))
            {
                context.Response.Headers.Remove(CommonConst.CommonField.EXECUTE_TYPE);
            }
            if (context.Response.Headers.ContainsKey(CommonConst.CommonField.ROUTE))
            {
                context.Response.Headers.Remove(CommonConst.CommonField.ROUTE);
            }
            if (context.Response.Headers.ContainsKey(CommonConst.CommonField.CREATED_DATA_DATE_TIME))
            {
                context.Response.Headers.Remove(CommonConst.CommonField.CREATED_DATA_DATE_TIME);
            }
            if (context.Response.Headers.ContainsKey(CommonConst.CommonField.TENANT_ID))
            {
                context.Response.Headers.Remove(CommonConst.CommonField.TENANT_ID);
            }
        }

        private bool AuthorizedRoute(HttpContext context, RoutingModel route, IAuthorizationService authorizationService)
        {
            var ssourl = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.SSOURL_CONFIG_KEY);
            
            if (!route.auth_users.Where(f => f == CommonConst.CommonValue.ACCESS_ALL).Any() && !string.IsNullOrEmpty(ssourl))
            {
                try
                {

                    if (route.auth_users.IndexOf(CommonConst.CommonField.API_AUTH_TOKEN) != -1)
                    {
                        var api_access_key = _httpContextProxy.GetHeader(CommonConst.CommonField.API_AUTH_TOKEN);
                        return api_access_key == CommonUtility.GetApiAuthKey();
                    }
                    // check for auth client 

                    var oauthclient = context.Request.Headers[CommonConst.CommonField.OAUTH_CLIENT_ID];
                    if (!string.IsNullOrEmpty(oauthclient))
                    {
                        var oauthUser =  ValidateOAuthRequest(oauthclient, context, route);
                        return oauthUser != null;
                    }

                    UserModel userModel = null;
                    var accessToken = _httpContextProxy.GetAccessTokenAync().GetAwaiter().GetResult();
                    var orgkey = string.Empty;

                    if (!string.IsNullOrEmpty(accessToken))
                    {

                        orgkey = _httpContextProxy.GetHeader(CommonConst.CommonValue.TENANT_KEY);
                        if (string.IsNullOrEmpty(orgkey))
                        {
                            orgkey = _httpContextProxy.GetQueryString(CommonConst.CommonValue.TENANT_KEY);
                        }
                        var cackeKey = $"{accessToken}_{orgkey}";

                        userModel = _inMemoryCacheService.Get<UserModel>(cackeKey);
                        if (userModel == null)
                        {

                            var response = _apiGatewayService.CallAsync(CommonConst.ActionMethods.GET, "~/user/getuserinfo", "", null, new Dictionary<string, string>() { [CommonConst.CommonValue.TENANT_KEY] = orgkey }, ApplicationConfig.AppEndpoint).GetAwaiter().GetResult();
                            if (response["user"] != null)
                            {
                                userModel = JsonConvert.DeserializeObject<UserModel>(response["user"].ToString());
                                //if(userModel.orgs == null || userModel.orgs.Count ==0)
                                //{
                                //   // _serviceResolver.Resolve<IZNxtUserService>().SetUserOrgs(userModel);
                                //}

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
                                if (userModel.tenants != null)
                                {
                                    //var org = userModel.tenants.FirstOrDefault(f => f.org_key == orgkey);
                                    //if (org != null)
                                    //{
                                    //    //roles.Add(org.Groups.)
                                    //}
                                }
                                identity.AddClaim(new System.Security.Claims.Claim("roles", Newtonsoft.Json.JsonConvert.SerializeObject(roles)));
                            }
                            else
                            {
                                identity.AddClaim(new System.Security.Claims.Claim(claim.Key, claim.Value));
                            }
                        }

                        if(userModel.tenants!=null && userModel.tenants.Any())
                        {
                            context.Response.Headers[CommonConst.CommonField.TENANT_ID] = userModel.tenants.First().tenant_id;
                        }
                        context.User = new ClaimsPrincipal(identity);
                        var u = _httpContextProxy.User;
                        _logger.Debug($"Assign user id :{u.user_id} Claims:{string.Join(", ", u.claims.Select(f => $"{f.Key}:{f.Value}"))} OrgRoles: { string.Join(",", userModel.roles)}");

                        var hasaccess = false;

                        hasaccess = userModel.roles.Where(f => route.auth_users.IndexOf(f) != -1).Any();
                        if (!hasaccess)
                        {
                            _logger.Debug($"Access :{hasaccess}:{route.ToString()}:{  string.Join(",", route.auth_users)}");
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

        private UserModel ValidateOAuthRequest(StringValues oauthclientid, HttpContext context, RoutingModel route)
        {
            var secrect  = context.Request.Headers[CommonConst.CommonField.OAUTH_CLIENT_SECRET];
            var oauthclient = _oAuthClientService.GetClient(oauthclientid);
            if (oauthclient != null)
            {
                var client = oauthclient.Client;
                if (oauthclient.Secret == $"{secrect.ToString()}{oauthclient.Salt}".Sha256())
                { 
                    if (oauthclient.Roles.Where(f => route.auth_users.IndexOf(f) != -1).Any())
                    {
                        var user = new UserModel()
                        {
                            first_name = client.ClientName,
                            user_type = "oauth",
                        };
                        var roles = Newtonsoft.Json.JsonConvert.SerializeObject(oauthclient.Roles);
                        if (!string.IsNullOrEmpty(oauthclient.TenantId))
                        {
                            user.tenants.Add(new TenantModel()
                            {
                                tenant_id = oauthclient.TenantId,
                                tenant_key = oauthclient.TenantId
                            });
                        }
                        user.claims = new List<Model.Claim>() {
                             new Model.Claim("roles", roles),
                             new Model.Claim(JwtClaimTypes.Subject, oauthclient.TenantId),
                             new Model.Claim("user_type","oauth"),
                             new Model.Claim("first_name",client.ClientName)
                        };
                        var identity = new ClaimsIdentity();
                        identity.AddClaim(new System.Security.Claims.Claim("roles", roles));
                        context.User = new ClaimsPrincipal(identity);
                        if (!string.IsNullOrEmpty(oauthclient.TenantId))
                        {
                            context.Response.Headers[CommonConst.CommonField.TENANT_ID] = oauthclient.TenantId;
                        }
                        return user;
                    }
                }
            }

            return null;
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
