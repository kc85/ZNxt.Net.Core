using Newtonsoft.Json.Linq;
using System;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Enums;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Helpers
{
    public class ResponseBuilder : IResponseBuilder
    {
        private readonly ILogger _logger;
        public ILogReader _logReader;


        public ResponseBuilder(ILogger logger, ILogReader logReader)
        {
            _logger = logger;
            _logReader = logReader;
        }
        public JObject Success(JToken data = null, JObject extraData = null)
        {
            return CreateReponse(CommonConst._1_SUCCESS, data, extraData);
        }
        public JObject BadRequest(string error)
        {
            var errorobj = new JObject();
            errorobj["error"] = error;
            return BadRequest(errorobj);
        }
        public JObject BadRequest(JToken data = null, JObject extraData = null)
        {
            return CreateReponse(CommonConst._400_BAD_REQUEST, data, extraData);
        }
        public JObject Unauthorized(JToken data = null, JObject extraData = null)
        {
            return CreateReponse(CommonConst._401_UNAUTHORIZED, data, extraData);
        }
        public JObject NotFound(JToken data = null, JObject extraData = null)
        {
            return CreateReponse(CommonConst._404_RESOURCE_NOT_FOUND, data, extraData);
        }
        public JObject ServerError(JToken data = null, JObject extraData = null)
        {
            return CreateReponse(CommonConst._500_SERVER_ERROR, data, extraData);
        }
        public JObject CreateReponse(int code, JToken data = null, JObject extraData = null)
        {
            var response = CreateResponseObject(code);
            if (extraData != null)
            {
                foreach (var item in extraData)
                {
                    response[item.Key] = item.Value;
                }
            }
            if (data != null)
            {
                response[CommonConst.CommonField.HTTP_RESPONE_DATA] = data;
            }
            AddDebugData(response);
            return response;
        }

        public JObject CreateReponse(int code)
        {
            var response = CreateResponseObject(code);
            AddDebugData(response);
            return response;
        }

        private JObject CreateResponseObject(int code)
        {
            JObject response = new JObject();
            response[CommonConst.CommonField.HTTP_RESPONE_CODE] = code;
            response[CommonConst.CommonField.HTTP_RESPONE_MESSAGE] = CommonConst.Messages[code];
            response[CommonConst.CommonField.TRANSACTION_ID] = _logger.TransactionId;
            return response;
        }

        private void AddDebugData(JObject response)
        {
            if (ApplicationMode.Maintenance == ApplicationConfig.GetApplicationMode)
            {
               
                JObject objDebugData = new JObject();
                objDebugData[CommonConst.CommonValue.TIME_SPAN] = Math.Round(CommonUtility.GetTimestampMilliseconds(DateTime.Now) - _logger.TransactionStartTime, 0);
                objDebugData[CommonConst.CommonValue.LOGS] = _logReader.GetLogs(_logger.TransactionId);
                response[CommonConst.CommonField.HTTP_RESPONE_DEBUG_INFO] = objDebugData;
            }
        }
    }
}