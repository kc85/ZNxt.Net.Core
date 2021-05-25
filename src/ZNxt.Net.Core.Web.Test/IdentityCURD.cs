using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ZNxt.Net.Core.DB.MySql;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Web.Models.DBO;

namespace ZNxt.Net.Core.Web.Test
{
    [TestClass]
    public class IdentityCURD
    {
        IRDBService _rDBService;

        public IdentityCURD()
        {
            Environment.SetEnvironmentVariable("NPGSQLConnectionString", "Host=103.212.120.XXX;Username=root;Password=root;Database=identity;");
            _rDBService = new SqlRDBService();

        }
        [TestMethod]
        public void InsertRole()
        {
           var response =  _rDBService.WriteData<RoleDbo>(new RoleDbo()
            {
               is_enabled = true,
               name = "sys_admin"

            });

        }

        [TestMethod]
        public void InsertUserAuthType()
        {
            var response = _rDBService.WriteData<UserAuthTypeDbo>(new UserAuthTypeDbo()
            {
                is_enabled = true,
                name = "userpass"
            });

        }


        [TestMethod]
        public void InsertUser()
        {
            var response = _rDBService.WriteData<UserModelDbo>(new UserModelDbo()
            {
                first_name= "sys",
                last_name = "admin",
                user_name = "sys_admin",
                email = "sys_admin@a.com",
                is_enabled = true,
                salt = "27896dj",
                user_auth_type_id = 1
            });

        }
        [TestMethod]
        public void InsertPass()
        {
            var response = _rDBService.WriteData<UserPasswordDbo>(new UserPasswordDbo()
            {
                user_id = 1,
                is_enabled = true,
                password = "abc"

            });

        }
        [TestMethod]
        public void InsertUserProfile()
        {
            var response = _rDBService.WriteData<UserProfileDbo>(new UserProfileDbo()
            {
                user_id = 1,
                phone_number = "9943949494"
            });

        }

        [TestMethod]
        public void InsertUserRole()
        {
            var response = _rDBService.WriteData<UserRoleDbo>(new UserRoleDbo()
            {
                user_id = 1,
                role_id = 1,
                is_enabled = true
            });
        }

        [TestMethod]
        public void InsertLoginHistory()
        {
            var response = _rDBService.WriteData<UserLoginHistoryDbo>(new UserLoginHistoryDbo()
            {
                user_id = 1,
              is_success = true,
              note ="login to syste from chrome",

            });

        }

        [TestMethod]
        public void InsertLoginFail()
        {
            var response = _rDBService.WriteData<UserLoginFailDbo>(new UserLoginFailDbo()
            {
                user_id = 1,
               count = 1,
               is_locked = false
               
            });

        }
    }
}
