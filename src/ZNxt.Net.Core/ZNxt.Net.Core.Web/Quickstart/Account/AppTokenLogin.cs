using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZNxt.Net.Core.Web.Quickstart.Account
{
    [SecurityHeaders]
    [AllowAnonymous]

    public class AppTokenLogin : Controller
    {

        [HttpGet]
        public async Task<IActionResult> Login(LoginViewModel mv)
        {
            return  await Task.FromResult(View(mv));
        }
    }
}
