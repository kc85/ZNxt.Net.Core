using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Helpers;
using System.Linq;

namespace ZNxt.Net.Core.Module.Admin.Services.Api
{
   
    public class GetJavascriptController
    {
        private readonly IResponseBuilder _responseBuilder;
        public GetJavascriptController(IResponseBuilder responseBuilder)
        {

            _responseBuilder = responseBuilder;
        }
        [Route("/admin/js", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "application/javascript")]
        public string GetJS()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("alert('Hello from get js'");
            return sb.ToString();
        }
    }
}
