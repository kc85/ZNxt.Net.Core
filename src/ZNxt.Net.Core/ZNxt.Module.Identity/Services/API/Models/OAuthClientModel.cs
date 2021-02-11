using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API.Models
{
    public class OAuthClientModel
    {
        public string id { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Client name min length 5 max length 20")]
        public string name { get; set; }

        //[Required]
        //[StringLength(20, MinimumLength = 10, ErrorMessage = "ClientSecret  min length 10 max length 120")]
        public string client_secret { get; set; }

        public List<string> allowed_scopes { get; set; } = new List<string>();

        public bool is_active { get; set; } = true;
    }
}
