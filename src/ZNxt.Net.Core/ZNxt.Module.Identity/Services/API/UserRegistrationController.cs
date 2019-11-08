using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
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
        
        private readonly IServiceResolver _serviceResolver;

      
        public UserRegistrationController(IServiceResolver serviceResolver, IUserNotifierService userNotifierService, IHttpContextProxy httpContextProxy, IResponseBuilder responseBuilder, IDBService dBService, IKeyValueStorage keyValueStorage, ILogger logger)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _serviceResolver = serviceResolver;
            _userNotifierService = userNotifierService;
        }
        [Route("/sso/admin/adduser", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject AdminAddUser()
        {
            var request = _httpContextProxy.GetRequestBody<AdminAddUserModel>();
            _logger.Debug("Validation model");
            var results = new Dictionary<string, string>();
            if (request.IsValidModel(out results))
            {
                _logger.Debug("Getting user service");
                var userService = _serviceResolver.Resolve(Type.GetType("ZNxt.Identity.Services.IZNxtUserService", true, true));
                var type = userService.GetType();
                _logger.Debug("Calling GetUserByEmail");

                if (type.GetMethod("GetUserByEmail").Invoke(userService, new object[] { request.email }) == null)
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

                    var methodInfo = type.GetMethod("CreateUser");
                    _logger.Debug("Calling CreateUser");
                    var response = methodInfo.Invoke(userService, new object[] { userModel, false });
                    if ((bool)response)
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
    }
}
