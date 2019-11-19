using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IdentityServer4.Quickstart.UI
{
    public class SetPasswordViewModel: ViewModelBase
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsSuccess { get; set; }

    }
}
