using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using Microsoft.AspNetCore.Authentication;
namespace ZNxt.Identity.Services.Api
{
    public class SSOPingController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SSOPingController(IResponseBuilder responseBuilder, IHttpContextAccessor httpContextAccessor)
        {
            _responseBuilder = responseBuilder;
            _httpContextAccessor = httpContextAccessor;

        }
        [Route("/sso/ping", CommonConst.ActionMethods.GET)]
        public async Task<JObject> Ping()
        {


            var user = User;
            if (user != null)
            {
                return await Task.FromResult<JObject>(_responseBuilder.Success(Newtonsoft.Json.JsonConvert.SerializeObject(user)));
            }
            else
            {
                return await Task.FromResult<JObject>(_responseBuilder.Unauthorized());

            }

        }
        [Route("/sso/accesstoken", CommonConst.ActionMethods.GET)]
        public async Task<JObject> Accesstoken()
        {
            var accesstoke = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            return await Task.FromResult<JObject>(_responseBuilder.Success(accesstoke));
        }
        public UserModel User
        {
            get
            {

                if (_httpContextAccessor.HttpContext.User != null &&  _httpContextAccessor.HttpContext.User.Identity!=null &&_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                {
                    var user = new UserModel()
                    {
                        name = _httpContextAccessor.HttpContext.User.Identity.Name,

                    };
                    var claims = new List<Claim>();
                    foreach(var claim in _httpContextAccessor.HttpContext.User.Claims)
                    {
                        claims.Add( new Claim(claim.Type, claim.Value));
                    }
                    user.claims = claims;
                    user.id = user.user_id = user.claims.FirstOrDefault(f => f.Key == "sub").Value;
                    return user;
                }
                return null;
            }
        }

    }
}
