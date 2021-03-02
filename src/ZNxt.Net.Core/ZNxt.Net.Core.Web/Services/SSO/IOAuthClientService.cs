using IdentityServer4.Models;
using ZNxt.Net.Core.Web.Models;

namespace ZNxt.Net.Core.Web.Services.SSO
{
    public interface IOAuthClientService
    {
        OAuthClient GetClient(string clientId);
        OAuthClient FetchClient(string clientId);
    }
}