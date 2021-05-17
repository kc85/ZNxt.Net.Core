using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZNxt.Net.Core.Web.Models.DBO
{
    [Table(IdentityTable.USER_LOGIN_HISTORY)]
    public class UserLoginHistoryDbo
    {

        [Dapper.Contrib.Extensions.Key]
        public long user_login_history_id { get; set; }
        public long user_id { get; set; }
        public bool is_success { get; set; }
        public long created_on { get; set; }
        public string note { get; set; }
        public UserLoginHistoryDbo()
        {
            created_on = ZNxt.Net.Core.Helpers.CommonUtility.GetUnixTimestamp(DateTime.UtcNow);
        }
    }
}
