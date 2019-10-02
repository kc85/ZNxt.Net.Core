using IdentityServer4.Extensions;
using IdentityServer4.Test;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
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
        public bool CreateUser(TestUser user)
        {
            if (user != null && !IsExists(user.SubjectId))
            {
                var znxtuser = new ZNxt.Net.Core.Model.UserModel()

                {
                    id = user.SubjectId,
                    user_id = user.SubjectId,
                    name = user.Username,
                    email_validation_required = Boolean.FalseString,
                    claims = user.Claims.Select(f => new ZNxt.Net.Core.Model.Claim(f.Type, f.Value)).ToList(),
                    roles = new List<string>() { "init_user" },
                    user_type = user.ProviderName
                    
                };
                var emailclaim = user.Claims.FirstOrDefault(f => f.Type == "email");
                if (emailclaim != null)
                {
                    znxtuser.email = emailclaim.Value;
                }
                if(_dBService.WriteData(Collection.USERS, JObject.Parse(JsonConvert.SerializeObject(znxtuser))))
                {
                    var userInfo = new JObject();
                    userInfo[CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
                    userInfo[CommonField.USER_ID] = znxtuser.user_id;
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
        public bool IsExists(string subjectId)
        {
            return _dBService.Get(Collection.USERS, new Net.Core.Model.RawQuery("{" + CommonField.DISPLAY_ID + ": '" + subjectId + "'}")).Any();
        }
    }
}
