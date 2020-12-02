using System;
using System.Collections.Generic;
using ZNxt.Net.Core.Web.ContentHandler;

namespace StaticContentTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var hash = ZNxt.Net.Core.Helpers.CommonUtility.Sha256Hash("21AFF254AE134F3F843D176100715C6F");

            var rv = new RazorTemplateEngine();

            Func<string, string> getAppSetting =
            (string key) =>
            {
                   //var response = AppSettingService.Instance.GetAppSettingData(key);
                   //if (string.IsNullOrEmpty(response))
                   //{
                   //    response = ConfigurationManager.AppSettings[key];
                   //}
                   //return response;
                   return key;
            };

            Dictionary<string, dynamic> dataModel = new Dictionary<string, dynamic>();
            dataModel["Name"] = "Khanin";
            dataModel["getAppSetting"] = getAppSetting;
            var data = rv.Compile($"Hello -- {DateTime.Now.ToString()} @Raw((1+1).ToString()) @Model[\"Name\"] @Model[\"getAppSetting\"](\"Hello 1111\")", "aa", dataModel);

            Console.WriteLine("Hello World!" + data);
            Console.Read();
        }
    }
}
