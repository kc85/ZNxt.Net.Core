using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IApiGatewayService
    {
        Task<JObject> CallAsync(string Method, string route, string querystring = "", JObject requestBody = null, Dictionary<string, string> headres = null, string baseUrl = "");
    }
}
