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
using ZNxt.Net.Core.Web.Models.DBO;
using ZNxt.Net.Core.Web.Services;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Identity.Services
{
    public class ZNxtUserRDBService : ZNxtUserServiceBase
    {
        private readonly IRDBService _rdBService;
        private readonly IInMemoryCacheService _inMemoryCacheService;
        private const int DefaultGetpageLength = 1000;
        private const string cacheprefix = "ZNxtUserRDBService";
        private static object lockobject = new object();
        public ZNxtUserRDBService(IRDBService rdBService, IUserNotifierService userNotifierService, ILogger logger, IApiGatewayService apiGatewayService, ITenantSetterService tenantSetterService, IInMemoryCacheService inMemoryCacheService) : base(
             userNotifierService, logger, apiGatewayService, tenantSetterService
            )
        {

            _rdBService = rdBService;
            _inMemoryCacheService = inMemoryCacheService;
        }

        public override MobileAuthActivateResponse ActivateRegisterMobile(MobileAuthActivateRequest request)
        {
            throw new NotImplementedException();
        }

        public override bool CreateUser(ZNxt.Net.Core.Model.UserModel user, bool sendEmail = true)
        {
            return CreateUserAsync(user, sendEmail).GetAwaiter().GetResult();
        }

        public override async Task<bool> CreateUserAsync(UserModel user, bool sendEmail = true)
        {
            user.user_name = user.user_name.ToLower();
            if (user != null && !IsExists(user.user_name))
            {

                var dbtxn = _rdBService.BeginTransaction();
                try
                {
                    if (user.roles == null)
                    {
                        user.roles = new List<string>();
                    }
                    var roles = new List<string>() { "user", "init_user" };
                    roles.AddRange(user.roles);
                    user.roles = roles.Distinct().ToList();
                    user.salt = CommonUtility.RandomString(10);
                    user.email = user.email?.ToLower();
                    var dbroles = GetAndAddDbValues<RoleDbo>(dbtxn, roles, "role", (d) =>
                             {
                                 return roles.IndexOf(d.name) != -1;
                             },
                            (d, r) =>
                            {
                                return d.name == r;
                            }
                            , (d) =>
                            {
                                return new RoleDbo()
                                {
                                    is_enabled = true,
                                    name = d
                                };
                            });
                    var usertypes = new List<string>() { user.user_type };
                    var dbauthtype = GetAndAddDbValues<UserAuthTypeDbo>(dbtxn, usertypes, "user_auth_type", (d) =>
                             {
                                 return usertypes.IndexOf(d.name) != -1;
                             },
                           (d, r) =>
                           {
                               return d.name == r;
                           }
                           , (d) =>
                           {
                               return new UserAuthTypeDbo()
                               {
                                   is_enabled = true,
                                   name = d
                               };
                           });


                    var usermodel = new UserModelDbo()
                    {
                        is_enabled = true,
                        first_name = user.first_name,
                        middle_name = user.middle_name,
                        last_name = user.last_name,
                        user_name = user.user_name,
                        email = user.email,
                        salt = user.salt,
                        user_auth_type_id = dbauthtype.First().user_auth_type_id
                    };


                    var dbuserid = _rdBService.WriteData<UserModelDbo>(usermodel, dbtxn);
                    var userinfo = new UserProfileDbo()
                    {
                        user_id = dbuserid,
                        phone_number = ""
                    };
                    _rdBService.WriteData<UserProfileDbo>(userinfo, dbtxn);
                    user.user_id = dbuserid.ToString();
                    foreach (var role in dbroles)
                    {
                        _rdBService.WriteData<UserRoleDbo>(new UserRoleDbo()
                        {
                            is_enabled = true,
                            user_id = dbuserid,
                            role_id = role.role_id
                        }, dbtxn);
                    }

                    _rdBService.CommitTransaction(dbtxn);

                    if (sendEmail)
                    {
                        await _userNotifierService.SendWelcomeEmailAsync(user);
                    }

                    return await Task.FromResult(true);
                }
                catch (Exception)
                {
                    _rdBService.RollbackTransaction(dbtxn);
                    return await Task.FromResult(false);
                }
            }
            return await Task.FromResult(false);
        }

        private RoleDbo GetDbRole(long roleid)
        {
            var roles = GetCacheValue<RoleDbo>("role", new JObject() { ["is_enabled"] = true });

            var role = roles.FirstOrDefault(f => f.role_id == roleid);
            if (role == null)
            {
                roles = GetDBValueAddToCache<RoleDbo>("role", null, new JObject() { ["is_enabled"] = true });
                return roles.FirstOrDefault(f => f.role_id == roleid);
            }
            else
            {
                return role;
            }
        }
        private UserAuthTypeDbo GetDbUserAuthType(long id)
        {
            var data = GetCacheValue<UserAuthTypeDbo>("user_auth_type", new JObject() { ["is_enabled"] = true });

            var authtype = data.FirstOrDefault(f => f.user_auth_type_id == id);
            if (authtype == null)
            {
                data = GetDBValueAddToCache<UserAuthTypeDbo>("user_auth_type", null, new JObject() { ["is_enabled"] = true });
                return data.FirstOrDefault(f => f.user_auth_type_id == id);
            }
            else
            {
                return authtype;
            }
        }

        private List<T> GetCacheValue<T>(string tablename, JObject filter = null) where T : class
        {
            var dbroles = _inMemoryCacheService.Get<List<T>>($"{cacheprefix}-{tablename}");
            if (dbroles == null)
            {
                dbroles = GetDBValueAddToCache<T>(tablename, null, filter);
            }
            return dbroles;
        }
        private List<T> GetDBValueAddToCache<T>(string tablename, List<T> input = null, JObject filter = null) where T : class
        {
            lock (lockobject)
            {
                if (input == null) { input = new List<T>(); }
                if (filter == null) { filter = new JObject(); }
                var r = _rdBService.Get<T>(tablename, DefaultGetpageLength, 0, filter);
                if (r != null && r.Any())
                {
                    input.AddRange(r.ToList());
                }
                if (input.Any())
                {
                    _inMemoryCacheService.Put<List<T>>($"{cacheprefix}-{tablename}", input);
                }
                return input;
            }
        }
        private List<T> GetAndAddDbValues<T>(RDBTransaction tnx, List<string> values, string tablename, Func<T, bool> comparein, Func<T, string, bool> compare, Func<string, T> data) where T : class
        {
            var dbroles = _inMemoryCacheService.Get<List<T>>($"{cacheprefix}-{tablename}");
            Func<List<T>, IEnumerable<T>> loadRoles = (lstdata) =>
             {
                 return GetDBValueAddToCache<T>(tablename, lstdata, new JObject() { ["is_enabled"] = true });
             };
            if (dbroles == null)
            {
                dbroles = loadRoles(null).ToList();
            }

            Func<List<T>> loadmyroles = () =>
            {
                var dbmytoles = new List<T>();
                if (dbroles != null && dbroles.Any())
                {
                    dbmytoles.AddRange(dbroles.Where(f => comparein(f)).Select(f => f));
                }
                return dbmytoles;
            };
            var finalroles = loadmyroles();

            if (!finalroles.Any() || finalroles.Count != values.Count)
            {
                dbroles = loadRoles(null).ToList();
                finalroles = loadmyroles();
            }
            if (!finalroles.Any() || finalroles.Count != values.Count)
            {
                var dbvalues = new List<T>();
                foreach (var r in values)
                {
                    if (!finalroles.Where(f => compare(f, r)).Any())
                    {
                        T d = data(r);
                        var response = _rdBService.WriteData<T>(d, tnx);
                        dbvalues.Add(d);
                    }
                }
                dbroles = loadRoles(dbvalues).ToList();
                finalroles = loadmyroles();
            }
            if (finalroles.Count != values.Count)
            {
                throw new Exception("Error while loading roles ");
            }
            return finalroles;
        }
       

        public override bool GetIsUserConsecutiveLoginFailLocked(string user_id)
        {
            throw new NotImplementedException();
        }

        public override PasswordSaltModel GetPassword(string userid)
        {
            var userpass = _rdBService.Get<UserPasswordDbo>("\"user_password\"", 1, 0, new JObject() { ["user_id"] = userid, ["is_enabled"] = true });
            if (userpass != null && userpass.Any())
            {
                return new PasswordSaltModel()
                {
                    Password = userpass.First().password,
                };
            }
            else
            {
                return null;
            }
        }

        public override UserModel GetUser(string userid)
        {
            return GetUser(new JObject() { ["user_id"] = userid, ["is_enabled"] = true });
        }

        private  UserModel GetUser(JObject filter)
        {
            var user = _rdBService.Get<UserModelDbo>("\"user\"", 1, 0, filter);
            if (user.Any())
            {
                var userdata = user.First();
                return GetDtoUserModel(userdata);
            }
            else
            {
                return null;
            }

        }

        private List<UserModel> GetUsers(JObject filter)
        {
            var users = _rdBService.Get<UserModelDbo>("\"user\"", DefaultGetpageLength, 0, filter);

            List<UserModel> usermodels = new List<UserModel>();

            foreach (var item in users)
            {
                usermodels.Add(GetDtoUserModel(item));
            }

            return usermodels;

        }
            private UserModel GetDtoUserModel(UserModelDbo userdata)
        {
            var userModel = new UserModel()
            {
                user_id = userdata.user_id.ToString(),
                first_name = userdata.first_name,
                middle_name = userdata.middle_name,
                last_name = userdata.last_name,
                email = userdata.email,
                is_enabled = userdata.is_enabled,
                salt = userdata.salt,
                user_type = GetDbUserAuthType(userdata.user_auth_type_id).name,
            };
            var roles = _rdBService.Get<UserRoleDbo>("\"user_role\"", DefaultGetpageLength, 0, filter);
            userModel.roles = roles.Select(f => GetDbRole(f.role_id).name).ToList();
            SetUserTenants(userModel);
            return userModel;
        }
        public override UserModel GetUserByEmail(string email)
        {
            return GetUser(new JObject() { ["email"] = email.ToLower(), ["is_enabled"] = true });
        }

        public override UserModel GetUserByUsername(string username)
        {
            return GetUser(new JObject() { ["user_name"] = username.ToLower(), ["is_enabled"] = true });
        }

        public override List<UserModel> GetUsersByEmail(string email)
        {
            return GetUsers(new JObject() { ["email"] = email.ToLower(), ["is_enabled"] = true });
        }

        public override bool IsExists(string user_name)
        {

            var m = new UserModelDbo();
            var count = _rdBService.GetCount("\"user\"", new JObject() { [nameof(m.user_name)] = user_name });

            return count != 0;

        }

       
        public override void ResetUserLoginFailCount(string user_id)
        {
            var sql = "update user_login_fail_lock set lock_end_time=@current_time where lock_end_time > @current_time and user_id=@user_id";

                //var filter = "{" + consecutive_check_end_time + ": { $gt: " + CommonUtility.GetTimestampMilliseconds(DateTime.Now) + " }," + CommonConst.CommonField.USER_ID + ": '" + user_id + "'}";
                //var data = _dBService.Get(collectonUserLoginFail, new RawQuery(filter.ToString()));
                //if (data.Any())
                //{
                //    data.First()[consecutive_check_end_time] = CommonUtility.GetTimestampMilliseconds(DateTime.Now);
                //    if (_dBService.Update(collectonUserLoginFail, new RawQuery(filter.ToString()), data.First() as JObject, true, MergeArrayHandling.Replace) != 1)
                //    {
                //        _logger.Error("Error while updating consecutive_check_end_time");
                //    }
                //}


            throw new NotImplementedException();
        }

        public override bool UpdateUser(string userid, JObject data)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateUserLoginFailCount(string user_id)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateUserProfile(string userid, JObject data)
        {
            throw new NotImplementedException();
        }

        protected override UserModel GetUserByMobileAuthPhoneNumber(string mobileNumber)
        {
            throw new NotImplementedException();
        }

        public override bool CreatePassword(string user_id, string password)
        {
            var user = GetUser(user_id);
            if (user != null)
            {
                var dbtxn = _rdBService.BeginTransaction();
                try
                {
                    var passwordhash = CommonUtility.Sha256Hash($"{password}{user.salt}");
                    var passworddata = _rdBService.Get<UserPasswordDbo>("\"user_password\"", 1, 0, new JObject() { ["user_id"] = user_id, ["is_enabled"] = true });
                    if (passworddata.Any())
                    {
                        passworddata.First().is_enabled = false;
                        passworddata.First().updated_on = ZNxt.Net.Core.Helpers.CommonUtility.GetUnixTimestamp(DateTime.UtcNow);
                        _rdBService.Update<UserPasswordDbo>(passworddata.First(), dbtxn);
                    }
                    _rdBService.WriteData<UserPasswordDbo>(new UserPasswordDbo()
                    {
                        password = passwordhash,
                        user_id = long.Parse(user.user_id),
                        is_enabled = true,
                    }, dbtxn);
                    _rdBService.CommitTransaction(dbtxn);
                }
                catch (Exception ex)
                {
                    _logger.Error($"CreatePassword :{ex.Message}", ex);
                    _rdBService.RollbackTransaction(dbtxn);
                    throw;
                }

                return true;
            }
            else
            {
                _logger.Info($"CreatePassword : unable to find the user:{user_id}");
                return false;

            }
        }
        public override MobileAuthRegisterResponse RegisterMobile(MobileAuthRegisterRequest request)
        {
            throw new NotImplementedException();
        }

    }
}
