using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
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
            string queryfields = null;// _httpContextProxy.GetQueryString("fields");
            List<string> fields = null;
            if (null != queryfields)
            {
                fields = queryfields.Split(',').ToList<string>();
            }
            JArray joinData = new JArray();
            JObject ClasscollectionJoin = GetCollectionJoin("class_id", "s2f_class_master", "id", new List<string> { "name", "key" }, "class");
            JObject AcadClsSyllcollectionJoin = GetCollectionJoin("acadsyll_id", "s2f_acadsyllabus_master", "id", new List<string> { "key" }, "acadsyll");
            joinData.Add(ClasscollectionJoin);
            joinData.Add(AcadClsSyllcollectionJoin);
            return GetPaggedData("s2f_acadclasssyll_master", joinData, null, null, fields);

            //JArray joinData = new JArray();
            //JObject collectionJoin = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USERS, CommonConst.CommonField.USER_ID, new List<string> { "user_id", "user_name", "first_name", "middle_name", "last_name", "user_type", "email", "dob" }, "user");
            //JObject collectionJoinUserInfo = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USER_INFO, CommonConst.CommonField.USER_ID, new List<string> { "user_id", "mobile_number", "gender", "whatsapp_mobile_number" }, "user_info");
            //joinData.Add(collectionJoin);
            // joinData.Add(collectionJoinUserInfo);

            //return GetPaggedData("s2f_org_users", joinData, null, null, new List<string> { "org_key", "groups", "user_id" });

            //return GetPaggedData(CommonConst.Collection.SERVER_LOGS);
        }

        
    }
}
