using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ZNxt.Net.Core.Model;

namespace ZNxt.Identity.Services
{
    public class ZNxtUserStore 
    {
        private readonly IZNxtUserService _userService;
        private readonly IZNxtUserService _ZNxtUserService;

        public ZNxtUserStore(IZNxtUserService userService, IZNxtUserService ZNxtUserService)
        {
            _userService = userService;
            _ZNxtUserService = ZNxtUserService;
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
            await _ZNxtUserService.CreateUserAsync(user);
            return user;
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
            return _userService.GetUser(username);
        }
       
        public bool ValidateCredentials(string username, string password)
        {
            var user =  _userService.GetUser(username);
            if (user != null)
            {
                var pass = _userService.GetPassword(user.user_id);
                if (pass != null)
                {
                    var passwordwithsalt = $"{password}{user.salt}";
                    if (pass.Password.Equals(Sha256Hash(passwordwithsalt)))
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
            else
            {
                return false;
            }
        }

        private string Sha256Hash(string value)
        {
          var data =   (System.Security.Cryptography.SHA256.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(value))
                    .Select(item => item.ToString("x2")));

            return data.First();
        }

    }
}
