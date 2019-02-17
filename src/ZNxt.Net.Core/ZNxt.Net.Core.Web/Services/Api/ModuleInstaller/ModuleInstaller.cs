using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ZNxt.Net.Core.Consts;
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
                using (ZipArchive zip = new ZipArchive(_httpFileUploader.GetFileStream(_httpFileUploader.GetFiles()[0])))
                {
                    foreach (var entry in zip.Entries)
                    {

                        using (StreamReader sr = new StreamReader(entry.Open()))
                        {
                            //sr.ReadToEnd();
                            _dbService.WriteData(CommonConst.Collection.FILE_UPLOAD_CACHE, new JObject() { [CommonConst.CommonField.NAME] = entry.FullName });
                        }
                    }
                }
                return _responseBuilder.Success();
            }
            return _responseBuilder.BadRequest();
        }

    }
}
