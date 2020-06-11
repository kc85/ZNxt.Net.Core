using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Module.Identity.Services.API
{
    public class UserRoleController : IdentityControllerBase
    {

        public UserRoleController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService, IZNxtUserService zNxtUserService)
         : base(responseBuilder, logger, httpContextProxy,dBService,keyValueStorage,staticContentHandler,apiGatewayService, zNxtUserService)
        {
        }

        [Route("/sso/user/roles", ActionMethods.GET, "user")]
        public JObject UserRoles()
        {
            return GetPaggedData(Collection.USER_ROLES, null, "{'override_by' : 'none'}", null, new List<string>() { "key", "name", "description", "module_name", "version", "is_default" });
        }
        [Route("/sso/user/remove_my_init_roles", ActionMethods.POST, CommonField.USER_ROLE)]
        public JObject RemoteUserInitRole()
        {
            JObject request = new JObject()
            {
                ["user_id"] = _httpContextProxy.User.user_id,
                ["role"] = "mobile_auth_init_user"
            };
            return AddRemoveUserRole(false, request);
        }
        [Route("/sso/user/apiaddrole", ActionMethods.POST, CommonField.API_AUTH_TOKEN)]
        public JObject AddUserRoleFromAPI()
        {
            return AddRemoveUserRole(true);
        }
        [Route("/sso/user/apiremoverole", ActionMethods.POST, CommonField.API_AUTH_TOKEN)]
        public JObject RemoveUserRoleFromAPI()
        {
            return AddRemoveUserRole(false);
        }
        [Route("/sso/user/addrole", ActionMethods.POST, "sys_admin")]
        public JObject AddUserRole()
        {
            return AddRemoveUserRole(true);
        }
        [Route("/sso/user/removerole", ActionMethods.POST, "sys_admin")]
        public JObject RemoveUserRole()
        {
            return AddRemoveUserRole(false);
        }
        private JObject AddRemoveUserRole(bool isAdded)
        {
            var request = _httpContextProxy.GetRequestBody<JObject>();
            return AddRemoveUserRole(isAdded, request);

        }
        private JObject AddRemoveUserRole(bool isAdded, JObject request)
        {
            try
            {
                if (request == null)
                {
                    return _responseBuilder.BadRequest();
                }
                if (request["user_id"] == null || request["role"] == null)
                {
                    JObject error = new JObject()
                    {
                        ["Error"] = "user_id and role required parameter"
                    };
                    return _responseBuilder.BadRequest(error);
                }
                var user_id = request["user_id"].ToString();
                var role = request["role"].ToString();
                var user = UserInfoByUserId(user_id);
                if (user != null)
                {
                    if (AddRemoveRole(isAdded, role, user))
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
