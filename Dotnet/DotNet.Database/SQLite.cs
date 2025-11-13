using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DotNet.Database
{
    public class SQLite : DBCommon
    {
        private SQLiteConnection Conn { get { return (SQLiteConnection)GetConnection(); } }
        private SQLiteTransaction Transaction { get { return (SQLiteTransaction)base._transaction; } }

        protected override string ConnectionString
        {
            get
            {
                if (base._dataSource == string.Empty)
                    return string.Empty;

                if (base._password == string.Empty || base._password == "")
                    return $"Data Source={base._dataSource}";

                return $"Data Source={base._dataSource};Password={base._password}";
            }
        }

        public SQLite(string filePath, string password = "")
        {
            base._dataSource = filePath;
            base._password = password;
            base._conn = this.GetConnection();
        }

        protected override IDbConnection GetConnection()
        {
            if(base._conn == null)
            {
                base._conn = new SQLiteConnection(this.ConnectionString);
            }
            else
            {
                if(base._conn.State == System.Data.ConnectionState.Closed)
                    base._conn.Open();
            }

            return (SQLiteConnection)base._conn;
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
                if(base._transaction != null) cmd.Transaction = this.Transaction;

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
                    if (base._transaction == null)
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
                if (base._transaction != null) cmd.Transaction = this.Transaction;

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
                    if (base._transaction == null)
                        cmd.Dispose();
                }
            }

            return result;
        }
    }
}
