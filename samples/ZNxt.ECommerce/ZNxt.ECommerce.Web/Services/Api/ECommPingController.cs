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
        private readonly IViewEngine _viewEngine;
        public ECommPingController(IResponseBuilder responseBuilder, IViewEngine viewEngine)
        {

            _responseBuilder = responseBuilder;
            _viewEngine = viewEngine;
        }
        [Route("/ecommping", CommonConst.ActionMethods.GET)]
        public JObject Ping()
        {
            var data = _viewEngine.Compile($"Hello-- { DateTime.Now.ToString()} @Raw((1 + 1).ToString())", "aa", null);
            var dataResponse = new JObject();
            dataResponse["data"] = data;
            return _responseBuilder.Success(dataResponse);
        }
    }
}
