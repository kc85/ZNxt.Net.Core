using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

            return Challenge(new AuthenticationProperties() { RedirectUri = "/" });
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
