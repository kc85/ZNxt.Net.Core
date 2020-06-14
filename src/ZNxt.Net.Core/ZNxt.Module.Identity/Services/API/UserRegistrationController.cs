using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ZNxt.Module.Identity.Services.API.Models;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Helpers;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Module.Identity.Services.API
{
    public class UserRegistrationController
    {
        private readonly IHttpContextProxy _httpContextProxy;

        private readonly IResponseBuilder _responseBuilder;

        private readonly IUserNotifierService _userNotifierService;
        private readonly ILogger _logger;

        private readonly IZNxtUserService _ZNxtUserService;

        protected readonly IApiGatewayService _apiGatewayService;

        private const bool MOBILE_AUTH_IGNORE_OTP_VALIDATION = true;
        public UserRegistrationController(IZNxtUserService ZNxtUserService, IUserNotifierService userNotifierService, IHttpContextProxy httpContextProxy, IResponseBuilder responseBuilder, IDBService dBService, IKeyValueStorage keyValueStorage, ILogger logger, IApiGatewayService apiGatewayService)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _ZNxtUserService = ZNxtUserService;
            _userNotifierService = userNotifierService;
            _apiGatewayService = apiGatewayService;
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
                            dob = request.dob,
                            id = CommonUtility.GetNewID(),
                            roles = new List<string> { "init_login_email_otp" }
                        };

                        _logger.Debug("Calling CreateUser");
                        var response = _ZNxtUserService.CreateUser(userModel, false);
                        if (response)
                        {
                            JObject userInfo = new JObject();
                            userInfo["mobile_number"] = request.mobile_number;
                            userInfo["whatsapp_mobile_number"] = request.whatsapp_mobile_number;
                            userInfo["gender"] = request.gender;
                            if (_ZNxtUserService.UpdateUserProfile(userModel.user_id, userInfo))
                            {
                                _userNotifierService.SendWelcomeEmailWithOTPLoginAsync(userModel).GetAwaiter().GetResult();
                                var resonseData = new JObject()
                                {
                                    [CommonConst.CommonField.USER_ID] = userModel.user_id
                                };
                                return _responseBuilder.Success(resonseData);
                            }
                            else
                            {
                                _logger.Error($"Error while updating user profile {userModel.user_id}", null, userInfo);
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
        [Route("/sso/admin/userrelation/add", CommonConst.ActionMethods.POST, "sys_admin")]
        public JObject AddUserRelation()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                var parentkey = $"parent_" + CommonField.USER_ID;
                var childkey = $"child_" + CommonField.USER_ID;
                var relationkey = $"relation";
                if (request != null && request[parentkey] != null && request[childkey] != null && request[relationkey] != null)
                {
                    if (_ZNxtUserService.AddUserRelation(request[parentkey].ToString(), request[childkey].ToString(), request[relationkey].ToString()))
                    {
                        return _responseBuilder.Success();
                    }
                    else
                    {
                        _logger.Error("Error While adding the relationship");
                        return _responseBuilder.ServerError();
                    }
                }
                else
                {
                    _logger.Error("Requied parameter missing ", null, request);
                    return _responseBuilder.BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error {ex.Message}", ex);
                return _responseBuilder.ServerError();
            }

        }

        [Route("/sso/mobile_auth/register", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject RegisterMobile()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<MobileAuthRegisterRequest>();
                request.device_address = _httpContextProxy.GetHeader("device_address");
                request.app_version = _httpContextProxy.GetHeader("app_version");
                request.x_auth_token = _httpContextProxy.GetHeader("x_auth_token");

                var results = new Dictionary<string, string>();
                if (request.IsValidModel(out results))
                {

                    // TO DO : Send mobile validation OTP 
                    MobileAuthRegisterResponse mobileAuthRegisterResponse =  _ZNxtUserService.RegisterMobile(request);

                    if (mobileAuthRegisterResponse.code == CommonConst._1_SUCCESS)
                    {
                        if (_userNotifierService.SendMobileAuthRegistrationOTPAsync(mobileAuthRegisterResponse).GetAwaiter().GetResult() || MOBILE_AUTH_IGNORE_OTP_VALIDATION)
                        {
                            return _responseBuilder.Success(null, mobileAuthRegisterResponse.ToJObject());
                        }
                        else
                        {
                            return _responseBuilder.ServerError("Error sending SMS OTP");
                        }
                    }
                    else
                    {
                        return _responseBuilder.CreateReponseWithError(mobileAuthRegisterResponse.code, mobileAuthRegisterResponse.errors);
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
        [Route("/sso/mobile_auth/activate", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject RegisterActivate()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<MobileAuthActivateRequest>();
                request.device_address = _httpContextProxy.GetHeader("device_address");
                request.app_version = _httpContextProxy.GetHeader("app_version");
                request.x_auth_token = _httpContextProxy.GetHeader("x_auth_token");

                var results = new Dictionary<string, string>();
                if (request.IsValidModel(out results))
                {
                    var validateRequest = new JObject()
                    {
                        ["To"] = request.mobile_number,
                        ["OTP"] = request.OTP,
                        ["OTPType"] = "mobile_auth_activation",
                        ["SecurityToken"] = request.validation_token
                    };

                    var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/validate", null, validateRequest).GetAwaiter().GetResult();
                    if (result["code"].ToString() == "1" || MOBILE_AUTH_IGNORE_OTP_VALIDATION)
                    {
                        MobileAuthActivateResponse mobileAuthActivateResponse = _ZNxtUserService.ActivateRegisterMobile(request);

                        if (mobileAuthActivateResponse.code == CommonConst._1_SUCCESS)
                        {
                            JObject userInfo = new JObject();
                            userInfo["mobile_number"] = "";
                            userInfo["whatsapp_mobile_number"] = "";
                            userInfo["gender"] = "";
                            if (_ZNxtUserService.UpdateUserProfile(mobileAuthActivateResponse.user_id, userInfo))
                            {
                                var obj = mobileAuthActivateResponse.ToJObject();
                                obj.Remove("user_id");
                                return _responseBuilder.Success(null, obj);
                            }
                            else
                            {
                                return _responseBuilder.CreateReponseWithError(mobileAuthActivateResponse.code, mobileAuthActivateResponse.errors);
                            }
                                
                        }
                        else
                        {
                            return _responseBuilder.CreateReponseWithError(mobileAuthActivateResponse.code, mobileAuthActivateResponse.errors);
                        }
                    }
                    else
                    {
                        return _responseBuilder.Unauthorized();
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
