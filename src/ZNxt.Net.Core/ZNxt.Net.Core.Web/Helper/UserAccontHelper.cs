using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Helper
{
    public class UserAccontHelper
    {
        private readonly IEncryption _encryptionService;
        private readonly IDBService _dbService;
        public UserAccontHelper(IEncryption encryptionService, IDBService dbService)
        {
            _encryptionService = encryptionService;
            _dbService = dbService;
        }
        public JObject CreateNewUserObject(string userName, string email, string name, string password, UserIDType userType)
        {
            JObject customConfig = new JObject
            {
                [CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID(),
                [CommonConst.CommonField.USER_TYPE] = userType.ToString(),
                [CommonConst.CommonField.IS_EMAIL_VALIDATE] = true,
                [CommonConst.CommonField.IS_ENABLED] = true
            };
            customConfig[CommonConst.CommonField.DATA_KEY] =
            customConfig[CommonConst.CommonField.NAME] =
            customConfig[CommonConst.CommonField.EMAIL] =
            customConfig[CommonConst.CommonField.USER_ID] = userName;
            customConfig[CommonConst.CommonField.SALT_KEY] = CommonUtility.RandomString(19);
            customConfig[CommonConst.CommonField.PASSWORD] = _encryptionService.GetHash(password, customConfig[CommonConst.CommonField.SALT_KEY].ToString());

            var claims = CreateUserDeafultClaims(userName, name, email);
            foreach (var claim in claims)
            {
                AddClaim(customConfig, claim.Key, claim.Value);
            }

            return customConfig;
        }
        public void AddClaim(JObject customConfig, string claimkey, string claimvalue)
        {
            if (customConfig[CommonConst.CommonField.CLAIMS] == null)
            {
                customConfig[CommonConst.CommonField.CLAIMS] = new JArray();
            }
            (customConfig[CommonConst.CommonField.CLAIMS] as JArray).Add(new JObject()
            {
                [CommonConst.CommonField.KEY] = claimkey,
                [CommonConst.CommonField.VALUE] = claimvalue,

            });
        }

        public List<Claim> CreateUserDeafultClaims(string userName, string name, string email)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(CommonConst.CommonField.NAME, name),
                new Claim(CommonConst.CommonField.EMAIL, email),
                new Claim(CommonConst.CommonField.ROLE_CLAIM_TYPE, CommonConst.CommonField.USER_ROLE)
            };
            return claims;
        }

        public bool ValidateUser(string userName, string password)
        {
            var user = GetUser(userName);
            return ValidateUser(user, password);
        }
        public bool ValidateUser(UserModel user, string password)
        {
            if (user != null)
            {
                var passwordHash = _encryptionService.GetHash(password, user.salt);
                JObject filter = new JObject
                {
                    [CommonConst.CommonField.USER_ID] = user.user_id,
                    [CommonConst.CommonField.PASSWORD] = passwordHash,
                    [CommonConst.CommonField.IS_ENABLED] = true
                };
                var userData = _dbService.Get(CommonConst.Collection.USERS, new RawQuery(filter.ToString()));
                if (userData != null && userData.Count == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public UserModel GetUser(string userName)
        {
            JObject filter = new JObject
            {
                [CommonConst.CommonField.USER_ID] = userName,
                [CommonConst.CommonField.IS_ENABLED] = true
            };
            var userData = _dbService.Get(CommonConst.Collection.USERS, new RawQuery(filter.ToString()));
            if (userData != null && userData.Count == 1)
            {
                UserModel user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(userData.First().ToString());
                return user;
            }
            else
            {
                return null;
            }
        }
    }
}
