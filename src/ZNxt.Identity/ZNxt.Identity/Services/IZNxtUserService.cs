using System.Security.Claims;
using IdentityServer4.Test;

namespace ZNxt.Identity.Services
{
    public interface IZNxtUserService
    {
        bool CreateUser(ClaimsPrincipal subject);
        bool CreateUser(TestUser subject);
        bool IsExists(string subjectId);
    }
}