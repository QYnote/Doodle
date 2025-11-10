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

        protected string _dataSource = string.Empty;
        protected string _password = string.Empty;

        protected abstract string ConnectionString { get; }

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

        public abstract DataSet ExecuteQuery(string query);

        public abstract bool ExecuteNonQuery(string query);

        public abstract bool ExecuteNonQuery<T>(string query, List<T> parameters);

        public abstract void BeginTransaction();

        public abstract void EndTransaction(bool result);

        protected void RunLogEvent(string log)
        {
            this.LogEvent?.Invoke(log);
        }
    }
}
