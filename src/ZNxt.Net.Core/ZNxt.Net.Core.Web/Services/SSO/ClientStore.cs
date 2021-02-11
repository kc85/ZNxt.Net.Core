using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZNxt.Identity;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services.SSO
{
    public class ClientStore : IClientStore
    {
        private readonly IOAuthClientService _oAuthClientService;
        public ClientStore(IOAuthClientService oAuthClientService)
        {
            _oAuthClientService = oAuthClientService;
        }
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return Task.FromResult(_oAuthClientService.GetClient(clientId));
        }
    }
}
