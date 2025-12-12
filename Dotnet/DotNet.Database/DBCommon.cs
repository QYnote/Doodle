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

        /// <summary>DataBase 주소</summary>
        protected string _dataSource = string.Empty;
        /// <summary>DataBase ID</summary>
        private string _id = string.Empty;
        /// <summary>DataBase PassWord</summary>
        private string _password = string.Empty;

        /// <summary>
        /// DataBase Connection
        /// </summary>
        protected IDbConnection conn;
        /// <summary>
        /// Transaction
        /// </summary>
        private IDbTransaction transaction;


        public abstract string DataSource { get; set; }
        public string ID
        {
            get => _id;
            set
            {
                if (this.BaseConn == null || this.BaseConn.State == ConnectionState.Closed)
                {
                    this._id = value;
                    this.BaseConn = null;
                }
                else throw new ConstraintException("DataBase가 사용중입니다.");
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                if (this.BaseConn == null || this.BaseConn.State == ConnectionState.Closed)
                {
                    this._password = value;
                    this.BaseConn = null;
                }
                else throw new ConstraintException("DataBase가 사용중입니다.");
            }
        }
        /// <summary>
        /// DataBase 연결 String
        /// </summary>
        protected abstract string ConnectionString { get; }
        protected IDbConnection BaseConn
        { 
            get => conn; 
            set => conn = value;
        }
        protected IDbTransaction BaseTransaction { get => transaction; set => transaction = value; }


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

            if (this.BaseConn.State == ConnectionState.Open)
            {
                this.BaseTransaction = this.BaseConn.BeginTransaction();
            }
        }
        /// <summary>
        /// Transaction 종료처리
        /// </summary>
        /// <param name="result">Query 최종 실행결과</param>
        public void EndTransaction(bool result)
        {
            if (this.BaseTransaction != null)
            {
                if (result)
                {
                    this.BaseTransaction.Commit();
                }
                else
                {
                    this.BaseTransaction.Rollback();
                }

                this.BaseTransaction.Dispose();
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
