using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;

namespace DotNet.Database
{
    /// <summary>
    /// SQLCe Database 처리 Class
    /// </summary>
    public class SQLCe : DBCommon
    {
        private SqlCeConnection _conn = null;
        private SqlCeTransaction _transaction = null;

        #region 편의성 Property

        override protected string ConnectionString
        {
            get
            {
                if(base._dataSource == string.Empty
                    || base._password == string.Empty)
                    return string.Empty;

                return string.Format(
                    "Data Source={0};" +
                    "Password={1};" +
                    "Persist Security Info=True",
                    base._dataSource, base._password);
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
            base._dataSource = filePath;
            base._password = password;
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
                if(System.IO.File.Exists(this._dataSource))
                    this._conn = new SqlCeConnection(this.ConnectionString);
            }
            else
            {
                if (this._conn.State == System.Data.ConnectionState.Closed)
                    this._conn.Open();
            }

            return this._conn;
        }

        public override DataTable ExecuteQuery(string query, int idx = 0)
        {
            string[] spltQuery = query.Split(';');
            DataSet ds = new DataSet();

            for (int i = 0; i < spltQuery.Length; i++)
            {
                DataSet readDs = this.ExecuteQuery(spltQuery[i].Trim());

                if (readDs != null)
                    ds.Tables.Add(readDs.Tables[0]);
                else
                    break;
            }

            if (ds.Tables.Count > idx)
                return ds.Tables[idx];
            else if (ds.Tables.Count > 0)
                return ds.Tables[0];
            else
                return null;
        }

        /// <summary>
        /// 반환이 있는 Query 실행
        /// </summary>
        /// <param name="query">실행 Query</param>
        /// <returns>반환 받은 Table</returns>
        public override DataSet ExecuteQuery(string query)
        {
            string[] spltQuery = query.Split(';');
            SqlCeDataAdapter adapter = null;
            DataSet ds = null;

            if (spltQuery.Length == 1)
            {
                try
                {
                    SqlCeConnection conn = this.GetConnection();

                    ds = new DataSet();
                    adapter = new SqlCeDataAdapter(query, conn);
                    adapter.Fill(ds);

                }
                catch (Exception ex)
                {
                    base.RunLogEvent(string.Format("Query Error: {0}\r\n\r\nLog:{2}", ex.Message, query));
                }
                finally
                {
                    if (adapter != null)
                        adapter.Dispose();
                }
            }
            else
            {
                ds = new DataSet();

                for (int i = 0; i < spltQuery.Length; i++)
                {
                    DataSet runDs = ExecuteQuery(spltQuery[i]);

                    if (runDs != null)
                    {
                        foreach (DataTable table in runDs.Tables)
                            ds.Tables.Add(table);
                    }
                }
            }

            return ds;
        }

        /// <summary>
        /// 반환이 없는 Query 실행
        /// </summary>
        /// <param name="query">query</param>
        /// <returns>실행성공여부</returns>
        public override bool ExecuteNonQuery(string query)
        {
            SqlCeCommand cmd = null;
            bool result = false;

            try
            {
                string[] spltQuery = query.Split(';');

                cmd = new SqlCeCommand();
                cmd.Connection = this.GetConnection();
                if (this._transaction != null)
                    cmd.Transaction = this._transaction;

                for (int i = 0; i < spltQuery.Length; i++)
                {
                    spltQuery[i] = spltQuery[i].Replace("\r\n", " ").Trim();
                    if (spltQuery[i] == string.Empty) continue;

                    //주석 Row 스킵
                    if (spltQuery[i][0] == '-' && spltQuery[i][1] == '-') continue;


                    cmd.CommandText = spltQuery[i];
                    cmd.ExecuteNonQuery();
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                base.RunLogEvent(string.Format("Query Error: {0}\r\n\r\nLog:{1}", ex.Message, query));
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
        public override bool ExecuteNonQuery<SqlCeParameter>(string query, List<SqlCeParameter> parameters)
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

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                base.RunLogEvent(string.Format("Query Error: {0}\r\n\r\nLog:{1}", ex.Message, query));
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
        public override void BeginTransaction()
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
        public override void EndTransaction(bool result)
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

        public string GetTableColumnInfoQuery(string tableName)
        {
            return string.Format(
                "SELECT * " +
                  "FROM INFORMATION_SCHEMA.COLUMNS " +
                 "WHERE TABLE_NAME = '{0}';", tableName);
        }
    }
}
