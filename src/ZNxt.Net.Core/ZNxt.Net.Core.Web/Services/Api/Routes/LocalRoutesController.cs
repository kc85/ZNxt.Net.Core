using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;


namespace ZNxt.Net.Core.Web.Services.Api.Routes
{
    public class LocalRoutesController
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IRouting _routing;
        public LocalRoutesController(IResponseBuilder responseBuilder, IRouting routing)
        {
            _responseBuilder = responseBuilder;
            _routing = routing;
        }
        [Route("/local/routes", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public async Task<JObject> Get()
        {
            var routes = _routing.GetRoutes();
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(routes);
            return await Task.FromResult<JObject>(_responseBuilder.Success(JArray.Parse(data)));
        }
    }
}
