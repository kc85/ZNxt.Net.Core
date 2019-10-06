using System.Security.Claims;
using IdentityServer4.Test;
using ZNxt.Identity.Models;
using ZNxt.Net.Core.Model;

namespace ZNxt.Identity.Services
{
    public interface IZNxtUserService
    {
        bool CreateUser(ClaimsPrincipal subject);
        bool CreateUser(UserModel subject);
        bool IsExists(string userid);

        UserModel GetUser(string userid);
        PasswordSaltModel GetPassword(string userid);
    }
}