using MyModule.EComm.Api;
using MyModule.EComm.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyModule.EComm.test.Helpers
{
    public static class ControllerHelper
    {
        public static PingController GetPingController(JToken httpRequestBody = null, Dictionary<string, string> querystring = null, Dictionary<string, string> headers = null)
        {
            var logger = new LoggerMock();
            var httpProxy = CommonExtensions.GetHttpProxyMock(httpRequestBody, querystring, headers);
            var responseBuilder = new ZNxt.Net.Core.Helpers.ResponseBuilder(logger, logger);
            return new PingController(httpProxy, CommonExtensions.GetDBService(httpProxy), CommonExtensions.GetRDBService(httpProxy), logger, responseBuilder);
        }

    }
}
