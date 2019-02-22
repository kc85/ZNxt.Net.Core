using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IHttpContextProxy : IInitData
    {
        string GetURIAbsolutePath();

        string GetHttpMethod();

        Dictionary<string, string> ResponseHeaders { get; set; }

        int ResponseStatusCode { get; }

        string ResponseStatusMessage { get; }

        byte[] Response { get; }

        void SetResponse(int statusCode, JObject data = null);

        void SetResponse(int statusCode, string data);

        void SetResponse(int statusCode, byte[] data);

        void SetResponse(string data);

        void SetResponse(byte[] data);
        
        string GetRequestBody();

        T GetRequestBody<T>();

        string GetQueryString(string key);

        string GetQueryString();

        string GetFormData(string key);

        string SessionID { get; }

        void UnloadAppDomain();

        string GetHeader(string key);

        Dictionary<string, string> GetHeaders();
        
        UserModel User { get; }
    }
}