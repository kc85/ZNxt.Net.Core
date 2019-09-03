using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Identity.Services.Api
{
    public class SSOPingController
    {
        private readonly IResponseBuilder _responseBuilder;
        public SSOPingController(IResponseBuilder responseBuilder)
        {
            _responseBuilder = responseBuilder;
        }
        [Route("/sso/ping", CommonConst.ActionMethods.GET)]
        public async Task<JObject> Ping()
        {
            return await Task.FromResult<JObject>(_responseBuilder.Success());
        }
    }
}
