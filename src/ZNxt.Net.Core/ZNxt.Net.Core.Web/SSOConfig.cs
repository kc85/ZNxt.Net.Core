using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;

namespace ZNxt.Identity
{
    public static class SSOConfig
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("ZNxtCoreAppApi", "ZNxtCoreAppApi")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            var appSecret = CommonUtility.GetAppConfigValue(CommonConst.CommonValue.APP_SECRET_CONFIG_KEY);
            var relyingPartyUrls = CommonUtility.GetAppConfigValue("RelyingPartyUrls");
            relyingPartyUrls.Split(",");
            var redirectUrisPrefix = relyingPartyUrls.Split(",").ToList();
            if(redirectUrisPrefix.Count == 0)
            {
                redirectUrisPrefix.Add("https://localhost:44373");
            }
            var redirectUris = new List<string>() ;
            redirectUris.AddRange(redirectUrisPrefix.Select(f => $"{f}/signin-oidc"));
            var postLogoutRedirectUris = redirectUrisPrefix.Select(f => $"{f}/signout-callback-oidc").ToList();
            var frontChannelLogoutUri = $"{redirectUrisPrefix.First()}/signout-oidc";

            return new[]
            {
                //// MVC client using hybrid flow
                //new Client
                //{
                //    ClientId = "ZNxtCoreApp",
                //    ClientName = "ZNxtCoreApp",
                //    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                //    ClientSecrets = { new Secret(appSecret.Sha256()) },
                //    RedirectUris = { "https://localhost:44373/signin-oidc" },
                //    FrontChannelLogoutUri = "https://localhost:44373/signout-oidc",
                //    PostLogoutRedirectUris = { "https://localhost:44373/signout-callback-oidc" },
                //    AllowOfflineAccess = true,
                //    RequireConsent = false,
                //    AllowedScopes = { "openid", "profile", "ZNxtCoreAppApi" }
                //},
                new Client
                {
                    ClientId = "ZNxtApp",
                    ClientName = "ZNxtApp",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets = { new Secret(appSecret.Sha256()) },
                    RedirectUris = redirectUris,
                    FrontChannelLogoutUri = frontChannelLogoutUri,
                    PostLogoutRedirectUris = postLogoutRedirectUris,
                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "ZNxtCoreAppApi" },
                    RequireConsent = false,
                }
            };
        }
    }
}