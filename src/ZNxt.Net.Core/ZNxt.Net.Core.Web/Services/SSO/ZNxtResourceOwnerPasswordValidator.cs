using IdentityServer4.Models;
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
        protected readonly IAppAuthTokenHandler _appAuthTokenHandler;
        public ZNxtResourceOwnerPasswordValidator(ZNxtUserStore zNxtUserStore, IHttpContextProxy httpContextProxy, IAppAuthTokenHandler appAuthTokenHandler)
        {
            _zNxtUserStore = zNxtUserStore;
            _httpContextProxy = httpContextProxy;
            _appAuthTokenHandler = appAuthTokenHandler;
        }
        public virtual Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {

            var headers  = _httpContextProxy.GetHeaders();
            var  user = _zNxtUserStore.FindByUsername(context.UserName);
            if (user != null)
            {
                //    var validate = _zNxtUserStore.ValidateCredentials(context.UserName, context.Password, null, null);
                if (_appAuthTokenHandler.Validate(context.UserName, headers["token"]))
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
