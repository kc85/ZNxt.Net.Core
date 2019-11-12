using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IApiGatewayService
    {
        Task<JObject> CallAsync(string Method, string route, string querystring = "", JObject requestBody = null, Dictionary<string, string> headres = null, string baseUrl = "");
        Task<RoutingModel> GetRouteAsync(string method, string route);
    }
}
