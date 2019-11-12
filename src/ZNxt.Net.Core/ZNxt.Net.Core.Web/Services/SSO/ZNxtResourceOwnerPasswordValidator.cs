using IdentityServer4.Validation;
using System;
using System.Threading.Tasks;

namespace ZNxt.Identity.Services
{
    public class ZNxtResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
