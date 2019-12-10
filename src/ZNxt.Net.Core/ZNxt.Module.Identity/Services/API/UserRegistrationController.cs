using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ZNxt.Module.Identity.Services.API.Models;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Helpers;

namespace ZNxt.Module.Identity.Services.API
{
    public  class UserRegistrationController
    {
        private readonly IHttpContextProxy _httpContextProxy;

        private readonly IResponseBuilder _responseBuilder;

        private readonly IUserNotifierService _userNotifierService;
        private readonly ILogger _logger;
        
        private readonly IZNxtUserService _ZNxtUserService;

      
        public UserRegistrationController(IZNxtUserService ZNxtUserService, IUserNotifierService userNotifierService, IHttpContextProxy httpContextProxy, IResponseBuilder responseBuilder, IDBService dBService, IKeyValueStorage keyValueStorage, ILogger logger)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _ZNxtUserService = ZNxtUserService;
            _userNotifierService = userNotifierService;
        }
        [Route("/sso/admin/adduser", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject AdminAddUser()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<AdminAddUserModel>();
                _logger.Debug("Validation model");
                var results = new Dictionary<string, string>();
                if (request.IsValidModel(out results))
                {
                    _logger.Debug("Getting user service");
                   
                    _logger.Debug("Calling GetUserByEmail");

                    if (_ZNxtUserService.GetUserByUsername(request.user_name) == null)
                    {
                        var userModel = new UserModel()
                        {
                            email = request.email,
                            user_id = CommonUtility.GetNewID(),
                            user_name = request.user_name,
                            first_name = request.first_name,
                            middle_name = request.middle_name,
                            last_name = request.last_name,
                            salt = CommonUtility.GetNewID(),
                            is_enabled = true,
                            user_type = "user_pass",
                            dob = new DOBModel() { },
                            id = CommonUtility.GetNewID(),
                            roles = new List<string> { "init_login_email_otp" }
                        };

                        _logger.Debug("Calling CreateUser");
                        var response = _ZNxtUserService.CreateUser( userModel, false );
                        if (response)
                        {
                            JObject userInfo = new JObject();
                            userInfo["mobile_number"] = request.mobile_number;
                            userInfo["whatsapp_mobile_number"] = request.whatsapp_mobile_number;
                            userInfo["gender"] = request.gender;
                            if (_ZNxtUserService.UpdateUserProfile(userModel.user_id, userInfo))
                            {
                                _userNotifierService.SendWelcomeEmailWithOTPLoginAsync(userModel).GetAwaiter().GetResult();
                                return _responseBuilder.Success();
                            }
                            else
                            {
                                _logger.Error($"Error while updating user profile {userModel.user_id}", null,userInfo);
                                return _responseBuilder.ServerError();
                            }
                        }
                        else
                        {
                            return _responseBuilder.ServerError();
                        }
                    }
                    else
                    {
                        JObject errors = new JObject();
                        errors["error"] = $"User id already registered : {request.user_name}";
                        return _responseBuilder.BadRequest(errors);
                    }
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
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
    }
}
