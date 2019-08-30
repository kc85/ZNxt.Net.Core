using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Services.Api.ModuleInstaller.Models;

namespace ZNxt.Net.Core.Web.Services.Api.ModuleInstaller
{
    public class ModuleInstaller
    {
        private readonly IDBService _dbService;
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly IDBServiceConfig _dbConfig;
        private readonly IServiceResolver _serviceResolver;
        private readonly IKeyValueStorage _keyValueStorage;
        private readonly IHttpFileUploader _httpFileUploader;
        private readonly ILogger _logger;
        private readonly IRouting _routing;
        private readonly IApiGatewayService _apiGateway;


        public ModuleInstaller(IDBService dbService, IHttpFileUploader httpFileUploader, IKeyValueStorage keyValueStorage, IServiceResolver serviceResolver, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy, IDBServiceConfig dbConfig,ILogger logger,IRouting routing, IApiGatewayService apiGateway)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _dbConfig = dbConfig;
            _serviceResolver = serviceResolver;
            _keyValueStorage = keyValueStorage;
            _httpFileUploader = httpFileUploader;
            _logger = logger;
            _routing = routing;
            _apiGateway = apiGateway;
        }
        [Route("/moduleinstaller/install", CommonConst.ActionMethods.POST)]
        public JObject InstallModule()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<ModuleInstallRequest>();
                if (request == null)
                {
                    return _responseBuilder.BadRequest();
                }

                JObject moduleObject = new JObject();
                moduleObject[CommonConst.CommonField.NAME] = request.Name;
                moduleObject[CommonConst.CommonField.VERSION] = request.Version;
                moduleObject[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER] = "collections";// config[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER];

                InstallWWWRoot(request);
                InstallCollections(request);
                InstallDlls(request);
                _routing.ReLoadRoutes();
                return _responseBuilder.Success();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
        private void InstallDlls(ModuleInstallRequest request)
        {

            var dllFilter = @"{name: /^lib\/netstandard2.0/, " + CommonConst.CommonField.MODULE_NAME + ": '" + request.Name + "', " + CommonConst.CommonField.VERSION + ": '" + request.Version + "'}";

            foreach (var item in _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(dllFilter)))
            {

                var fileSourceId = item[CommonConst.CommonField.DISPLAY_ID].ToString();
                var fileName = item[CommonConst.CommonField.NAME].ToString();
                var fileSize = int.Parse(item[CommonConst.CommonField.FILE_SIZE].ToString());
                var contentType = Mime.GetMimeType(fileName);
                var fileData = JObjectHelper.GetJObjectDbDataFromFile(fileName, contentType, "lib/netstandard2.0/", request.Name, fileSize);
                fileData[CommonConst.CommonField.VERSION] = request.Version;
                var id = fileData[CommonConst.CommonField.DISPLAY_ID].ToString();
                var data = _keyValueStorage.Get<string>(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, fileSourceId);
                var assembly = Assembly.Load(Convert.FromBase64String(data));
                fileData[CommonConst.CommonField.NAME] = assembly.FullName;
                WriteToDB(fileData, request.Name, CommonConst.Collection.DLLS, CommonConst.CommonField.FILE_PATH);
                InstallRoutes(request, assembly);

                _keyValueStorage.Put<string>(CommonConst.Collection.DLLS, id, data);
            }
        }

        private void InstallRoutes(ModuleInstallRequest request, Assembly assembly)
        {
            var routes = new List<RoutingModel>();
            List<Type> routeclasses = new List<Type>();

            routeclasses.AddRange(
                    assembly.GetTypes()
                                .Where(t => !t.IsAbstract)
                                 .Distinct()
                                 .ToList());

            foreach (Type routeClass in routeclasses)
            {
                System.Reflection.MemberInfo[] info = routeClass.GetMethods();
                foreach (var mi in info)
                {
                    object[] objroutes = mi.GetCustomAttributes(typeof(Route), true);
                    if (objroutes.Length != 0)
                    {
                        var r = (Route)objroutes.First();
                        routes.Add(new RoutingModel()
                        {
                            Method = r.Method,
                            Route = r.RoutePath.ToLower(),
                            ExecultAssembly = assembly.FullName,
                            ExecuteMethod = mi.Name,
                            ExecuteType = routeClass.FullName,
                            ContentType = r.ContentType,
                            auth_users = r.AuthUsers
                        });
                    }
                }
            }

            _dbService.OverrideData(new JObject() { [CommonConst.CommonField.MODULE_NAME] = request.Name }, request.Name, CommonConst.CommonField.MODULE_NAME, CommonConst.Collection.SERVER_ROUTES);

            foreach (var route in routes)
            {
                var data =  JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(route));
                data[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
                data[CommonConst.CommonField.MODULE_NAME] = request.Name;
                data[CommonConst.CommonField.VERSION] = request.Version;
                data[CommonConst.CommonField.ÌS_OVERRIDE] = false;
                data[CommonConst.CommonField.OVERRIDE_BY] = CommonConst.CommonValue.NONE;
                data[CommonConst.CommonField.KEY] = $"{route.Method}:{route.Route}";
                WriteToDB(data, request.Name, CommonConst.Collection.SERVER_ROUTES, CommonConst.CommonField.KEY);
                data[CommonConst.CommonField.MODULE_ENDPOINT] = ApplicationConfig.AppEndpoint;
                if (request.Name != "ZNxt.Net.Core.Module.Gateway")
                {
                    _apiGateway.CallAsync(CommonConst.ActionMethods.POST, "/gateway/installroute", "", data, null, ApplicationConfig.ApiGatewayEndpoint).GetAwaiter().GetResult();
                }
            }
        }
      

        private void InstallCollections(ModuleInstallRequest request)
        {
            var collectionFilter = @"{name: /^content\/collections/, " + CommonConst.CommonField.MODULE_NAME + ": '" + request.Name + "', " + CommonConst.CommonField.VERSION + ": '" + request.Version + "'}";
            
            foreach (var item in _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(collectionFilter)))
            {

                var fileSourceId = item[CommonConst.CommonField.DISPLAY_ID].ToString();
                var fileName = item[CommonConst.CommonField.NAME].ToString();
                var fileSize = int.Parse(item[CommonConst.CommonField.FILE_SIZE].ToString());
                var contentType = Mime.GetMimeType(fileName);
                var fileData = JObjectHelper.GetJObjectDbDataFromFile(fileName, contentType, "content/wwwroot", request.Name, fileSize);
                var id = fileData[CommonConst.CommonField.DISPLAY_ID].ToString();
                var collectionName = new FileInfo(fileName).Name.Replace(CommonConst.CONFIG_FILE_EXTENSION, "");

                foreach (JObject joData in JObjectHelper.GetJArrayFromString(Encoding.UTF8.GetString(Convert.FromBase64String(_keyValueStorage.Get<string>(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, fileSourceId))).Remove(0,1)))
                {
                    joData[CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID();
                    joData[CommonConst.CommonField.CREATED_DATA_DATE_TIME] = DateTime.Now;
                    joData[CommonConst.CommonField.MODULE_NAME] = request.Name;
                    joData[CommonConst.CommonField.VERSION] = request.Version;
                    joData[CommonConst.CommonField.ÌS_OVERRIDE] = false;
                    joData[CommonConst.CommonField.OVERRIDE_BY] = CommonConst.CommonValue.NONE;
                    WriteToDB(joData, request.Name, collectionName, CommonConst.CommonField.DATA_KEY);
                }
            }
        }

        private void WriteToDB(JObject joData, string moduleName, string collection, string compareKey)
        {
            _dbService.OverrideData(joData, moduleName, compareKey, collection);
            if (!_dbService.Write(collection, joData))
            {
                _logger.Error(string.Format("Error while uploading file data {0}", joData.ToString()), null);
            }
        }
       

        private void InstallWWWRoot(ModuleInstallRequest request)
        {

            var wwwrootFilter = @"{name: /^content\/wwwroot/, " + CommonConst.CommonField.MODULE_NAME + ": '" + request.Name + "', " + CommonConst.CommonField.VERSION + ": '" + request.Version + "'}";

            _dbService.OverrideData(new JObject() { [CommonConst.CommonField.MODULE_NAME] = request.Name }, request.Name, CommonConst.CommonField.MODULE_NAME, CommonConst.Collection.STATIC_CONTECT);

            foreach (var item in _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(wwwrootFilter)))
            {
                var fileSourceId = item[CommonConst.CommonField.DISPLAY_ID].ToString();
                var fileName = item[CommonConst.CommonField.NAME].ToString();
                var fileSize = int.Parse(item[CommonConst.CommonField.FILE_SIZE].ToString());
                var contentType = Mime.GetMimeType(fileName);
                var fileData = JObjectHelper.GetJObjectDbDataFromFile(fileName, contentType, "content/wwwroot", request.Name, fileSize);
                fileData[CommonConst.CommonField.VERSION] = request.Version;
                var id = fileData[CommonConst.CommonField.DISPLAY_ID].ToString();
                WriteToDB(fileData, request.Name, CommonConst.Collection.STATIC_CONTECT, CommonConst.CommonField.FILE_PATH);
                _keyValueStorage.Put<string>(CommonConst.Collection.STATIC_CONTECT, id, _keyValueStorage.Get<string>(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, fileSourceId));
            }
        }

        protected void CleanDBCollection(string moduleName, string collection)
        {
            string cleanupFilter = "{ " + CommonConst.CommonField.MODULE_NAME + ":'" + moduleName + "'}";

            _dbService.Delete(collection, new RawQuery(cleanupFilter));
        }
        private JObject GetModuleConfigFile(ModuleInstallRequest request)
        {
            //var moduleConfig = _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(new JObject()
            //{
            //    [CommonConst.CommonField.MODULE_NAME] = request.Name,
            //    [CommonConst.CommonField.VERSION] = request.Version,
            //    [CommonConst.CommonField.NAME] = "Content/module.json"
            //}.ToString())).FirstOrDefault();

            //if (moduleConfig == null)
            //{
            //    throw new Exception("Module config not found");
            //}
            var data = _keyValueStorage.Get<string>(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, request.Name);
            return JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(data)));

        }

        [Route("/moduleinstaller/upload", CommonConst.ActionMethods.POST)]
        public JObject UploadModule()
        {
            List<string> ingoreFiles = new List<string>() { "NuGet.exe" };
            if (_httpFileUploader.GetFiles().Count != 0)
            {
                string moduleName = string.Empty;
                string moduleVersion = string.Empty;
                var uploadId = CommonUtility.RandomString(19);

                using (ZipArchive zip = new ZipArchive(_httpFileUploader.GetFileStream(_httpFileUploader.GetFiles()[0])))
                {
                    foreach (var entry in zip.Entries)
                    {
                        if (ingoreFiles.Where(f => entry.FullName.IndexOf(f) != -1).Any())
                        {
                            continue;
                        }

                        using (var stream = entry.Open())
                        {
                            byte[] data = new byte[entry.Length];
                            stream.Read(data, 0, data.Length);
                            var base64String = Convert.ToBase64String(data, 0, data.Length);

                            if (entry.FullName.LastIndexOf(".nuspec") == (entry.FullName.Length - ".nuspec".Length))
                            {
                                var stringdata = System.Text.Encoding.UTF8.GetString(data);

                                XmlDocument moduleConfig = new XmlDocument();
                                moduleConfig.LoadXml(stringdata.Remove(0, stringdata.IndexOf("<")));
                                moduleName = moduleConfig.GetElementsByTagName("id").Item(0).InnerText;
                                moduleVersion = moduleConfig.GetElementsByTagName("version").Item(0).InnerText;
                            }


                            var objectkey = CommonUtility.GetNewID();
                            _dbService.Write(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE,
                           new JObject()
                           {
                               [CommonConst.CommonField.DISPLAY_ID] = objectkey,
                               [CommonConst.CommonField.TRANSACTION_ID] = uploadId,
                               [CommonConst.CommonField.NAME] = entry.FullName,
                               [CommonConst.CommonField.FILE_SIZE] = data.Length,
                           });

                            _keyValueStorage.Put<string>(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, objectkey, base64String);
                        }
                    }
                }

                // clean old files
                foreach (var fileItem in _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(new JObject()
                {
                    [CommonConst.CommonField.MODULE_NAME] = moduleName,
                    [CommonConst.CommonField.VERSION] = moduleVersion
                }.ToString())))
                {
                    DeleteFileEntry(fileItem);

                }
                if (!string.IsNullOrEmpty(moduleName) && !string.IsNullOrEmpty(moduleVersion))
                {
                    _dbService.Update(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(new JObject()
                    {
                        [CommonConst.CommonField.TRANSACTION_ID] = uploadId
                    }.ToString()),
                               new JObject()
                               {
                                   [CommonConst.CommonField.MODULE_NAME] = moduleName,
                                   [CommonConst.CommonField.VERSION] = moduleVersion,

                               });
                    return _responseBuilder.Success();
                }
                else
                {
                    foreach (var fileItem in _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(new JObject()
                    {
                        [CommonConst.CommonField.TRANSACTION_ID] = uploadId
                    }.ToString())))
                    {
                        DeleteFileEntry(fileItem);

                    }
                    return _responseBuilder.BadRequest(null, new JObject()
                    {
                        [CommonConst.CommonField.ERR_MESSAGE] = "Module configuration missing"

                    });
                }
            }
            return _responseBuilder.BadRequest();
        }

        private void DeleteFileEntry(JToken fileItem)
        {
            _keyValueStorage.Delete(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, fileItem[CommonConst.CommonField.DISPLAY_ID].ToString());
            _dbService.Delete(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(new JObject()
            {
                [CommonConst.CommonField.DISPLAY_ID] = fileItem[CommonConst.CommonField.DISPLAY_ID].ToString(),
            }.ToString()));
        }

        [Route("/moduleinstaller/uninstall", CommonConst.ActionMethods.POST)]
        public JObject UninstallModule()
        {
            try
            {
                var request = _httpContextProxy.GetRequestBody<ModuleInstallRequest>();
                if (request == null)
                {
                    return _responseBuilder.BadRequest();
                }

                JObject moduleObject = new JObject();
                moduleObject[CommonConst.CommonField.NAME] = request.Name;
                moduleObject[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER] = "collections";

                foreach (var fileItem in _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(new JObject()
                {
                    [CommonConst.CommonField.MODULE_NAME] = request.Name,

                }.ToString())))
                {
                    DeleteFileEntry(fileItem);
                }
                // UninstallWWWRoot
                CleanDBCollection(request.Name, CommonConst.Collection.STATIC_CONTECT);

                // Uninstall Dlls
                CleanDBCollection(request.Name, CommonConst.Collection.DLLS);

                // Uninstall Server routes 
                CleanDBCollection(request.Name, CommonConst.Collection.SERVER_ROUTES);

                if (request.Name != "ZNxt.Net.Core.Module.Gateway")
                {
                    _apiGateway.CallAsync(CommonConst.ActionMethods.POST, "/gateway/uninstallmodule", "", moduleObject, null, ApplicationConfig.ApiGatewayEndpoint).GetAwaiter().GetResult();
                }

                _routing.ReLoadRoutes();
                return _responseBuilder.Success();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }
    }
}
