using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Module.Gateway.Services.Api
{
    public class BasePingController
    {
        private readonly IResponseBuilder _responseBuilder;
        public BasePingController(IResponseBuilder responseBuilder)
        {

            _responseBuilder = responseBuilder;
        }
        [Route("/gateway/ping", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject Ping()
        {

            return _responseBuilder.Success();
        }
    }
}
