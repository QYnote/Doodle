using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;

namespace DotNet.Database
{
    /// <summary>
    /// SQLCe Database 처리 Class
    /// </summary>
    public class SQLCe
    {
        public delegate void LogHandler(string log);
        public event LogHandler Log;

        private string _filePath = string.Empty;
        private string _password = string.Empty;
        private SqlCeConnection _conn = null;
        private SqlCeTransaction _transaction = null;

        #region 편의성 Property

        private string ConnectionString
        {
            get
            {
                if(this._filePath == string.Empty
                    || this._password == string.Empty)
                    return string.Empty;

                return string.Format(
                    "Data Source={0};" +
                    "Password={1};" +
                    "Persist Security Info=True",
                    this._filePath, this._password);
            }
        }

        #endregion 편의성 Property
        /// <summary>
        /// SQLCe Database class
        /// </summary>
        /// <param name="filePath">DB File Path</param>
        /// <param name="password">DB Password</param>
        public SQLCe(string filePath, string password)
        {
            this._filePath = filePath;
            this._password = password;
            this._conn = GetConnection();
        }
        /// <summary>
        /// Connection 생성 및 연결
        /// </summary>
        /// <returns>생성 or 연결된 Connection</returns>
        private SqlCeConnection GetConnection()
        {
            if(this._conn == null)
            {
                this._conn = new SqlCeConnection(this.ConnectionString);
            }
            else
            {
                if (this._conn.State == System.Data.ConnectionState.Closed)
                    this._conn.Open();
            }

            return this._conn;
        }
        /// <summary>
        /// 반환이 있는 Query 실행
        /// </summary>
        /// <param name="query">실행 Query</param>
        /// <returns>반환받은 DataTable</returns>
        public DataTable ExcuteQuery(string query)
        {
            SqlCeDataAdapter adapter = null;
            DataTable dt = null;

            try
            {
                SqlCeConnection conn = this.GetConnection();

                dt = new DataTable();
                adapter = new SqlCeDataAdapter(query, conn);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                this.Log?.Invoke(string.Format("Query Error: {0}\r\nLog:{1}", query, ex.Message));
            }
            finally
            {
                if (adapter != null)
                    adapter.Dispose();
            }

            return dt;
        }
        /// <summary>
        /// 반환이 있는 Query 실행<br/>
        /// DataSet or DataTable만 사용가능
        /// </summary>
        /// <typeparam name="T">반환 받을 Type - DataSet or DataTable</typeparam>
        /// <param name="query">실행 Query</param>
        /// <returns>반환받은 Data</returns>
        /// <exception cref="InvalidOperationException">DataSet, Table이 아님</exception>
        public T ExcuteQuery<T>(string query) where T : class
        {
            if (typeof(T) != typeof(DataTable)
                && typeof(T) != typeof(DataSet))
                throw new InvalidOperationException("DataType is not DataSet or DataTable");

            SqlCeDataAdapter adapter = null;
            DataSet ds = null;

            try
            {
                SqlCeConnection conn = this.GetConnection();

                ds = new DataSet();
                adapter = new SqlCeDataAdapter(query, conn);
                adapter.Fill(ds);
            }
            catch (Exception ex)
            {
                this.Log?.Invoke(string.Format("Query Error: {0}\r\nLog:{1}", query, ex.Message));
            }
            finally
            {
                if (adapter != null)
                    adapter.Dispose();
            }

            if (typeof(T) == typeof(DataTable))
            {
                return ds.Tables.Count > 0 ? ds.Tables[0] as T : null;
            }
            else if (typeof(T) == typeof(DataSet))
            {
                return ds as T;
            }

            return null;
        }
        /// <summary>
        /// 반환이 없는 Query 실행
        /// </summary>
        /// <param name="query">query</param>
        /// <returns>실행성공여부</returns>
        public bool ExcuteNonQuery(string query)
        {
            SqlCeCommand cmd = null;
            bool result = false;

            try
            {
                cmd = new SqlCeCommand(query, this.GetConnection());
                if (this._transaction != null) cmd.Transaction = this._transaction;

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                result = false;
                this.Log?.Invoke(string.Format("Query Error: {0}\r\nLog:{1}", query, ex.Message));
            }
            finally
            {
                if(cmd != null)
                {
                    if (this._transaction == null)
                        cmd.Dispose();
                }
            }

            return result;
        }
        /// <summary>
        /// @변수를 사용하는 Query 실행
        /// </summary>
        /// <param name="query">@변수를 사용하는 Query</param>
        /// <param name="parameters">변수목록</param>
        /// <returns>실행성공여부</returns>
        public bool ExcuteNonQuery(string query, List<SqlCeParameter> parameters)
        {
            //Parametr 지정값 @가 없으면 false처리
            if (query.Contains("@") == false) return false;

            SqlCeCommand cmd = null;
            bool result = false;

            try
            {
                cmd = new SqlCeCommand(query, this.GetConnection());
                if (this._transaction != null) cmd.Transaction = this._transaction;

                foreach (var param in parameters)
                    cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                result = false;
                this.Log?.Invoke(string.Format("Query Error: {0}\r\nLog:{1}", query, ex.Message));
            }
            finally
            {
                if (cmd != null)
                {
                    if (this._transaction == null)
                        cmd.Dispose();
                }
            }

            return result;
        }
        /// <summary>
        /// Transaction 시작처리
        /// </summary>
        public void BeginTransaction()
        {
            GetConnection();

            if(this._conn.State == ConnectionState.Open)
            {
                this._transaction = this._conn.BeginTransaction();
            }
        }
        /// <summary>
        /// Transaction 종료처리
        /// </summary>
        /// <param name="result">Query 실행 최종결과</param>
        public void EndTransaction(bool result)
        {
            if (this._transaction != null)
            {
                if (result)
                {
                    this._transaction.Commit();
                }
                else
                {
                    this._transaction.Rollback();
                }

                this._transaction.Connection.Dispose();
                this._transaction.Dispose();
            }
        }
    }
}
