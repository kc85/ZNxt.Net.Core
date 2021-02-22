using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ZNxt.Net.Core.Model;

namespace ZNxt.Net.Core.Interfaces
{
    public interface IRDBService
    {
        void Init(string dbType, string connectionString);

        bool WriteData<T>(T data) where T : class;
        bool WriteData<T>(IEnumerable<T> data) where T : class;
        bool WriteData(string sql, object param = null);
        bool WriteData(string sql, object param = null, RDBTransaction transaction = null);
        int WriteDataGetId(string sql, object param = null);
        int WriteDataGetId(string sql, object param = null, RDBTransaction transaction = null);
        bool Update(string sql);
        bool Update(string sql, RDBTransaction transaction);
        bool Update (IEnumerable<string> sql);
        bool Update<T>(T data) where T : class;
        bool Update<T>(T data, RDBTransaction transaction) where T : class;


        bool Delete(string sql);
        bool Delete(string sql, RDBTransaction transaction);
        bool Delete(IEnumerable<string> sql);
        bool Delete<T>(T data) where T : class;
        bool Delete<T>(T data, RDBTransaction transaction) where T : class;

        // int Update<T>(T data, JObject filter);
        //  int Update(string sql);
        // IEnumerable<T> Get<T>(string table, JObject filter) where T : class;
        IEnumerable<T> Get<T>(string sql, object param = null) where T : class;
        T GetFirst<T>(string sql, object param = null) where T : class;
        T GetFirst<T>(string id) where T : class;

        RDBTransaction BeginTransaction();
        void CommitTransaction(RDBTransaction transaction);
        void RollbackTransaction(RDBTransaction transaction);
    }

    public class RDBTransaction
    {
        public IDbTransaction Transaction { get; private set; }
        public DbConnection Connection { get; private set; }

        public RDBTransaction(IDbTransaction transaction, DbConnection  conn)
        {
            Transaction = transaction;
            Connection = conn;
        }
    }
}