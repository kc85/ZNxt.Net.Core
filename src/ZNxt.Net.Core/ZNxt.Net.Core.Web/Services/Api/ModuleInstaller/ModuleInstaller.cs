using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
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

        public ModuleInstaller(IDBService dbService, IKeyValueStorage keyValueStorage, IServiceResolver serviceResolver, IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy, IDBServiceConfig dbConfig)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _dbConfig = dbConfig;
            _serviceResolver = serviceResolver;
            _keyValueStorage = keyValueStorage;
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
    }
}
