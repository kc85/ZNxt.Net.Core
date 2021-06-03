using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;
using System.Linq;
using ZNxt.Net.Core.Consts;
using ZNxt.Net.Core.Helpers;

namespace ZNxt.Identity
{
    public static class SSOConfig
    {
        private const int MOBILE_ACCESS_TOKEN_LIFETIME = 604800; //7  days 

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
            var relyingPartyMobileUrls = CommonUtility.GetAppConfigValue("MobileRelyingPartyUrls");

            if (relyingPartyUrls == null)
            {
                relyingPartyUrls = "";
            }
            if (string.IsNullOrEmpty(appSecret))
            {
                appSecret = "sqaSecret";
            }
            relyingPartyUrls.Split(",");
            var redirectUrisPrefix = relyingPartyUrls.Split(",").ToList();
            if (redirectUrisPrefix.Count == 0)
            {
                redirectUrisPrefix.Add("https://localhost:44373");
            }
            var redirectUris = new List<string>();
            redirectUris.AddRange(redirectUrisPrefix.Select(f => $"{f}/signin-oidc"));
            var postLogoutRedirectUris = redirectUrisPrefix.Select(f => $"{f}/signout-callback-oidc").ToList();
            var frontChannelLogoutUri = $"{redirectUrisPrefix.First()}/signout-oidc";

            //var mobileClient = new Client
            //{
            //    ClientId = "ZNxtCoreAppMobile",
            //    ClientName = "ZNxtCoreAppMobile",
            //    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            //    RequireClientSecret = false,
            //    RedirectUris = { relyingPartyMobileUrls },
            //    PostLogoutRedirectUris = { relyingPartyMobileUrls },
            //    AllowOfflineAccess = true,
            //    RequireConsent = false,
            //    AllowedScopes = { "openid", "profile", "ZNxtCoreAppApi", IdentityServerConstants.StandardScopes.OfflineAccess }
            //};

            var mobile_auth_client = new Client
            {
                ClientId = "mobile_auth_client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                AllowedScopes = { "profile", "ZNxtCoreAppApi", IdentityServerConstants.StandardScopes.OfflineAccess },
                AllowOfflineAccess = true,
                RequireConsent = false,
                AccessTokenLifetime = MOBILE_ACCESS_TOKEN_LIFETIME
            };
            var auth_client = new Client
            {
                ClientId = "auth_client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                ClientSecrets =
                    {
                        new Secret(appSecret.Sha256())
                    },
                AllowedScopes = { "profile", IdentityServerConstants.StandardScopes.OfflineAccess },
                AllowOfflineAccess = true,
                RequireConsent = false,
                AccessTokenLifetime = MOBILE_ACCESS_TOKEN_LIFETIME
            };


            return new[]
            {
             auth_client,
              mobile_auth_client,
                new Client
                {
                    ClientId = "ZNxtApp",
                    ClientName = "ZNxtApp",
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret(appSecret.Sha256()) },
                    RedirectUris = redirectUris,
                    FrontChannelLogoutUri = frontChannelLogoutUri,
                    PostLogoutRedirectUris = postLogoutRedirectUris,
                   // AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile"},
                    RequireConsent = false,
                    RequirePkce = false
                }
            };
        }
    }
}