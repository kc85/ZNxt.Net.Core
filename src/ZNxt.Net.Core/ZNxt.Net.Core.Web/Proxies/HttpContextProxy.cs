using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Proxies
{
    public class HttpContextProxy : IHttpContextProxy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextProxy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
       
        public Dictionary<string, string> ResponseHeaders { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ResponseStatusCode => throw new NotImplementedException();

        public string ResponseStatusMessage => throw new NotImplementedException();

        public byte[] Response => throw new NotImplementedException();

        public string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string SessionID => throw new NotImplementedException();

        public DateTime InitDateTime => throw new NotImplementedException();

        public string TransactionId => throw new NotImplementedException();

        public UserModel User { get {

                //_httpContextAccessor.HttpContext.User;
                return null;
            } }

        public string GetFormData(string key)
        {
            throw new NotImplementedException();
        }

        public string GetHeader(string key)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetHeaders()
        {
            var header = new Dictionary<string, string>();
            foreach (var h in _httpContextAccessor.HttpContext.Request.Headers)
            {
                header.Add(h.Key, h.Value);
            }
            return header;
        }

        public string GetHttpMethod()
        {
           return _httpContextAccessor.HttpContext.Request.Method;
        }

        public string GetMimeType(string fileName)
        {
            throw new NotImplementedException();
        }

        public string GetQueryString(string key)
        {
            return _httpContextAccessor.HttpContext.Request.Query[key];
        }
        public string GetQueryString()
        {
            return _httpContextAccessor.HttpContext.Request.QueryString.Value;
        }

        public string GetRequestBody()
        {
            if (_httpContextAccessor.HttpContext.Request.Body != null)
            {
                var requestBody = new StreamReader(_httpContextAccessor.HttpContext.Request.Body).ReadToEnd();
                return requestBody;
            }
            return null;
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
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                //logger.Error(string.Format("Error while converting data [{0}], Error :  {1}", body, ex.Message), ex);
                return default(T);
            }
        }

        public string GetURIAbsolutePath()
        {
            return _httpContextAccessor.HttpContext.Request.Path.ToString().ToLower();
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

        public string GetContentType(string path)
        {
            throw new NotImplementedException();
        }

       
    }
}
