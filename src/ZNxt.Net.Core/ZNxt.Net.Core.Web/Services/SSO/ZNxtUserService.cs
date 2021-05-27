using IdentityServer4.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Exceptions;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Services;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Identity.Services
{
    public class ZNxtUserService : ZNxtUserServiceBase
    {
        private readonly IDBService _dBService;
        private const string collectonUserLoginFail = "user_login_fail";
        private const string consecutive_check_end_time = "consecutive_check_end_time";
        private const string is_locked = "is_locked";

        public ZNxtUserService(IDBService dBService, IUserNotifierService userNotifierService, ILogger logger, IApiGatewayService apiGatewayService, ITenantSetterService tenantSetterService):base(
             userNotifierService, logger, apiGatewayService, tenantSetterService
            )
        {

            _dBService = dBService;
        }
        public override bool CreateUser(ZNxt.Net.Core.Model.UserModel user, bool sendEmail = true)
        {
            return CreateUserAsync(user, sendEmail).GetAwaiter().GetResult();
        }

        public override async Task<bool> CreateUserAsync(ZNxt.Net.Core.Model.UserModel user, bool sendEmail = true)
        {
            if (user != null && !IsExists(user.user_id))
            {
                if (user.roles == null)
                {
                    user.roles = new List<string>();
                }
                var roles = new List<string>() { "user", "init_user" };
                roles.AddRange(user.roles);
                user.roles = roles.Distinct().ToList();
                user.salt = CommonUtility.RandomString(10);
                var userObject = JObject.Parse(JsonConvert.SerializeObject(user));
                userObject[CommonField.IS_ENABLED] = true;
                if (_dBService.WriteData(Collection.USERS, userObject))
                {
                    var userInfo = new JObject();
                    userInfo[CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
                    userInfo[CommonField.USER_ID] = user.user_id;
                    var result = _dBService.WriteData(Collection.USER_INFO, userInfo);
                    if (result)
                    {
                        if (sendEmail)
                        {
                            await _userNotifierService.SendWelcomeEmailAsync(user);
                        }
                    }
                    else
                    {
                        userObject[CommonField.IS_ENABLED] = false;
                        _dBService.Write(Collection.USERS, userObject, "{user_id: '" + user.user_id + "'}");
                        _logger.Error($"Error while creating user user_id : {user.user_id} Type: {user.user_type}, email:{user.email}");
                    }
                    return result;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public override bool CreatePassword(string user_id, string password)
        {
            var user = GetUser(user_id);
            var passwordhash = CommonUtility.Sha256Hash($"{password}{user.salt}");
            _dBService.Delete($"{Collection.USERS}-pass", new Net.Core.Model.RawQuery("{user_id: '" + user_id + "'}"));
            return _dBService.Write($"{Collection.USERS}-pass", new JObject()
            {
                [CommonField.DISPLAY_ID] = CommonUtility.GetNewID(),
                ["Password"] = passwordhash,
                ["user_id"] = user_id,
                ["is_enabled"] = true
            });
        }

        public override bool RemoveUserRole(string userid, string rolename)
        {
            return AddRemoveRole(false, rolename, GetUser(userid).ToJObject());
        }
        public override bool AddUserRole(string userid, string rolename)
        {
            return AddRemoveRole(true, rolename, GetUser(userid).ToJObject());
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
        public override bool IsExists(string userid)
        {
            return _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_id: '" + userid + "'}")).Any();
        }
        public override UserModel GetUser(string userid)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_id: '" + userid + "','is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                SetUserTenants(userModel);
                return userModel;
            }
            else
            {
                return null;
            }

        }
        public override UserModel GetUserByUsername(string username)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_name: /^" + username + "$/i,'is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                SetUserTenants(userModel);
                return userModel;
            }
            else
            {
                return null;
            }
        }
        protected override UserModel GetUserByMobileAuthPhoneNumber(string mobileNumber)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{mobile_auth_phone_number: /^" + mobileNumber + "$/i,'is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                SetUserTenants(userModel);
                return userModel;
            }
            else
            {
                return null;
            }
        }
        public override JObject GetRoleById(long roleid)
        {
            return new JObject() { ["role_id"] = roleid, ["name"] = roleid };
        }
        public override bool GetIsUserConsecutiveLoginFailLocked(string user_id)
        {
            var filter = "{"+ consecutive_check_end_time + ": { $gt: " + CommonUtility.GetTimestampMilliseconds(DateTime.Now) + " }," + CommonConst.CommonField.USER_ID + ": '" + user_id + "'," + is_locked + ":true}";
            var data = _dBService.Get(collectonUserLoginFail, new RawQuery(filter.ToString()));
            if (data.Any())
            {
                double endtime = 0;
                if (double.TryParse(data.First()[consecutive_check_end_time].ToString(), out endtime))
                {
                    var datatime = CommonUtility.GetTimestampMilliseconds(DateTime.Now);
                    throw new UserConsecutiveLoginFailLockException(endtime - datatime);
                }
                else
                {
                    throw new FormatException($"Error on : {consecutive_check_end_time} convert to long : {data.First().ToString()}");
                }
            }
            return false;

        }
        public override void  ResetUserLoginFailCount(string user_id)
        {
            var filter = "{"+ consecutive_check_end_time + ": { $gt: " + CommonUtility.GetTimestampMilliseconds(DateTime.Now) + " }," + CommonConst.CommonField.USER_ID + ": '" + user_id + "'}";
            var data = _dBService.Get(collectonUserLoginFail, new RawQuery(filter.ToString()));
            if (data.Any())
            {
                data.First()[consecutive_check_end_time] = CommonUtility.GetTimestampMilliseconds(DateTime.Now);
                if (_dBService.Update(collectonUserLoginFail, new RawQuery(filter.ToString()), data.First() as JObject, true, MergeArrayHandling.Replace) != 1)
                {
                    _logger.Error("Error while updating consecutive_check_end_time");
                }
            }

        }
        public override bool UpdateUserLoginFailCount(string user_id)
        {
        

            var consecutiveLockFailDuratoinTimestamp = CommonUtility.GetTimestampMilliseconds(DateTime.Now.AddMinutes(consecutiveLockFailDuratoin));

            var filter = "{" + consecutive_check_end_time + ": { $gt: " + CommonUtility.GetTimestampMilliseconds(DateTime.Now) + " }," + CommonConst.CommonField.USER_ID + ": '" + user_id + "' }";// new JObject() { [CommonConst.CommonField.USER_ID] = user_id };
            var data = _dBService.Get(collectonUserLoginFail, new RawQuery(filter.ToString()));

            if (data.Count == 0)
            {
                JObject failData = new JObject()
                {
                    [CommonConst.CommonField.USER_ID] = user_id,
                    [CommonField.DISPLAY_ID] = CommonUtility.GetNewID(),
                    [CommonField.COUNT] = 1,
                    [is_locked] = false,
                    [consecutive_check_end_time] = consecutiveLockFailDuratoinTimestamp
                };
                var result = _dBService.Write(collectonUserLoginFail, failData);
                if (!result)
                {
                    _logger.Error("Error while updating login fail count")
;
                }
            }
            else
            {
                var count = int.Parse(data.First()[CommonField.COUNT].ToString());
                data.First()[CommonField.COUNT] = (count + 1);
                if (count + 1 >= consecutiveMaxfail)
                {
                    data.First()[is_locked] = true;
                    consecutiveLockFailDuratoinTimestamp = ZNxt.Net.Core.Helpers.CommonUtility.GetTimestampMilliseconds(DateTime.Now.AddMinutes(consecutiveLockDuratoin));
                }
                data.First()[consecutive_check_end_time] = consecutiveLockFailDuratoinTimestamp;
                if (_dBService.Update(collectonUserLoginFail, new RawQuery(filter.ToString()), data.First() as JObject, true, MergeArrayHandling.Replace) != 1)
                {
                    _logger.Error("Error while updating login fail count");
                }
                if(count +3 > consecutiveMaxfail)
                {
                    throw new UserConsecutiveLoginFailLockCountException(consecutiveMaxfail - count);
                }

            }
            return true;
        }

        public override List<UserModel> GetUsersByEmail(string email)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{email: /^" + email + "$/i,'is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<List<UserModel>>(user.ToString());
                return userModel;
            }
            else
            {
                return new List<UserModel>();
            }
        }
        public override UserModel GetUserByEmail(string email)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{email: /^" + email + "$/i,'is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                SetUserTenants(userModel);
                return userModel;
            }
            else
            {
                return null;
            }
        }
        public override PasswordSaltModel GetPassword(string userid)
        {
            var user = _dBService.Get($"{Collection.USERS}-pass", new Net.Core.Model.RawQuery("{user_id: '" + userid + "','is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<PasswordSaltModel>(user.First().ToString());
                return userModel;
            }
            else
            {
                return null;
            }

        }
        public override bool UpdateUser(string userid, JObject data)
        {

            var filter = new JObject();
            filter[CommonField.USER_ID] = userid;
            var userData = _dBService.FirstOrDefault(Collection.USERS, new RawQuery(filter.ToString()));
            if (userData != null)
            {
                foreach (var d in data)
                {   
                    userData[d.Key] = d.Value;
                }
                return _dBService.Update(Collection.USERS, new RawQuery(filter.ToString()), userData) == 1;
            }
            else
            {
                _logger.Error($"User not found User Id : {userData}");
                return false;
            }

        }
        public override bool UpdateUserProfile(string userid, JObject data)
        {

            var filter = new JObject();
            filter[CommonField.USER_ID] = userid;
            var userInfo = _dBService.FirstOrDefault(Collection.USER_INFO, new RawQuery(filter.ToString()));
            if (userInfo != null)
            {
                foreach (var d in data)
                {
                    userInfo[d.Key] = d.Value;
                }
                return _dBService.Update(Collection.USER_INFO, new RawQuery(filter.ToString()), userInfo,true, MergeArrayHandling.Replace) == 1;
            }
            else
            {
                _logger.Error($"User not found User Id : {userInfo}");
                return false;
            }

        }
     
        public override MobileAuthRegisterResponse RegisterMobile(MobileAuthRegisterRequest request)
        {
            var requestObj = request.ToJObject();
            var id = CommonUtility.GetNewID();
            requestObj[CommonConst.CommonField.DISPLAY_ID] = id;
            if (_dBService.Write(Collection.MOBILE_AUTH_REGISTRATION, requestObj))
            {
                return new MobileAuthRegisterResponse() { code = CommonConst._1_SUCCESS, validation_token = id, device_address  = request.device_address, mobile_number = request.mobile_number };
            }
            else
            {
                throw new Exception("RegisterMobile error");
            }
        }
        public override MobileAuthActivateResponse ActivateRegisterMobile(MobileAuthActivateRequest request)
        {
            var user = GetUserByMobileAuthPhoneNumber(request.mobile_number);
            if (user == null)
            {
                user = new UserModel()
                {
                    id = CommonUtility.GetNewID(),
                    user_id = CommonUtility.GetNewID(),
                    user_name = CommonUtility.GetNewID(),
                    first_name = request.mobile_number,
                    middle_name = "",
                    last_name = "",
                    salt = CommonUtility.GetNewID(),
                    is_enabled = true,
                    user_type = "mobile_auth",
                    mobile_auth_phone_number = request.mobile_number,
                    //dob = new DOBModel() { day = 1, month = 1, year = 1 },
                    roles = new List<string> { "mobile_auth", "mobile_auth_init_user" }
                };
                if (!CreateUser(user, false))
                {
                    throw new Exception("Error adding user");
                }
            }

            var mobileActivationObj = new JObject();
            mobileActivationObj[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
            mobileActivationObj["device_address"] = request.device_address;
            var meta_data = new JObject();
            foreach (var metadata in request.meta_data)
            {
                meta_data[metadata.Key] = metadata.Value;
            }
            mobileActivationObj["meta_data"] = meta_data;
            if (_dBService.WriteData(CommonConst.Collection.MOBILE_AUTH_ACTIVATION, mobileActivationObj))
            {
                var secretKey = CommonUtility.RandomString(19);
                if (CreatePassword(user.user_id, secretKey))
                {
                    return new MobileAuthActivateResponse() { code = CommonConst._1_SUCCESS, user_name = user.user_name, secret_key = secretKey, user_id = user.user_id};
                }
                else
                {
                    throw new Exception("Error adding create password");
                }
            }
            else
            {
                throw new Exception("Error adding mobile activation");
            }
        }
    }
}
