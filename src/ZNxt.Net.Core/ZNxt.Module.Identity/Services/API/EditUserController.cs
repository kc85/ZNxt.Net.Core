using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Module.Identity.Services.API.Models;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API
{
    public class EditUserController : IdentityControllerBase
    {
        public EditUserController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService, IZNxtUserService zNxtUserService)
         : base(responseBuilder, logger, httpContextProxy, dBService, keyValueStorage, staticContentHandler, apiGatewayService, zNxtUserService)
        {
        }

        [Route("/sso/admin/userinfo/edit", CommonConst.ActionMethods.POST, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject EditUserInfo()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                if (request["user_id"] != null)
                {



                    var userdata = JsonConvert.DeserializeObject<AdminAddUserModel>(request.ToString());
                    var user = _zNxtUserService.GetUser(request["user_id"].ToString());
                    if (user != null)
                    {
                        userdata.user_name = user.user_name;
                        var results = new Dictionary<string, string>();
                        if (request.IsValidModel(out results))
                        {
                            if (EditUser(user.user_id, userdata))
                            {
                                foreach (var item in userdata.ToJObject())
                                {
                                    if (request[item.Key] != null)
                                    {
                                        request.Remove(item.Key);
                                    }
                                }
                                if (_zNxtUserService.UpdateUserProfile(user.user_id, request))
                                {
                                    return _responseBuilder.Success();
                                }
                            }
                            return _responseBuilder.ServerError();
                        }
                        else
                        {
                            _logger.Debug("Model validation fail");
                            JObject errors = new JObject();
                            foreach (var error in results)
                            {
                                errors[error.Key] = error.Value;
                            }
                            return _responseBuilder.BadRequest(errors);
                        }
                    }
                    else
                    {
                        _logger.Error($"User not found for user id {request["user_id"]}");
                        return _responseBuilder.NotFound();
                    }
                }
                else
                {
                    return _responseBuilder.BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"EditUserInfo {ex.Message}", ex);
                return _responseBuilder.ServerError();
            }
        }
        public bool EditUser(string user_id, AdminAddUserModel request)
        {
            try
            {

                return _zNxtUserService.UpdateUser(user_id, request.ToJObject());

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }


    }
}
