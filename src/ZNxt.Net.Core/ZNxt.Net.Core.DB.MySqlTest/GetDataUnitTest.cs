using Dapper.Contrib.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZNxt.Net.Core.DB.MySql;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.DB.MySqlTest
{
    [TestClass]
    public class GetDataUnitTest
    {
        IRDBService _rDBService;


        public GetDataUnitTest()
        {
            Environment.SetEnvironmentVariable("MSSQLConnectionString", "Server=.\\SQLEXPRESS;Database=TEST;Trusted_Connection=True;");
            _rDBService = new SqlRDBService();

        }
        [TestMethod]
        public void GetDataWithSQL()
        {
            var events = _rDBService.Get<Event>("Select * from event");
        }
        [TestMethod]
        public void GetDataWithSQLFilterParam()
        {
            var events = _rDBService.Get<Event>("Select * from event where [EventName] = @eventname" , new { eventname = "Event2" });
        }

        [TestMethod]
        public void GetDataWithSQLFilterParamFirst()
        {
           var e =  _rDBService.GetFirst<Event>("select * from event");
        }

       
    }
      
}
