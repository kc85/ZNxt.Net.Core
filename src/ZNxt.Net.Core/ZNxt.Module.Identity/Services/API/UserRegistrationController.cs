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

                    if (_ZNxtUserService.GetUserByEmail( request.email) == null)
                    {
                        var userModel = new UserModel()
                        {
                            email = request.email,
                            user_id = ZNxt.Net.Core.Helpers.CommonUtility.GetNewID(),
                            name = request.name,
                            salt = ZNxt.Net.Core.Helpers.CommonUtility.GetNewID(),
                            is_enabled = true,
                            user_type = "user_pass",
                            id = ZNxt.Net.Core.Helpers.CommonUtility.GetNewID(),
                            roles = new List<string> { "user", "init_pass_set_required" }
                        };

                        _logger.Debug("Calling CreateUser");
                        var response = _ZNxtUserService.CreateUser( userModel, false );
                        if (response)
                        {
                            _userNotifierService.SendWelcomeEmailWithOTPLoginAsync(userModel).GetAwaiter().GetResult();
                            return _responseBuilder.Success();
                        }
                        else
                        {
                            return _responseBuilder.ServerError();
                        }
                    }
                    else
                    {
                        JObject errors = new JObject();
                        errors["error"] = $"Email id already registered {request.email}";
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
