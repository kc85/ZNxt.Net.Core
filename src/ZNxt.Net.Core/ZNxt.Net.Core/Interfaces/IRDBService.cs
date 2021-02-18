using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IRDBService
    {
        void Init(string dbName = null, string connectionString = null);

        bool IsConnected{ get; }
        bool WriteData<T>(string table, T data);
        bool WriteData(string sql);
        
        int Update<T>(string table, T data, JObject filter);
        int Update(string sql);
        IEnumerable<T> Get<T>(string table, JObject filter);
        IEnumerable<T> Get<T>(string sql);
    }
}