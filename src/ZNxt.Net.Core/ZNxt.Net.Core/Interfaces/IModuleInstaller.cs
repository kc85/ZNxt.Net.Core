using Newtonsoft.Json.Linq;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IModuleInstaller
    {
        bool Install(string moduleName, IHttpContextProxy httpProxy, bool IsOverride = true);

        JObject GetDetails(string moduleName);
    }
}