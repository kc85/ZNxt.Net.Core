using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;

namespace ZNxt.Module.Identity.MySql.Services.API
{
    public class ClientController : ZNxt.Net.Core.Services.ApiBaseService
    {
        private const string CollectionName = "oauth_clients";
        
        protected readonly IResponseBuilder _responseBuilder;
        protected readonly IHttpContextProxy _httpContextProxy;
        protected readonly ILogger _logger;
        private readonly IRDBService _rDBService;

        public ClientController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService,IRDBService rDBService)
       : base(httpContextProxy, dBService, logger, responseBuilder)
        {

            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _rDBService = rDBService;
        }

        [Route("/sso/oauthclient", CommonConst.ActionMethods.GET, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject GetOAuthClient()
        {
            return _responseBuilder.NotFound();
        }
        [Route("/sso/oauthclient/byname", CommonConst.ActionMethods.GET, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject GetOAuthClientByName()
        {
            return _responseBuilder.NotFound();
        }
        [Route("/sso/oauthclient/byid", CommonConst.ActionMethods.GET, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject GetOAuthClientById()
        {
            return _responseBuilder.NotFound();
        }
        [Route("/sso/oauthclient/edit", CommonConst.ActionMethods.POST, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject EditClient()
        {
            return _responseBuilder.NotFound();
        }
        [Route("/sso/oauthclient/add", CommonConst.ActionMethods.POST, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject AddClient()
        {
            return _responseBuilder.NotFound();
        }
    }
}
