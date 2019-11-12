using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.Notifier.Services.Api
{
    public class NotifierPingController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IDBService _dbService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        public NotifierPingController(IResponseBuilder responseBuilder, IDBService dbService, IHttpContextProxy httpContextProxy, ILogger logger)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
        }
        [Route("/notifier/ping", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject Ping()
        {
            return _responseBuilder.Success();
        }
    }
}
