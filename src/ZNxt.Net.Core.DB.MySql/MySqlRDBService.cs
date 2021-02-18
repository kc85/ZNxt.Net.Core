using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;

namespace ZNxt.Net.Core.DB.MySql
{
    public class MySqlRDBService : IRDBService
    {
        private  string  _connectionStr = "";

        public MySqlRDBService() {
            _connectionStr = CommonUtility.GetAppConfigValue("MYSQLConnectionString");
        }
        public bool IsConnected => throw new NotImplementedException();

        public IEnumerable<T> Get<T>(string table, JObject filter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Get<T>(string sql)
        {
            using (var conn =  GetConnection()){
                return  conn.Query<T>(sql);
            }
        }

        public void Init(string dbName = null, string connectionString = null)
        {
            _connectionStr = connectionString;
        }

        public int Update<T>(string table, T data, JObject filter)
        {
            throw new NotImplementedException();
        }

        public int Update(string sql)
        {
            throw new NotImplementedException();
        }

        public bool WriteData<T>(string table, T data)
        {
            throw new NotImplementedException();
        }

        public bool WriteData(string sql)
        {
            using (var conn = GetConnection())
            {
                return conn.Execute(sql) == 1;
            }
        }
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionStr);
            
        }
    }
}
