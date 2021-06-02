using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.MySql.Services.API.Models
{
    [Table(ClientController.OAUTH_CLIENT_IP_TABLE)]
    public class OAuthClientIpModelDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long oauth_client_ip_id { get; set; }
        public long oauth_client_id { get; set; }
        public string host_ip { get; set; }
        public string host_name { get; set; }
        public bool is_enabled { get; set; }
    }
}
