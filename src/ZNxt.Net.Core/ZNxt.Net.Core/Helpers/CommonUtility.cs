using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;

namespace ZNxt.Net.Core.Helpers
{
    public static class CommonUtility
    {
        public const string CONFIGRATION_FILE = "znxtsettings.json.config";
        private static IConfigurationRoot _configuration;
        private static object lockObject = new object();
        public static string GetNewID()
        {
            return GetUnixTimestamp(DateTime.Now) + RandomString(3) + RandomNumber(5);
        }

        public static string GetNewSessionID()
        {
            return GetNewID();
        }

        public static Int32 GetUnixTimestamp(DateTime dt)
        {
            TimeSpan epochTicks = new TimeSpan(new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            TimeSpan unixTicks = new TimeSpan(dt.Ticks) - epochTicks;
            return (Int32)unixTicks.TotalSeconds;
        }
        public static double GetTimestampMilliseconds(DateTime dt)
        {
            TimeSpan epochTicks = new TimeSpan(new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            TimeSpan unixTicks = new TimeSpan(dt.Ticks) - epochTicks;
            return (double)unixTicks.TotalMilliseconds;
        }

        public static bool IsServerSidePage(string url, bool checkBlocks = false)
        {
            var fi = new FileInfo(url);
            if (!checkBlocks)
                return fi.Extension == CommonConst.CommonField.SERVER_SIDE_PROCESS_HTML_EXTENSION;
            else
            {
                return
                    fi.Extension == CommonConst.CommonField.SERVER_SIDE_PROCESS_HTML_EXTENSION ||
                    fi.Extension == CommonConst.CommonField.SERVER_SIDE_PROCESS_HTML_BLOCK_EXTENSION ||
                    fi.Extension == CommonConst.CommonField.SERVER_SIDE_PROCESS_HTML_CSS_EXTENSION ||
                    fi.Extension == CommonConst.CommonField.SERVER_SIDE_PROCESS_HTML_TEMPLATE_EXTENSION ||
                    fi.Extension == CommonConst.CommonField.SERVER_SIDE_PROCESS_HTML_JS_EXTENSION
                    ;
            }
        }
        public static List<string> IsDefaultPages(string url)
        {
            List<string> defultPages = new List<string> { $"{url}/index.html", $"{url}/index.z", $"{url}/home.html", $"{url}/home.z" };
            var fi = new FileInfo(url);
            if (string.IsNullOrEmpty(fi.Extension))
            {
                return defultPages;
            }
            else
            {
                return new List<string>() { url };
            }
        }

        public static string GetBase64(byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        public static string GetBase64(string data)
        {
            return GetBase64(Encoding.ASCII.GetBytes(data));
        }

        public static string GenerateTxnId(string prefix = "HT")
        {
            TimeSpan epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);

            TimeSpan unixTicks = new TimeSpan(DateTime.Now.Ticks) - epochTicks;
            string txnId = prefix + RandomNumber(2) + unixTicks.Ticks.ToString();

            return txnId;
        }

        private static Random random = new Random();
        private static object syncObj = new object();

        public static string RandomString(int length)
        {
            lock (syncObj)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }

        public static string RandomNumber(int length)
        {
            lock (syncObj)
            {
                const string chars = "0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }

        public static IConfigurationRoot GetWebAppConfig()
        {
            if (_configuration == null)
            {
                lock (lockObject)
                {
                    _configuration = new ConfigurationBuilder()
                                      .AddJsonFile(CONFIGRATION_FILE, optional: false)
                                      .Build();
                }

            }

            return _configuration;
        }
        public static void SaveConfig(string key, string value)
        {


            lock (lockObject)
            {
                var path = string.Format("{0}\\{1}", ApplicationConfig.AppBinPath, CONFIGRATION_FILE);

                if (!new FileInfo(path).IsReadOnly)
                {
                    var config = JObjectHelper.GetJObjectFromFile(path);
                    config[key] = value;
                    JObjectHelper.WriteJSONData(path, config);
                    _configuration = null;
                    GetWebAppConfig();
                }
                Environment.SetEnvironmentVariable(key, value);
            }
        }
        public static string GetAppConfigValue(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                return GetWebAppConfig()[key];
            }
            else
            {
                return value;
            }
        }

        public static bool IsTextConent(string contentType)
        {
            return contentType.Contains("text/") || contentType.Contains("application/json") || contentType.Contains("application/xml");
        }

        public static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
    }
}