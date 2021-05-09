using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.DB.Mongo;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Module.MyModule1.Helpers
{
    public  class AppSettingServiceMock : IAppSettingService
    {
        Dictionary<string, string> data = new Dictionary<string, string>() {
            ["cloudfront_url"] = "https://d1garn5dlo0m6m.cloudfront.net",
            ["gps_api_key"] = "ddd"
        };
        public JObject GetAppSetting(string key)
        {
            throw new NotImplementedException();
        }

        public string GetAppSettingData(string key)
        {
            return data[key];
        }

        public JArray GetAppSettings()
        {
            throw new NotImplementedException();
        }

        public void SetAppSetting(string key, JObject data, string module = null)
        {
            throw new NotImplementedException();
        }

        public void SetAppSetting(string key, string data, string module = null)
        {
            throw new NotImplementedException();
        }
    }

    public static class CommonExtensions
    {
        public static IHttpContextProxy GetHttpProxyMock(JToken httpRequestBody = null, Dictionary<string, string> querystring = null, Dictionary<string, string> headers = null)
        {
            return new HttpProxyMock(httpRequestBody, querystring, headers);
        }
        public static MongoDBService GetDBService(IHttpContextProxy httpContextProxy)
        {

            var dbconfig = new ZNxt.Net.Core.DB.Mongo.MongoDBServiceConfig();
            dbconfig.Set("znxt_mut_app_core", "mongodb+srv://admin2:mvp%40123@127.0.0.1:27017/?authSource=admin");
            var dBService = new ZNxt.Net.Core.DB.Mongo.MongoDBService(dbconfig, httpContextProxy);
            return dBService;
        }
        public static IRDBService GetRDBService(IHttpContextProxy httpContextProxy)
        {

            Environment.SetEnvironmentVariable("MSSQLConnectionString", "Server=.\\SQLEXPRESS;Database=TEST;Trusted_Connection=True;");
            var dBService = new ZNxt.Net.Core.DB.MySql.SqlRDBService();
            return dBService;
        }
    }

}
