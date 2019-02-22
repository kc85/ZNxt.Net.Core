using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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

        public ModuleInstaller(IDBService dbService, IHttpFileUploader httpFileUploader, IKeyValueStorage keyValueStorage, IServiceResolver serviceResolver, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy, IDBServiceConfig dbConfig)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _dbConfig = dbConfig;
            _serviceResolver = serviceResolver;
            _keyValueStorage = keyValueStorage;
            _httpFileUploader = httpFileUploader;
        }
        [Route("/moduleinstaller/install", CommonConst.ActionMethods.POST)]
        public JObject InstallModule()
        {
            var request = _httpContextProxy.GetRequestBody<ModuleInstallRequest>();
            if (request == null)
            {
                return _responseBuilder.BadRequest();
            }

            JObject moduleObject = new JObject();
            moduleObject[CommonConst.CommonField.NAME] = request.Name;
            moduleObject[CommonConst.CommonField.VERSION] = request.Version;
            var config = GetModuleConfigFile(request);
            moduleObject[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER] = config[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER];

            //   var moduleCollections = new JArray();
            //if (moduleObject[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER] != null)
            //{
            //    moduleCollections = moduleObject[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER] as JArray;
            //}
            //    else
            //    {
            //        moduleObject[CommonConst.MODULE_INSTALL_COLLECTIONS_FOLDER] = moduleCollections;
            //    }
           // moduleCollections.Add(CreateCollectionEntry(CommonConst.Collection.STATIC_CONTECT, CommonConst.CollectionAccessTypes.READONLY));
            //moduleCollections.Add(CreateCollectionEntry(CommonConst.Collection.DLLS, CommonConst.CollectionAccessTypes.READONLY));

               InstallWWWRoot(request);
            //    InstallDlls(moduleDir, moduleName);
            //    InstallCollections(moduleDir, moduleName, moduleCollections);

            //    _dbProxy.Update(CommonConst.Collection.MODULES, "{" + CommonConst.CommonField.DATA_KEY + " :'" + moduleName + "'}", moduleObject, true);
            //    return true;
            //}
            //        else
            //        {
            //            _logger.Error(string.Format("Module directory not found {0}", moduleDir), null);
            //            return false;
            //        }

            //4. Get Module info 
            //5. Update defaults in module info.
            //6. Insall www root files
            //7. Inatall dlls 
            //8. Install collection seed data
            return _responseBuilder.BadRequest();
        }

        private void InstallWWWRoot(ModuleInstallRequest request)
        {

            var wwwrootFilter = @"{name: /^Content\/wwwroot/, " + CommonConst.CommonField.MODULE_NAME + ": '" + request.Name + "', " + CommonConst.CommonField.VERSION + ": '" + request.Version + "'}";
            CleanDBCollection(request.Name, CommonConst.Collection.STATIC_CONTECT);

            foreach (var item in _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(wwwrootFilter)))
            {
                var fileSourceId= item[CommonConst.CommonField.DISPLAY_ID].ToString();
                var fileName = item[CommonConst.CommonField.NAME].ToString();
                var fileSize = int.Parse(item[CommonConst.CommonField.FILE_SIZE].ToString());
                var contentType = Mime.GetMimeType(fileName);
                var fileData = JObjectHelper.GetJObjectDbDataFromFile(fileName, contentType, "Content/wwwroot", request.Name, fileSize);
                var id = fileData[CommonConst.CommonField.DISPLAY_ID].ToString();
                if (_dbService.Write(CommonConst.Collection.STATIC_CONTECT, fileData))
                {
                    _keyValueStorage.Put<string>(CommonConst.Collection.STATIC_CONTECT, id, _keyValueStorage.Get<string>(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, fileSourceId));
                }
            }
        }

        protected void CleanDBCollection(string moduleName, string collection)
        {
            string cleanupFilter = "{ " + CommonConst.CommonField.MODULE_NAME + ":'" + moduleName + "'}";

            _dbService.Delete(collection, new RawQuery(cleanupFilter));
        }
        private JObject GetModuleConfigFile(ModuleInstallRequest request)
        {
            var moduleConfig = _dbService.Get(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, new RawQuery(new JObject()
            {
                [CommonConst.CommonField.MODULE_NAME] = request.Name,
                [CommonConst.CommonField.VERSION] = request.Version,
                [CommonConst.CommonField.NAME] = "Content/module.json"
            }.ToString())).FirstOrDefault();

            if (moduleConfig == null)
            {
                throw new Exception("Module config not found");
            }
            var data = _keyValueStorage.Get<string>(CommonConst.Collection.MODULE_FILE_UPLOAD_CACHE, moduleConfig[CommonConst.CommonField.DISPLAY_ID].ToString());
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
    }
}
