using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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
        [Route("/base/log", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject GetJS()
        {
            JArray joinData = new JArray();
            JObject collectionJoin = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USERS, CommonConst.CommonField.USER_ID, new List<string> { "user_id", "user_name", "first_name", "middle_name", "last_name", "user_type", "email", "dob" }, "user");
            JObject collectionJoinUserInfo = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USER_INFO, CommonConst.CommonField.USER_ID, new List<string> { "user_id", "mobile_number", "gender", "whatsapp_mobile_number" }, "user_info");
            joinData.Add(collectionJoin);
             joinData.Add(collectionJoinUserInfo);

            return GetPaggedData("s2f_org_users", joinData, null, null, new List<string> { "org_key", "groups", "user_id" });

            return GetPaggedData(CommonConst.Collection.SERVER_LOGS);
        }

        
    }
}
