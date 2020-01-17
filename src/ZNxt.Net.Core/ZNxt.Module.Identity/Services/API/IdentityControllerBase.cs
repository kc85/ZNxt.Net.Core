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
        protected readonly IZNxtUserService _zNxtUserService;
        public IdentityControllerBase(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService, IZNxtUserService zNxtUserService)
         : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _dBService = dBService;
            _apiGatewayService = apiGatewayService;
            _zNxtUserService = zNxtUserService;
        }
        protected JObject UserInfoByUserId(string user_id)
        {
            
            _logger.Debug($"Get User by User_id {user_id}");
            var user = _zNxtUserService.GetUser(user_id);
            if (user!=null)
            {
                var userResponse = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(user));
                return userResponse;
            }
            else
            {
                _logger.Debug($"User NOT FOUND by  {user_id}");
                return null;
            }
        }

        protected bool AddRemoveRole(bool isAdded, string role, JObject user)
        {
            if (!(user["roles"] as JArray).Where(f => f.ToString() == role).Any() && isAdded)
            {
                _logger.Debug($"Adding role {role}");
                (user["roles"] as JArray).Add(role);
            }
            else if ((user["roles"] as JArray).Where(f => f.ToString() == role).Any() && !isAdded)
            {
                _logger.Debug($"Removing role {role}");
                (user["roles"] as JArray).Remove((user["roles"] as JArray).FirstOrDefault(f => f.ToString() == role));
            }
            else
            {  
                _logger.Debug($"{role} , User :{user.ToString()} isAdded: {isAdded}");
                return true;
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
