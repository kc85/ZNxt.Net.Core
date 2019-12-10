﻿using Newtonsoft.Json.Linq;
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
        UserModel GetUserByEmail(string email);

        UserModel GetUser(string userid);
        UserModel GetUserByUsername(string username);
        PasswordSaltModel GetPassword(string userid);
        bool UpdateUserProfile(string userid, JObject data);

    }
}
