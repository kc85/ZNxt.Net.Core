using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Test;
using ZNxt.Identity.Models;
using ZNxt.Net.Core.Model;

namespace ZNxt.Identity.Services
{
    public interface IZNxtUserService
    {
        Task<bool> CreateUserAsync(ClaimsPrincipal subject);
        Task<bool> CreateUserAsync(UserModel subject);
        bool IsExists(string userid);

        UserModel GetUser(string userid);
        PasswordSaltModel GetPassword(string userid);
    }
}