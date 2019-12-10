using System;
using System.Collections.Generic;

namespace ZNxt.Net.Core.Model
{
    public class UserModel
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string email { get; set; }
        [System.Obsolete("use first_name,middle_namelast_name") ]
        public string name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public DOBModel dob { get; set; }
        public string user_type { get; set; }
        public string salt { get; set; }
        public string email_validation_required { get; set; }
        public bool is_enabled { get; set; }

        [System.Obsolete]
        public string phone_validation_required { get; set; }
        public List<Claim> claims { get; set; }
        public List<string> roles { get; set; }
        public List<string> temp_roles { get; set; }
        public List<UserOrgModel> orgs { get; set; }
        public UserModel()
        {
            claims = new List<Claim>();
            roles = new List<string>();
            temp_roles = new List<string>();
            orgs = new List<UserOrgModel>();
        }
    }
    public class UserOrgModel
    {
        public string org_key { get; set; }
        public bool is_enabled { get; set; }
        public string user_id { get; set; }
        public List<Org_Group> Groups { get; set; }
    }


    public class Org_Group
    {
        public string name { get; set; }
        public string key { get; set; }
        public List<string> roles { get; set; }
    }

    public class DOBModel
    {
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public DOBModel()
        {
            day = DateTime.MinValue.Day;
            month = DateTime.MinValue.Month;
            year = DateTime.MinValue.Year;
        }
    }

}