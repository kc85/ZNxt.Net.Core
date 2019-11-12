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
    public class UserGroupController : IdentityControllerBase
    {
        public UserGroupController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService)
         : base(responseBuilder, logger, httpContextProxy,dBService,keyValueStorage,staticContentHandler,apiGatewayService)
        {
        }

        [Route("/sso/user/groups", CommonConst.ActionMethods.GET, "user")]
        public JObject UserGroups()
        {
            return GetPaggedData("user_groups", null, "{'override_by' : 'none'}", null, new List<string>() { "key", "name", "description", "module_name", "version", "is_default" });
        }
        [Route("/sso/user/addgroup", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject AddUserGroup()
        {
            return AddRemoveUserGroup(true);
        }
        [Route("/sso/user/removegroup", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject RemoveUserGroup()
        {
            return AddRemoveUserGroup(false);
        }
        private JObject AddRemoveUserGroup(bool isAdded)
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                if (request == null)
                {
                    return _responseBuilder.BadRequest();
                }
                if (request["user_id"] == null || request["group"] == null)
                {
                    JObject error = new JObject()
                    {
                        ["Error"] = "user_id and group required parameter"
                    };
                    return _responseBuilder.BadRequest(error);
                }
                var user_id = request["user_id"].ToString();
                var group = request["group"].ToString();
                var user = UserInfoByUserId(user_id);
                if (user != null)
                {
                    if (AddRemoveRole(isAdded, group, user))
                    {
                        return _responseBuilder.Success(user);
                    }
                    else
                    {
                        return _responseBuilder.ServerError(user);
                    }
                }
                else
                {
                    return _responseBuilder.NotFound();
                }
            }
            catch(FormatException ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.BadRequest();

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
           
        }

    }
}
