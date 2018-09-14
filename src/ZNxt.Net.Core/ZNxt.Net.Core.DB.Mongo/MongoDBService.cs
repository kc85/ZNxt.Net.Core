using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.DB.Mongo
{
    public class MongoDBService : IDBService
    {
        private IMongoDatabase _mongoDataBase;
        private MongoClient _mongoClient;
        public Func<string> User;
        private const string DUPLICATE_KEY_ERROR = "duplicate key error";


        private readonly string _dbName;

        public MongoDBService(string dbName)
        {
            _dbName = dbName;
            Init();
        }
        private void Init()
        {
            _mongoClient = new MongoClient(ApplicationConfig.MongoDBConnectionString);
            _mongoDataBase = _mongoClient.GetDatabase(_dbName);
        }
        public long Delete(string collection, FilterQuery filters)
        {
            return Delete(collection, new MongoQueryBuilder(filters));
        }

        public long Delete(string collection, IDBQueryBuilder query)
        {
            var result = _mongoDataBase.GetCollection<BsonDocument>(collection).DeleteMany(query.GetQuery());
            return result.DeletedCount;
        }

        public bool DropDB()
        {
            try
            {
                _mongoClient.DropDatabase(_dbName);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public JArray Get(string collection, DBQuery query, int? top = null, int? skip = null)
        {
            return Get(collection, new MongoQueryBuilder(query.Filters), GetProperties(query), GetSortBy(query), top, skip);
        }

        public JArray Get(string collection, IDBQueryBuilder query, List<string> properties = null, Dictionary<string, int> sortColumns = null, int? top = null, int? skip = null)
        {
            var findOptions = new FindOptions<BsonDocument>();

            GetFilterProperty(properties, top, skip, findOptions);
            JObject sort = new JObject();
            if (sortColumns != null)
            {
                foreach (var col in sortColumns)
                {
                    sort[col.Key] = col.Value;
                }
            }
            findOptions.Sort = sort.ToString();
            return GetData(collection, query.GetQuery(), findOptions);
        }

        public long GetCount(string collection, FilterQuery filters)
        {
            return GetCount(collection, new MongoQueryBuilder(filters));
        }

        public long GetCount(string collection, IDBQueryBuilder query)
        {
            IMongoCollection<BsonDocument> queryFilter = _mongoDataBase.GetCollection<BsonDocument>(collection);
            long result = queryFilter.Find(GetFilter(query.GetQuery())).CountDocuments();
            return result;
        }

        public JObject GetPageData(string collection, IDBQueryBuilder query, List<string> fields = null, Dictionary<string, int> sortColumns = null, int pageSize = 10, int currentPage = 1)
        {
            throw new NotImplementedException();
        }

        public long Update(string collection, FilterQuery filters, JObject data, bool overrideData = false, MergeArrayHandling mergeType = MergeArrayHandling.Union)
        {
            return Update(collection, new MongoQueryBuilder(filters), data, overrideData, mergeType);
        }

        public long Update(string collection, IDBQueryBuilder query, JObject data, bool overrideData = false, MergeArrayHandling mergeType = MergeArrayHandling.Union)
        {
            throw new NotImplementedException();
        }

        public bool WriteData(string collection, JObject data)
        {
            throw new NotImplementedException();
        }

        private List<string> GetProperties(DBQuery query)
        {
            List<string> fields = new List<string>();
            foreach (var field in query.Fields)
            {
                fields.Add(field.Name);
            }
            return fields;
        }
        private Dictionary<string, int> GetSortBy(DBQuery query)
        {
            Dictionary<string, int> sort = new Dictionary<string, int>();
            foreach (var sortBy in query.SortBy)
            {
                sort[sortBy.Name] = (int)sortBy.Sort;
            }
            return sort;
        }

        private static void GetFilterProperty(List<string> properties, int? top, int? skip, FindOptions<BsonDocument> findOptions)
        {
            findOptions.Limit = top;
            findOptions.Skip = skip;
            if (properties != null)
            {
                JObject objProjection = new JObject();

                foreach (var item in properties)
                {
                    objProjection[item] = 1;
                }
                findOptions.Projection = objProjection.ToString();
            }
        }
        private JArray GetData(string collection, string bsonQuery, FindOptions<BsonDocument> findOptions)
        {
            IMongoCollection<BsonDocument> query = _mongoDataBase.GetCollection<BsonDocument>(collection);
            JArray resultData = new JArray();

            using (var cursor = query.FindAsync<BsonDocument>(GetFilter(bsonQuery), findOptions).Result)
            {
                while (cursor.MoveNext())
                {
                    var batch = cursor.Current;
                    foreach (BsonDocument document in batch)
                    {
                        var documentJson = BsonExtensionMethods.ToJson(document, new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict });
                        var jobjData = JObject.Parse(documentJson);
                        jobjData.Remove(CommonConst.CommonField.ID);
                        resultData.Add(jobjData);
                    }
                }
            }
            return resultData;
        }

        private FilterDefinition<BsonDocument> GetFilter(string query)
        {
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Empty;
            filter &= MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(query);
            return query;
        }

    }
}
