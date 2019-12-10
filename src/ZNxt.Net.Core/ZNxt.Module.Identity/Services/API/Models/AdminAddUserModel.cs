using System.ComponentModel.DataAnnotations;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API.Models
{
    public class AdminAddUserModel
    {
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "user name min length 5 max length 20")]
        public string user_name { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "First name min length 3 max length 150")]
        public string first_name { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Last name min length 3 max length 150")]
        public string last_name { get; set; }

        public string middle_name { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Email min length 3 max length 150")]
        public string email { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number 10 max length 10")]
        public string mobile_number { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "whatsapp mobile number 10 max length 10")]
        public string whatsapp_mobile_number { get; set; }

        public DOBModel dob { get; set; }

        [StringLength(1, MinimumLength = 1, ErrorMessage = "Gender only one char")]

        public string gender {get;set;}

    }
}
