﻿using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services.Api.Log
{

    public class LogController:ZNxt.Net.Core.Services.ApiBaseService
    {

        public LogController(IResponseBuilder responseBuilder,ILogger logger, IHttpContextProxy httpContextProxy,IDBService dBService,IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
            :base(httpContextProxy,dBService,logger,responseBuilder)
        {

        }
        [Route("/base/log", CommonConst.ActionMethods.GET, CommonConst.CommonValue.SYS_ADMIN, "application/javascript")]
        public JObject GetJS()
        {
            return GetPaggedData(CommonConst.Collection.SERVER_LOGS);
        }
        
    }
}