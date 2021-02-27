using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Web.Interfaces;
using ZNxt.Net.Core.Web.Models;

namespace ZNxt.Net.Core.Web.Services
{
    public class AppAuthTokenHandler : IAppAuthTokenHandler
    {
        public TimeSpan GetLoginDuration()
        {
            throw new NotImplementedException();
        }

        public AppTokenModel GetTokenModel(string OAuthClientId, string token)
        {
            throw new NotImplementedException();
        }

        public bool IsInAction()
        {
            return false;
        }

        public string LoginFailRedirect()
        {
            throw new NotImplementedException();
        }
    }
}
