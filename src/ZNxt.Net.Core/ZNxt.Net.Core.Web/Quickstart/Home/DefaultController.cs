// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZNxt.Net.Core.Helpers;

namespace IdentityServer4.Quickstart.UI
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            var page = CommonUtility.GetAppConfigValue("default_page");
            if (string.IsNullOrEmpty(page))
            {
                page = "/index.html";
            }
            return Redirect(page);
        }
    }
}