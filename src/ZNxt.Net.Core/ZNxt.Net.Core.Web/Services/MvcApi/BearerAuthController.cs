using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services.MvcApi
{
    [Route("user")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BearerAuthController :  Controller
    {
        private readonly IHttpContextProxy _httpContextProxy;

        public BearerAuthController(IHttpContextProxy httpContextProxy)
        {
            _httpContextProxy = httpContextProxy;
        }
        [HttpGet]
        [Route("getuserinfo")]
        public IActionResult GetUserInfo()
        {
            return Ok(new
            {
                User = _httpContextProxy.User
            });
        }

    }
}
