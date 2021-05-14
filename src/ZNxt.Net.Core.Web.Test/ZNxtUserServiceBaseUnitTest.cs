using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZNxt.Identity.Services;
using ZNxt.Net.Core.DB.MySql;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Models.DBO;
using ZNxt.Net.Core.Web.Services;

namespace ZNxt.Net.Core.Web.Test
{
    [TestClass]
    public class ZNxtUserServiceBaseUnitTest
    {
        IRDBService _rDBService;
        IZNxtUserService _zNxtUserService;

        public ZNxtUserServiceBaseUnitTest()
        {
            Environment.SetEnvironmentVariable("NPGSQLConnectionString", "Host=103.212.120.203;Username=root;Password=root;Database=identity;");
            _rDBService = new SqlRDBService();

            _zNxtUserService = new ZNxtUserRDBService(_rDBService, new MockUserNotifierService(), new MockLogger(), new MockApiGatewayService(), new MockTenantSetterService(), new MockinMemoryCacheService());

        }


        [TestMethod]
        public void CreateUser()
        {
            var result =  _zNxtUserService.CreateUser(new UserModel()
            {
                user_name = "sys_admin_01",
                email = "sys_gmai@g.com",
                first_name = "abc",
                middle_name ="xyx",
                last_name = "test",
                salt = "äaa",
                roles = new List<string>() { "sys_admin" },
                user_type = "userpass",
                is_enabled= true
            });
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void GetUser()
        {
            var result = _zNxtUserService.GetUser("7");
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void CreatePassword()
        {
            var result = (_zNxtUserService as ZNxtUserRDBService).CreatePassword("7", "abc@12346");
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void UpdateUserLoginFailCount()
        {
            var result = (_zNxtUserService).UpdateUserLoginFailCount("7");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateUserLoginFailCount_lockuser()
        {
            for (int i = 0; i < 5; i++)
            {
                var result = (_zNxtUserService).UpdateUserLoginFailCount("7");
                Assert.IsTrue(result);
            }
         
        }
        [TestMethod]
        public void UpdateUserLoginFailCount_isuser_locked()
        {
            for (int i = 0; i < 5; i++)
            {
                var result1 = (_zNxtUserService).UpdateUserLoginFailCount("7");
                Assert.IsTrue(result1);
            }
            var result3 = (_zNxtUserService).UpdateUserLoginFailCount("7");
            var result = (_zNxtUserService).GetIsUserConsecutiveLoginFailLocked("7");
            Assert.IsTrue(result);
            (_zNxtUserService).ResetUserLoginFailCount("7");
            var result4 = (_zNxtUserService).GetIsUserConsecutiveLoginFailLocked("7");
            Assert.IsTrue(!result4);

        }
    }


    class MockinMemoryCacheService : IInMemoryCacheService
    {
        public T Get<T>(string key)
        {
            return default(T);
        }

        public void Put<T>(string key, T data, int duration = 10)
        {
           
        }
    }

    class MockTenantSetterService : ITenantSetterService
    {
        public bool AddUserToTenant(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public void SetTenant(UserModel userModel)
        {
          
        }
    }
    class MockApiGatewayService : IApiGatewayService
    {
        public Task<JObject> CallAsync(string Method, string route, string querystring = "", JObject requestBody = null, Dictionary<string, string> headres = null, string baseUrl = "")
        {
            return Task.FromResult(new JObject());
        }

        public Task<RoutingModel> GetRouteAsync(string method, string route)
        {
            return Task.FromResult(new RoutingModel());
        }
    
    }
    class MockLogger : ILogger
    {
        public string TransactionId => "111";

        public double TransactionStartTime => 111;

        public void Debug(string message, JObject logData = null)
        {
        }

        public void Error(string message, Exception ex)
        {
        }

        public void Error(string message, Exception ex = null, JObject logData = null)
        {
        }

        public void Info(string message, JObject logData = null)
        {
        }

        public void Transaction(JObject transactionData, TransactionState state)
        {
        }
    }
    class MockUserNotifierService : IUserNotifierService
    {
        public Task<bool> SendForgetpasswordEmailWithOTPAsync(UserModel user, string message = null)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendForgetUsernamesEmailAsync(List<UserModel> user, string message = null)
        {
            return Task.FromResult(true); 
        }

        public Task<bool> SendMobileAuthRegistrationOTPAsync(MobileAuthRegisterResponse mobileAuth, string message = null)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendWelcomeEmailAsync(UserModel user, string message = null)
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendWelcomeEmailWithOTPLoginAsync(UserModel user, string message = null)
        {
            return Task.FromResult(true);
        }
    }

} 
