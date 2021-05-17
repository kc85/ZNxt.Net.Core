using System;
using System.Collections.Generic;
using System.Text;

namespace ZNxt.Module.Identity.MySql.Services.API.Models
{
    public class OAuthClientModelDto
    {
        public string client_id { get; set; }

        public string description { get; set; }

        public string client_secret { get; set; }

        public long tenant_id { get; set; }

        public string salt { get; set; }

        public List<string> roles = new List<string>();

    }
}
