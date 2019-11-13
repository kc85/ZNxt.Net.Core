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
        public PasswordSetController(ZNxtUserStore zNxtUserStore, IHttpContextProxy httpContextProxy)
        {
            
            _zNxtUserStore = zNxtUserStore;
            _httpContextProxy = httpContextProxy;
        }

        [HttpGet]
        public IActionResult Index(RedirectViewModel model)
        {
            var user = _httpContextProxy.User;
            if (user != null && user.roles.Where(f => f == "pass_set_required").Any())
            {
                var viewmodel = new SetPasswordViewModel()
                {
                    ReturnUrl = model.RedirectUrl
                };
                return View(viewmodel);
            }
            else
            {
                return Redirect(model.RedirectUrl);
            }
        }
        [HttpPost]
        public IActionResult Index(SetPasswordViewModel model)
        {
            var user = _httpContextProxy.User;
            if (user != null && user.roles.Where(f => f == "pass_set_required").Any())
            {
                if (model.ConfirmPassword == model.Password)
                {
                    if (_zNxtUserStore.SetPassword(user.user_id, model.Password))
                    {
                        return RedirectToAction("PasswordCreateSuccess", "PasswordSet", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }
                }
            }
            ModelState.AddModelError(string.Empty, "Error while setting password");
            return View(model) ;
        }

        [HttpGet]
        public IActionResult PasswordCreateSuccess(RedirectViewModel model)
        {
            var user = _httpContextProxy.User;
            if (user != null && user.roles.Where(f => f == "pass_set_required").Any())
            {
                if (User?.Identity.IsAuthenticated == true)
                {
                    HttpContext.SignOutAsync().GetAwaiter().GetResult();
                }
                return View(model);
            }
            else
            {
                return Redirect(model.RedirectUrl);
            }
        }
    
    }
}
