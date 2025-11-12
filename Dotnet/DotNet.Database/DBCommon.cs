using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Database
{
    public enum DataBaseType
    {
        SQLCe,
        SQLite,
    }

    public abstract class DBCommon
    {
        public delegate void DBLoghandler(string errorMessage);
        public event DBLoghandler LogEvent;
        /// <summary>
        /// DataBase 주소
        /// </summary>
        protected string _dataSource = string.Empty;
        /// <summary>
        /// DataBase PassWord
        /// </summary>
        protected string _password = string.Empty;
        /// <summary>
        /// DataBase Connection
        /// </summary>
        protected IDbConnection _conn;
        /// <summary>
        /// Transaction
        /// </summary>
        protected IDbTransaction _transaction;
        /// <summary>
        /// DataBase 연결 String
        /// </summary>
        protected abstract string ConnectionString { get; }
        /// <summary>
        /// DataBase 연결
        /// </summary>
        /// <returns>연결된 Connection</returns>
        protected abstract IDbConnection GetConnection();
        /// <summary>
        /// Select Query 실행
        /// </summary>
        /// <param name="query">실행 Query</param>
        /// <param name="idx">가져올 View 순번</param>
        /// <returns>읽어온 View</returns>
        public virtual DataTable ExecuteQuery(string query, int idx = 0)
        {
            DataSet ds = this.ExecuteQuery(query);

            if (ds.Tables.Count > idx)
                return ds.Tables[idx];
            else if (ds.Tables.Count > 0)
                return ds.Tables[0];
            else
                return null;
        }
        /// <summary>
        /// Select Query 실행
        /// </summary>
        /// <param name="query">실행 Query</param>
        /// <returns>읽어온 View 목록</returns>
        public abstract DataSet ExecuteQuery(string query);
        /// <summary>
        /// 반환 없는 Query 실행
        /// </summary>
        /// <param name="query">실행 Query</param>
        /// <returns>실행결과</returns>
        public abstract bool ExecuteNonQuery(string query);
        /// <summary>
        /// 반환 없는 Query 실행
        /// </summary>
        /// <typeparam name="T">DataBase Parameter Type</typeparam>
        /// <param name="query">실행 Query</param>
        /// <param name="parameters">Query Parameter</param>
        /// <returns>실행 결과</returns>
        public abstract bool ExecuteNonQuery<T>(string query, IList<T> parameters);
        /// <summary>
        /// Transaction 시작처리
        /// </summary>
        public void BeginTransaction()
        {
            this.GetConnection();

            if (this._conn.State == ConnectionState.Open)
            {
                this._transaction = this._conn.BeginTransaction();
            }
        }
        /// <summary>
        /// Transaction 종료처리
        /// </summary>
        /// <param name="result">Query 최종 실행결과</param>
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

                this._transaction.Dispose();
            }
        }
        /// <summary>
        /// 로그
        /// </summary>
        /// <param name="log"></param>
        protected void RunLogEvent(string log)
        {
            this.LogEvent?.Invoke(log);
        }
    }
}
