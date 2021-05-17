using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Models.DBO
{
    [Table(IdentityTable.USER)]
    public class UserModelDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long user_id { get; set; }
        public string user_name { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public long user_auth_type_id { get; set; }
        public string salt { get; set; }
        public bool is_enabled { get; set; }
    }
}
