using System;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.Web.Services
{
    public class SessionProvider : ISessionProvider
    {
        public T GetValue<T>(string key)
        {
            throw new NotImplementedException();
        }

        public void ResetSession()
        {
            throw new NotImplementedException();
        }

        public void SetValue<T>(string key, T value)
        {
            throw new NotImplementedException();
        }
    }
}
