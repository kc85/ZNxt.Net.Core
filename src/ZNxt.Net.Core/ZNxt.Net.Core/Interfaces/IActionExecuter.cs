using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IActionExecuter
    {
        T Exec<T>(string action, IDBService dbProxy, ParamContainer helper);

        object Exec(string action, IDBService dbProxy, ParamContainer helper);

        object Exec(RoutingModel route, ParamContainer helper);

        object Exec(string execultAssembly, string executeType, string executeMethod, ParamContainer helper);
    }
}