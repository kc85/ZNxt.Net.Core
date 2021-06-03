// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using ZNxt.Identity.Services;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Exceptions;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace IdentityServer4.Quickstart.UI
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ZNxtUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
       // private readonly IAppAuthTokenHandler _appAuthTokenHandler;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
          //  IAppAuthTokenHandler appAuthTokenHandler,
            ZNxtUserStore users = null)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            _users = users;

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
         //   _appAuthTokenHandler = appAuthTokenHandler;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)

        {
            var vm = await BuildLoginViewModelAsync(returnUrl);

            //if (vm.LoginUIType == CommonConst.USER_TYPE.APP_TOKEN && _appAuthTokenHandler.IsInAction())
            //{
            //    var context = await _interaction.GetAuthorizationContextAsync(vm.ReturnUrl);
            //    var token = _appAuthTokenHandler.GetTokenModel(context.Client.ClientId, vm.AppToken);
            //    if (token == null)
            //    {
            //        return Redirect(_appAuthTokenHandler.LoginFailRedirect());
            //    }
            //    else
            //    {
            //        var user = _appAuthTokenHandler.GetUser(token);
            //        if (user == null)
            //        {
            //            return Redirect(_appAuthTokenHandler.LoginFailRedirect());
            //        }
            //        await _events.RaiseAsync(new UserLoginSuccessEvent(user.user_name, user.user_id, user.GetDisplayName(), clientId: context?.Client?.ClientId));
            //        var props = new AuthenticationProperties
            //        {
            //            IsPersistent = true,
            //            ExpiresUtc = DateTimeOffset.UtcNow.Add(_appAuthTokenHandler.GetLoginDuration())
            //        };
            //        var isuser = new IdentityServerUser(user.user_id)
            //        {
            //            DisplayName = user.GetDisplayName()
            //        };
            //        await HttpContext.SignInAsync(isuser, props);
            //    }
            //}
            //else
            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }
            SetAppName(vm);
            return View(vm);
        }

        private void SetAppName(ViewModelBase vm)
        {
            ViewData["ApplicationName"] = vm.ApplicationName = ApplicationConfig.AppName;
        }

        [HttpPost("account/otplogin")]
        public async Task<IActionResult> OTPLogin(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                if (_users.ValidateCredentials(model.Username, model.Password, model.EmailOTP, model.ResetPassOTP))
                {
                    var user = _users.FindByUsername(model.Username);
                    var isuser = new IdentityServerUser(user.user_id)
                    {
                        DisplayName = user.GetDisplayName()
                    };
                    await HttpContext.SignInAsync(isuser);
                    return Ok();
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return BadRequest();
            }
        }
        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {

            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            

            try
            {


                // the user clicked the "cancel" button
                if (button == "cancel" || button == null)
                {
                    if (context != null)
                    {
                        // if the user cancels, send a result back into IdentityServer as if they 
                        // denied the consent (even if this client does not require consent).
                        // this will send back an access denied OIDC error response to the client.
                        await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        if (await _clientStore.IsPkceClientAsync(context?.Client.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                        }

                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        // since we don't have a valid context, then we just go back to the home page
                        return Redirect("~/");
                    }
                }

                if (ModelState.IsValid)
                {
                    // validate username/password against in-memory store
                    if (_users.ValidateCredentials(model.Username, model.Password, model.EmailOTP, model.ResetPassOTP))
                    {
                        var user = _users.FindByUsername(model.Username);
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.user_name, user.user_id, user.GetDisplayName(), clientId: context?.Client?.ClientId));

                        // only set explicit expiration here if user chooses "remember me". 
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties props = null;
                        if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                        {
                            props = new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        };


                        var isuser = new IdentityServerUser(user.user_id)
                        {
                            DisplayName = user.GetDisplayName()
                        };
                        // issue authentication cookie with subject ID and username
                        await HttpContext.SignInAsync(isuser, props);
                        var passsetview = IsPasswordSetRequired(user, model);
                        if (passsetview != null)
                        {
                            return passsetview;

                        }
                        if (context != null)
                        {
                            if (await _clientStore.IsPkceClientAsync(context.Client.ClientId))
                            {
                                // if the client is PKCE then we assume it's native, so this change in how to
                                // return the response is for better UX for the end user.
                                return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                            }

                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            return Redirect(model.ReturnUrl);
                        }
                        // request for a local page
                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                        else
                        {
                            // user might have clicked on a malicious link - should be logged
                            throw new Exception("invalid return URL");
                        }

                    }

                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client?.ClientId));
                    ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                }
            }
            catch (UserConsecutiveLoginFailLockException ex)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "User Consecutive Logined Fail Locked", clientId: context?.Client?.ClientId));
                ModelState.AddModelError(string.Empty, $"{AccountOptions.UserConsecutiveLoginFailLock}. Please try after {Math.Round((ex.Duration) / 1000 / 60)} minutes");
            }
            catch(UserConsecutiveLoginFailLockCountException ex)
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client?.ClientId));
                var message = $"{ex.RemainingCount} {AccountOptions.UserConsecutiveLoginBeforeLock}";
                if(ex.RemainingCount == 1)
                {
                    message = AccountOptions.UserConsecutiveLoginBeforeLastAttempt;
                }
                ModelState.AddModelError(string.Empty, message);

            }

            // something went wrong, show form with error

            var vm = await BuildLoginViewModelAsync(model);
            SetAppName(vm);
            return View(vm);
        }

        private IActionResult IsPasswordSetRequired(UserModel user, LoginInputModel model)
        {
            if (user.roles.Where(f => f == "pass_set_required").Any())
            {
                return RedirectToAction("index", "PasswordSet", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
            }
            return null;
        }


        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }
            SetAppName(vm);
            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }
            SetAppName(vm);
            return View("LoggedOut", vm);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                    LoginUIType = context.Parameters["login_ui_type"] !=null ? context.Parameters["login_ui_type"]  : "",
                    AppToken = context.Parameters["app_token"] != null ? context.Parameters["app_token"] : "",

                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context?.Client?.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray(),
                LoginUIType = (context != null ?( context.Parameters["login_ui_type"] != null ? context.Parameters["login_ui_type"] : ""): ""),
                AppToken = (context!=null ? (context.Parameters["app_token"] != null ? context.Parameters["app_token"] : ""): "")


            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        [HttpGet]
        public IActionResult ForgetUsername(string returnUrl)

        {
            // build a model so we know what to show on the login page
            var vm = new ViewModelBase();
            SetAppName(vm);
            return View(vm);
        }
        [HttpGet]
        public IActionResult ForgetPassword(string returnUrl)

        {
            // build a model so we know what to show on the login page
            var vm = new LoginViewModel();
            SetAppName(vm);
            return View(vm);
        }
    }
}