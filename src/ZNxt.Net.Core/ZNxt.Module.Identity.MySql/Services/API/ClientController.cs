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
    public class ClientController : ZNxt.Net.Core.Services.ApiBaseService
    {
        public const string OAUTH_CLIENT_TABLE = "oauth_client";
        public const string OAUTH_CLIENT_ROLE_TABLE = "oauth_client_role";
        public const string OAUTH_CLIENT_TENANT_TABLE = "oauth_client_tenant";
        protected readonly IResponseBuilder _responseBuilder;
        protected readonly IHttpContextProxy _httpContextProxy;
        protected readonly ILogger _logger;
        private readonly IRDBService _rDBService;

        private readonly IZNxtUserService _zNxtUserService;
        public ClientController(IResponseBuilder responseBuilder, ILogger logger, IHttpContextProxy httpContextProxy, IDBService dBService,IRDBService rDBService, IZNxtUserService zNxtUserService)
       : base(httpContextProxy, dBService, logger, responseBuilder)
        {

            _responseBuilder = responseBuilder;
            _httpContextProxy = httpContextProxy;
            _logger = logger;
            _rDBService = rDBService;
            _zNxtUserService = zNxtUserService;
        }

        [Route("/sso/oauthclient", CommonConst.ActionMethods.GET, CommonConst.CommonField.SYS_ADMIN_ROLE)]
        public JObject GetOAuthClient()
        {
            try
            {
                int pagesize = 100;
                int currentpage = 1;
                var responedata = _rDBService.Get<OAuthClientModelDbo>(OAUTH_CLIENT_TABLE, 100, 0, new JObject() {["oauth_client"] = true });

                List<OAuthClientModelDto> clientDto = new List<OAuthClientModelDto>();
                foreach (var item in responedata.ToList())
                {
                    clientDto.Add(GetDto(item, GetOAuthClientRoles(item)));
                }
                return _responseBuilder.SuccessPaggedData(clientDto.ToJArray(),currentpage, pagesize);

            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
               return _responseBuilder.ServerError();
            }
        }
        [Route("/sso/oauthclient/byid", CommonConst.ActionMethods.GET, CommonConst.CommonField.API_AUTH_TOKEN)]
        public JObject GetOAuthClientById()
        {
            try
            {
                string clientid = _httpContextProxy.GetQueryString("client_id");
                JObject filter = new JObject()
                {
                    ["client_id"] = clientid,
                    ["is_enabled"] = true
                };
                var data = _rDBService.Get<OAuthClientModelDbo>(OAUTH_CLIENT_TABLE, 1, 0, filter);
                if (data.Any())
                {
                    var client = data.First();
                    OAuthClientModelDto clientDto =  GetDto(client, GetOAuthClientRoles(client));
                    return _responseBuilder.Success(clientDto.ToJObject());
                }
                else
                {
                    return _responseBuilder.NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                return _responseBuilder.ServerError();
            }
        }

        private OAuthClientModelDto GetDto(OAuthClientModelDbo client, List<string> roles)
        {
            return new OAuthClientModelDto()
            {
                client_id = client.client_id,
                client_secret = client.client_secret,
                description = client.description,
                tenant_id = client.tenant_id,
                salt = client.salt,
                roles = roles
            };
        }

        private List<string> GetOAuthClientRoles(OAuthClientModelDbo client)
        {
            var roles = new List<string>();
            JObject filter = new JObject()
            {
                ["oauth_client_id"] = client.oauth_client_id,
                ["is_enabled"] = true
            };
            var roledata = _rDBService.Get<OAuthClientRoleModelDbo>(OAUTH_CLIENT_ROLE_TABLE, 100, 0, filter);
            foreach (var item in roledata)
            {
                var role = _zNxtUserService.GetRoleById(item.role_id);
                if (role != null)
                {
                    roles.Add(role["name"].ToString());
                }
            }
            return roles;
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
