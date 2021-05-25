using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult SignIn(string token)
        {
            _logger.Debug($"SignIn token: {token}");
            var redirectUri = $"/";
            var authtokenProp = new AuthenticationProperties() { RedirectUri = redirectUri };
            if (string.IsNullOrEmpty(token))
            {
                token = "";
            }
            authtokenProp.Items.Add("app_token", token);
            authtokenProp.Items.Add("login_ui_type", "app_token");
            _logger.Debug($"auth2/signin: RedirectUri: {redirectUri}");
            return Challenge(authtokenProp);
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
