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
    public class UserController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IDBService _dBService;
        private readonly IApiGatewayService _apiGatewayService;
        public UserController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler, IApiGatewayService apiGatewayService)
         : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _dBService = dBService;
            _apiGatewayService = apiGatewayService;
        }

        [Route("/sso/users", CommonConst.ActionMethods.GET, "user")]
        public JObject Users()
        {
            return GetPaggedData(CommonConst.Collection.USERS);
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
        private JObject UserInfoByUserId(string user_id)
        {
            var user = _dBService.Get(CommonConst.Collection.USERS, new RawQuery("{'user_id':'" + user_id + "'}"));
            if (user.Count != 0)
            {
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                var userResponse = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                return userResponse;
            }
            else
            {
                return null;
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
            return AddRoles(userModel.user_id, "phone_verification_required", "init_user");
        }

        private bool AddRoles(string user_id,string addrole, string removerole,JObject updateProprty = null)
        {
            var user = UserInfoByUserId(user_id);
            if (!string.IsNullOrEmpty(removerole)) {
                var initUser = (user["roles"] as JArray).FirstOrDefault(f => f.ToString() == removerole);
                if (initUser != null)
                {
                    (user["roles"] as JArray).Remove(initUser);
                }
            }
            if (!string.IsNullOrEmpty(addrole)) {
                (user["roles"] as JArray).Add(addrole);
            }
            if (updateProprty != null)
            {
                foreach (var item in updateProprty)
                {
                    user[item.Key] = item.Value;
                }
            }
              return _dBService.Write(CommonConst.Collection.USERS, user, "{'user_id' : '" + user_id + "'}",true, MergeArrayHandling.Replace);
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
                    ["Message"] = "Validate your phone numer OTP is {{OTP}} ",
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

                var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, "/notifier/otp/validate", null, request).GetAwaiter().GetResult();
                if (result["code"].ToString() == "1")
                {
                    AddRoles(_httpContextProxy.User.user_id, "", "phone_verification_required", new JObject() {
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
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in ValidatePhone {0}", ex.Message), ex);
                return _responseBuilder.ServerError();
            }
        }
    }
}
