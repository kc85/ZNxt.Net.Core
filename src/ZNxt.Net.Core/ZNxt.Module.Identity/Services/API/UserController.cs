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
    public class UserController : IdentityControllerBase
    {
        public UserController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService)
         : base(responseBuilder, logger, httpContextProxy, dBService, keyValueStorage, staticContentHandler, apiGatewayService)
        {
        }

        [Route("/sso/users", CommonConst.ActionMethods.GET, "user")]
        public JObject Users()
        {
            try
            {
                string filterQuery = _httpContextProxy.GetQueryString("filter");
                _logger.Debug($"Filter: {filterQuery}");
                return GetPaggedData(CommonConst.Collection.USERS);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }

        [Route("/user/userinfo", CommonConst.ActionMethods.GET, "user")]
        public JObject UserInfoAdmin()
        {
            return UserInfo();
        }
        [Route("/sso/userinfo", CommonConst.ActionMethods.GET, "user")]
        public JObject UserInfo()
        {
            var user_id = _httpContextProxy.GetQueryString(CommonConst.CommonField.USER_ID);
            var data = GetUser(user_id);
            if (data != null)
            {
                return _responseBuilder.Success(data);
            }
            else
            {
                return _responseBuilder.NotFound();
            }
        }

        [Route("/sso/info/me", CommonConst.ActionMethods.GET, "user")]
        public JObject UserMe()
        {
            if (_httpContextProxy.User != null)
            {
                var data = UserInfoByUserId(_httpContextProxy.User.user_id);
                if (data != null)
                {
                    return _responseBuilder.Success(data);
                }
                else
                {

                    _logger.Debug($"UserInfoByUserId is null UserId: { _httpContextProxy.User.user_id}");
                    return _responseBuilder.Unauthorized();
                }
            }
            else
            {

                _logger.Debug("_httpContextProxy.User is null");
                return _responseBuilder.Unauthorized();
            }
        }

        private JObject GetUser(string user_id)
        {
            JArray joinData = new JArray();
            JObject collectionJoin = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USER_INFO, CommonConst.CommonField.USER_ID, null, CommonConst.CommonField.USER_INFO);
            joinData.Add(collectionJoin);
            JObject filter = new JObject();
            filter[CommonConst.CommonField.USER_ID] = user_id;
            _logger.Debug($"Getting user data {user_id}, filter:{filter.ToString()}");
            var data = GetPaggedData(CommonConst.Collection.USERS, null, filter.ToString());
            _logger.Debug($"Data found {data.Count}");
            if ((data[CommonConst.CommonField.DATA] as JArray).Count != 0)
            {
                return data[CommonConst.CommonField.DATA][0] as JObject;
            }
            else
            {
                return null;
            }
        }

        [Route("/sso/js/user", CommonConst.ActionMethods.GET, "user", "application/javascript")]
        public string GetUserJs()
        {

            try
            {
                _logger.Debug("Calling Get User");
                var response = new StringBuilder();
                var user = _httpContextProxy.User;
                if (user != null)
                {
                    var userModel = new JObject();
                    var userData = UserInfoByUserId(user.user_id);
                    if (userData != null)
                    {
                        var model = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(userData.ToString());
                        userModel = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                    }
                    response.AppendLine($"var __userData = {userModel.ToString() };");
                }
                else
                {
                    response.AppendLine($"var __userData = {_responseBuilder.Unauthorized().ToString() };");
                }
                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetJs {0}", ex.Message), ex);
                return $"/****Error {ex.Message} , {ex.StackTrace }****/";
            }
        }

        public bool AcceptTnc(UserModel userModel)
        {
            var user = UserInfoByUserId(userModel.user_id);
            if (AddRemoveRole(false, "init_user", user))
            {
                return AddRemoveRole(true, "phone_verification_required", user);
            }
            return false;
        }

        [Route("/sso/entermobile", CommonConst.ActionMethods.POST, "user")]
        public JObject EnterPhone()
        {

            try
            {
                var mobileNo = _httpContextProxy.GetQueryString("mobile");
                var otpReqeust = new JObject()
                {
                    ["To"] = mobileNo,
                    ["Message"] = "Validate your mobile number OTP is {{OTP}} ",
                    ["Type"] = "SMS",
                    ["OTPType"] = "mobile_number_validation",
                    ["SecurityToken"] = ""
                };
                var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/send", null, otpReqeust).GetAwaiter().GetResult();

                if (result["code"].ToString() == "1")
                {
                    return _responseBuilder.Success();
                }
                else
                {
                    return _responseBuilder.ServerError();
                }

            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in EnterPhone {0}", ex.Message), ex);
                return _responseBuilder.ServerError();
            }
        }
        [Route("/sso/validatemobile", CommonConst.ActionMethods.POST, "user")]
        public JObject ValidatePhone()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                var validateRequest = new JObject()
                {
                    ["To"] = request["mobile"],
                    ["OTP"] = request["OTP"],
                    ["OTPType"] = "mobile_number_validation",
                    ["SecurityToken"] = ""
                };

                var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/validate", null, validateRequest).GetAwaiter().GetResult();
                if (result["code"].ToString() == "1")
                {

                    if (AddRemoveRole(false, "phone_verification_required", UserInfoByUserId(_httpContextProxy.User.user_id)))
                    {
                        UpdateUserProperty(UserInfoByUserId(_httpContextProxy.User.user_id), new JObject()
                        {
                            ["phone_validation_required"] = false,
                            ["mobile_no"] = request["mobile"]
                        });
                        return _responseBuilder.Success();
                    }
                    else
                    {
                        return _responseBuilder.ServerError();
                    }
                }
                else
                {
                    return _responseBuilder.ServerError();
                }

            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in ValidatePhone {0}", ex.Message), ex);
                return _responseBuilder.ServerError();
            }
        }
    }
}
