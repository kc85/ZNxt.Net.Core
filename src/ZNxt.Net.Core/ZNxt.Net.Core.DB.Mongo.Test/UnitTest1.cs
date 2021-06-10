using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.DB.Mongo.Test
{
    [TestClass]
    public class UnitTest1
    {
        const string DBName = "ZNxt_UT";
        const string connectionString = "mongodb://localhost:27017/" + DBName;
        [TestMethod]
        public void TestMethod1()
        {
         
           

            var dbService = GetDBService(GetHttpProxyMock());
            var response = dbService.Aggregate("users", "{$addFields:{'city': 'Pune'}}", "{$project:{'name': 1, 'user_id': 1,'city':1}}");
            //Assert.AreEqual("hello", response[0][]);
        }
        public static MongoDBService GetDBService(IHttpContextProxy httpContextProxy)
        {

            var dbconfig = new ZNxt.Net.Core.DB.Mongo.MongoDBServiceConfig();
            dbconfig.Set(DBName, connectionString);
            var dBService = new ZNxt.Net.Core.DB.Mongo.MongoDBService(dbconfig, httpContextProxy);
            return dBService;
        }
        public static IHttpContextProxy GetHttpProxyMock(JObject httpRequestBody = null, Dictionary<string, string> querystring = null, Dictionary<string, string> headers = null)
        {
            return new HttpProxyMock(httpRequestBody, querystring, headers);
        }
    }

    public class HttpProxyMock : ZNxt.Net.Core.Interfaces.IHttpContextProxy

    {
        private readonly Dictionary<string, string> _querystring;
        private readonly Dictionary<string, string> _headers;
        private readonly JObject _httpRequestBody;
        public HttpProxyMock(JObject httpRequestBody = null, Dictionary<string, string> querystring = null, Dictionary<string, string> headers = null)
        {
            _httpRequestBody = httpRequestBody;
            _querystring = querystring;
            _headers = headers;

        }

        public int ResponseStatusCode => throw new NotImplementedException();

        public string ResponseStatusMessage => throw new NotImplementedException();

        public byte[] Response => throw new NotImplementedException();

        public string SessionID => throw new NotImplementedException();

        public UserModel User => new UserModel() { id = "goo115559040204585310230", first_name = "Khanin", last_name = "C", user_id = "goo115559040204585310230" };

        public DateTime InitDateTime => throw new NotImplementedException();

        public string TransactionId => throw new NotImplementedException();

        public Task<string> GetAccessTokenAync()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public string GetQueryString(string key)
        {
            if (_querystring != null && _querystring.ContainsKey(key))
            {
                return _querystring[key];
            }
            return null;
        }

        public string GetQueryString()
        {
            throw new NotImplementedException();
        }

        public string GetReponseHeader(string key)
        {
            throw new NotImplementedException();
        }

        public string GetRequestBody()
        {
            if (_httpRequestBody != null)
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
            catch (Exception)
            {
                return default;
            }
        }

        public string GetRequestTenantId()
        {
            throw new NotImplementedException();
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
