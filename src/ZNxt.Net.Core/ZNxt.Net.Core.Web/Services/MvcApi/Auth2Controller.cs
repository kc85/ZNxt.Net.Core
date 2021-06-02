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
using ZNxt.Net.Core.Interfaces;


namespace ZNxt.Net.Core.Web.Services.MvcApi
{

    [Route("auth2")]
    public class Auth2Controller : Controller
    {
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        public Auth2Controller(IHttpContextProxy httpContextProxy, ILogger logger)
        {
            _httpContextProxy = httpContextProxy;
            _logger = logger;
        }
        // GET: /<controller>/
        [Route("signin")]
        public async Task<IActionResult> SignIn(string token)
        {

            // //
            //
            // grant_type=password&username=rett.bryon%40fineoak.org&password=rett.bryon%40fineoak.org&client_id=mobile_auth_client&client_secret=secret&scope=profile
            //
            var tokenrequest = new Dictionary<string,string>()
            {
                ["grant_type"] = "password",
                ["username"] = "kydon.len@fineoak.org",
                ["password"] = "abc@1234",
                ["client_id"] = "mobile_auth_client",
                ["client_secret"] = "secret",
                ["scope"] = "profile"
            };

            


            var client = new RestClient($"{ApplicationConfig.SSOEndpoint}/connect/token");
            client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");

            
            request.AddParameter("application/x-www-form-urlencoded", $"{ string.Join("&", tokenrequest.Select(f=> $"{f.Key}={f.Value}").ToList())}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Access Token cannot obtain, process terminate");
                return null;
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
                 new Claim("123", "333"),
                }
                
            };
            // issue authentication cookie with subject ID and username
            await HttpContext.SignInAsync(isuser, props);


            return Redirect("~/auth2test/test");
            //_logger.Debug($"SignIn token: {token}");
            //var redirectUri = $"/";
            //var authtokenProp = new AuthenticationProperties() { RedirectUri = redirectUri };
            //if (string.IsNullOrEmpty(token))
            //{
            //    token = "";
            //}
            //authtokenProp.Items.Add("app_token", token);
            //authtokenProp.Items.Add("login_ui_type", "app_token");
            //_logger.Debug($"auth2/signin: RedirectUri: {redirectUri}");
            //return Challenge(authtokenProp);
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
