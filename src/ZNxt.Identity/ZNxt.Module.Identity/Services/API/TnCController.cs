using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Interfaces;
using Newtonsoft.Json.Linq;

namespace ZNxt.Module.Identity.Services.API
{
    public class TnCController
    {
        private readonly IHttpContextProxy _httpContextProxy;

        private readonly IResponseBuilder _responseBuilder;
        public TnCController(IHttpContextProxy httpContextProxy, IResponseBuilder responseBuilder)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
        }
        [Route("/ssp/tnc", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "text/html")]
        public string GetTnC()
        {
            return "<h1> ZNXT TnC </h1>";
        }

        [Route("/ssp/accepttnc", CommonConst.ActionMethods.POST, "user,init_user")]
        public JObject AcceptTnC()
        {
            var user = _httpContextProxy.User;
            if (user != null)
            {
                return _responseBuilder.Success();
            }
            else
            {
                return _responseBuilder.Unauthorized();
            }
        }
    }
}
