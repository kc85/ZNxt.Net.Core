using System;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Module.MyModule1.Helpers
{
    public class LoggerMock : ZNxt.Net.Core.Interfaces.ILogger, ZNxt.Net.Core.Interfaces.ILogReader
    {
        public string TransactionId => "tx-ut-1234";

        public double TransactionStartTime => CommonUtility.GetTimestampMilliseconds(DateTime.Now);

        public void Debug(string message, JObject logData = null)
        {
            Write(message, logData);
        }

        private void Write(string message, JObject logData)
        {
            Console.WriteLine($"{message}{ (logData != null ? logData.ToString() : "")}");
        }

        public void Error(string message, Exception ex)
        {
            Write(message, null);
        }

        public void Error(string message, Exception ex = null, JObject logData = null)
        {
            Write(message, logData);
        }

        public void Info(string message, JObject logData = null)
        {
            Write(message, logData);
        }

        public void Transaction(JObject transactionData, TransactionState state)
        {
            throw new NotImplementedException();
        }

        public JArray GetLogs(string transactionId)
        {
            return new JArray();
        }
    }
}
