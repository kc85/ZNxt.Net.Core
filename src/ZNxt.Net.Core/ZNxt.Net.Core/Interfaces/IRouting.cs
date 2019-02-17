using System.Collections.Generic;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IRouting
    {
        RoutingModel GetRoute(string Method, string url);
        List<RoutingModel> GetRoutes();

    }
}