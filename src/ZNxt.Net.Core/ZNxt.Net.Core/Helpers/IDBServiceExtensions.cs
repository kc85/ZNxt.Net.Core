using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Helpers
{
    public static class IDBServiceExtensions
    {
        public static T FirstOrDefault<T>(this IDBService dbProxy, string collection, string filterKey, string filterValue, bool isOverrideCheck = false)
        {
            var data = FirstOrDefault(dbProxy, collection, filterKey, filterValue, isOverrideCheck);
            if (data != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data.ToString());
            }
            else
            {
                return default(T);
            }
        }

        public static T FirstOrDefault<T>(this IDBService dbProxy, string collection, Dictionary<string, string> filter, bool isOverrideCheck = false)
        {
            var data = FirstOrDefault(dbProxy, collection, filter, isOverrideCheck);
            if (data != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data.ToString());
            }
            else
            {
                return default(T);
            }
        }

        public static JObject FirstOrDefault(this IDBService dbProxy, string collection, string filterKey, string filterValue, bool isOverrideCheck = false)
        {
            var dbQuery = new DBQuery();
            dbQuery.Filters.Add(new Filter(filterKey, filterValue));
            var response = dbProxy.Get(collection, dbQuery);
            if (response.Count != 0)
            {
                return response[0] as JObject;
            }
            else
            {
                return null;
            }
        }

        public static JObject FirstOrDefault(this IDBService dbProxy, string collection, Dictionary<string, string> filters, bool isOverrideCheck = false)
        {
            return FirstOrDefault(dbProxy, collection, QueryBuilder(filters), isOverrideCheck);
        }

        public static JObject FirstOrDefault(this IDBService dbProxy, string collection, DBQuery filterQuery, bool isOverrideCheck = false)
        {
            var response = dbProxy.Get(collection, filterQuery);
            if (response.Count != 0)
            {
                return response[0] as JObject;
            }
            else
            {
                return null;
            }
        }

        public static T FirstOrDefault<T>(this IDBService dbProxy, string collection, string keyValue, bool isOverrideCheck = false)
        {
            return FirstOrDefault<T>(dbProxy, collection, CommonConst.CommonField.DATA_KEY, keyValue, isOverrideCheck);
        }

        public static bool Write<T>(this IDBService dbProxy, string collection, T data)
        {
            JObject jdata = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            return Write(dbProxy, collection, jdata);
        }

        public static bool Write(this IDBService dbProxy, string collection, JObject data)
        {
            return dbProxy.WriteData(collection, data);
        }

        public static bool Write(this IDBService dbProxy, string collection, JObject data, Dictionary<string, string> filters, bool overrideData = false, MergeArrayHandling mergeType = MergeArrayHandling.Union)
        {
            if (dbProxy.Update(collection, QueryBuilder(filters).Filters, data, overrideData, mergeType) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Write(this IDBService dbProxy, string collection, JObject data, string filter, bool overrideData = false, MergeArrayHandling mergeType = MergeArrayHandling.Union)
        {
            if (dbProxy.Update(collection, new RawQuery(filter), data, overrideData, mergeType) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static DBQuery QueryBuilder(Dictionary<string, string> filterInput)
        {
            DBQuery dbQuery = new DBQuery();
            JObject filter = new JObject();
            foreach (var item in filterInput)
            {
                dbQuery.Filters.Add(new Filter(item.Key,item.Value));
            }
            return dbQuery;
        }
    }
}