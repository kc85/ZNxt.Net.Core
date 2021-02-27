using IdentityServer4.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ZNxt.Identity;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Exceptions;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using ZNxt.Net.Core.Model;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Net.Core.Web.Services.SSO
{
    public class OAuthClientService : IOAuthClientService
    {
        protected readonly IDBService _dBService;
        protected readonly ILogger _logger;
        protected readonly IApiGatewayService _apiGatewayService;
        protected const string oauthClientApiPath = "/sso/oauthclient/byname";
        protected const string cachePrefix = "oauthclient_";
        protected readonly IInMemoryCacheService _inMemoryCacheService;
        public OAuthClientService(IDBService dBService, ILogger logger, IApiGatewayService apiGatewayService, IInMemoryCacheService inMemoryCacheService)
        {
            _inMemoryCacheService = inMemoryCacheService;
            _dBService = dBService;
            _logger = logger;
            _apiGatewayService = apiGatewayService;
        }

        public  virtual Client GetClient(string clientId)
        {
            var client = _inMemoryCacheService.Get<Client>($"{cachePrefix}{clientId}");
            if (client == null)
            {
                client = FetchClient(clientId);
            }
            if(client == null)
            {
                 client = SSOConfig.GetClients().FirstOrDefault(f => f.ClientId == clientId);
            }
            return client;

        }
        public Client FetchClient(string clientId)
        {

            var route = _apiGatewayService.GetRouteAsync(CommonConst.ActionMethods.GET, oauthClientApiPath).GetAwaiter().GetResult();
            if (route != null)
            {
                var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.GET, oauthClientApiPath,$"name={clientId}").GetAwaiter().GetResult();
                if (result["code"].ToString() == "1")
                {
                    Client client = new Client()
                    {
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientName = result["data"]["name"].ToString(),
                        ClientSecrets = { new Secret(result["data"]["client_secret"].ToString().Sha256()) },
                        AllowOfflineAccess = true,
                    };
                    var allowedScopes = new List<string>() { "openid", "profile", "ZNxtCoreAppApi" };
                    allowedScopes.AddRange((result["data"]["allowed_scopes"] as JArray).Select(f => f.ToString()).ToList());
                    client.AllowedScopes = allowedScopes;
                    _inMemoryCacheService.Put<Client>($"{cachePrefix}{clientId}", client);
                    return client;
                }
            }
            return null;
        }
    }


}
