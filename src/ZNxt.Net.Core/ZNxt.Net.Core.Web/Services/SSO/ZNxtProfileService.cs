using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Identity.Services
{
    public class ZNxtProfileService : IProfileService
    {
        private readonly IZNxtUserService _userService;
        private readonly ILogger _logger;
        public ZNxtProfileService(IZNxtUserService userService, ILogger logger)
        {
            _userService = userService;
            _logger = logger;
        }
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                var subjectId = context.Subject.GetSubjectId();
                _logger.Debug($"Getting user by subjectId: {subjectId}");
                var user = _userService.GetUser(subjectId);

                var claims = new List<Claim>
                 {
                    new Claim(JwtClaimTypes.Subject, user.user_id),
                };

                foreach (var item in user.claims)
                {
                    claims.Add(new Claim(item.Key, item.Value));
                }
                if (user.user_id != null)
                    claims.Add(new Claim("user_id", user.user_id));
                if (user.user_name != null)
                    claims.Add(new Claim("user_name", user.user_name));
                if (user.first_name != null)
                    claims.Add(new Claim("first_name", user.first_name));
                if (user.last_name != null)
                    claims.Add(new Claim("last_name", user.last_name));
                if (user.middle_name != null)
                    claims.Add(new Claim("middle_name", user.middle_name));
                if (user.email != null)
                    claims.Add(new Claim("email", user.email));
                if (user.user_type != null)
                    claims.Add(new Claim("user_type", user.user_type));
                claims.Add(new Claim("roles", Newtonsoft.Json.JsonConvert.SerializeObject(user.roles)));
                var tenants = Newtonsoft.Json.JsonConvert.SerializeObject(user.tenants);
                _logger.Debug($"User tenants {tenants}");
                claims.Add(new Claim(CommonConst.CommonValue.TENANT_KEY, tenants));
                context.IssuedClaims = claims;

                return Task.FromResult(0);
            }
            catch (System.Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw;
            }

        }

        public Task IsActiveAsync(IsActiveContext context)
        {

            context.IsActive = _userService.IsExists(context.Subject.GetSubjectId());
            return Task.FromResult(0);
        }
    }
}
