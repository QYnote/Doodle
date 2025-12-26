using DotNet.Database;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.DataBase.ViewModel
{
    internal class DatabaseHandler : QYBindingBase
    {
        private DataBaseType _type;
        private string _db_connector_datasource;
        private string _db_connector_id;
        private string _db_connector_pw;
        private string _db_log_dir;

        private DBBase _db_connector;
        private List<QYUtils.EnumItem<DataBaseType>> _type_list;
        private DataSet _db_current;
        private BindingList<string> _db_current_table_list = new BindingList<string>();
        private string _db_current_table_name;

        public DataBaseType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    this.OnPropertyChanged(nameof(Type));

                    this.UpdateType(value);
                }
            }
        }
        public string DataSource
        {
            get => _db_connector_datasource;
            set
            {
                if (this._db_connector_datasource != value)
                {
                    this._db_connector_datasource = value;
                    this.OnPropertyChanged(nameof(DataSource));
                }
            }
        }
        public string ID
        {
            get => _db_connector_id;
            set
            {
                if (this._db_connector_id != value)
                {
                    this._db_connector_id = value;
                    this.OnPropertyChanged(nameof(ID));
                }
            }
        }
        public string Password
        {
            get => this._db_connector_pw;
            set
            {
                if (this._db_connector_pw != value)
                {
                    this._db_connector_pw = value;
                    this.OnPropertyChanged(nameof(Password));
                }
            }
        }
        public string LogDirectory
        {
            get => this._db_log_dir;
            set
            {
                if (this._db_log_dir != value)
                {
                    this._db_log_dir = value;
                    this.OnPropertyChanged(nameof(LogDirectory));
                }
            }
        }

        public List<QYUtils.EnumItem<DataBaseType>> TypeList => this._type_list;
        public BindingList<string> TableNames => this._db_current_table_list;
        public string SelectedName
        {
            get => this._db_current_table_name;
            set
            {
                if(this._db_current_table_name != value)
                {
                    this._db_current_table_name = value;
                    this.OnPropertyChanged(nameof(SelectedName));
                    this.OnPropertyChanged(nameof(CurrentTable));
                }
            }
        }
        public DataTable CurrentTable
        {
            get
            {
                if (this._db_current == null || string.IsNullOrEmpty(this.SelectedName))
                    return null;

                return this._db_current.Tables[this.SelectedName];
            }
        }

        internal DatabaseHandler()
        {
            Initialize();
        }

        private void Initialize()
        {
            this._type_list = QYUtils.GetEnumItems<DataBaseType>().ToList();
            this.Type = DataBaseType.SQLite;
            this._db_connector.DataSource = this.DataSource = $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite\\{SQLite.DEFAULT_SQLITE_DIR_FILENAME}";
            this._db_connector.Password = this.Password = SQLite.DEFAULT_SQLITE_PASSWORD;

            this._db_connector.GetConnection();
        }

        private void UpdateType(DataBaseType type)
        {
            try
            {
                switch (type)
                {
                    case DataBaseType.SQLCe:
                        this._db_connector = new SQLCe();
                        this.DataSource = $"{SQLCe.DEFAULT_SQLCE_DIR}\\{SQLCe.DEFAULT_SQLCE_DIR_FILENAME}";
                        this.Password = SQLCe.DEFAULT_SQLCE_PASSWORD;

                        this._db_log_dir = SQLCe.DEFAULT_SQLCE_DIR;
                        break;
                    case DataBaseType.SQLite:
                        this._db_connector = new SQLite();
                        this.DataSource = $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite\\{SQLite.DEFAULT_SQLITE_DIR_FILENAME}";
                        this.Password = SQLite.DEFAULT_SQLITE_PASSWORD;

                        this._db_log_dir = $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite";
                        break;
                }

                this._db_current = null;
                this.SelectedName = null;
            }
            catch (Exception ex)
            {
                base.OnErrorMessage($"Message: {ex.Message}\r\nTrace:{ex.StackTrace}");
            }
        }
        internal bool ConnectionTest()
        {
            bool result = false;

            try
            {
                this._db_connector.DataSource = this.DataSource;
                this._db_connector.ID = this.ID;
                this._db_connector.Password = this.Password;

                this._db_connector.GetConnection();
                result = true;
            }
            catch(Exception ex)
            {
                result = false;
                base.OnErrorMessage($"Message: {ex.Message}\r\nTrace:{ex.StackTrace}");
            }

            return result;
        }
        internal bool SendQuery(string query)
        {
            bool result = false;

            try
            {
                if ((query.ToUpper().Contains("CREATE") || query.ToUpper().Contains("ALTER") || query.ToUpper().Contains("DROP") ||
                     query.ToUpper().Contains("INSERT") || query.ToUpper().Contains("UPDATE") || query.ToUpper().Contains("DELETE")
                    )
                    && query.ToUpper().Contains("SELECT"))
                    throw new Exception("SELECT와 다른 명령어는 동시에 사용 불가능 - 기술 부족");

                if (query.ToUpper().Contains("CREATE") || query.ToUpper().Contains("ALTER") || query.ToUpper().Contains("DROP") ||
                    query.ToUpper().Contains("INSERT") || query.ToUpper().Contains("UPDATE") || query.ToUpper().Contains("DELETE"))
                {
                    this._db_connector.BeginTransaction();

                    result = this._db_connector.ExecuteNonQuery(query);

                    this._db_connector.EndTransaction(result);

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

                    ds = this._db_connector.ExecuteQuery(query);

                    if (ds != null)
                    {
                        this._db_current = ds;

                        this.TableNames.Clear();
                        for (int i = 0; i < this._db_current.Tables.Count; i++)
                            this.TableNames.Add(this._db_current.Tables[i].TableName);
                        TableNames.ResetBindings();

                        this.SelectedName = TableNames[0];

                        result = true;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result = false;
                this._db_connector.EndTransaction(result);

                base.OnErrorMessage($"Message: {ex.Message}\r\nTrace:{ex.StackTrace}");
            }

            return result;
        }
    }
}
