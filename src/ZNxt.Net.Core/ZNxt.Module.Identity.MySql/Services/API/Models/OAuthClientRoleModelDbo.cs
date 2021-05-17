using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.MySql.Services.API.Models
{
    [Table(ClientController.OAUTH_CLIENT_ROLE_TABLE)]
    public class OAuthClientRoleModelDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long oauth_client_role_id { get; set; }
        public long oauth_client_id { get; set; }
        public long role_id { get; set; }
        public bool is_enabled { get; set; }
    }
}
