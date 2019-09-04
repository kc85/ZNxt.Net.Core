using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services.Api.Ping
{
    public class PingController
    {
        private readonly IResponseBuilder _responseBuilder;
        public PingController(IResponseBuilder responseBuilder)
        {
            _responseBuilder = responseBuilder;
        }
        [Route("/ping", CommonConst.ActionMethods.GET,CommonConst.CommonValue.ACCESS_ALL)]
        public async Task<JObject> Ping()
        {
            return await Task.FromResult<JObject>(_responseBuilder.Success());
        }
    }
}
