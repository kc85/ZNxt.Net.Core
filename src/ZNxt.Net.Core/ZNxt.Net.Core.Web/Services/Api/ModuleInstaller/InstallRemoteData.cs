using Newtonsoft.Json.Linq;
using System;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

using ZNxt.Net.Core.Helpers;
namespace ZNxt.Net.Core.Web.Services.Api.ModuleInstaller
{
    public class InstallRemoteData
    {
        private readonly IDBService _dbService;
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IDBServiceConfig _dbConfig;
        private readonly IServiceResolver _serviceResolver;
        private readonly IKeyValueStorage _keyValueStorage;
        private readonly IHttpFileUploader _httpFileUploader;
        private readonly ILogger _logger;

        public InstallRemoteData(IDBService dbService, IHttpFileUploader httpFileUploader, IKeyValueStorage keyValueStorage, IServiceResolver serviceResolver, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy, IDBServiceConfig dbConfig, ILogger logger, IRouting routing, IApiGatewayService apiGateway)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _dbConfig = dbConfig;
            _serviceResolver = serviceResolver;
            _keyValueStorage = keyValueStorage;
            _httpFileUploader = httpFileUploader;
            _logger = logger;
        }
        [Route("/ui/installpage", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject InstallPage()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                if (request == null)
                {
                    return _responseBuilder.BadRequest();
                }
                var id = request[CommonConst.CommonField.DISPLAY_ID].ToString();
                var moduleName = request[CommonConst.CommonField.MODULE_NAME].ToString();
                var path = request[CommonConst.CommonField.FILE_PATH].ToString();

                var data = request[CommonConst.CommonField.DATA].ToString();
                
                string cleanupWWWRootFilter = "{ " + CommonConst.CommonField.MODULE_NAME + ":'" + moduleName + "', "+ CommonConst.CommonField.FILE_PATH + ": '"+ path + "'}";
                foreach (var item in _dbService.Get(CommonConst.Collection.STATIC_CONTECT, new RawQuery(cleanupWWWRootFilter)))
                {
                    _keyValueStorage.Delete(CommonConst.Collection.STATIC_CONTECT, item[CommonConst.CommonField.DISPLAY_ID].ToString());
                }

                _keyValueStorage.Put<string>(CommonConst.Collection.STATIC_CONTECT, id, data);
                request.Remove(CommonConst.CommonField.DATA);
                WriteToDB(request, moduleName, CommonConst.Collection.STATIC_CONTECT, CommonConst.CommonField.FILE_PATH);
                return _responseBuilder.Success();


            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        [Route("/ui/installcollection", CommonConst.ActionMethods.POST, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject InstallCollection()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<JObject>();
                if (request == null)
                {
                    return _responseBuilder.BadRequest();
                }
                var id = request[CommonConst.CommonField.DISPLAY_ID].ToString();

                var collectionName = request[CommonConst.CommonValue.COLLECTION].ToString();
                var moduleName = request[CommonConst.CommonField.MODULE_NAME].ToString();
                request.Remove(CommonConst.CommonValue.COLLECTION);
                WriteToDB(request, moduleName, collectionName, CommonConst.CommonField.KEY);
                return _responseBuilder.Success();


            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        // TODO : Uninstall remote module 

        private void WriteToDB(JObject joData, string moduleName, string collection, string compareKey)
        {
            _dbService.OverrideData(joData, moduleName, compareKey, collection);
            if (!_dbService.Write(collection, joData))
            {
                _logger.Error(string.Format("Error while uploading file data {0}", joData.ToString()), null);
            }
        }
    }
}
