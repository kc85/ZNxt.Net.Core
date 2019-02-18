using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

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
            //1. get module request
            //2. Get module cache folder. 
            //3. Download module from Nuget Server. 
            //4. Get Module info 
            //5. Update defaults in module info.
            //6. Insall www root files
            //7. Inatall dlls 
            //8. Install collection seed data
            return _responseBuilder.BadRequest();
        }
        [Route("/moduleinstaller/upload", CommonConst.ActionMethods.POST)]
        public JObject UploadModule()
        {
            if (_httpFileUploader.GetFiles().Count != 0)
            {
                string moduleName = string.Empty;
                string moduleVersion = string.Empty;
                var uploadId = CommonUtility.RandomString(19);
                _dbService.Delete(CommonConst.Collection.FILE_UPLOAD_CACHE, new RawQuery(new JObject()
                {
                    [CommonConst.CommonField.MODULE_NAME] = moduleName
                }.ToString()));
                using (ZipArchive zip = new ZipArchive(_httpFileUploader.GetFileStream(_httpFileUploader.GetFiles()[0])))
                {
                    foreach (var entry in zip.Entries)
                    {
                        var stream = entry.Open();
                        
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            string stringData = sr.ReadToEnd() ;

                            if (entry.FullName.LastIndexOf(".nuspec") == (entry.FullName.Length - ".nuspec".Length))
                            {
                                XmlDocument moduleConfig = new XmlDocument();
                                moduleConfig.LoadXml(stringData);
                                moduleName = moduleConfig.GetElementsByTagName("id").Item(0).InnerText;
                                moduleVersion = moduleConfig.GetElementsByTagName("version").Item(0).InnerText;
                            }



                            _dbService.Write(CommonConst.Collection.FILE_UPLOAD_CACHE,
                            new JObject()
                            {
                                [CommonConst.CommonField.TRANSACTION_ID] = uploadId,
                                [CommonConst.CommonField.NAME] = entry.FullName,
                                [CommonConst.CommonField.MODULE_NAME] = moduleName,
                                [CommonConst.CommonField.DATA] = stringData,
                                [CommonConst.CommonField.VERSION] = moduleVersion,
                            });
                        }
                    }
                }
                _dbService.Update(CommonConst.Collection.FILE_UPLOAD_CACHE, new RawQuery(new JObject()
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
            return _responseBuilder.BadRequest();
        }

    }
}
