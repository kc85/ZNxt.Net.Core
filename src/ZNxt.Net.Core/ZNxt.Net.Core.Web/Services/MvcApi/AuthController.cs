using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ZNxt.Net.Core.Web.Services.MvcApi
{
    [Route("auth")]
    public class AuthController : Controller
    {
        // GET: /<controller>/
        [Route("signin")]
        public IActionResult SignIn()
        {

            return Challenge(new AuthenticationProperties() { RedirectUri = "/" });
        }
        [Route("signout")]
        public async Task SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("oidc");
        }
    }
}
