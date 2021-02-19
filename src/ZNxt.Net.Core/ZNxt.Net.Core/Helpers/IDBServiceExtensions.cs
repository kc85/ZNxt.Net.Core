using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Helpers
{
    public static class IRDBServiceExtensions
    {
        public static string GetInsertSQLWithParam(this IRDBService dbService, JObject data, string table)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"INSERT INTO {table}");

            var columns = new List<string>();
            var values = new List<string>();
            foreach (var item in data)
            {
                columns.Add(item.Key);
                if (item.Value.Type == JTokenType.Integer || item.Value.Type == JTokenType.Boolean)
                {
                    values.Add(item.Value.ToString().ToLower());
                }
                else
                {

                    values.Add($"'{item.Value.ToString()}'");
                }
            }
            sb.Append($"([{string.Join("],[", columns) }])");
            sb.Append($" values ({string.Join(",", values) })");
            return sb.ToString();
        }
        //public static object  GetSQLParam(this IRDBService dbService, JObject data)
        //{
        //    //dynamic param = new ExpandoObject();
        //    //var values = new List<string>();
        //    //foreach (var item in data)
        //    //{
        //    //    param[item.Key] = item.Value;
        //    //}
        //    Dictionary<string, string> dataDic = new Dictionary<string, string>();
        //    foreach (var item in data)
        //    {
        //        dataDic[item.Key] = item.Value.ToString();

        //    }
        //    return dataDic;
        //}
    }
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
            var response = dbProxy.Get(collection, new RawQuery("{" + filterKey + ":'" + filterValue + "'}"));
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

        public static JObject FirstOrDefault(this IDBService dbProxy, string collection, IDBQueryBuilder filterQuery, bool isOverrideCheck = false)
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
            if (dbProxy.Update(collection, QueryBuilder(filters), data, overrideData, mergeType) != 0)
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
        public static  void OverrideData(this IDBService dbProxy,JObject joData, string moduleName, string compareKey, string collection)
        {
            string updateOverrideFilter = "{ $and: [ { " + CommonConst.CommonField.IS_OVERRIDE + ":false }, {" + compareKey + ":'" + joData[compareKey].ToString() + "'}] } ";
            var updateObject = new JObject();
            updateObject[CommonConst.CommonField.ÌS_OVERRIDE] = true;
            JArray lastOverrides = new JArray();
            if (updateObject[CommonConst.CommonField.OVERRIDE_BY] != null)
            {
                lastOverrides = updateObject[CommonConst.CommonField.OVERRIDE_BY] as JArray;
            }
            lastOverrides.Add(moduleName);
            updateObject[CommonConst.CommonField.OVERRIDE_BY] = lastOverrides;
            updateObject[CommonConst.CommonField.OVERRIDE_BY] = moduleName;
            dbProxy.Write(collection, updateObject, updateOverrideFilter);
        }

        private static RawQuery QueryBuilder(Dictionary<string, string> filterInput)
        {
            JObject filter = new JObject();
            foreach (var item in filterInput)
            {
                filter[item.Key] = item.Value;
            }
            return new RawQuery(filter.ToString());
        }
    }
}