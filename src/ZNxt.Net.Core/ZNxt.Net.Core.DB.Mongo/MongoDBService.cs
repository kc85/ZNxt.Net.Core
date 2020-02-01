using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Config;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Exceptions;
using ZNxt.Net.Core.Exceptions.ErrorCodes;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.DB.Mongo
{
    public class MongoDBService : IDBService
    {
        private IMongoDatabase _mongoDataBase;
        private static MongoClient _mongoClient;
        private IDBServiceConfig _DBConfig;
        private readonly IHttpContextProxy _httpContextProxy;
        private const string DUPLICATE_KEY_ERROR = "duplicate key error";
        public bool IsConnected { get; private set; }
        public MongoDBService(IDBServiceConfig DBConfig, IHttpContextProxy httpContextProxy)
        {
            try
            {
                _httpContextProxy = httpContextProxy;
                _DBConfig = DBConfig;
                if (ApplicationConfig.AppInstallStatus != Enums.AppInstallStatus.DBNotSet)
                {
                    Init();
                    IsConnected = true;
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch (Exception)
            {
                IsConnected = false;
            }
        }
        public void Init(string dbName = null, string connectionString = null)
        {
            if (dbName == null)
            {
                dbName = _DBConfig.DBName;
            }
            if (connectionString == null)
            {
                connectionString = _DBConfig.ConnectingString;
            }
            if (_mongoClient == null)
            {
                _mongoClient = new MongoClient(connectionString);
            }
            _mongoDataBase = _mongoClient.GetDatabase(dbName);
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
                _mongoClient.DropDatabase(_DBConfig.DBName);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
        public bool DropCollection(string collection)
        {
            _mongoDataBase.DropCollection(collection);
            return true;
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
            data[CommonConst.CommonField.UPDATED_DATE_TIME] = CommonUtility.GetUnixTimestamp(DateTime.Now);
            data[CommonConst.CommonField.UPDATED_BY] = GetUserId();
            var dataResut = Get(collection, query, null, null);
            var dbcollection = _mongoDataBase.GetCollection<BsonDocument>(collection);
            if (overrideData)
            {
                if (dataResut.Count > 1)
                {
                    throw new InvalidFilterException((int)ErrorCode.DB.MULTIPLE_ROW_RETURNED, string.Format("Update replace command cannot execute in multiple rows"));
                }
                if (dataResut.Count == 1)
                {
                    (dataResut[0] as JObject).Merge(data, new JsonMergeSettings
                    {
                        MergeArrayHandling = mergeType
                    });

                    if (data[CommonConst.CommonField.ID] != null)
                    {
                        dataResut[0][CommonConst.CommonField.ID] = data[CommonConst.CommonField.ID];
                    }
                    BsonDocument document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(dataResut[0].ToString());
                    ReplaceOneResult result = dbcollection.ReplaceOne(GetFilter(query.GetQuery()), document, new UpdateOptions() { IsUpsert = true });
                    if (dataResut.Count != result.ModifiedCount)
                    {
                        throw new ClientValidationError((int)ErrorCode.DB.UPDATE_DATA_COUNT_NOT_MATCH, ErrorCode.DB.UPDATE_DATA_COUNT_NOT_MATCH.ToString(), null);
                    }
                }
                else
                {
                    WriteData(collection, data);
                    return 1;
                }
            }
            else
            {
                foreach (var item in dataResut)
                {
                    (item as JObject).Merge(data, new JsonMergeSettings
                    {
                        MergeArrayHandling = mergeType
                    });
                }
                foreach (var item in dataResut)
                {
                    if (data[CommonConst.CommonField.ID] != null)
                    {
                        item[CommonConst.CommonField.ID] = data[CommonConst.CommonField.ID];
                    }
                    BsonDocument document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(item.ToString());

                    // todo
                    string filter = "{" + CommonConst.CommonField.DISPLAY_ID + " : '" + item[CommonConst.CommonField.DISPLAY_ID].ToString() + "'}";
                    ReplaceOneResult result = dbcollection.ReplaceOne(GetFilter(filter), document, new UpdateOptions() { IsUpsert = false });
                }
            }
            return dataResut.Count;
        }


        public bool WriteData(string collection, JObject data)
        {
            try
            {
                UpdateCommonData(data);
                data[CommonConst.CommonField.CREATED_DATA_DATE_TIME] = CommonUtility.GetUnixTimestamp(DateTime.Now);
                data[CommonConst.CommonField.UPDATED_DATE_TIME] = CommonUtility.GetUnixTimestamp(DateTime.Now);
                data[CommonConst.CommonField.CREATED_BY] = GetUserId();
                var dbcollection = _mongoDataBase.GetCollection<BsonDocument>(collection);
                MongoDB.Bson.BsonDocument document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(data.ToString());
                dbcollection.InsertOne(document);
                return true;
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("duplicate key error"))
                {
                    throw new DuplicateDBIDException((int)ErrorCode.DB.DUPLICATE_ID, ErrorCode.DB.DUPLICATE_ID.ToString(), ex);
                }
                else
                {
                    throw;
                }
            }
        }
        private void UpdateCommonData(Newtonsoft.Json.Linq.JObject data)
        {
            if (data[CommonConst.CommonField.ID] == null)
            {
                data[CommonConst.CommonField.ID] = CommonUtility.GetNewID();
            }
            if (data[CommonConst.CommonField.ID] == null && data[CommonConst.CommonField.DISPLAY_ID] != null)
            {
                data[CommonConst.CommonField.ID] = data[CommonConst.CommonField.DISPLAY_ID];
            }
            else if (data[CommonConst.CommonField.ID] != null && data[CommonConst.CommonField.DISPLAY_ID] == null)
            {
                data[CommonConst.CommonField.DISPLAY_ID] = data[CommonConst.CommonField.ID];
            }
        }
        private string GetUserId()
        {
            var user = _httpContextProxy.User;
            return user != null ? user.user_id : "";
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
        public JObject RunCommand(JObject command)
        {

            var result =  _mongoDataBase.RunCommand<JObject>(command.ToString());
            return JObject.Parse(result.ToJson());

        }
        public T RunCommand<T>(JObject command)
        {

            return _mongoDataBase.RunCommand<T>(command.ToString());

        }
        public JArray Aggregate(string collection, string stage1, params string[] stages)
        {
            var dbcollection = _mongoDataBase.GetCollection<BsonDocument>(collection);
            var stage = dbcollection.Aggregate().AppendStage<BsonDocument>(BsonDocument.Parse(stage1));
            foreach (var stagecommand in stages)
            {
                stage = stage.AppendStage<BsonDocument>(BsonDocument.Parse(stagecommand));
            }
            return JArray.Parse(stage.ToList().ToJson());
        }
        public JObject UpdateMany(string collection, string updateQuery, string filter, params string[] arrayFilters)
        {
            FilterDefinition<BsonDocument> filterDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filter);
            UpdateDefinition<BsonDocument> update = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(updateQuery);
            var arrFilters = new List<ArrayFilterDefinition>();
            foreach (var f in arrayFilters)
            {
                ArrayFilterDefinition<BsonDocument> arrayFtr = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(f);
                arrFilters.Add(arrayFtr);

            }

            var result = _mongoDataBase.GetCollection<BsonDocument>(collection).UpdateMany(filterDoc, update, new UpdateOptions { ArrayFilters = arrFilters });
            return JObject.Parse(result.ToJson());
        }
    }
}
