using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ZNxt.Net.Core.Consts;

namespace ZNxt.Net.Core.Model
{
   public class MobileAuthBaseRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "device_address min length 3 max length 50")]
        public string device_address { get; set; }
        
        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Invalid x_auth_token")]
        public string x_auth_token { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Invalid mobile number")]
        public string mobile_number { get; set; }


        [Required]
        [StringLength(6, MinimumLength = 3, ErrorMessage = "Invalid App Version")]
        public string app_version { get; set; }

        public Dictionary<string, string> meta_data { get; set; } = new Dictionary<string, string>();

    }
    public class MobileAuthRegisterRequest : MobileAuthBaseRequest
    {
        

    }
    public class MobileAuthActivateRequest : MobileAuthBaseRequest
    {
        [Required]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "Invaid OTP")]
        public string OTP { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Invaid validation_token")]
        public string validation_token { get; set; }
    }
    public class MobileAuthResponseBase
    {
        [JsonIgnore]
        public int code { get; set; } = CommonConst._400_BAD_REQUEST;
        [JsonIgnore]
        public List<string> errors { get; set; } = new List<string>();

    }

    public class MobileAuthRegisterResponse : MobileAuthResponseBase
    {
        public string validation_token { get; set; }

        public string device_address { get; set; }

        public string mobile_number { get; set; }

    }
    public class MobileAuthActivateResponse : MobileAuthResponseBase
    {
        public string user_id { get; set; }

        public string user_name { get; set; }

        public string secret_key { get; set; }


    }
}
