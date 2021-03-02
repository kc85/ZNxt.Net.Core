using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Services
{
    public class FileKeyValueFileStorage : IKeyValueStorage
    {
        private IEncryption _encryption;
        private readonly IAppSettingService _appSettingService;
        private readonly string _storageBasePath = string.Empty;
        private readonly ILogger _logger;
        private readonly string _fileExtn = ".zdata";

        public FileKeyValueFileStorage(IEncryption encryption, IAppSettingService appSettingService,ILogger logger)
        {
            _logger = logger;
            _encryption = encryption;
            _appSettingService = appSettingService;
            _storageBasePath  = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZNxtApp", appSettingService.GetAppSettingData("DataBaseName"));
        }

        public bool Delete(string bucket, string key)
        {
            var path = GetPath(bucket, key);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            else
            {
                return false;
            }
        }

        public T Get<T>(string bucket, string key, string encriptionKey = null)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return (T)Convert.ChangeType(Get(bucket, key, encriptionKey), typeof(T));
            }
            else if ( typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(GetString(bucket, key, encriptionKey),typeof(T));
            }
            else
            {
                var data = GetString(bucket, key, encriptionKey);
                return JsonConvert.DeserializeObject<T>(data.Trim());
            }
        }

        public List<string> GetBuckets()
        {
            List<string> buckets = new List<string>();
            DirectoryInfo di = new DirectoryInfo(GetBaseFolder());
            foreach (var item in di.GetDirectories())
            {
                buckets.Add(item.Name);
            }
            return buckets;
        }

        public List<string> GetKeys(string bucket)
        {
            List<string> keys = new List<string>();
            var path = GetBucketFolder(bucket);
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (var item in di.GetFiles(string.Format("*{0}", _fileExtn)))
            {
                keys.Add(item.Name.Replace(item.Extension, ""));
            }
            return keys;
        }

        public bool Put<T>(string bucket, string key, T data, string encriptionKey = null, string moduleName = null)
        {
            if (typeof(T) == typeof(Byte[]))
            {
                byte[] byteData = data as Byte[];
                //if (!string.IsNullOrEmpty(encriptionKey))
                //{
                //    byteData = _encryption.Encrypt(byteData, encriptionKey);
                //}
                File.WriteAllBytes(GetPath(bucket, key), byteData);
                return true;
            }
            if (typeof(T) == typeof(string))
            {
                File.WriteAllText(GetPath(bucket, key), data as string);
                return true;
            }
            else
            {
                var sttringData = JsonConvert.SerializeObject(data);
                File.WriteAllText(GetPath(bucket, key), sttringData);
                return true;
            }
        }

        public bool DeleteBucket(string bucket)
        {
            try
            {
                Directory.Delete(GetBucketFolder(bucket), true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return false;
            }
        }

        private string GetBaseFolder()
        {
            string path = _storageBasePath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private string GetBucketFolder(string bucket)
        {
            string path = Path.Combine(GetBaseFolder(), bucket);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private string GetPath(string bucket, string key = null)
        {
            var path = GetBucketFolder(bucket);
            if (!string.IsNullOrEmpty(key))
            {
                path = Path.Combine(path, $"{key}{_fileExtn}");
            }
            return path;
        }

        public byte[] Get(string bucket, string key, string encriptionKey = null)
        {
            var path = GetPath(bucket, key);
            if (!File.Exists(path))
            {
                throw new KeyNotFoundException(key);
            }

            byte[] byteData = File.ReadAllBytes(path);
            //if (!string.IsNullOrEmpty(encriptionKey))
            //{
            //    byteData = _encryption.Decrypt(byteData, encriptionKey);
            //}
            return byteData;
        }

        public string GetString(string bucket, string key, string encriptionKey = null)
        {
            try
            {
                var path = GetPath(bucket, key);
                if (!File.Exists(path))
                {
                    throw new KeyNotFoundException(key);
                }
                byte[] byteData = File.ReadAllBytes(path);
                //if (!string.IsNullOrEmpty(encriptionKey))
                //{
                //    byteData = _encryption.Decrypt(byteData, encriptionKey);
                //}
                return System.Text.Encoding.UTF8.GetString(byteData);
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}" , ex);
                
                throw;
            }
           
        }
    }
}

