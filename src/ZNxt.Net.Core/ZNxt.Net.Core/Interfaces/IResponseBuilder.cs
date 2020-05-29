using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IResponseBuilder
    {
        JObject Success(JToken data = null, JObject extraData = null);
        JObject CreateReponse(int code);
        JObject CreateReponseWithError(int code, List<string> errors);
        JObject CreateReponse(int code, JToken data = null, JObject extraData = null);
        JObject NotFound(JToken data = null, JObject extraData = null);
        JObject ServerError(JToken data = null, JObject extraData = null);
        JObject BadRequest(JToken data = null, JObject extraData = null);
        JObject BadRequest(string error);
        JObject Unauthorized(JToken data = null, JObject extraData = null);

    }
}