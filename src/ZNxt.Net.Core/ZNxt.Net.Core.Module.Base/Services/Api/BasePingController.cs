using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.Base.Services.Api
{
    public class BasePingController
    {
        private readonly IResponseBuilder _responseBuilder;
        public BasePingController(IResponseBuilder responseBuilder)
        {

            _responseBuilder = responseBuilder;
        }
        [Route("/base/ping", CommonConst.ActionMethods.GET)]
        public JObject Ping()
        {

            return _responseBuilder.Success();
        }
    }
}
