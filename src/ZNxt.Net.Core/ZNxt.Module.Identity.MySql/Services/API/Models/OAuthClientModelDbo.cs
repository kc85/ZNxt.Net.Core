using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.MySql.Services.API.Models
{

    [Table(ClientController.OAUTH_CLIENT_TABLE)]
    public class OAuthClientModelDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long oauth_client_id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "client_id  min length 5 max length 20")]
        public string client_id { get; set; }

        public string description { get; set; }
        public string salt { get; set; }
        public long tenant_id { get; set; }
        public string client_secret { get; set; }
        public string encryption_key { get; set; }

        public bool is_enabled { get; set; }
    }
}
