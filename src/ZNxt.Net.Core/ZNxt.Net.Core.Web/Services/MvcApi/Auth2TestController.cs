using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZNxt.Net.Core.Web.Services.MvcApi
{
    [Route("auth2test")]
    [Authorize]
    public class Auth2TestController : Controller
    {
        [HttpGet]
        [Route("test")]
        public  IActionResult test()
        {
            var use = HttpContext.User;
            return new JsonResult(new { StatusCode = 1 });
        }

    }
}
