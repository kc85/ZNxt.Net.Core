using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.MyModule1.Helpers
{
    public class HttpProxyMock : ZNxt.Net.Core.Interfaces.IHttpContextProxy

    {
        private readonly Dictionary<string, string> _querystring;
        private readonly Dictionary<string, string> _headers;
        private readonly JToken _httpRequestBody;
        public HttpProxyMock(JToken httpRequestBody = null, Dictionary<string, string> querystring = null, Dictionary<string, string> headers = null)
        {
            _httpRequestBody = httpRequestBody;
            _querystring = querystring;
            if (_querystring == null)
            {
                _querystring = new Dictionary<string, string>();
            }
            _headers = headers;
            if (_headers == null) { _headers = new Dictionary<string, string>(); }

        }
        public Dictionary<string, string> ResponseHeaders { get; set; }

        public int ResponseStatusCode { get { return 200; } }

        public string ResponseStatusMessage => throw new NotImplementedException();

        public byte[] Response => throw new NotImplementedException();

        public string SessionID => throw new NotImplementedException();

        public UserModel User => new UserModel() { id = "goo115559040204585310230", name = "Khanin", user_id = "goo115559040204585310230" };

        public DateTime InitDateTime => throw new NotImplementedException();

        public string TransactionId => throw new NotImplementedException();

        public Task<string> GetAccessTokenAync()
        {
            return Task.FromResult("");
        }

        public string GetFormData(string key)
        {
            throw new NotImplementedException();
        }

        public string GetHeader(string key)
        {
            if (_headers != null && _headers.ContainsKey(key))
            {
                return _headers[key];
            }
            return string.Empty;
        }

        public Dictionary<string, string> GetHeaders()
        {
            return _headers;
        }

        public string GetHttpMethod()
        {
            return "GET";
        }

        public string GetQueryString(string key)
        {
            if (_querystring != null && _querystring.ContainsKey(key))
            {
                return _querystring[key];
            }
            return null ;
        }

        public string GetQueryString()
        {
            return string.Join("&", _querystring.Select(f=> $"{f.Key}={f.Value}"));
        }

        public string GetRequestBody()
        {
            if (_httpRequestBody!= null )
            {
                return _httpRequestBody.ToString();
            }
            return string.Empty;
        }

        public T GetRequestBody<T>()
        {
            string body = GetRequestBody();
            try
            {
                if (!string.IsNullOrEmpty(body))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(body);
                }
                else
                {
                    return default;
                }
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public string GetURIAbsolutePath()
        {
            throw new NotImplementedException();
        }

        public void SetResponse(int statusCode, JObject data = null)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(int statusCode, string data)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(int statusCode, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(string data)
        {
            throw new NotImplementedException();
        }

        public void SetResponse(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void UnloadAppDomain()
        {
            throw new NotImplementedException();
        }
    }
}
