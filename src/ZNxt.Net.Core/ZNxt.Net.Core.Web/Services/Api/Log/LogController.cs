using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Web.Services.Api.Log
{

    public class LogController:ZNxt.Net.Core.Services.ApiBaseService
    {
        private IDBService _dBService;
        public LogController(IResponseBuilder responseBuilder,ILogger logger, IHttpContextProxy httpContextProxy,IDBService dBService,IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
            :base(httpContextProxy,dBService,logger,responseBuilder)
        {

            _dBService = dBService;
        }
        [Route("/base/log", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL)]
        public JObject GetJS()
        {


            var loggerdb = CommonUtility.GetAppConfigValue("LoggerDb");
            if (string.IsNullOrEmpty(loggerdb))
            {
                loggerdb = "ZNxt_Log";
            }
            _dBService.Init(loggerdb);
            return GetPaggedData(CommonConst.Collection.SERVER_LOGS);



            string collection = "s2f_acadstdsyll_master";
            string queryfields = null;
            //queryfields = _httpContextProxy.GetQueryString("fields");

            //List<string> fields = new List<string>();
            //if (null != queryfields)
            //{
            //    fields = queryfields.Split(',').ToList<string>();
            //}
            string standardProject = @" standard: {
                                            '$map': {
                                                'input': '$standard',
                                                'as': 'st',
                                                'in': {
                                                'key': '$$st.key'
                                                }
                                            }
                                        }";

            string acadsyllProject = @" acadsyll: {
                                            '$map': {
                                                'input': '$acadsyll',
                                                'as': 'ac',
                                                'in': {
                                                    'key': '$$ac.key'
                                                }
                                            }
                                        }";

            string subjgroupProject = @" subjectgroups: {
                                            '$map': {
                                                'input': '$subjectgroups',
                                                'as': 'subjgrp',
                                                'in': {
                                                    'key': '$$subjgrp.key'
                                                }
                                            }
                                        }";
            queryfields = string.IsNullOrEmpty(queryfields) ? "id:1,key:1," : queryfields + ",";
            queryfields += standardProject + "," + acadsyllProject + "," + subjgroupProject;
            //fields.Add("standard");
            //fields.Add("acadsyll");
            //JArray joinData = new JArray();
            //JObject StandardcollectionJoin = GetCollectionJoin("std_id", "s2f_std_master", "id", new List<string> { "name", "key" }, "standard");
            //JObject AcadStdSyllcollectionJoin = GetCollectionJoin("acadsyll_id", "s2f_acadsyllabus_master", "id", new List<string> { "key" }, "acadsyll");
            //joinData.Add(StandardcollectionJoin);
            //joinData.Add(AcadStdSyllcollectionJoin);
            //return GetPaggedData(collection, joinData,null,null,fields);

            List<string> stages = new List<string>();
            stages.Add(@"{$lookup: {
                                        from: 's2f_std_master',
                                        let: {
                                                status: 'active',
                                                std_id: '$std_id'
                                            },
                                            pipeline: [{
                                                $match: {
                                                    $expr: {
                                                        $and: [{
                                                                $eq: ['$$std_id', '$id']
                                                            },
                                                            {
                                                                $eq: ['$$status', 'active']
                                                            }]
                                                            }
                                                        }
                                                    }],
                                            as: 'standard'
                                   }
                            }");
            stages.Add(@"{$lookup: {
                                        from: 's2f_acadsyllabus_master',
                                        localField: 'acadsyll_id',
                                        foreignField: 'id',
                                        as: 'acadsyll'
                                    }
                        }");

            stages.Add(@"{$lookup: {
                                        from: 's2f_subjgroup_master',
                                        localField: 'subjectgroups',
                                        foreignField: 'id',
                                        as: 'subjectgroups'
                                    }
                        }");
            stages.Add(@"{$project: {" + queryfields + "}}");
            string query = @"{aggregate:'" + collection + "', pipeline:[" + string.Join(",", stages.ToArray()) + "],'cursor':{}}";
            JObject jquery = JObject.Parse(query);
            var data =  _dBService.RunCommand<BsonDocument>(jquery);

            return null;

            //string queryfields = null;// _httpContextProxy.GetQueryString("fields");
            //List<string> fields = null;
            //if (null != queryfields)
            //{
            //    fields = queryfields.Split(',').ToList<string>();
            //}
            //JArray joinData = new JArray();
            //JObject ClasscollectionJoin = GetCollectionJoin("class_id", "s2f_class_master", "id", new List<string> { "name", "key" }, "class");
            //JObject AcadClsSyllcollectionJoin = GetCollectionJoin("acadsyll_id", "s2f_acadsyllabus_master", "id", new List<string> { "key" }, "acadsyll");
            //joinData.Add(ClasscollectionJoin);
            //joinData.Add(AcadClsSyllcollectionJoin);
            //return GetPaggedData("s2f_acadclasssyll_master", joinData, null, null, fields);

            //JArray joinData = new JArray();
            //JObject collectionJoin = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USERS, CommonConst.CommonField.USER_ID, new List<string> { "user_id", "user_name", "first_name", "middle_name", "last_name", "user_type", "email", "dob" }, "user");
            //JObject collectionJoinUserInfo = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USER_INFO, CommonConst.CommonField.USER_ID, new List<string> { "user_id", "mobile_number", "gender", "whatsapp_mobile_number" }, "user_info");
            //joinData.Add(collectionJoin);
            // joinData.Add(collectionJoinUserInfo);

            //return GetPaggedData("s2f_org_users", joinData, null, null, new List<string> { "org_key", "groups", "user_id" });

        }

        
    }
}
