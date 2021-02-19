using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using Dapper;
using Dapper.Contrib.Extensions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Npgsql;
using ZNxt.Net.Core.Helpers;
using ZNxt.Net.Core.Interfaces;
using Dapper.Contrib;

namespace ZNxt.Net.Core.DB.MySql
{
    public class SqlRDBService : IRDBService
    {
        private  string  _connectionStr = "";
        private  string _DBType= "NONE";


        public SqlRDBService()
        {
            _connectionStr = CommonUtility.GetAppConfigValue("MYSQLConnectionString");
            if (string.IsNullOrEmpty(_connectionStr))
            {
                _connectionStr = CommonUtility.GetAppConfigValue("MSSQLConnectionString");
                if (!string.IsNullOrEmpty(_connectionStr))
                {
                    _DBType = "MSSQL";
                }
                else
                {
                    _connectionStr = CommonUtility.GetAppConfigValue("NPGSQLConnectionString");
                    if (!string.IsNullOrEmpty(_connectionStr))
                    {
                        _DBType = "NPGSQL";
                    }
                }
            }
            else
            {
                _DBType = "MYSQL";
            }
        }
        public bool IsConnected => throw new NotImplementedException();

        public RDBTransaction BeginTransaction()
        {
            var conn = GetConnection();
            conn.Open();
            return new RDBTransaction(conn.BeginTransaction(), conn);
        }

        public void CommitTransaction(RDBTransaction transaction)
        {
            transaction.Transaction.Commit();
            transaction.Connection.Close();
            transaction.Connection.Dispose();
        }
      
        public void Init(string dbType, string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString) && string.IsNullOrEmpty(dbType))
            {
                _connectionStr = connectionString;
                _DBType = dbType;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void RollbackTransaction(RDBTransaction transaction)
        {
            transaction.Transaction.Rollback();
            transaction.Connection.Close();
            transaction.Connection.Dispose();
        }
        private DbConnection GetConnection()
        {
            switch (_DBType.ToUpper())
            {
                case "MYSQL":
                    return new MySqlConnection(_connectionStr);
                case "MSSQL":
                    return new SqlConnection(_connectionStr);
                case "NPGSQL":
                    return new NpgsqlConnection(_connectionStr);
            }
            throw new NotSupportedException($"DB Type {_DBType} not supported");
        }

        #region get 
        public IEnumerable<T> Get<T>(string table, JObject filter) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Get<T>(string sql, object param = null) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.Query<T>(sql, param);
            }
        }
        public T GetFirst<T>(string sql, object param = null) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.QueryFirst<T>(sql, param);
            }
        }
        public T GetFirst<T>(string id) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.Get<T>(id);
            }
        }
        #endregion

        #region update

        public bool Update(string sql)
        {
            using (var conn = GetConnection())
            {
                return conn.Update(sql);
            }
        }

        public bool Update(string sql, RDBTransaction transaction)
        {
            return transaction.Connection.Update(sql, transaction.Transaction);
        }

        public bool Update(IEnumerable<string> sql)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    foreach (var item in sql)
                    {
                        if (conn.Update(item, transaction))
                        {
                            try
                            {
                                transaction.Rollback();
                                return false;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            return true;
        }

        public bool Update<T>(T data) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.Update<T>(data);
            }
        }
        public bool Update<T>(T data, RDBTransaction transaction) where T : class
        {
            return transaction.Connection.Update<T>(data, transaction.Transaction);
        }


        #endregion

        #region insert


        public bool WriteData<T>(IEnumerable<T> data) where T : class
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    foreach (var item in data)
                    {
                        var result = conn.Insert<T>(item, transaction);
                        if (result != 0)
                        {
                            try
                            {
                                transaction.Rollback();
                                return false;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            return true;
        }
        public bool WriteData<T>( T data) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.Insert<T>(data) == 1;
            }
        }

        public bool WriteData(string sql)
        {
            using (var conn = GetConnection())
            {
                return conn.Execute(sql) == 1;
            }
        }
        public bool WriteData(string sql, RDBTransaction transaction)
        {
            var result = transaction.Connection.Execute(sql, null, transaction.Transaction);
            return result == 1;
        }

        #endregion

        #region delete 
        public bool Delete(string sql)
        {
            using (var conn = GetConnection())
            {
                return conn.Delete(sql);
            }
        }

        public bool Delete(string sql, RDBTransaction transaction)
        {
            return transaction.Connection.Delete(sql, transaction.Transaction);
        }

        public bool Delete(IEnumerable<string> sql)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    foreach (var item in sql)
                    {
                        if (conn.Delete(item, transaction))
                        {
                            try
                            {
                                transaction.Rollback();
                                return false;
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            return true;
        }

        public bool Delete<T>(T data) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.Delete<T>(data);
            }
        }
        public bool Delete<T>(T data, RDBTransaction transaction) where T : class
        {
            return transaction.Connection.Delete<T>(data, transaction.Transaction);
        }
        #endregion

      
    }
}
