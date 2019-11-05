using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ZNxt.Identity.Services
{
    public class ZNxtProfileService : IProfileService
    {
        private readonly IZNxtUserService _userService;
        public ZNxtProfileService(IZNxtUserService userService)
        {
            _userService = userService;
        }
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();

            var user = _userService.GetUser(subjectId);

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, user.user_id),
            };

            foreach (var item in user.claims)
            {
                claims.Add( new Claim(item.Key, item.Value));
            }
            claims.Add(new Claim("roles", Newtonsoft.Json.JsonConvert.SerializeObject(user.roles)));
            context.IssuedClaims = claims;

            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {

            context.IsActive = _userService.IsExists(context.Subject.GetSubjectId());
            return Task.FromResult(0);
        }
    }
}
