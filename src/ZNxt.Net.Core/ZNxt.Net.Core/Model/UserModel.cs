﻿using System.Collections.Generic;

namespace ZNxt.Net.Core.Model
{
    public class UserModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string user_type { get; set; }
        public string salt { get; set; }

        public string email_validation_required { get; set; }
        public string phone_validation_required { get; set; }
        public List<Claim> claims { get; set; }

        public UserModel()
        {
            claims = new List<Claim>();
        }
    }
}