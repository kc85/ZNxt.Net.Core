using IdentityServer4.Models;

namespace ZNxt.Net.Core.Web.Services.SSO
{
    public interface IOAuthClientService
    {
        Client GetClient(string clientId);
        Client FetchClient(string clientId);
    }
}