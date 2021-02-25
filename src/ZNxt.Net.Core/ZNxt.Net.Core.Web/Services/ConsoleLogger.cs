using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public class ConsoleLogger : ILogger, ILogReader
    {
        public string TransactionId { get; private set; }

        public double TransactionStartTime { get; private set; }
        private static List<string> LogLevels = null;
        public ConsoleLogger(IHttpContextAccessor httpContextAccessor, IHttpContextProxy httpContextProxy)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                double.TryParse(httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.CREATED_DATA_DATE_TIME], out double starttime);
                TransactionStartTime = starttime;
                TransactionId = httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.TRANSACTION_ID];
            }
            if (LogLevels == null)
            {
                LogLevels = new List<string>();

                var levels = CommonUtility.GetAppConfigValue("LogLevel");
                if (!string.IsNullOrEmpty(levels))
                {
                    LogLevels.AddRange(levels.Split(",").Select(f => f.Trim()).ToList());
                }
                else
                {
                    LogLevels.Add("Error");
                }
            }
        }
        public void Debug(string message, JObject logData = null)
        {
            if (LogLevels.Contains("Debug"))
            {
                WriteLog(message, logData);
            }
        }
        private string GetFullMessage(Exception ex)
        {
            if (ex != null)
            {
                return ex.InnerException == null
                     ? ex.Message
                     : ex.Message + " --> " + GetFullMessage(ex.InnerException);
            }
            else
            {
                return "";
            }
        }
        private void WriteLog(string message, JObject logData, Exception ex = null)
        {
            if(logData == null) { logData = new JObject(); }
            Console.WriteLine($"message: {message}, logdata : {logData }, { GetFullMessage(ex) }");
        }

        public void Error(string message, Exception ex)
        {

            if (LogLevels.Contains("Error"))
            {
                WriteLog(message, null, ex);
            }
        }

        public void Error(string message, Exception ex = null, JObject logData = null)
        {
            if (LogLevels.Contains("Error"))
            {
                WriteLog(message, logData, ex);
            }
        }

        public JArray GetLogs(string transactionId)
        {
            return new JArray();
        }

        public void Info(string message, JObject logData = null)
        {
            if (LogLevels.Contains("Info"))
            {
                WriteLog(message, logData);
            }
        }

        public void Transaction(JObject transactionData, TransactionState state)
        {
            if (LogLevels.Contains("Transaction"))
            {
                WriteLog("Transaction", transactionData);
            }
        }
    }
}
