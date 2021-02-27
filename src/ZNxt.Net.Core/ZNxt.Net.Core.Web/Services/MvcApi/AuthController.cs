using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services.MvcApi
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        public AuthController(IHttpContextProxy httpContextProxy,ILogger logger)
        {
            _httpContextProxy = httpContextProxy;
            _logger = logger;
        }
        // GET: /<controller>/
        [Route("signin")]
        public IActionResult SignIn()
        {
            var redirectUri = $"/";

            var authtokenProp = new AuthenticationProperties() { RedirectUri = redirectUri };
           
            Console.WriteLine($"auth/signin: RedirectUri: {redirectUri}");
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
            catch(Exception ex)
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
