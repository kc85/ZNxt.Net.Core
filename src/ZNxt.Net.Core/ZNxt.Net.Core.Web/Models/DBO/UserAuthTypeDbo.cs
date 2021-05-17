using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Models.DBO
{
    [Table(IdentityTable.USER_AUTH_TYPE)]
    public class UserAuthTypeDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long user_auth_type_id { get; set; }
        public string name { get; set; }
        public bool is_enabled { get; set; }
    }

}
