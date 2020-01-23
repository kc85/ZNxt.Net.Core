using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Services
{
    public abstract class ApiBaseService
    {
        private readonly IHttpContextProxy HttpProxy;
        private readonly ILogger Logger;
        private readonly IDBService DBProxy;
        private readonly IResponseBuilder _responseBuilder;

        public ApiBaseService(IHttpContextProxy httpContextProxy, IDBService dBService, ILogger logger, IResponseBuilder responseBuilder)

        {
            HttpProxy = httpContextProxy;
            Logger = logger;
            DBProxy = dBService;
            _responseBuilder = responseBuilder;

        }

        protected JObject GetPaggedData(string collection, JArray joins = null, string overrideFilters = null, Dictionary<string, int> sortColumns = null, List<string> fields = null)
        {
            int pageSize = 10, pageSizeData = 0;
            int currentPage = 1, currentPageData = 0;
            if (int.TryParse(HttpProxy.GetQueryString("pagesize"), out pageSizeData))
            {
                pageSize = pageSizeData;
            }

            if (int.TryParse(HttpProxy.GetQueryString("currentpage"), out currentPageData))
            {
                currentPage = currentPageData;
            }
            string filterQuery = HttpProxy.GetQueryString("filter");

            if (string.IsNullOrEmpty(filterQuery))
            {
                filterQuery = CommonConst.EMPTY_JSON_OBJECT;
            }
            if (!string.IsNullOrEmpty(overrideFilters))
            {
                filterQuery = overrideFilters;
            }

            var sortData = HttpProxy.GetQueryString("sort");

            if (sortData != null)
            {
                sortColumns = (Dictionary<string, int>)JsonConvert.DeserializeObject<Dictionary<string, int>>(sortData);
            }
            if (sortColumns == null)
            {
                sortColumns = new Dictionary<string, int>();
                sortColumns[CommonConst.CommonField.CREATED_DATA_DATE_TIME] = -1;
            }

            if (fields == null)
            {
                fields = new List<string>();
            }
            if (HttpProxy.GetQueryString("fields") != null)
            {
                fields = new List<string>();
                fields.AddRange(HttpProxy.GetQueryString("fields").Split(','));
            }
            DBQuery query = new DBQuery();
            foreach (var field in fields)
            {
                query.Fields.Add(new Field(field));
            }
            foreach (var sort in sortColumns)
            {
                query.SortBy.Add(new SortField(sort.Key, (SortType)sort.Value));
            }

            var data = GetPagedData(collection, query, filterQuery, pageSize, currentPage);
            if (data[CommonConst.CommonField.DATA] != null && (data[CommonConst.CommonField.DATA] as JArray).Count > 0)
            {
                DoJoins(data, collection, joins);
            }
            return data;
        }

        protected JObject GetCollectionJoin(string soureField, string destinationCollection, string destinationJoinField, List<string> fields, string valueKey)
        {
            JObject collectionJoin = new JObject();
            collectionJoin[CommonConst.CommonField.DB_JOIN_DESTINATION_COLELCTION] = destinationCollection;
            collectionJoin[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELD] = destinationJoinField;
            collectionJoin[CommonConst.CommonField.DB_JOIN_SOURCE_FIELD] = soureField;
            collectionJoin[CommonConst.CommonField.DB_JOIN_VALUE] = valueKey;
            if (fields != null)
            {
                JArray jarrFields = new JArray();
                foreach (var item in fields)
                {
                    jarrFields.Add(item);
                }
                collectionJoin[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELDS] = jarrFields;
            }
            return collectionJoin;
        }

        protected JObject GetPagedData(string collection, DBQuery query, string rawQuery, int pageSize = 10, int currentPage = 1)
        {
            int? top = null;
            int? skip = null;

            top = pageSize;
            skip = (pageSize * (currentPage - 1));
            Logger.Debug(string.Format("GetPageData. Top:{0} Skip:{1} Query:{2}", top, skip, query));
            var sort = new Dictionary<string, int>();
            foreach (var item in query.SortBy)
            {
                sort[item.Name] = item.Sort == SortType.ASC ? 1 : -1;
            }
            var dbArrData = DBProxy.Get(collection, new RawQuery(rawQuery), query.Fields.Select(f => f.Name).ToList(), sort, top, skip);
            JObject extraData = new JObject();
            long count = DBProxy.GetCount(collection, new RawQuery(rawQuery));
            extraData[CommonConst.CommonField.TOTAL_RECORD_COUNT_KEY] = count;
            extraData[CommonConst.CommonField.TOTAL_PAGES_KEY] = Math.Ceiling(((double)count / pageSize));
            extraData[CommonConst.CommonField.PAGE_SIZE_KEY] = pageSize;
            extraData[CommonConst.CommonField.CURRENT_PAGE_KEY] = currentPage;

            return _responseBuilder.Success(dbArrData, extraData);
        }

        private void DoJoins(JObject data, string sourceCollection, JArray joins)
        {
            if (joins != null)
            {
                Dictionary<string, List<string>> collectionIds = new Dictionary<string, List<string>>();

                // get the join keys
                foreach (JObject join in joins)
                {
                    collectionIds[join[CommonConst.CommonField.DB_JOIN_SOURCE_FIELD].ToString()] = new List<string>();
                }

                // get the join ids
                if (data[CommonConst.CommonField.DATA] == null)
                {
                    return;
                }

                foreach (JObject item in data[CommonConst.CommonField.DATA] as JArray)
                {
                    foreach (var joinColumn in collectionIds)
                    {
                        if (item[joinColumn.Key] != null)
                        {
                            joinColumn.Value.Add(item[joinColumn.Key].ToString());
                        }
                    }
                }
                foreach (var join in joins)
                {
                    var joinCoumnId = collectionIds[join[CommonConst.CommonField.DB_JOIN_SOURCE_FIELD].ToString()]; ;
                    {
                        RawQuery query = new RawQuery(GetJoinFilter(join, joinCoumnId));
                        JoinToDestination(data, join, query, GetJoinDestinationFields(join));
                    }
                }
            }
        }
        private void JoinToDestination(JObject data, JToken join, RawQuery query, List<string> fields)
        {
            JArray joinCollectionData = DBProxy.Get(join[CommonConst.CommonField.DB_JOIN_DESTINATION_COLELCTION].ToString(), query, fields);
            foreach (JObject joinData in joinCollectionData)
            {
                if (joinData[join[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELD].ToString()] != null)
                {
                    var joinid = joinData[join[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELD].ToString()].ToString();

                    var dataArr = (data[CommonConst.CommonField.DATA] as JArray).Where(f => f[join[CommonConst.CommonField.DB_JOIN_SOURCE_FIELD].ToString()].ToString() == joinid);
                    if (dataArr.Any())
                    {
                        foreach (var dataJoin in dataArr)
                        {
                            if (dataJoin[join[CommonConst.CommonField.DB_JOIN_VALUE].ToString()] == null)
                            {
                                dataJoin[join[CommonConst.CommonField.DB_JOIN_VALUE].ToString()] = new JArray();
                            }
                        (dataJoin[join[CommonConst.CommonField.DB_JOIN_VALUE].ToString()] as JArray).Add(JObject.Parse(joinData.ToString()));
                        }

                    }
                }
            }
        }
        private List<string> GetJoinDestinationFields(JToken join)
        {
            List<string> fields = new List<string>();
            if (join[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELDS] != null)
            {
                fields.Add(join[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELD].ToString());
                foreach (var field in join[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELDS] as JArray)
                {
                    fields.Add(field.ToString());
                }
            }
            else
            {
                fields = null;
            }
            return fields;
        }
        private string GetJoinFilter(JToken join, List<string> values)
        {

            string filters = "{$or : {{filter}}}";
            var filterArr = new JArray();
            foreach (var item in values)
            {
                filterArr.Add(new JObject()
                {
                    [join[CommonConst.CommonField.DB_JOIN_DESTINATION_FIELD].ToString()] = item
                });
            }
            filters = filters.Replace("{{filter}}", filterArr.ToString());

            return filters;

        }
    }
}