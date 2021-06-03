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
        private static List<string> LogLevels = null;
        private  string ModuleExcutionType = "";
        private  string Route = "";


        public Logger(IHttpContextAccessor httpContextAccessor, IHttpContextProxy httpContextProxy, IDBService dbService)
        {
            _dbService = dbService;

            var loggerdb = CommonUtility.GetAppConfigValue("LoggerDb");
            if (string.IsNullOrEmpty(loggerdb))
            {
                loggerdb = "ZNxt_Log";

            }
          
            _dbService.Init(loggerdb);
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextProxy = httpContextProxy;
                double.TryParse(_httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.CREATED_DATA_DATE_TIME], out double starttime);
                TransactionStartTime = starttime;
                TransactionId = _httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.TRANSACTION_ID];
            }
            if (LogLevels == null)
            {
                LogLevels = new List<string>();

                var levels = CommonUtility.GetAppConfigValue("LogLevel");
                if(!string.IsNullOrEmpty(levels))
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
                JObject log = LoggerCommon(message, logData, "Debug");
                WriteLog(log);
            }
        }

        public void Error(string message, Exception ex)
        {
            if (LogLevels.Contains("Error"))
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
        }
       
        public void Error(string message, Exception ex = null, JObject logData = null)
        {
            if (LogLevels.Contains("Error"))
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
                Console.WriteLine(log.ToString());
                WriteLog(log);
            }
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
            if (LogLevels.Contains("Info"))
            {
                JObject log = LoggerCommon(message, logData, "Info");
                WriteLog(log);
            }
        }

        public void Transaction(JObject transactionData, TransactionState state)
        {
            if (LogLevels.Contains("Transaction"))
            {
                var log = LoggerCommon(string.Format("Transaction State:{0}", state.ToString()), transactionData, "Transaction");
                log[CommonConst.CommonField.TRANSACTION_STATE] = state.ToString();
                WriteLog(log);
            }
        }
        private JObject LoggerCommon(string message, JObject loginputData, string level)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                TransactionId = _httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.TRANSACTION_ID];
                LoggerName = _httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.MODULE_NAME];
                ModuleExcutionType = _httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.EXECUTE_TYPE];
                Route = _httpContextAccessor.HttpContext.Response.Headers[CommonConst.CommonField.ROUTE];
            }
            var logData = new JObject();
            logData[CommonConst.CommonField.CREATED_DATA_DATE_TIME] = DateTime.Now;
            logData[CommonConst.CommonField.DISPLAY_ID] = Guid.NewGuid().ToString();
            logData[CommonConst.CommonField.LOGGER_NAME] = LoggerName;
            logData[CommonConst.CommonField.LOG_TYPE] = level;
            logData[CommonConst.CommonField.TRANSACTION_ID] = TransactionId;
            logData[CommonConst.CommonField.LOG_MESSAGE] = message;
            logData[CommonConst.CommonField.DATA] = loginputData;
            logData[CommonConst.CommonField.TRANSACTION_ID] = TransactionId;
            logData[CommonConst.CommonField.EXECUTE_TYPE] = ModuleExcutionType;
            logData[CommonConst.CommonField.ROUTE] = Route;
            logData["AppEndpoint"] = CommonUtility.GetAppConfigValue("AppEndpoint"); ;

            return logData;
        }
        private void WriteLog(JObject logData)
        {
           // Console.WriteLine(logData.ToString());
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
