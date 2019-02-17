using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Services
{
    public class FileKeyValueStorage : IKeyValueStorage
    {
        public bool Delete(string bucket, string key)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string bucket, string key, string encriptionKey = null)
        {
            throw new NotImplementedException();
        }

        public byte[] Get(string bucket, string key, string encriptionKey = null)
        {
            throw new NotImplementedException();
        }

        public List<string> GetBuckets()
        {
            throw new NotImplementedException();
        }

        public List<string> GetKeys(string bucket)
        {
            throw new NotImplementedException();
        }

        public bool Put<T>(string bucket, string key, T data, string encriptionKey = null)
        {
            throw new NotImplementedException();
        }
    }
}
