using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Module.Notifier.Services;

namespace ZNxt.Net.Core.DB.Mongo.Test
{
    [TestClass]
    public class SendSMSTest
    {
        
        [TestMethod]
        public void SendSMS()
        {

            Assert.AreEqual(true, PsbulkSMSHelper.SendSMS("hello from code", "9623946679", "dfe355e22fb62f2651c5878d7973ba6c", "http://sms.psbulksms.in/api/v2/sms/send/json", "TXTSMS", new MockLogger()));
        }
       
    }
    class MockLogger : ILogger
    {
        public string TransactionId { get; set; }

        public double TransactionStartTime { get; set; }

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
}
