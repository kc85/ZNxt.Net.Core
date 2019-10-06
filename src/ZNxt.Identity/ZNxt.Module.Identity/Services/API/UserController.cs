using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.Services.API
{
   public  class UserController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private readonly IResponseBuilder _responseBuilder;
        private readonly IHttpContextProxy _httpContextProxy;
        private readonly ILogger _logger;
        private readonly IDBService _dBService;
        public UserController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService, IKeyValueStorage keyValueStorage, IStaticContentHandler staticContentHandler)
         : base(httpContextProxy, dBService, logger, responseBuilder)
        {
            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _dBService = dBService;
        }
       
        [Route("/sso/users", CommonConst.ActionMethods.GET, "user")]
        public  JObject Users()
        {
            return GetPaggedData(CommonConst.Collection.USERS);
        }
        [Route("/user/userinfo", CommonConst.ActionMethods.GET, "user")]
        public JObject UserInfoAdmin()
        {
            return UserInfo();
        }
        [Route("/sso/userinfo", CommonConst.ActionMethods.GET, "user")]
        public JObject UserInfo()
        {
            var user_id = _httpContextProxy.GetQueryString(CommonConst.CommonField.USER_ID);
            var data = GetUser(user_id);
            if (data != null)
            {
                return _responseBuilder.Success(data);
            }
            else
            {
                throw new NotSupportedException();
                return _responseBuilder.NotFound();
            }
        }

        [Route("/sso/info/me", CommonConst.ActionMethods.GET, "user")]
        public JObject UserMe()
        {
            if (_httpContextProxy.User != null)
            {
                var data = UserInfoByUserId(_httpContextProxy.User.user_id);
                if (data != null)
                {
                    return _responseBuilder.Success(data);
                }
                else
                {

                    _logger.Debug($"UserInfoByUserId is null UserId: { _httpContextProxy.User.user_id}");
                    return _responseBuilder.Unauthorized();
                }
            }
            else
            {

                throw new NotSupportedException();
                _logger.Debug("_httpContextProxy.User is null");
                return _responseBuilder.Unauthorized();
            }
        }
        private JObject UserInfoByUserId(string user_id)
        {
            var user = _dBService.Get(CommonConst.Collection.USERS, new RawQuery("{'user_id':'" + user_id + "'}"));
            if (user.Count != 0)
            {
                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(user.First().ToString());
                var userResponse = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                return _responseBuilder.Success(userResponse);
            }
            else
            {
                return _responseBuilder.NotFound();
            }
        }

        private JObject GetUser(string user_id)
        {
            JArray joinData = new JArray();
            JObject collectionJoin = GetCollectionJoin(CommonConst.CommonField.USER_ID, CommonConst.Collection.USER_INFO, CommonConst.CommonField.USER_ID, null, CommonConst.CommonField.USER_INFO);
            joinData.Add(collectionJoin);
            JObject filter = new JObject();
            filter[CommonConst.CommonField.USER_ID] = user_id;
            _logger.Debug($"Getting user data {user_id}, filter:{filter.ToString()}");
            var data = GetPaggedData(CommonConst.Collection.USERS, null, filter.ToString());
            _logger.Debug($"Data found {data.Count}");
            if ((data[CommonConst.CommonField.DATA] as JArray).Count != 0)
            {
                return data[CommonConst.CommonField.DATA][0] as JObject;
            }
            else
            {
                return null;
            }
        }

        //[Route("/sso/js/user", CommonConst.ActionMethods.GET, CommonConst.CommonValue.ACCESS_ALL, "application/javascript")]
        //public string GetUserJs()
        //{

        //    try
        //    {
        //        _logger.Debug("Calling Get User");
        //        var response = new StringBuilder();
        //        var user = _httpContextProxy.User;
        //        var userModel = new JObject();
        //        var userData = UserInfoByUserId(user.user_id);
        //        if (userData != null)
        //        {
        //            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(userData.ToString());
        //            userModel = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(model));
        //        }
        //        response.AppendLine($"var __userData = {userModel.ToString() };");
        //        return response.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(string.Format("Error in GetJs {0}", ex.Message), ex);
        //        return $"/****Error {ex.Message} , {ex.StackTrace }****/";
        //    }
        //}
    }
}
