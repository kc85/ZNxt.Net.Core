using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Module.Identity.MySql.Services.API.Models;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.MySql.Services.API
{
    public class UserJsController : ZNxt.Net.Core.Services.ApiBaseService
    {
        public const string OAUTH_CLIENT_TABLE = "oauth_client";
        public const string OAUTH_CLIENT_ROLE_TABLE = "oauth_client_role";
        public const string OAUTH_CLIENT_TENANT_TABLE = "oauth_client_tenant";
        protected readonly IResponseBuilder _responseBuilder;
        protected readonly IHttpContextProxy _httpContextProxy;
        protected readonly ILogger _logger;
        private readonly IRDBService _rDBService;

        private readonly IZNxtUserService _zNxtUserService;
        public UserJsController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService,IRDBService rDBService, IZNxtUserService zNxtUserService)
       : base(httpContextProxy, dBService, logger, responseBuilder)
        {

            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _rDBService = rDBService;
            _zNxtUserService = zNxtUserService;
        }

        [Route("/sso/js/user", CommonConst.ActionMethods.GET, "user", "application/javascript")]
        public string GetUserJs()
        {

            try
            {
                _logger.Debug("Calling Get User");
                var response = new StringBuilder();
                var user = _httpContextProxy.User;
                if (user != null)
                {
                    var userModel = new JObject();
                    var userData = _zNxtUserService.GetUser(user.user_id);
                    if (userData != null)
                    {
                        var model = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(userData.ToJObject().ToString());
                        userModel = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(model));
                    }
                    response.AppendLine($"var __userData = {userModel.ToString() };");
                }
                else
                {
                    response.AppendLine($"var __userData = {_responseBuilder.Unauthorized().ToString() };");
                }
                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error in GetJs {0}", ex.Message), ex);
                return $"/****Error {ex.Message} , {ex.StackTrace }****/";
            }
        }
    }
}
