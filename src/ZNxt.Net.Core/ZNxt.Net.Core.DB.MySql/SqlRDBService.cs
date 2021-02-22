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
using System.Data;
using System.Linq;

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
       
        public IEnumerable<T> Get<T>(string sql, object param = null) where T : class
        {
            using (var conn = GetConnection())
            {
                if(typeof(T) == typeof(JObject))
                {
                    IEnumerable<JObject> data  =  GetDateAsJObject(conn,sql, param);
                    return data as IEnumerable<T>;
                }
                return conn.Query<T>(sql, param);
            }
        }

        
        public T GetFirst<T>(string sql, object param = null) where T : class
        {
            using (var conn = GetConnection())
            {
                if (typeof(T) == typeof(JObject))
                {
                    IEnumerable<JObject> data = GetDateAsJObject(conn, sql, param);
                    if (data.Any())
                    {
                        return data.First() as T;
                    }
                    else
                    {
                        return default;
                    }
                }
                else
                {
                    return conn.QueryFirst<T>(sql, param);
                }
            }
        }
        public T GetFirst<T>(string id) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.Get<T>(id);
            }
        }
        private IEnumerable<JObject> GetDateAsJObject(DbConnection conn, string sql, object param = null)
        {
            List<JObject> dataResult = new List<JObject>();
            conn.Open();
            List<string> columns = new List<string>();
            DbCommand oCmd = null;
            try
            {
                switch (_DBType)
                {
                    case "MSSQL":
                        oCmd = new SqlCommand(sql, conn as SqlConnection);
                        break;
                    case "MYSQL":
                        oCmd = new MySqlCommand(sql, conn as MySqlConnection);
                        break;
                    case "NPGSQL":
                        oCmd = new NpgsqlCommand(sql, conn as NpgsqlConnection);
                        break;
                }
                using (DbDataReader oReader = oCmd.ExecuteReader())
                {
                    for (int i = 0; i < oReader.FieldCount; i++)
                    {
                        columns.Add(oReader.GetName(i));
                    }
                    while (oReader.Read())
                    {
                        JObject jdata = new JObject();
                        foreach (var col in columns)
                        {
                            jdata[col] = oReader[col].ToString();
                        }
                        dataResult.Add(jdata);
                    }

                }
            }
            finally
            {
                conn.Close();
            }

            return dataResult;
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
        public bool WriteData<T>(T data) where T : class
        {
            using (var conn = GetConnection())
            {
                return conn.Insert<T>(data) == 1;
            }
        }

        public bool WriteData(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.Execute(sql, param) == 1;
            }
        }
        public int WriteDataGetId(string sql, object param = null)
        {
            using (var conn = GetConnection())
            {
                return conn.Query<int>(sql, param).Single();
            }
        }
        public int WriteDataGetId(string sql, object param = null, RDBTransaction transaction = null)
        {
            return transaction.Connection.Query<int>(sql, param, transaction.Transaction).Single();
        }
        public bool WriteData(string sql, object param = null,RDBTransaction transaction = null)
        {
            if (transaction != null)
            {
                var result = transaction.Connection.Execute(sql, param, transaction.Transaction);
                return result == 1;
            }
            else
            {
                var result = transaction.Connection.Execute(sql, param);
                return result == 1;
            }
            
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
