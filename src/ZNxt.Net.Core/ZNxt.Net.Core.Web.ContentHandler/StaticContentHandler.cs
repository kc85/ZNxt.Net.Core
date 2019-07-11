using System;
using System.Threading.Tasks;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.ContentHandler
{
    public class StaticContentHandler : IStaticContentHandler
    {
        private readonly IDBService _dbService;
        private readonly ILogger _logger;
        private readonly IActionExecuter _actionExecuter;
        private readonly IViewEngine _viewEngine;
        private readonly IKeyValueStorage _keyValueStorage;
        private readonly IHttpContextProxy _httpProxy;
        private readonly ISessionProvider _sessionProvider;
        public StaticContentHandler(IDBService dbService,ILogger logger, IActionExecuter actionExecuter,IHttpContextProxy httpProxy,ISessionProvider sessionProvider, IViewEngine viewEngine,IKeyValueStorage keyValueStorage)
        {
            _dbService = dbService;
            _logger = logger ;
            _actionExecuter = actionExecuter;
            _viewEngine = viewEngine;
            _keyValueStorage = keyValueStorage;
            _httpProxy = httpProxy;
            _sessionProvider = sessionProvider;
        }
        public Task<byte[]> GetContentAsync(string path)
        {
            path = ContentHelper.MappedUriPath(path);
            var data = ContentHelper.GetContent(_dbService, _logger, path);

            return Task.FromResult<byte [] >(data);
        }

        public Task<string> GetStringContentAsync(string path)
        {
            path = ContentHelper.MappedUriPath(path);

          

            if (CommonUtility.IsServerSidePage(path))
            {
                var response = ServerPageModelHelper.ServerSidePageHandler(path, _dbService, _httpProxy, _viewEngine, _actionExecuter, _logger, _sessionProvider,_keyValueStorage);

                //var data = ContentHelper.GetStringContent(_dbService, _logger, path, _keyValueStorage);
                return Task.FromResult<string>(response);
                // return response;
               // return Task.FromResult<string>($"In Progress :: {DateTime.Now.ToLongTimeString()}");

            }
            else
            {
                var data = ContentHelper.GetStringContent(_dbService, _logger, path, _keyValueStorage);
                return Task.FromResult<string>(data);
            }
        }
    }
}
