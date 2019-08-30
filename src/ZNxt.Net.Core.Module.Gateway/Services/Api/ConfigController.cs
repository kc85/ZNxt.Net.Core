using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Helpers;
using System.Linq;


namespace ZNxt.Net.Core.Module.Gateway.Services.Api
{
    public class ConfigController
    {
        private const string GATEWAY_CONFIG_COLLECTION = "gateway_config";
        private const string GATEWAY_ROUTE_COLLECTION = "gateway_server_route";
        private const string GATEWAY_MODULE_ENDPOINT = "gateway_module_endpoint";
        private readonly IResponseBuilder _responseBuilder;
        private readonly IDBService _dbService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;

        public ConfigController(IResponseBuilder responseBuilder,IDBService dbService, IHttpContextProxy httpContextProxy, ILogger logger)
        {

            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
        }
        [Route("/gateway/config", CommonConst.ActionMethods.GET)]
        public JObject GetConfig()
        {
            var data = _dbService.Get(GATEWAY_CONFIG_COLLECTION, new RawQuery("{'isactive' : true, 'is_override': false } "), new List<string>() { "key", "data" });
            return _responseBuilder.Success(data);
        }

        [Route("/gateway/routes", CommonConst.ActionMethods.GET)]
        public JObject GetRoute()
        {

            var routeData = _dbService.Get(GATEWAY_ROUTE_COLLECTION, new RawQuery("{ 'is_override': false } "), new List<string>() { CommonConst.CommonField.MODULE_NAME, "Route", "Method", "key" });
            var moduleEndpoint = _dbService.Get(GATEWAY_MODULE_ENDPOINT, new RawQuery("{ 'is_override': false } "), new List<string>() { CommonConst.CommonField.MODULE_NAME, CommonConst.CommonField.MODULE_ENDPOINT });

            foreach (var route in routeData)
            {
                var moduleName = route[CommonConst.CommonField.MODULE_NAME].ToString() ;
                var endpoint = moduleEndpoint.FirstOrDefault(f => f[CommonConst.CommonField.MODULE_NAME].ToString() == moduleName);
                if (endpoint != null)
                {
                    route[CommonConst.CommonField.MODULE_ENDPOINT] = endpoint[CommonConst.CommonField.MODULE_ENDPOINT];
                }
            }
            return _responseBuilder.Success(routeData);

        }
        [Route("/gateway/installroute", CommonConst.ActionMethods.POST)]
        public JObject InstallRoute()
        {
            var routeObj = _httpContextProxy.GetRequestBody<JObject>();
            var endpoint = routeObj[CommonConst.CommonField.MODULE_ENDPOINT];
            var moduleName = routeObj[CommonConst.CommonField.MODULE_NAME];
            if (endpoint == null || moduleName == null)
            {
                _responseBuilder.BadRequest("module_name or module_endpoint missing");
            }
            routeObj.Remove(CommonConst.CommonField.MODULE_ENDPOINT);

            WriteToDB(routeObj, moduleName.ToString(), GATEWAY_ROUTE_COLLECTION, CommonConst.CommonField.KEY);

            var moduleEndPountData = new JObject()
            {
                [CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID(),
                [CommonConst.CommonField.MODULE_ENDPOINT] = endpoint,
                [CommonConst.CommonField.MODULE_NAME] = moduleName,
                [CommonConst.CommonField.ÌS_OVERRIDE] = false

            };
            WriteToDB(moduleEndPountData, moduleName.ToString(), GATEWAY_MODULE_ENDPOINT, CommonConst.CommonField.MODULE_NAME);

            return _responseBuilder.Success();
        }

        [Route("/gateway/uninstallmodule", CommonConst.ActionMethods.POST)]
        public JObject UninstallRoute()
        {
            var routeObj = _httpContextProxy.GetRequestBody<JObject>();
            var moduleName = routeObj[CommonConst.CommonField.NAME];
            if (moduleName == null)
            {
                _responseBuilder.BadRequest("module_name missing");
            }
            CleanDBCollection(moduleName.ToString(), GATEWAY_ROUTE_COLLECTION);
            CleanDBCollection(moduleName.ToString(), GATEWAY_MODULE_ENDPOINT);

            return _responseBuilder.Success();
        }
        protected void CleanDBCollection(string moduleName, string collection)
        {
            string cleanupFilter = "{ " + CommonConst.CommonField.MODULE_NAME + ":'" + moduleName + "'}";

            _dbService.Delete(collection, new RawQuery(cleanupFilter));
        }
        private void WriteToDB(JObject joData, string moduleName, string collection, string compareKey)
        {
            _dbService.OverrideData(joData, moduleName, compareKey, collection);
            if (!_dbService.Write(collection, joData))
            {
                _logger.Error(string.Format("Error while uploading data {0}", joData.ToString()), null);
            }
        }
    }
}
