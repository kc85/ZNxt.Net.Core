using System.Collections.Generic;
using System.Reflection;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IRouting
    {
        void ReLoadRoutes();
        RoutingModel GetRoute(string Method, string url);
        List<RoutingModel> GetRoutes();

        void PushAssemblyRoute(Assembly assembly);

    }
}