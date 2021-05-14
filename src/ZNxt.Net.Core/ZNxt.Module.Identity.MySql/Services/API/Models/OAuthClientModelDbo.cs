using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.MySql.Services.API.Models
{

    [Table("oauth_client")]
    public class OAuthClientModelDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long oauth_client_id { get; set; }

        [Required]
        [Range(1000, 9999, ErrorMessage = "Client name min length 4 max length 4")]
        public long client_id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Client name min length 5 max length 20")]
        public string name { get; set; }

        public string description { get; set; }

        public string client_secret { get; set; }

        public bool is_enabled { get; set; }
    }
}
