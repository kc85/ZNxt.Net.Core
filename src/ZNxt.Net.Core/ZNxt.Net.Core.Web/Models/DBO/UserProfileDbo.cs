using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Models.DBO
{
    [Table(IdentityTable.USER_PROFILE)]
    public class UserProfileDbo :   BaseModelDbo
    {
        [Dapper.Contrib.Extensions.Key]
        public long user_profile_id { get; set; }
        public long user_id { get; set; }
        public string phone_number { get; set; }
    }
}
