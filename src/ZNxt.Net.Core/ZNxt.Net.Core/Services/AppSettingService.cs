using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Services
{
    public class AppSettingService : IAppSettingService
    {
        private JArray _settings;
        private static readonly object _lockObj = new object();
        private readonly IDBService _dbService;

        public AppSettingService(IDBService dbService)
        {
            _dbService = dbService;
        }

        public void ReloadSettings(bool forceReload = false)
        {
            if (_settings == null || forceReload)
            {
                lock (_lockObj)
                {
                    if (!string.IsNullOrEmpty(ApplicationConfig.ConnectionString))
                    {
                        _settings = _dbService.Get(CommonConst.Collection.APP_SETTING, new RawQuery(CommonConst.Filters.IS_OVERRIDE_FILTER));
                    }
                }
            }
        }

        public JObject GetAppSetting(string key)
        {
            ReloadSettings();
            var data = CommonUtility.GetAppConfigValue(key);
            if (!string.IsNullOrEmpty(data))
            {
                return JObject.Parse(data);
            }
           
            return _settings.FirstOrDefault(f => f[CommonConst.CommonField.DATA_KEY].ToString() == key) as JObject;
        }

        public void SetAppSetting(string key, JObject data, string module = null)
        {
            lock (_lockObj)
            {
                string filter = "{" + CommonConst.CommonField.DATA_KEY + " : '" + key + "','is_override' : false}";
                JObject setting = new JObject();
                setting[CommonConst.CommonField.DATA_KEY] = key;
                setting[CommonConst.CommonField.DISPLAY_ID] = Guid.NewGuid().ToString();
                setting[CommonConst.CommonField.DATA] = data;
                setting[CommonConst.CommonField.ÌS_OVERRIDE] = false;
                setting[CommonConst.CommonField.OVERRIDE_BY] = CommonConst.CommonValue.NONE;
                setting[CommonConst.CommonField.MODULE_NAME] = module;
                if (!string.IsNullOrEmpty(ApplicationConfig.ConnectionString))
                {
                    var dbresponse = _dbService.Update(CommonConst.Collection.APP_SETTING, new RawQuery(filter), setting, true);
                }
                _settings = null;
                ReloadSettings();
            }
        }
        public void SetAppSetting(string key, string data, string module = null)
        {
            lock (_lockObj)
            {
                string filter = "{" + CommonConst.CommonField.DATA_KEY + " : '" + key + "', 'is_override' : false}";
                JObject setting = new JObject();
                setting[CommonConst.CommonField.DATA_KEY] = key;
                setting[CommonConst.CommonField.DISPLAY_ID] = Guid.NewGuid().ToString();
                setting[CommonConst.CommonField.DATA] = data;
                setting[CommonConst.CommonField.ÌS_OVERRIDE] = false;
                setting[CommonConst.CommonField.OVERRIDE_BY] = CommonConst.CommonValue.NONE;
                setting[CommonConst.CommonField.MODULE_NAME] = module;
                var dbresponse = _dbService.Update(CommonConst.Collection.APP_SETTING, new RawQuery(filter), setting, true);
                _settings = null;
                ReloadSettings();
            }
        }

        public string GetAppSettingData(string key)
        {
            var val = CommonUtility.GetAppConfigValue(key);
            if (string.IsNullOrEmpty(val))
            {
                var data = GetAppSetting(key);
                if (data != null && data[CommonConst.CommonField.DATA] != null)
                {
                    return data[CommonConst.CommonField.DATA].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return val;
            }
        }

        public JArray GetAppSettings()
        {
            ReloadSettings();
            return _settings;
        }
    }
}
