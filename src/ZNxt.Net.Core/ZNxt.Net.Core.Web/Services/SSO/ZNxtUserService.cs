using IdentityServer4.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Identity.Services
{
    public class ZNxtUserService : IZNxtUserService
    {
        private readonly IDBService _dBService;
        private readonly IUserNotifierService _userNotifierService;
        private readonly ILogger _logger;
        private readonly IApiGatewayService _apiGatewayService;
        public ZNxtUserService(IDBService dBService, IUserNotifierService userNotifierService,ILogger logger,IApiGatewayService apiGatewayService)
        {

            _userNotifierService = userNotifierService;
            _dBService = dBService;
            _logger = logger;
            _apiGatewayService = apiGatewayService;
        }
        public bool CreateUser(ZNxt.Net.Core.Model.UserModel user, bool sendEmail = true)
        {
            return CreateUserAsync(user, sendEmail).GetAwaiter().GetResult();
        }

        public async Task<bool> CreateUserAsync(ZNxt.Net.Core.Model.UserModel user, bool sendEmail = true)
        {
            if (user != null && !IsExists(user.user_id))
            {
                if(user.roles == null)
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

        internal bool CreatePassword(string user_id, string password)
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
     
        public async Task<bool> CreateUserAsync(ClaimsPrincipal subject)
        {
            var subjectId = subject.GetSubjectId();
            if (!IsExists(subjectId))
            {
                var user = new ZNxt.Net.Core.Model.UserModel()

                {
                    id = subjectId,
                    user_id = subjectId,
                    name = subject.GetDisplayName(),
                    email = subject.GetSubjectId()
                   
                };
                return await Task.FromResult(_dBService.WriteData(Collection.USERS, JObject.Parse(JsonConvert.SerializeObject(user))));
            }
            return false;
        }
        public bool IsExists(string userid)
        {
            return _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_id: '" + userid + "'}")).Any();
        }
        public UserModel GetUser(string userid)
        {
            var user =  _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_id: '" + userid + "','is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                SetUserOrgs(userModel);
                return userModel;
            }
            else
            {
                return null;
            }

        }
        public UserModel GetUserByUsername(string username)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_name: /^"+username+"$/i,'is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                SetUserOrgs(userModel);
                return userModel;
            }
            else
            {
                return null;
            }
        }

        private void SetUserOrgs(UserModel userModel)
        {
            var extennalOrgEndpoint = "/s2fschool/identity/user/allorgs";
            try
            {
                _logger.Debug($"Calling {extennalOrgEndpoint}");
                var response = _apiGatewayService.CallAsync(ActionMethods.GET, extennalOrgEndpoint, $"user_id={userModel.user_id}").GetAwaiter().GetResult();
                if (response[CommonField.HTTP_RESPONE_CODE].ToString() == "1" && response[CommonField.DATA] != null)
                {
                    userModel.orgs = JsonConvert.DeserializeObject<List<UserOrgModel>>(response[CommonField.DATA].ToString());
                }
                else
                {
                    _logger.Error($"Error responsefrom {extennalOrgEndpoint}", null, response);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public UserModel GetUserByEmail(string email)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{email: /^" + email + "$/i,'is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                SetUserOrgs(userModel);
                return userModel;
            }
            else
            {
                return null;
            }
        }
        public PasswordSaltModel GetPassword(string userid)
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

    }
}
