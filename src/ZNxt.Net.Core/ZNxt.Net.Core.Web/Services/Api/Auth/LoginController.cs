using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Helper;
using ZNxt.Net.Core.Web.Services.Api.Auth.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ZNxt.Net.Core.Web.Services.Api.Auth
{
    [Authorize]
    public class LoginController 
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;

        public LoginController(IServiceResolver serviceResolver, ILogger logger, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _serviceResolver = serviceResolver;
            _logger = logger;
        }
        [Route("/user/login", CommonConst.ActionMethods.POST)]
        public async Task<JObject> Login()
        {
            try
            {
                JObject response = null;
                var data = _httpContextProxy.GetRequestBody<UserLoginModel>();
                if (data == null)
                {
                    response = _responseBuilder.BadRequest();
                }
                var userAccontHelper = _serviceResolver.Resolve<UserAccontHelper>();
                var user = userAccontHelper.GetUser(data.UserName);
                if (user == null)
                {
                    response = _responseBuilder.Unauthorized();
                }
                if (userAccontHelper.ValidateUser(user, data.Password))
                {
                    ClaimsIdentity identity = new ClaimsIdentity(this.GetUserClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);

                    await _serviceResolver.Resolve<IHttpContextAccessor>().HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    response = _responseBuilder.Success();
                }
                else
                {
                    response = _responseBuilder.Unauthorized();
                }
                return await Task.FromResult<JObject>(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return await Task.FromResult<JObject>(_responseBuilder.ServerError());
            }
        }
        private IEnumerable<System.Security.Claims.Claim> GetUserClaims(UserModel user)
        {
            List<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.user_id),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.first_name),
                new System.Security.Claims.Claim(ClaimTypes.Email, user.email)
            };
            // claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }
        [Route("/user/signout", CommonConst.ActionMethods.POST)]
        public async Task<JObject> Signout()
        {
            await _serviceResolver.Resolve<IHttpContextAccessor>().HttpContext.SignOutAsync();
            var response = _responseBuilder.Success();
            return await Task.FromResult(response);
        }
    }
}
