using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DotNet.Database
{
    public class SQLite : DBCommon
    {
        private SQLiteConnection _conn = null;
        private SQLiteTransaction _transaction = null;

        protected override string ConnectionString
        {
            get
            {
                if (base._dataSource == string.Empty)
                    return string.Empty;

                return $"Data Source={base._dataSource};Password={base._password};";
            }
        }

        public SQLite(string connStr, string password)
        {
            base._dataSource = connStr;
            base._password = password;
            GetConnection();
        }

        private SQLiteConnection GetConnection()
        {
            if(this._conn == null)
            {
                if (System.IO.File.Exists(base._dataSource))
                    this._conn = new SQLiteConnection(this.ConnectionString);
                else
                {
                    this._conn = new SQLiteConnection($"Data Source={base._dataSource};");
                    this._conn.SetPassword(base._password);
                }

            }
            else
            {
                if (this._conn.State == ConnectionState.Closed)
                    this._conn.Open();
            }

            return this._conn;
        }

        public override DataSet ExecuteQuery(string query)
        {
            SQLiteDataAdapter adapter = null;
            DataSet ds = null;

            try
            {
                ds = new DataSet();
                adapter = new SQLiteDataAdapter(query, this.GetConnection());
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

            return ds;
        }

        public override bool ExecuteNonQuery(string query)
        {
            SQLiteCommand cmd = null;
            bool result = false;

            try
            {
                cmd = new SQLiteCommand(query, this.GetConnection());
                if (this._transaction != null)
                    cmd.Transaction = this._transaction;

                cmd.CommandText = query;
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
                    if (this._transaction == null)
                        cmd.Dispose();
            }

            return result;
        }

        public override bool ExecuteNonQuery<SQLiteParameter>(string query, List<SQLiteParameter> parameters)
        {
            if (query.Contains("@") == false) return false;

            SQLiteCommand cmd = null;
            bool result = false;

            try
            {
                cmd = new SQLiteCommand(query, this.GetConnection());
                if (this._transaction != null)
                    cmd.Transaction = this._transaction;

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
                    if (this._transaction == null)
                        cmd.Dispose();
            }

            return result;
        }

        public override void BeginTransaction()
        {
            GetConnection();

            if(this._conn.State == ConnectionState.Open)
                this._transaction = this._conn.BeginTransaction();
        }

        public override void EndTransaction(bool result)
        {
            if(this._transaction != null)
            {
                if (result)
                    this._transaction.Commit();
                else
                    this._transaction.Rollback();

                this._transaction.Dispose();
            }
        }
    }
}
