using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API
{
    public class SSOPingController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        public SSOPingController(IResponseBuilder responseBuilder, IHttpContextProxy httpContextProxy)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;

        }
        [Route("/sso/ssoping", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public async Task<JObject> Ping()
        {
            return await Task.FromResult<JObject>(_responseBuilder.Success());
        }
        [Route("/sso/auth/ping", CommonConst.ActionMethods.GET, "user")]
        public async Task<JObject> Auth()
        {
            return await Task.FromResult<JObject>(_responseBuilder.Success());
        }
    }
}
