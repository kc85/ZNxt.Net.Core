using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.DB.AmazonDynamoDB
{
    public class DynamoDB : IDBService
    {
        public long Delete(string collection, string bsonQuery)
        {
            throw new NotImplementedException();
        }

        public bool DropDB()
        {
            throw new NotImplementedException();
        }

        public JArray Get(string collection, string bsonQuery, List<string> properties = null, Dictionary<string, int> sortColumns = null, int? top = null, int? skip = null)
        {
            throw new NotImplementedException();
        }

        public long GetCount(string collection, string bsonQuery)
        {
            throw new NotImplementedException();
        }

        public JObject GetPageData(string collection, string query, List<string> fields = null, Dictionary<string, int> sortColumns = null, int pageSize = 10, int currentPage = 1)
        {
            throw new NotImplementedException();
        }

        public long Update(string collection, string bsonQuery, JObject data, bool overrideData = false, MergeArrayHandling mergeType = MergeArrayHandling.Union)
        {
            throw new NotImplementedException();
        }

        public bool WriteData(string collection, JObject data)
        {
            throw new NotImplementedException();
        }
    }
}
