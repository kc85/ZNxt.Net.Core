using System.ComponentModel.DataAnnotations;

namespace ZNxt.Module.Identity.Services.API.Models
{
    public class AdminAddUserModel
    {

        [StringLength(150, MinimumLength = 3, ErrorMessage = "Name min length 3 max length 150")]
        public string name { get; set; }
        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Email min length 3 max length 150")]
        public string email { get; set; }
        
    }
}
