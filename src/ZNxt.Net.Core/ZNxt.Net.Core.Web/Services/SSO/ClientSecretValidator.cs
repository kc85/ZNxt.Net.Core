using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZNxt.Net.Core.Web.Services.SSO
{
    class ClientSecretValidator : IClientSecretValidator
    {
        public Task<ClientSecretValidationResult> ValidateAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
