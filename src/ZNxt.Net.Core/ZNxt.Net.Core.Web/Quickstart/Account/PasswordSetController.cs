using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZNxt.Identity.Services;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Quickstart.Account
{
    [Authorize]
    public class PasswordSetController:Controller
    {
        private readonly ZNxtUserStore _zNxtUserStore;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IZNxtUserService _userService;
        public PasswordSetController(ZNxtUserStore zNxtUserStore, IZNxtUserService userService, IHttpContextProxy httpContextProxy)
        {
            _userService = userService;
            _zNxtUserStore = zNxtUserStore;
            _httpContextProxy = httpContextProxy;
        }

        [HttpGet]
        public IActionResult Index(RedirectViewModel model)
        {
            var user = _httpContextProxy.User;
            if (user != null)
            {
                var usermodel = _userService.GetUser(user.user_id);
                if (usermodel.roles.Where(f => f == "pass_set_required").Any())
                {
                    var viewmodel = new SetPasswordViewModel()
                    {
                        ReturnUrl = model.RedirectUrl
                    };
                    SetAppName(viewmodel);
                    return View(viewmodel);
                }

            }

            return Redirect(model.RedirectUrl);
        }
        private void SetAppName(ViewModelBase vm)
        {
            ViewData["ApplicationName"] = vm.ApplicationName = "ZNxtApp";
        }
        [HttpPost]
        public async Task<IActionResult> Index(SetPasswordViewModel model)
        {
            var user = _httpContextProxy.User;

            if (user != null)
            {
                var usermodel = _userService.GetUser(user.user_id);
                if (usermodel.roles.Where(f => f == "pass_set_required").Any())
                {
                    if (model.ConfirmPassword == model.Password)
                    {
                        if (_zNxtUserStore.SetPassword(user.user_id, model.Password))
                        {

                            model.IsSuccess = true;
                            if (User?.Identity.IsAuthenticated == true)
                            {
                                // delete local authentication cookie
                                await HttpContext.SignOutAsync();
                            }
                            return View(model);
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Error while setting password");
                    SetAppName(model);
                    return View(model);
                }
                
            }
            return Redirect(model.ReturnUrl);

        }

    }
}
