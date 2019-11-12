using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;


namespace ZNxt.Module.Identity.Services.API
{
    public class IdentityControllerBase : ZNxt.Net.Core.Services.ApiBaseService
    {
        protected readonly IResponseBuilder _responseBuilder;
        protected  readonly IHttpContextProxy _httpContextProxy;
        protected  readonly ILogger _logger;
        protected  readonly IDBService _dBService;
        protected  readonly IApiGatewayService _apiGatewayService;
        public IdentityControllerBase(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService)
         : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _dBService = dBService;
            _apiGatewayService = apiGatewayService;
        }
        protected JObject UserInfoByUserId(string user_id)
        {
            _logger.Debug($"Get User by User_id {user_id}");
            var filter = "{'user_id':'" + user_id + "'}";
            var user = _dBService.Get(CommonConst.Collection.USERS, new RawQuery(filter));
            if (user.Count != 0)
            {
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                var userResponse = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                return userResponse;
            }
            else
            {
                _logger.Debug($"User NOT FOUND by  {filter}, Collection {CommonConst.Collection.USERS}");
                return null;
            }
        }

        protected bool AddRemoveRole(bool isAdded, string group, JObject user)
        {
            if (!(user["roles"] as JArray).Where(f => f.ToString() == group).Any() && isAdded)
            {
                _logger.Debug($"Adding group {group}");
                (user["roles"] as JArray).Add(group);
            }
            else if ((user["roles"] as JArray).Where(f => f.ToString() == group).Any() && !isAdded)
            {
                _logger.Debug($"Removing group {group}");
                (user["roles"] as JArray).Remove((user["roles"] as JArray).FirstOrDefault(f => f.ToString() == group));
            }
            else
            {
                throw new FormatException($"Role not found  {group} , User :{user.ToString()}");
            }
            return _dBService.Write(CommonConst.Collection.USERS, user, "{'user_id' : '" + user["user_id"].ToString() + "'}", true, MergeArrayHandling.Replace);
            
        }
        protected bool UpdateUserProperty(JObject user, JObject updateProprty)
        {
            if (updateProprty != null)
            {
                foreach (var item in updateProprty)
                {
                    user[item.Key] = item.Value;
                }
            }
            return _dBService.Write(CommonConst.Collection.USERS, user, "{'user_id' : '" + user["user_id"].ToString() + "'}", true, MergeArrayHandling.Replace);

        }
    }
}
