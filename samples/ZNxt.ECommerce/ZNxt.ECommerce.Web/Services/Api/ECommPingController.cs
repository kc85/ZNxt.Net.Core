using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.ECommerce.Web.Services.Api
{
    public class ECommPingController
    {

        private readonly IResponseBuilder _responseBuilder;
        public ECommPingController(IResponseBuilder responseBuilder)
        {

            _responseBuilder = responseBuilder;
        }
        [Route("/ecommping", CommonConst.ActionMethods.GET)]
        public JObject Ping()
        {

            return _responseBuilder.Success();
        }
    }
}
