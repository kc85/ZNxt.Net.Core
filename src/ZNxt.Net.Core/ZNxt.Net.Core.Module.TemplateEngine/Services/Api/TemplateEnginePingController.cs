using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.TemplateEngine.Services.Api
{
    public class TemplateEnginePingController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IDBService _dbService;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        public TemplateEnginePingController(IResponseBuilder responseBuilder, IDBService dbService, IHttpContextProxy httpContextProxy, ILogger logger)
        {
            _responseBuilder = responseBuilder;
            _dbService = dbService;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
        }
        [Route("/template/ping", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject Ping()
        {
            return _responseBuilder.Success();
        }
    }
    public static class Load {
        public static string load = "";
    }
}
