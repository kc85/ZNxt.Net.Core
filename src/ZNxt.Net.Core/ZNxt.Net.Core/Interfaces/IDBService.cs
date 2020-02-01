using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IDBService
    {
        void Init(string dbName = null, string connectionString = null);

        bool IsConnected{ get; }
        bool WriteData(string collection, JObject data);

        long GetCount(string collection, FilterQuery filters);

        long GetCount(string collection, IDBQueryBuilder query);

        JArray Get(string collection, DBQuery query, int? top = null, int? skip = null);

        JArray Get(string collection, IDBQueryBuilder query, List<string> properties = null, Dictionary<string, int> sortColumns = null, int? top = null, int? skip = null);

        long Delete(string collection, FilterQuery filters);

        long Delete(string collection, IDBQueryBuilder query);

        long Update(string collection, FilterQuery filters, JObject data, bool overrideData = false, MergeArrayHandling mergeType = MergeArrayHandling.Union);

        long Update(string collection, IDBQueryBuilder query , JObject data, bool overrideData = false, MergeArrayHandling mergeType = MergeArrayHandling.Union);

        JObject GetPageData(string collection, IDBQueryBuilder query, List<string> fields = null, Dictionary<string, int> sortColumns = null, int pageSize = 10, int currentPage = 1);

        bool DropDB();
        bool DropCollection(string collection);

        [System.Obsolete("Use T RunCommand<T>(JObject command)")]
        JObject RunCommand(JObject command);
        T RunCommand<T>(JObject command);

        JArray Aggregate(string collection, string stage1, params string[] stages);
        JObject UpdateMany(string collection, string updateQuery, string filter, params string[] arrayFilters);


    }

    public enum SortBy
    {
        Ascending,
        Descending
    }
}