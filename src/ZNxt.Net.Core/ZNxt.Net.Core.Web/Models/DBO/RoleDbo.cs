using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Models.DBO
{
    [Table(IdentityTable.ROLE)]
    public class RoleDbo : BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long role_id { get; set; }
        public string name { get; set; }
        public bool is_enabled { get; set; }
    }

}
