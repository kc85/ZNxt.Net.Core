using IdentityServer4.Extensions;
using IdentityServer4.Test;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
                    claims = user.Claims.Select(f => new  ZNxt.Net.Core.Model.Claim(f.Type, f.Value) ).ToList()
                };
                return _dBService.WriteData(Collection.USERS, JObject.Parse(JsonConvert.SerializeObject(znxtuser)));


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
