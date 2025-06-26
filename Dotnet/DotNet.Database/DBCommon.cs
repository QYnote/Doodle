using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Database
{

    public abstract class DBCommon
    {
        protected string _filePath = string.Empty;
        protected string _password = string.Empty;

        protected abstract string ConnectionString { get; }

        public abstract T ExcuteQuery<T>(string query) where T : class;

        public abstract bool ExcuteNonQuery(string query);

        public abstract bool ExcuteNonQuery<TParam>(string query, List<TParam> parameters);
        
        public abstract void BeginTransaction();

        public abstract void EndTransaction(bool result);
    }
}
