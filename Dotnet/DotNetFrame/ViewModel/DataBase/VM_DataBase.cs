using DotNet.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.ViewModel.DataBase
{
    internal class VM_DataBase
    {
        private DataBaseType _db_type = DataBaseType.SQLite;
        private DBCommon _db = null;
        private string _db_datasource = string.Empty;
        private string _db_id = string.Empty;
        private string _db_password = string.Empty;

        private string _db_log_dir = string.Empty;
        private DataSet _ds = null;


        public DataBaseType Type
        {
            get => _db_type;
            set
            {
                this._db = null;

                switch (value)
                {
                    case DataBaseType.SQLCe: 
                        this._db = new SQLCe();

                        this._db_log_dir = SQLCe.DEFAULT_SQLCE_DIR;
                        break;
                    case DataBaseType.SQLite:
                        this._db = new SQLite();

                        this._db_log_dir = $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite";
                        break;
                }

                this._db_type = value;
            }
        }
        public string DataSource
        {
            get => this._db_datasource;
            set
            {
                this._db.DataSource = value;
                this._db_datasource = value;
            }
        }
        public string Password
        {
            get => this._db.Password;
            set
            {
                this._db.Password = value;
                this._db_password = value;
            }
        }
        public DataSet DataSet { get => _ds; }


        internal VM_DataBase()
        {
            this.Type = DataBaseType.SQLite;

            this.DataSource =  $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite\\{SQLite.DEFAULT_SQLITE_DIR_FILENAME}";
            this.Password = SQLite.DEFAULT_SQLITE_PASSWORD;
        }

        public bool SendQuery(string query)
        {
            bool result = false;

            try
            {
                if ((query.ToUpper().Contains("CREATE") || query.ToUpper().Contains("ALTER")  || query.ToUpper().Contains("DROP") ||
                     query.ToUpper().Contains("INSERT") || query.ToUpper().Contains("UPDATE") || query.ToUpper().Contains("DELETE")
                    )
                    && query.ToUpper().Contains("SELECT"))
                    throw new Exception("SELECT와 다른 명령어는 동시에 사용 불가능 - 기술 부족");

                if (query.ToUpper().Contains("CREATE") || query.ToUpper().Contains("ALTER")  || query.ToUpper().Contains("DROP") ||
                    query.ToUpper().Contains("INSERT") || query.ToUpper().Contains("UPDATE") || query.ToUpper().Contains("DELETE"))
                {
                    this._db.BeginTransaction();

                    result = this._db.ExecuteNonQuery(query);

                    this._db.EndTransaction(result);

                    if (result)
                    {
                        //Query 로그 저장
                        string logText =
                            $"============================================================{Environment.NewLine}" +
                            $"{DateTime.Now}{Environment.NewLine}" +
                            $"{query}{Environment.NewLine}" +
                            $"============================================================{Environment.NewLine}" +
                            $"{Environment.NewLine}";
                        string logpath = $"{this._db_log_dir}\\sqlLog.txt";

                        Directory.CreateDirectory(Path.GetDirectoryName(logpath));

                        File.AppendAllText(logpath, logText);
                    }
                }
                else if (query.ToUpper().Contains("SELECT"))
                {
                    DataSet ds = null;

                    ds = this._db.ExecuteQuery(query);

                    if (ds != null)
                    {
                        this._ds = ds;

                        result = true;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result = false;
                this._db.EndTransaction(result);
                throw ex;
            }
        }
    }
}
