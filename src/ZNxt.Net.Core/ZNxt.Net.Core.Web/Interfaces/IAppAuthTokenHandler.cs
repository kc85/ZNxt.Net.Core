using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;
using ZNxt.Net.Core.Web.Models;

namespace ZNxt.Net.Core.Web.Interfaces
{
    public interface IAppAuthTokenHandler
    {
        bool IsInAction();
        TimeSpan GetLoginDuration();
        AppTokenModel GetTokenModel(string OAuthClientId, string token);
        string LoginFailRedirect();

        UserModel GetUser(AppTokenModel token);
    }
}
