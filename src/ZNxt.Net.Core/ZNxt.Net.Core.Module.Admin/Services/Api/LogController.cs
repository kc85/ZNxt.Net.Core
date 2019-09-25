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
   
    public class LogController:ZNxt.Net.Core.Services.ApiBaseService
    {

        public LogController(IResponseBuilder responseBuilder,ILogger logger, IHttpContextProxy httpContextProxy,IDBService dBService,IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
            :base(httpContextProxy,dBService,logger,responseBuilder)
        {
        }

        [Route("/admin/log", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "application/javascript")]
        public JObject GetJS()
        {
            return GetPaggedData(CommonConst.Collection.SERVER_LOGS);
        }
        
    }
}
