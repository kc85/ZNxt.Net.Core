using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.Admin.Services.Api
{
    
    public class ApiDocController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private readonly IApiGatewayService _apiGatewayService;
        public ApiDocController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService,IApiGatewayService apiGatewayService)
            : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _apiGatewayService = apiGatewayService;
        }

        [Route("/admin/apis", CommonConst.ActionMethods.GET, CommonConst.CommonValue.SYS_ADMIN)]
        public JObject GetApis()
        {
            var data = _apiGatewayService.CallAsync(CommonConst.ActionMethods.GET, "/gateway/routes", "", null, null, ApplicationConfig.ApiGatewayEndpoint).GetAwaiter().GetResult();
            return data;
        }

    }
}
