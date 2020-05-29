using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ZNxt.Module.Identity.Services.API.Models
{
    public class MobileAuthAcceptTnCRequestModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "device_address min length 3 max length 50")]
        public string  device_address{ get; set; }
        public Dictionary<string, string> meta_data { get; set; } = new Dictionary<string, string>();
    }
}
