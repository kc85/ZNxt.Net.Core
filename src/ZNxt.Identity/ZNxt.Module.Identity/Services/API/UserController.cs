using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API
{
   public  class UserController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        public UserController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
         : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
        }

        [Route("/sso/users", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public  JObject Users()
        {
            return GetPaggedData(CommonConst.Collection.USERS);
        }
        [Route("/sso/userinfo", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject UserInfo()
        {
            var user_id = _httpContextProxy.GetQueryString(CommonConst.CommonField.USER_ID);
            JArray joinData = new JArray();
            JObject collectionJoin = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USER_INFO, CommonConst.CommonField.USER_ID, null, CommonConst.CommonField.USER_INFO);
            joinData.Add(collectionJoin);
            JObject filter = new JObject();
            filter[CommonConst.CommonField.USER_ID] = user_id;
            var data = GetPaggedData(CommonConst.Collection.USERS, joinData, filter.ToString());
            if ((data[CommonConst.CommonField.DATA] as JArray).Count != 0)
            {
                return _responseBuilder.Success(data[CommonConst.CommonField.DATA][0] as JObject);
            }
            else
            {
                return _responseBuilder.NotFound();

            }
        }
    }
}
