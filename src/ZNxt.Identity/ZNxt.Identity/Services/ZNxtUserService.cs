using IdentityServer4.Extensions;
using IdentityServer4.Test;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Identity.Models;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Identity.Services
{
    public class ZNxtUserService : IZNxtUserService
    {
        private readonly IDBService _dBService;
        public ZNxtUserService(IDBService dBService)
        {
            _dBService = dBService;
        }
        public bool CreateUser(ZNxt.Net.Core.Model.UserModel user)
        {
            if (user != null && !IsExists(user.user_id))
            { 
                user.roles = new List<string>() { "init_user" };
                if(_dBService.WriteData(Collection.USERS, JObject.Parse(JsonConvert.SerializeObject(user))))
                {
                    var userInfo = new JObject();
                    userInfo[CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
                    userInfo[CommonField.USER_ID] = user.user_id;
                    var result = _dBService.WriteData(Collection.USER_INFO, userInfo);
                    
                    return result;
                }
                else
                {
                    return false;
                }


            }
            return false;
        }

        public bool CreateUser(ClaimsPrincipal subject)
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
                return _dBService.WriteData(Collection.USERS, JObject.Parse(JsonConvert.SerializeObject(user)));
            }
            return false;
        }
        public bool IsExists(string userid)
        {
            return _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_id: '" + userid + "'}")).Any();
        }
        public UserModel GetUser(string userid)
        {
            var user =  _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{user_id: '" + userid + "'}"));
            if (user.Any())
            {
                var userModel = JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                return userModel;
            }
            else
            {
                return null;
            }

        }
        public PasswordSaltModel GetPassword(string userid)
        {
            var user = _dBService.Get($"{Collection.USERS}-pass", new Net.Core.Model.RawQuery("{user_id: '" + userid + "'}"));
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
