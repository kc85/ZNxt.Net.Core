using System;
using System.Collections.Generic;
using System.Text;

namespace ZNxt.Net.Core.Web.Models
{
    public class AppTokenModel
    {
        public string oauth_client_id { get; set; }
        public string client_id { get; set; }
        public string user_id { get; set; }
        public string tenant_id { get; set; }
        public string token { get; set; }

        public DateTime created_on { get; set; } = DateTime.MinValue;
    }
}
