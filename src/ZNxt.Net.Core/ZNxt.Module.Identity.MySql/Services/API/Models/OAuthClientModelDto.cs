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

        public string encryption_key { get; set; }

        public string salt { get; set; }

        public List<string> roles = new List<string>();

        public List<ClientIP> ips { get; set; } = new List<ClientIP>();

    }
    public class ClientIP
    {
        public string host { get; set; }
        public string ip { get; set; }

    }
}
