using Dapper.Contrib.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ZNxt.Net.Core.DB.MySql;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Helpers;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace ZNxt.Net.Core.DB.MySqlTest
{
    [TestClass]
    public class InsertDataPGDBUnitTest
    {
        IRDBService _rDBService;


        public InsertDataPGDBUnitTest()
        {
            Environment.SetEnvironmentVariable("NPGSQLConnectionString", "Host=103.212.120.XXX;Username=root;Password=root;Database=mvp_tenant_mgt");
            _rDBService = new SqlRDBService(new HttpProxyMock());
        }
        [TestMethod]
        public void InsertWithSQL()
        {
            _rDBService.WriteData("insert into public.test (name) values('abc');", null);
        }
        [TestMethod]
        public void InsertWithModel()
        {
          var d =   _rDBService.WriteData<TenantRequestModel>(new TenantRequestModel() { name = "test", description = "test", email= "wmail@email.com", phone_number = "9999999999" });
        }
        [TestMethod]
        public void GetData()
        {
            var d = _rDBService.Get<TenantRequestModel>("tenant", 10,0,"");
        }
        [TestMethod]
        public void GetDataWithFilterJObject()
        {
            var d = _rDBService.Get<TenantRequestModel>("tenant", 10, 0, new JObject() { ["id"] = "1" });
        }

        [TestMethod]
        public void GetDataWithFilterString()
        {
            var d = _rDBService.Get<TenantRequestModel>("tenant", 10, 0, "id='1'");
        }


        [TestMethod]
        public void GetCount()
        {
            var d = _rDBService.GetCount("tenant",string.Empty);
        }

        [TestMethod]
        public void GetCountWithFilter()
        {
            var d = _rDBService.GetCount("tenant", new JObject() { ["id"] = "1"});
        }

    }

    [Table("tenant")]
    public class TenantRequestModel
    {

        [Dapper.Contrib.Extensions.Key]
        public long id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Tenant Name min length 5 max length 150")]
        public string name { get; set; }
        [Required]
        public TenantStatus status { get; set; }
        [Required]
        public string description { get; set; }

        //[Required]
        //[StringLength(10, MinimumLength = 2, ErrorMessage = "Tenant short name min length 2 max length 10")]
        //public string shortname { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Email min length 5 max 150")]
        public string email { get; set; }
        public string phone_number { get; set; }
    }
    public enum TenantStatus
    {
        active = 1,
        inactive = 2,
        suspended = 3
    }


}
