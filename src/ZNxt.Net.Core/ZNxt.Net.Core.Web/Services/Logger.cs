using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services
{
    public class Logger : ILogger, ILogReader
    {
        public string TransactionId { get; private set; }
        private static readonly object lockObjet = new object();
        public double TransactionStartTime { get; private set; }
        public string LoggerName { get; set; }
        public string RouteData { get; set; }

        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDBService _dbService;

        public Logger(IHttpContextAccessor httpContextAccessor, IHttpContextProxy httpContextProxy, IDBService dbService)
        {
            _dbService = dbService;
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextProxy = httpContextProxy;
                double.TryParse(_httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.CREATED_DATA_DATE_TIME], out double starttime);
                TransactionStartTime = starttime;

            }
        }

        public void Debug(string message, JObject logData = null)
        {
            JObject log = LoggerCommon(message, logData, "Debug");
            WriteLog(log);
        }

        public void Error(string message, Exception ex)
        {
            JObject logData = null;
            if (_httpContextProxy != null)
            {
                logData = new JObject()
                {
                    ["RequestBody"] = _httpContextProxy.GetRequestBody(),
                    ["RequestUrl"] = _httpContextProxy.GetURIAbsolutePath(),
                    ["RequestQueryString"] = _httpContextProxy.GetQueryString()
                };
                logData["RequestHeader"] = string.Join(";", _httpContextProxy.GetHeaders().Select(f => string.Format("{0}:{1}", f.Key, f.Value)));
            }
            Error(message, ex, logData);
        }
       
        public void Error(string message, Exception ex = null, JObject logData = null)
        {
            if (logData == null)
            {
                logData = new JObject();
            }
            JObject log = LoggerCommon(message, logData, "Error");
            if (ex != null)
            {
                log[CommonConst.CommonField.ERR_MESSAGE] = ex.Message;
                log[CommonConst.CommonField.STACKTRACE] = ex.StackTrace.ToString();
                log[CommonConst.CommonField.ERR_DETAILS] = GetFullMessage(ex);
            }
            log[CommonConst.CommonField.USER] = GetUserDetails();
            WriteLog(log);
        }

        public JArray GetLogs(string transactionId)
        {
            if (_dbService.IsConnected)
                return _dbService.Get(CommonConst.Collection.SERVER_LOGS, new RawQuery("{'" + CommonConst.CommonField.TRANSACTION_ID + "' : '" + this.TransactionId + "'}"));
            else
                return new JArray();
        }

        public void Info(string message, JObject logData = null)
        {
            JObject log = LoggerCommon(message, logData, "Info" );
            WriteLog(log);
        }

        public void Transaction(JObject transactionData, TransactionState state)
        {
            var log = LoggerCommon(string.Format("Transaction State:{0}", state.ToString()), transactionData, "Transaction");
            log[CommonConst.CommonField.TRANSACTION_STATE] = state.ToString();
            WriteLog(log);
        }
        private JObject LoggerCommon(string message, JObject loginputData, string level)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                TransactionId = _httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.TRANSACTION_ID];
                LoggerName = _httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.MODULE_NAME];
            }
            var logData = new JObject();
            logData[CommonConst.CommonField.CREATED_DATA_DATE_TIME] = DateTime.Now;
            logData[CommonConst.CommonField.DISPLAY_ID] = Guid.NewGuid().ToString();
            logData[CommonConst.CommonField.LOGGER_NAME] = LoggerName;
            logData[CommonConst.CommonField.LOG_TYPE] = level;
            logData[CommonConst.CommonField.TRANSACTION_ID] = TransactionId;
            logData[CommonConst.CommonField.LOG_MESSAGE] = message;
            logData[CommonConst.CommonField.DATA] = loginputData;
            return logData;
        }
        private void WriteLog(JObject logData)
        {
            Console.WriteLine(logData.ToString());
            lock (lockObjet)
            {
                if (_dbService.IsConnected)
                {
                    _dbService.WriteData(CommonConst.Collection.SERVER_LOGS, logData);
                }
            }
        }
        private string GetFullMessage(Exception ex)
        {
            return ex.InnerException == null
                 ? ex.Message
                 : ex.Message + " --> " + GetFullMessage(ex.InnerException);
        }
        public JObject GetUserDetails()
        {
            if (_httpContextProxy!=null && _httpContextProxy.User != null)
            {   
                return JObject.Parse(JsonConvert.SerializeObject(_httpContextProxy.User));
            }
            return new JObject();
        }
    }
}
