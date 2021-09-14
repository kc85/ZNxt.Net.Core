using ZNxt.Module.MyModule1.Consts;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
namespace ZNxt.Module.MyModule1.Api
{
    public class PingController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly ILogger _logger;

        public PingController(IHttpContextProxy httpContextProxy,
            IDBService dBService,
            IRDBService rDBService,
            ILogger logger,IResponseBuilder responseBuilder)
      : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _logger = logger;
            _responseBuilder = responseBuilder;
        }

        [Route(MyModule1Consts.SERVICE_API_PREFIX + "/ping", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject Ping()
        {
            return _responseBuilder.Success();
        }

    }
}
