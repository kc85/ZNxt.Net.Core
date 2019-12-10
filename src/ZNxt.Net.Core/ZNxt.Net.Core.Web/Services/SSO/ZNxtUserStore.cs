using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Identity.Services
{
    public class ZNxtUserStore 
    {
        private readonly IZNxtUserService _userService;
        private readonly IApiGatewayService _apiGatewayService;
        private readonly ILogger _logger;
        public ZNxtUserStore(IZNxtUserService userService,IApiGatewayService apiGatewayService,ILogger logger)
        {
            _userService = userService;
            _apiGatewayService = apiGatewayService;
            _logger = logger;
        }

        public async Task<UserModel> AutoProvisionUserAsync(string provider, string userId, List<System.Security.Claims.Claim> claims)
        {
            provider = provider.ToLower();
            var name = claims.FirstOrDefault(f => f.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
            var firstname = claims.FirstOrDefault(f => f.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
            var lastname = claims.FirstOrDefault(f => f.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
            var emailaddress = claims.FirstOrDefault(f => f.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            var privideuserid = $"{provider.Substring(0, 3)}{userId}";
            var user = new UserModel()
            {
                id = privideuserid,
                user_type = provider,
                user_id = privideuserid,
                email = emailaddress.Value,
                name = name.Value,
                email_validation_required = Boolean.FalseString.ToLower(),
                claims = claims.Select(f => new Net.Core.Model.Claim(f.Type, f.Value)).ToList()
            };
            await _userService.CreateUserAsync(user);
            return user;
        }

        internal bool SetPassword(string user_id, string password)
        {

            var user = _userService.GetUser(user_id);
            if (user != null)
            {
                if (user.roles.Where(f => f == "pass_set_required").Any())
                {
                    if ((_userService as ZNxtUserService).CreatePassword(user_id, password))
                    {
                        var role = "pass_set_required";
                        var request = new JObject()
                        {
                            ["user_id"] = user_id,
                            ["role"] = role,
                        };
                        return CallGatewayPost(request, "/sso/user/apiremoverole");
                    }
                }
            }
            return false;
        }

        //
        // Summary:
        //     Finds the user by external provider.
        //
        // Parameters:
        //   provider:
        //     The provider.
        //
        //   userId:
        //     The user identifier.
        public UserModel FindByExternalProvider(string provider, string userId)
        {
            return _userService.GetUser(userId);
        }
        //
        // Summary:
        //     Finds the user by subject identifier.
        //
        // Parameters:
        //   subjectId:
        //     The subject identifier.
        public UserModel FindBySubjectId(string subjectId)
        {
            return _userService.GetUser(subjectId);
        }
        //
        // Summary:
        //     Finds the user by username.
        //
        // Parameters:
        //   username:
        //     The username.
        public UserModel FindByUsername(string username)
        {
            return _userService.GetUserByEmail(username);
        }
       
        public bool ValidateCredentials(string username, string password, string emailotp)
        {
            var user =  _userService.GetUserByUsername(username);
            if (user != null)
            {
                if (user.roles.Where(f=>f == "init_login_email_otp").Any())
                {
                    if(ValidateEmailOTP(username, emailotp))
                    {
                        if (RemoveOTPValidateUserRole(user.user_id))
                        {
                            return AddFouceAddPassUserRole(user.user_id);
                        }
                    }
                    return false;
                }
                else
                {
                    return ValidatePassword(password, user);
                }
            }
            else
            {
                return false;
            }
        }

        private bool AddFouceAddPassUserRole(string user_id)
        {
            var role = "pass_set_required";
            var request = new JObject()
            {
                ["user_id"] = user_id,
                ["role"] = role,
            };
            return CallGatewayPost(request, "/sso/user/apiaddrole");
        }

        private bool RemoveOTPValidateUserRole(string user_id)
        {
            var role = "init_login_email_otp";
            var request = new JObject()
            {
                ["user_id"] = user_id,
                ["role"] = role,
            };
            return CallGatewayPost(request, "/sso/user/apiremoverole");
        }

        private bool ValidatePassword(string password, UserModel user)
        {
            var pass = _userService.GetPassword(user.user_id);
            if (pass != null)
            {
                var passwordwithsalt = $"{password}{user.salt}";
                if (pass.Password.Equals(CommonUtility.Sha256Hash(passwordwithsalt)))
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

        private bool ValidateEmailOTP(string username, string emailotp)
        {

            var request = new JObject()
            {
                ["Type"] = "Email",
                ["To"] = username,
                ["OTP"] = emailotp,
                ["OTPType"] = "registration_with_email_otp",
            };
            return CallGatewayPost(request, "/notifier/otp/validate");

        }
        private bool CallGatewayPost(JObject request, string url)
        {
            try
            {
                var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.POST, url, "", request).GetAwaiter().GetResult();
                if (result[CommonConst.CommonField.HTTP_RESPONE_CODE].ToString() == CommonConst.CommonField.SUCCESS_VAL.ToString())
                {
                    return true;
                }
                else
                {
                    _logger.Error($"Fail on call : {url}", null, result);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

    }
}
