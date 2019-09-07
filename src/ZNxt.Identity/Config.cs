// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace ZNxt.Identity
{
    public static class Config
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
            return new[]
            {
                // MVC client using hybrid flow
                new Client
                {
                    ClientId = "ZNxtCoreApp",
                    ClientName = "ZNxtCoreApp",

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets = { new Secret("MySecret".Sha256()) },

                    RedirectUris = { "https://localhost:44373/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:44373/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:44373/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "ZNxtCoreAppApi" }
                },
                new Client
                {
                    ClientId = "s2ftechnologies",
                    ClientName = "s2ftechnologies",

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets = { new Secret("MySecret".Sha256()) },

                    RedirectUris = { "http://s2ftechnologies.com/signin-oidc" },
                    FrontChannelLogoutUri = "http://s2ftechnologies.com/signout-oidc",
                    PostLogoutRedirectUris = { "http://s2ftechnologies.com/signout-callback-oidc" },
                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "ZNxtCoreAppApi" }
                }
            };
        }
    }
}