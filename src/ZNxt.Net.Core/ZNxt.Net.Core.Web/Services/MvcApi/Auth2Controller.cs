using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;


namespace ZNxt.Net.Core.Web.Services.MvcApi
{

    [Route("auth2")]
    public class Auth2Controller : Controller
    {
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IApiGatewayService _apiGatewayService;
        private const string appAuthValidateRoute = "/sso/authapptoken";
        public Auth2Controller(IHttpContextProxy httpContextProxy, ILogger logger,IApiGatewayService apiGatewayService)
        {
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _apiGatewayService = apiGatewayService ;
        }
        // GET: /<controller>/
        [Route("signin")]
        public async Task<IActionResult> SignIn(string token, string redirecturl="~/")
        {
            _logger.Debug($"Auth2Controller.SignIn calling {appAuthValidateRoute}");
            var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, appAuthValidateRoute, null, new JObject() { ["token"] = token }).GetAwaiter().GetResult();
            var appSecret = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.APP_SECRET_CONFIG_KEY);
            var unauthorizedPage = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.APP_TOKEN_UNAUTHORIZED_PAGE);
            if(string.IsNullOrEmpty(unauthorizedPage))
            {
                unauthorizedPage = "~/unauthorized.html";
            }
            if (result["code"].ToString() == CommonConst._1_SUCCESS.ToString())
            {
                var tokenrequest = new Dictionary<string, string>()
                {
                    ["grant_type"] = "password",
                    ["username"] = result["data"]["user_name"].ToString(),
                    ["password"] = "pass",
                    ["client_id"] = "auth_client",
                    ["client_secret"] = appSecret,
                    ["scope"] = "profile"
                };
                var conneturl = $"{ApplicationConfig.SSOEndpoint}/connect/token";
                var client = new RestClient(conneturl);
                client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("token", token);
                request.AddParameter("application/x-www-form-urlencoded", $"{ string.Join("&", tokenrequest.Select(f => $"{f.Key}={f.Value}").ToList())}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Error($"Auth2Controller.SignIn connect url fail: {conneturl},:  {response.StatusCode}");
                    return Redirect(unauthorizedPage);
                }

                var tokenResponse = JObject.Parse(response.Content);

                AuthenticationProperties props = null;

                props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromHours(1))
                };
                var isuser = new IdentityServerUser("auth2")
                {
                    DisplayName = "auth2",
                    AdditionalClaims = new List<Claim>() {
                    new Claim("access_token", tokenResponse["access_token"].ToString()),
                }

                };
                
                await HttpContext.SignInAsync(isuser, props);
                return Redirect(redirecturl);
            }
            else
            {
                _logger.Error($"Auth2Controller.SignIn token validation fail: {token}");
                return Redirect(unauthorizedPage);
            }
        }
        [Route("signout")]
        public async Task SignOut()
        {
            try
            {
                await HttpContext.SignOutAsync();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync("oidc");
            }
            catch (Exception ex)
            {
                _logger.Error($"SignOut {ex.Message}", ex);
            }
        }
        [Route("accesstoken")]
        public async Task<string> AccessToken()
        {
            var token = await _httpContextProxy.GetAccessTokenAync();
            return token;
        }
    }
}
