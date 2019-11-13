using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZNxt.Net.Core.Web.Quickstart.Account
{
    [Authorize]
    public class PasswordSetController:Controller
    {
        public PasswordSetController()
        {

        }

        [HttpGet]
        public IActionResult Index(RedirectViewModel model)
        {
            var viewmodel = new SetPasswordViewModel()
            {
                ReturnUrl = model.RedirectUrl
            };
            return View(viewmodel);
        }
    }
}
