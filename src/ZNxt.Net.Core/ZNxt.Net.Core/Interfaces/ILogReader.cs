using Newtonsoft.Json.Linq;

namespace ZNxt.Net.Core.Interfaces
{
    public interface ILogReader
    {
        JArray GetLogs(string transactionId);
    }
}