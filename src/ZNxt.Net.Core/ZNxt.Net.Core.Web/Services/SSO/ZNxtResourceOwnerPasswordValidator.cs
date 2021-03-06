﻿using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Threading.Tasks;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Identity.Services
{
    public class ZNxtResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private ZNxtUserStore _zNxtUserStore;
        private IHttpContextProxy _httpContextProxy;
        public ZNxtResourceOwnerPasswordValidator(ZNxtUserStore zNxtUserStore, IHttpContextProxy httpContextProxy)
        {
            _zNxtUserStore = zNxtUserStore;
            _httpContextProxy = httpContextProxy;
        }
        public virtual Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {

            var headers  = _httpContextProxy.GetHeaders();
            var  user = _zNxtUserStore.FindByUsername(context.UserName);
            if (user != null && user.user_type == "mobile_auth")
            {
                var validate = _zNxtUserStore.ValidateCredentials(context.UserName, context.Password, null, null);
                if (validate)
                {
                    //set the result
                    context.Result = new GrantValidationResult(
                        subject: user.user_id.ToString(),
                        authenticationMethod: "custom"
                        //  claims: GetUserClaims(user)
                        );

                    return Task.FromResult(0);

                }
            }
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid username or password");


            return Task.FromResult(0);
        }
    }
}
