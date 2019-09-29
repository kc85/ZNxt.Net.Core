using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API
{
   public  class UsersController : ZNxt.Net.Core.Services.ApiBaseService
    {
        public UsersController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
         : base(httpContextProxy, dBService, logger, responseBuilder)
        {

        }

        [Route("/sso/users", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public  JObject Users()
        {
            return GetPaggedData(CommonConst.Collection.USERS);
        }
    }
}
