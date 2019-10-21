//using Microsoft.AspNetCore.Http;
//using Microsoft.IdentityModel.Protocols.OpenIdConnect;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using ZNxt.Net.Core.Consts;
//using ZNxt.Net.Core.Interfaces;
//using ZNxt.Net.Core.Model;
//using Microsoft.AspNetCore.Authentication;
//namespace ZNxt.Identity.Services.Api
//{
//    public class SSOPingController
//    {
//        private readonly IResponseBuilder _responseBuilder;
//        private readonly IHttpContextProxy _httpContextProxy;
//        public SSOPingController(IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy)
//        {
//            _responseBuilder = responseBuilder;
//            _httpContextProxy = httpContextProxy;

//        }
//        [Route("/sso/ping", CommonConst.ActionMethods.GET)]
//        public async Task<JObject> Ping()
//        {


//            var user = _httpContextProxy.User;
//            if (user != null)
//            {
//                return await Task.FromResult<JObject>(_responseBuilder.Success(Newtonsoft.Json.JsonConvert.SerializeObject(user)));
//            }
//            else
//            {
//                return await Task.FromResult<JObject>(_responseBuilder.Unauthorized());

//            }
//        }
//        [Route("/sso/auth/ping", CommonConst.ActionMethods.GET,"user")]
//        public async Task<JObject> AuthPing()
//        {


//            var user = _httpContextProxy.User;
//            if (user != null)
//            {
//                return await Task.FromResult<JObject>(_responseBuilder.Success(Newtonsoft.Json.JsonConvert.SerializeObject(user)));
//            }
//            else
//            {
//                return await Task.FromResult<JObject>(_responseBuilder.Unauthorized());

//            }
//        }

//    }
//}
