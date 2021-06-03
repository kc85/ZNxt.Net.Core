using System;
using System.Collections.Generic;
using System.Text;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IAppAuthTokenHandler
    {
        bool IsInAction();
        TimeSpan GetLoginDuration();
        AppTokenModel GetTokenModel(string OAuthClientId, string token);
        string LoginFailRedirect();

        UserModel GetUser(AppTokenModel token);
        bool Validate(string username, string token);
    }
}
