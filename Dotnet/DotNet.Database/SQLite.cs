using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;

namespace DotNet.Database
{
    public class SQLite : DBBase
    {
        public static readonly string DEFAULT_SQLITE_DIR = Directory.GetCurrentDirectory();
        public const string DEFAULT_SQLITE_DIR_FILENAME = "QYDB.sqlite";
        public const string DEFAULT_SQLITE_PASSWORD = "";


        public override string DataSource
        {
            get => base._dataSource;
            set
            {
                string extention = value.Split('.').Last();

                if ((extention == "sqlite" || extention == "db" || extention == string.Empty) == false)
                    throw new NotImplementedException("파일 확장자 미지원");

                base._dataSource = value;
            }
        }
        protected override string ConnectionString
        {
            get
            {
                if (this.DataSource == string.Empty)
                    return string.Empty;

                if (base.Password == string.Empty || base.Password == "")
                    return $"Data Source={this.DataSource}";

                return $"Data Source={this.DataSource};Password={base.Password}";
            }
        }
        private SQLiteConnection Conn { get { return (SQLiteConnection)GetConnection(); } }
        private SQLiteTransaction Transaction { get { return (SQLiteTransaction)base.BaseTransaction; } }

        public SQLite()
        {
            this.DataSource = $"{DEFAULT_SQLITE_DIR}\\{DEFAULT_SQLITE_DIR_FILENAME}";
            base.Password = DEFAULT_SQLITE_PASSWORD;
            base.BaseConn = this.GetConnection();
        }

        protected override IDbConnection GetConnection()
        {
            if(base.BaseConn == null)
            {
                base.BaseConn = new SQLiteConnection(this.ConnectionString);
            }
            else if (base.BaseConn.ConnectionString != this.ConnectionString)
            {
                if (base.BaseConn.State != System.Data.ConnectionState.Closed)
                    base.BaseConn.Close();

                base.BaseConn = new SqlCeConnection(this.ConnectionString);
            }
            else
            {
                if(base.BaseConn.State == System.Data.ConnectionState.Closed)
                    base.BaseConn.Open();
            }

            return (SQLiteConnection)base.BaseConn;
        }

        public override DataSet ExecuteQuery(string query)
        {
            DataSet ds = null;
            SQLiteDataAdapter adapter = null;

            try
            {
                ds = new DataSet();
                adapter = new SQLiteDataAdapter(query, this.Conn);
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

            return ds;
        }

        public override bool ExecuteNonQuery(string query)
        {
            SQLiteCommand cmd = null;
            bool result;

            try
            {
                cmd = new SQLiteCommand(query, this.Conn);
                if(base.BaseTransaction != null) cmd.Transaction = this.Transaction;

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

        public override bool ExecuteNonQuery<SqliteParameter>(string query, IList<SqliteParameter> parameters)
        {
            if (query.Contains("@") == false) throw new Exception("Query에 Parameter 지정값이 없음");

            SQLiteCommand cmd = null;
            bool result;

            try
            {
                cmd = new SQLiteCommand(query, this.Conn);
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
    }
}
