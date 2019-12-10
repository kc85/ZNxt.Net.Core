using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.DB.Mongo
{
    public class MongoDBKeyValueService : ZNxt.Net.Core.Interfaces.IKeyValueStorage
    {
        private readonly IDBService _dBService;
        private readonly string _keyValueCollectionPrefix = "key_value_";
        private IEncryption _encryption;
        private readonly ILogger _logger;
        public MongoDBKeyValueService(IDBService dBService, IEncryption encryption, ILogger logger)
        {
            _dBService = dBService;
            _logger = logger;
            _encryption = encryption;
        }
        public bool Delete(string bucket, string key)
        {
            var collection = GetCollection(bucket);
            return _dBService.Delete(collection, new Model.RawQuery("{'key':'" + key + "'}")) == 1;
        }

        private string GetCollection(string bucket)
        {
            return $"{_keyValueCollectionPrefix }{bucket}";
        }

        public T Get<T>(string bucket, string key, string encriptionKey = null)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return (T)Convert.ChangeType(Get(bucket, key, encriptionKey), typeof(T));
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(GetString(bucket, key, encriptionKey), typeof(T));
            }
            else
            {
                var stringdata = GetString(bucket, key, encriptionKey);
                return JsonConvert.DeserializeObject<T>(stringdata.Trim());
            }

        }

        private string GetString(string bucket, string key, string encriptionKey)
        {
            return Encoding.UTF8.GetString(Get(bucket, key, encriptionKey));
        }
        public byte[] Get(string bucket, string key, string encriptionKey = null)
        {
            var collection = GetCollection(bucket);
            var data = _dBService.Get(collection, new Model.RawQuery("{'"+ CommonConst.CommonField.KEY+"':'" + key + "'}"));
            if (data.Count != 0)
            {
                var base64Data = data.First()[CommonConst.CommonField.DATA].ToString();

                byte[] byteData = Convert.FromBase64String(base64Data);
                if (!string.IsNullOrEmpty(encriptionKey))
                {
                    byteData = _encryption.Decrypt(byteData, encriptionKey);
                }
                return byteData;
            }
            else
            {
                throw new KeyNotFoundException(key);
            }
        }

        public List<string> GetBuckets()
        {
            throw new NotImplementedException();
        }

        public List<string> GetKeys(string bucket)
        {
            var collection = GetCollection(bucket);
            var data = _dBService.Get(collection, new Model.RawQuery("{}"), new List<string> { CommonConst.CommonField.KEY });
            return data.Select(f => f[CommonConst.CommonField.KEY].ToString()).ToList();
        }

        public bool Put<T>(string bucket, string key, T data, string encriptionKey = null)
        {
            byte[] byteData = null;
            if (typeof(T) == typeof(Byte[]))
            {
                byteData = data as Byte[];
                if (!string.IsNullOrEmpty(encriptionKey))
                {
                    byteData = _encryption.Encrypt(byteData, encriptionKey);
                }
            }
            if (typeof(T) == typeof(string))
            {
                byteData = Encoding.ASCII.GetBytes(data as string);
            }
            else
            {
                byteData = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data));
            }

            if (byteData != null)
            {
                var dbdata = new JObject()
                {
                    [CommonConst.CommonField.DISPLAY_ID] = CommonUtility.GetNewID(),
                    [CommonConst.CommonField.KEY] = key,
                    [CommonConst.CommonField.DATA] = Convert.ToBase64String(byteData)

                };
                return _dBService.WriteData(GetCollection(bucket), dbdata);
            }
            else
            {
                return false;
            }
        }
    }
}
