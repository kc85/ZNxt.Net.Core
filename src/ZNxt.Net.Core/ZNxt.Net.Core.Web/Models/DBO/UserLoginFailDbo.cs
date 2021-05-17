using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Models.DBO
{
    [Table(IdentityTable.USER_LOGIN_FAIL_LOCK)]
    public class UserLoginFailDbo : BaseModelDbo
    {

        [Dapper.Contrib.Extensions.Key]
        public long user_login_fail_lock_id { get; set; }
        public long user_id { get; set; }
        public int count { get; set; }
        public bool is_locked { get; set; }
        public double lock_start_time { get; set; }
        public double lock_end_time { get; set; }
        public long lock_type { get; set; }
        public UserLoginFailDbo()
        {
            lock_type = 1;
        }
    }
}
