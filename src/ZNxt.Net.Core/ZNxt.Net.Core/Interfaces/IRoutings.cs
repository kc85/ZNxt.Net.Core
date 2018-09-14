using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IRoutings
    {
        RoutingModel GetRoute(string Method, string url);

        void LoadRoutes();
    }
}