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
    public class SQLCe : DBBase
    {
        public static readonly string DEFAULT_SQLCE_DIR = "C:\\WorkerFile\\업무자료\\01_TCS\\01_source\\TCS.Data\\Products";
        public static readonly string DEFAULT_SQLCE_DIR_FILENAME = "tcsdr.sdf";
        public static readonly string DEFAULT_SQLCE_PASSWORD = "admin123";


        public override string DataSource
        {
            get => base._dataSource;
            set
            {
                string extention = value.Split('.').Last();

                if (extention != "sdf")
                    throw new NotImplementedException("파일 확장자 미지원");

                base._dataSource = value;
            }
        }
        override protected string ConnectionString
        {
            get
            {
                if(this.DataSource == string.Empty
                    || base.Password == string.Empty)
                    return string.Empty;

                return string.Format(
                    "Data Source={0};" +
                    "Password={1};" +
                    "Persist Security Info=True",
                    this.DataSource, base.Password);
            }
        }
        private SqlCeConnection Conn { get { return (SqlCeConnection)GetConnection(); } }
        private SqlCeTransaction Transaction { get { return (SqlCeTransaction)base.BaseTransaction; } }

        public SQLCe()
        {
            this.DataSource = $"{DEFAULT_SQLCE_DIR}\\{DEFAULT_SQLCE_DIR_FILENAME}";
            base.Password = DEFAULT_SQLCE_PASSWORD;
            base.BaseConn = GetConnection();
        }

        protected override IDbConnection GetConnection()
        {
            if(base.BaseConn == null)
            {
                if(System.IO.File.Exists(this.DataSource))
                    base.BaseConn = new SqlCeConnection(this.ConnectionString);
            }
            else if(base.BaseConn.ConnectionString != this.ConnectionString)
            {
                if (base.BaseConn.State != System.Data.ConnectionState.Closed)
                    base.BaseConn.Close();

                if (System.IO.File.Exists(this.DataSource))
                    base.BaseConn = new SqlCeConnection(this.ConnectionString);
            }
            else
            {
                if (base.BaseConn.State == System.Data.ConnectionState.Closed)
                    base.BaseConn.Open();
            }

            return (SqlCeConnection)base.BaseConn;
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

        public override DataSet ExecuteQuery(string query)
        {
            string[] spltQuery = query.Split(';');
            SqlCeDataAdapter adapter = null;
            DataSet ds = null;

            if (spltQuery.Length == 1)
            {
                try
                {
                    query = query.Replace("\r\n", " ").Trim();
                    if (query == string.Empty) return null;

                    //주석 Row 스킵
                    if (query[0] == '-' && query[1] == '-') return null;

                    ds = new DataSet();
                    adapter = new SqlCeDataAdapter(query, this.Conn);
                    adapter.Fill(ds);

                }
                catch (Exception ex)
                {
                    base.RunLogEvent(string.Format("Query Error: {0}\r\n\r\nLog:{1}", ex.Message, query));
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
                        {
                            if (ds.Tables.Contains(table.TableName))
                                table.TableName = $"Table{ds.Tables.Count + 1}";

                            ds.Tables.Add(table.Copy());
                        }
                    }
                }
            }

            return ds;
        }

        public override bool ExecuteNonQuery(string query)
        {
            SqlCeCommand cmd = null;
            bool result = false;

            try
            {
                string[] spltQuery = query.Split(';');

                cmd = new SqlCeCommand();
                cmd.Connection = this.Conn;
                if (base.BaseTransaction != null)
                    cmd.Transaction = this.Transaction;

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
                    if (base.BaseTransaction == null)
                        cmd.Dispose();
                }
            }

            return result;
        }

        public override bool ExecuteNonQuery<SqlCeParameter>(string query, IList<SqlCeParameter> parameters)
        {
            //Parametr 지정값 @가 없으면 false처리
            if (query.Contains("@") == false) return false;


            SqlCeCommand cmd = null;
            bool result = false;

            try
            {
                cmd = new SqlCeCommand(query, this.Conn);
                if (base.BaseTransaction != null) cmd.Transaction = this.Transaction;

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
                    if (base.BaseTransaction == null)
                        cmd.Dispose();
                }
            }

            return result;
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
