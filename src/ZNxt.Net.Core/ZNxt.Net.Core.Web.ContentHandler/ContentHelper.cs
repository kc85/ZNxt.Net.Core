using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.ContentHandler
{
    public class ContentHelper
    {
        public static byte[] GetContent(IDBService dbProxy, ILogger logger, string path, IKeyValueStorage keyValueStorage)
        {
            string wwwrootpath = ApplicationConfig.AppWWWRootPath;
            JObject document = null;
            if (dbProxy.IsConnected)
             document = (JObject)dbProxy.Get(CommonConst.Collection.STATIC_CONTECT, GetFilter(path)).First;

            if (document != null)
            {
                var data = document[CommonConst.CommonField.DATA];
                if (data == null)
                {
                    data = keyValueStorage.Get<string>(CommonConst.Collection.STATIC_CONTECT, document[CommonConst.CommonField.DISPLAY_ID].ToString());
                }
                if (data != null)
                {
                    if (CommonUtility.IsTextConent(document[CommonConst.CommonField.CONTENT_TYPE].ToString()))
                    {
                        return Encoding.ASCII.GetBytes(data.ToString());
                    }
                    else
                    {
                        byte[] dataByte = System.Convert.FromBase64String(data.ToString());
                        return dataByte;
                    }
                }
                
            }
            else
            {
                string filePath = GetFullPath(path);

                if (File.Exists(filePath))
                {
                    return File.ReadAllBytes(filePath);
                }
            }
            return null;
        }

        private static IDBQueryBuilder GetFilter(string path)
        {
            path = path.Replace("\\", "/");
            var query = "{ $and: [ { " + CommonConst.CommonField.IS_OVERRIDE + ":{ $ne: true}  }, {'" + CommonConst.CommonField.FILE_PATH + "':  {$regex :'^" + path.ToLower() + "$','$options' : 'i'}}] }";
            return new RawQuery(query);
        }

        public static string GetStringContent(IDBService dbProxy, ILogger _logger, string path, IKeyValueStorage keyValueStorage)
        {
            JObject document = null;
            if(dbProxy.IsConnected)
            document = (JObject)dbProxy.Get(CommonConst.Collection.STATIC_CONTECT, GetFilter(path)).First;
            if (document != null)
            {
                
                var data = document[CommonConst.CommonField.DATA];
                if (data != null)
                {
                    return data.ToString();
                }
                else
                {

                    var dataFromFile = keyValueStorage.Get<string>(CommonConst.Collection.STATIC_CONTECT, document[CommonConst.CommonField.DISPLAY_ID].ToString());
                    if (dataFromFile != null)
                    {
                        return CommonUtility.GetStringFromBase64(dataFromFile);
                    }
                }
            }
            else
            {

                string filePath = GetFullPath(path);
                  System.Console.WriteLine(filePath);
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
            }
            return null;
        }

        private static string GetFullPath(string path)
        {
            string wwwrootpath = ApplicationConfig.AppWWWRootPath;

            return string.Format("{0}{1}{2}", ApplicationConfig.AppBinPath, wwwrootpath, path);

        }
        public static string MappedUriPath(string url)
        {
            //if (url.IndexOf(ApplicationConfig.AppBackendPath) == 0)
            //{
            //    var path = url.Remove(0, ApplicationConfig.AppBackendPath.Length);
            //    path = string.Format("/{0}{1}", CommonConst.CommonValue.APP_BACKEND_FOLDERPATH, path);
            //    return path;
            //}
            //else
            //{
            //    var path = string.Format("/{0}{1}", CommonConst.CommonValue.APP_FRONTEND_FOLDERPATH, url);

            //    return path;
            //}
            return url;
        }

        public static string UnmappedUriPath(string path)
        {
            if (path.IndexOf(string.Format("/{0}", CommonConst.CommonValue.APP_BACKEND_FOLDERPATH)) == 0)
            {
                path = path.Remove(0, CommonConst.CommonValue.APP_BACKEND_FOLDERPATH.Length + 1);
                path = string.Format("{0}{1}", ApplicationConfig.AppBackendPath, path);
                return path;
            }
            else
            {
                //path = path.Remove(0, CommonConst.CommonValue.APP_FRONTEND_FOLDERPATH.Length + 1);
                return path;
            }
        }

        public static bool IsAdminPage(string url)
        {
            return url.IndexOf(string.Format("/{0}", CommonConst.CommonValue.APP_BACKEND_FOLDERPATH)) == 0;
        }
    }
}
