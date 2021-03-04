using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IZNxtUserService
    {
        Task<bool> CreateUserAsync(ClaimsPrincipal subject);
        Task<bool> CreateUserAsync(UserModel subject, bool sendEmail = true);
        bool CreateUser(UserModel subject, bool sendEmail = true);
        bool IsExists(string userid);
        [System.Obsolete]
        UserModel GetUserByEmail(string email);
        List<UserModel> GetUsersByEmail(string email);

        UserModel GetUser(string userid);
        UserModel GetUserByUsername(string username);
        PasswordSaltModel GetPassword(string userid);
        bool UpdateUserProfile(string userid, JObject data);

        bool AddUserRelation(string parentuserid, string childuserid, string relation);
        bool UpdateUserLoginFailCount(string userid);
        bool GetIsUserConsecutiveLoginFailLocked(string userid);
        void ResetUserLoginFailCount(string user_id);

         bool UpdateUser(string userid, JObject data);
        MobileAuthRegisterResponse RegisterMobile(MobileAuthRegisterRequest request);
        MobileAuthActivateResponse ActivateRegisterMobile(MobileAuthActivateRequest request);
       // void SetUserOrgs(UserModel userModel);
    }
}
