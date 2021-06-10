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
using ZNxt.Net.Core.Web.Models;
using static ZNxt.Net.Core.Consts.CommonConst;

namespace ZNxt.Net.Core.Web.Services.SSO
{
    public class OAuthClientService : IOAuthClientService
    {
        protected readonly IDBService _dBService;
        protected readonly ILogger _logger;
        protected readonly IApiGatewayService _apiGatewayService;
        protected const string oauthClientApiPath = "/sso/oauthclient/byid";
        protected const string cachePrefix = "oauthclient_";
        protected readonly IInMemoryCacheService _inMemoryCacheService;
        public OAuthClientService(IDBService dBService, ILogger logger, IApiGatewayService apiGatewayService, IInMemoryCacheService inMemoryCacheService)
        {
            _inMemoryCacheService = inMemoryCacheService;
            _dBService = dBService;
            _logger = logger;
            _apiGatewayService = apiGatewayService;
        }

        public  virtual OAuthClient GetClient(string clientId)
        {
            var client = _inMemoryCacheService.Get<OAuthClient>($"{cachePrefix}{clientId}");
            if (client == null)
            {
                var cln = SSOConfig.GetClients().FirstOrDefault(f => f.ClientId == clientId);
                if (cln != null)
                {
                    client = new OAuthClient {Client = cln };
                }
            }
            if(client == null)
            {
                client = FetchClient(clientId);
            }
            return client;

        }
        public OAuthClient FetchClient(string clientId)
        {
            var route = _apiGatewayService.GetRouteAsync(CommonConst.ActionMethods.GET, oauthClientApiPath).GetAwaiter().GetResult();
            if (route != null)
            {
                var result = _apiGatewayService.CallAsync(CommonConst.ActionMethods.GET, oauthClientApiPath, $"client_id={clientId}").GetAwaiter().GetResult();
                if (result[CommonConst.CommonField.HTTP_RESPONE_CODE].ToString() == CommonConst._1_SUCCESS.ToString())
                {
                    var clientname = result["data"]["client_id"].ToString();
                    if (result["data"]["name"] != null)
                    {
                        clientname = result["data"]["name"].ToString();
                    }
                    var tenantId = string.Empty;
                    if (result["data"]["tenant_id"] != null)
                    {
                        tenantId = result["data"]["tenant_id"].ToString();
                    }
                    var roles = new List<string>();

                    if (result["data"]["roles"] != null)
                    {
                        roles.AddRange((result["data"]["roles"] as JArray).Select(f => f.ToString()));
                    }

                    var salt = string.Empty;
                    if (result["data"]["salt"] != null)
                    {
                        salt = result["data"]["salt"].ToString();
                    }

                    var encKey = string.Empty;
                    if (result["data"]["encryption_key"] != null)
                    {
                        encKey = result["data"]["encryption_key"].ToString();
                    }
                    var ips = new List<string>();

                    if (result["data"]["ips"] != null)
                    {
                        foreach (var ip in result["data"]["ips"] as JArray)
                        {
                            ips.Add(ip["ip"].ToString());
                        }
                    }
                    var client = new OAuthClient
                    {
                        Client = new Client()
                        {
                            AllowedGrantTypes = GrantTypes.ClientCredentials,
                            ClientName = clientname,
                            ClientId = result["data"]["client_id"].ToString(),
                            ClientSecrets = { new Secret(result["data"]["client_secret"].ToString()) },
                            AllowOfflineAccess = true,
                        },
                        Secret = result["data"]["client_secret"].ToString(),
                        TenantId = tenantId,
                        Roles = roles,
                        Salt = salt,
                        EncryptionKey = encKey,
                        IPs = ips

                    };
                    var allowedScopes = new List<string>() { "openid", "profile", "ZNxtCoreAppApi" };
                    if (result["data"]["allowed_scopes"] != null)
                    {
                        allowedScopes.AddRange((result["data"]["allowed_scopes"] as JArray).Select(f => f.ToString()).ToList());
                    }

                    client.Client.AllowedScopes = allowedScopes;
                    _inMemoryCacheService.Put<OAuthClient>($"{cachePrefix}{clientId}", client);
                    return client;
                }
            }
            return null;
        }
    }


}
