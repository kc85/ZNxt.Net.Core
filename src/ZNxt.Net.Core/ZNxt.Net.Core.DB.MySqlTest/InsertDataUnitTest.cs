using Dapper.Contrib.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ZNxt.Net.Core.DB.MySql;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Helpers;

namespace ZNxt.Net.Core.DB.MySqlTest
{
    [TestClass]
    public class InsertDataUnitTest
    {
        IRDBService _rDBService;


        public InsertDataUnitTest()
        {
            Environment.SetEnvironmentVariable("MSSQLConnectionString", "Server=.\\SQLEXPRESS;Database=TEST;Trusted_Connection=True;");
            _rDBService = new SqlRDBService();
            
        }
        [TestMethod]
        public void InsertWithSQL()
        {
            _rDBService.WriteData("INSERT INTO [dbo].[tab1]([COL1]) VALUES (10000)");
        }
        [TestMethod]
        public void InsertWithModel()
        {
            _rDBService.WriteData<Event>(new Event() { Id = 1, DateCreated = DateTime.Now, EventDate = DateTime.Now.AddDays(30),EventName = "Event1" , EventLocationId = 1 });
        }
        [TestMethod]
        public void InsertListWithModel()
        {
            _rDBService.WriteData<Event>(
                new List<Event>() {
                        new Event() { Id = 2, DateCreated = DateTime.Now, EventDate = DateTime.Now.AddDays(30), EventName = "Event2", EventLocationId = 1 },
                        new Event() { Id = 3, DateCreated = DateTime.Now, EventDate = DateTime.Now.AddDays(30), EventName = "Event3", EventLocationId = 1 },
                        new Event() { Id = 4, DateCreated = DateTime.Now, EventDate = DateTime.Now.AddDays(30), EventName = "Event4", EventLocationId = 1 }

                }
                );
        }
        [TestMethod]
        public void InsertTransactionWithModel()
        {
            List<string> sqls = new List<string>()
            {
                "INSERT INTO [dbo].[tab1]([COL1]) VALUES (77777)",
                "INSERT INTO [dbo].[Event]([Id] ,[EventLocationId],[EventName])VALUES(77777, 77777, 'abc')"
            };
            Insertwithtxn(sqls);
        }
        [TestMethod]
        public void InsertTransactionRollbackWithModel()
        {
            List<string> sqls = new List<string>()
            {
                "INSERT INTO [dbo].[tab1]([COL1]) VALUES (66666)",
                "INSERT INTO [dbo].[Event]([Id] ,[EventLocationId],[EventName])VALUES(66666, 66666, 'xxx')",
                "INSERT INTO [dbo].[Event]([Id] ,[EventLocationId],[EventName])VALUES('66666XXX', 200, 'xxxx')"
            };
            Insertwithtxn(sqls);
        }

        [TestMethod]
        public void InsertWithJOBject()
        {

            JObject jdata = new JObject();
            jdata["COL1"] = 5555;

            _rDBService.WriteData(_rDBService.GetInsertSQLWithParam(jdata, "tab1"),null);
             jdata = new JObject();
            jdata["id"] = 555;
            jdata["EventLocationId"] = 555;
            jdata["EventName"] = "eV555";
            _rDBService.WriteData(_rDBService.GetInsertSQLWithParam(jdata, "event"), null);
        }

        private void Insertwithtxn(List<string> sqls)
        {
            var tran = _rDBService.BeginTransaction();
            try
            {
                foreach (var sql in sqls)
                {
                    if (!_rDBService.WriteData(sql,null, tran))
                    {
                        _rDBService.RollbackTransaction(tran);
                        return;
                    }
                }

                _rDBService.CommitTransaction(tran);
            }
            catch (Exception)
            {
                _rDBService.RollbackTransaction(tran);
                throw;
            }
        }
    }

    [Table("Event")]
    class Event
    {
        public int Id { get; set; }
        public int EventLocationId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
