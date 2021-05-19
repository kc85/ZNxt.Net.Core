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
    public abstract class ZNxtUserServiceBase : IZNxtUserService
    {
        protected readonly IUserNotifierService _userNotifierService;
        protected readonly ILogger _logger;
        protected readonly IApiGatewayService _apiGatewayService;
        protected readonly ITenantSetterService _tenantSetterService;

        protected const int consecutiveMaxfail = 5;
        protected const int consecutiveLockDuratoin = 30;
        protected const int consecutiveLockFailDuratoin = 10;

        public ZNxtUserServiceBase(IUserNotifierService userNotifierService, ILogger logger, IApiGatewayService apiGatewayService, ITenantSetterService tenantSetterService)
        {

            _userNotifierService = userNotifierService;
            _logger = logger;
            _apiGatewayService = apiGatewayService;
            _tenantSetterService = tenantSetterService;
        }
        public virtual bool CreateUser(ZNxt.Net.Core.Model.UserModel user, bool sendEmail = true)
        {
            return CreateUserAsync(user, sendEmail).GetAwaiter().GetResult();
        }

        public abstract Task<bool> CreateUserAsync(ZNxt.Net.Core.Model.UserModel user, bool sendEmail = true);

        public abstract bool CreatePassword(string user_id, string password);

        //public abstract Task<bool> CreateUserAsync(ClaimsPrincipal subject);

        public abstract bool IsExists(string userid);

        public abstract UserModel GetUser(string userid);
        public abstract UserModel GetUserByUsername(string username);
        protected abstract UserModel GetUserByMobileAuthPhoneNumber(string mobileNumber);

        public abstract bool GetIsUserConsecutiveLoginFailLocked(string user_id);
        public abstract void ResetUserLoginFailCount(string user_id);

        public abstract bool UpdateUserLoginFailCount(string user_id);

        public virtual void SetUserTenants(UserModel userModel)
        {
            _tenantSetterService.SetTenant(userModel);
        }
        public virtual bool AddUserToTenants(UserModel userModel)
        {
            return _tenantSetterService.AddUserToTenant(userModel);
        }

        public abstract List<UserModel> GetUsersByEmail(string email);

        public abstract UserModel GetUserByEmail(string email);
        public abstract PasswordSaltModel GetPassword(string userid);
        public abstract bool UpdateUser(string userid, JObject data);

        public abstract JObject GetRoleById(long roleid);


        public abstract bool UpdateUserProfile(string userid, JObject data);

        //public  bool AddUserRelation(string parentuserid, string childuserid, string relation)
        //{
        //    relation = relation.Trim().ToLower();
        //    var data = new JObject();
        //    data[$"parent_" + CommonField.USER_ID] = parentuserid;
        //    data[$"child_" + CommonField.USER_ID] = childuserid;
        //    data[CommonField.IS_ENABLED] = true;
        //    data[$"relation"] = relation;

        //    var userRelation = _dBService.FirstOrDefault(Collection.USER_RELATIONSHIP, new RawQuery(data.ToString()));
        //    if (userRelation == null)
        //    {
        //        data[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
        //        return _dBService.Write(Collection.USER_RELATIONSHIP, data);
        //    }
        //    else
        //    {
        //        _logger.Error($"Duplicate record found  not parentuserid: {parentuserid}, childuserid:{childuserid}, relation:{relation}");
        //        return false;
        //    }
        //}

        public abstract bool RemoveUserRole(string userid, string rolename);
        public abstract bool AddUserRole(string userid, string rolename);
        public abstract MobileAuthRegisterResponse RegisterMobile(MobileAuthRegisterRequest request);
        public abstract MobileAuthActivateResponse ActivateRegisterMobile(MobileAuthActivateRequest request);

    }
}
