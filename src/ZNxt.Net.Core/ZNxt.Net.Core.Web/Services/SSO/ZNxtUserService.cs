﻿using IdentityServer4.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public ZNxtUserService(IDBService dBService, IUserNotifierService userNotifierService,ILogger logger)
        {

            _userNotifierService = userNotifierService;
            _dBService = dBService;
            _logger = logger;
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

                // TODO need to get the Orf details for external model
                userModel.orgs = new List<UserOrgModel>()
                {
                    new UserOrgModel(){
                         orgkey = "VMSW2",
                         roles = new List<string>(){  "teacher" }
                    },
                    new UserOrgModel(){
                         orgkey = "ARVV2",
                         roles = new List<string>(){  "parent" }
                    },
                };
                _logger.Debug($"Added Dummny orgs : {userModel.orgs.Count}");

                return userModel;
            }
            else
            {
                return null;
            }

        }
        public UserModel GetUserByEmail(string email)
        {
            var user = _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{email: '" + email + "','is_enabled':true}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                // TODO need to get the Orf details for external model
                userModel.orgs = new List<UserOrgModel>()
                {
                    new UserOrgModel(){
                         orgkey = "VMSW2",
                         roles = new List<string>(){  "teacher" }
                    },
                    new UserOrgModel(){
                         orgkey = "ARVV2",
                         roles = new List<string>(){  "parent" }
                    },
                };
                _logger.Debug($"Added Dummny orgs : {userModel.orgs.Count}");
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
