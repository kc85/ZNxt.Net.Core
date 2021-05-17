using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Models.DBO
{
    [Table(IdentityTable.USER_ROLE)]
    public class UserRoleDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long user_role_id { get; set; }
        public long  user_id { get; set; }
        public long role_id { get; set; }
        public bool is_enabled { get; set; }
    }
}
