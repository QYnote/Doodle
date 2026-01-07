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
using System.Windows.Forms;

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

        public List<QYUtils.EnumItem<DataBaseType>> TypeList => this._type_list;

        public DataBaseType Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;

                    this.UpdateType(this.Type);

                    this.OnPropertyChanged(nameof(Type));
                    base.OnPropertyChanged(nameof(this.DataSource_Enable));
                    base.OnPropertyChanged(nameof(this.DataSource_Folder_Enable));
                    base.OnPropertyChanged(nameof(this.ID_Enable));
                    base.OnPropertyChanged(nameof(this.Pasword_Enable));
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

        public bool DataSource_Enable
        {
            get
            {
                switch (this.Type)
                {
                    case DataBaseType.SQLite: return true;
                    case DataBaseType.SQLCe:
                    default: return false;
                }
            }
        }
        public bool DataSource_Folder_Enable
        {
            get
            {
                switch (this.Type)
                {
                    case DataBaseType.SQLite: return true;
                    case DataBaseType.SQLCe:
                    default: return false;
                }
            }
        }
        public bool ID_Enable
        {
            get
            {
                switch (this.Type)
                {
                    case DataBaseType.SQLite:
                    case DataBaseType.SQLCe:
                    default: return false;
                }
            }
        }
        public bool Pasword_Enable
        {
            get
            {
                switch (this.Type)
                {
                    case DataBaseType.SQLite:
                    case DataBaseType.SQLCe:
                    default: return true;
                }
            }
        }


        public BindingList<string> TableNames => this._db_current_table_list;
        public string SelectedName
        {
            get => this._db_current_table_name;
            set
            {
                if(this._db_current_table_name != value)
                {
                    this._db_current_table_name = value;

                    base.OnPropertyChanged(nameof(this.SelectedName));
                    base.OnPropertyChanged(nameof(this.CurrentTable));
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
            this.DataSource = $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite\\{SQLite.DEFAULT_SQLITE_DIR_FILENAME}";
            this.Password = SQLite.DEFAULT_SQLITE_PASSWORD;
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

        private bool InputDatabaseInfo()
        {
            bool result = false;

            try
            {
                this._db_connector.DataSource = this.DataSource;
                this._db_connector.ID = this.ID;
                this._db_connector.Password = this.Password;

                result = true;
            }
            catch(Exception ex)
            {
                base.OnErrorMessage($"Message: {ex.Message}\r\nTrace:{ex.StackTrace}");
            }

            return result;
        }

        internal bool SendQuery(string query)
        {
            bool result = false;

            try
            {
                if (this.InputDatabaseInfo())
                {
                    if ((query.ToUpper().Contains("CREATE") || query.ToUpper().Contains("ALTER") || query.ToUpper().Contains("DROP") ||
                         query.ToUpper().Contains("INSERT") || query.ToUpper().Contains("UPDATE") || query.ToUpper().Contains("DELETE")
                        )
                        && query.ToUpper().Contains("SELECT"))
                        throw new Exception("SELECT와 다른 명령어는 동시에 사용 불가능 - 기술 부족");

                    //DB정보 변경사항 적용
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
                            this.TableNames.ResetBindings();

                            if(this.TableNames.Count > 0)
                                this.SelectedName = this.TableNames[0];

                            result = true;
                        }
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

        internal void Open_DataSource()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            if (this.Type == DataBaseType.SQLCe)
            {
                ofd.DefaultExt = "sdf";
                ofd.Filter = "SQL Server Compact Edition Database Files(*.sdf)|*.sdf";
            }
            else if (this.Type == DataBaseType.SQLite)
            {
                ofd.DefaultExt = "sqlite";
                ofd.Filter = "SQLite Database Files(*.sqlite)|*.sqlite" +
                            "|SQLite Database Files(*.db)|*.db";
            }

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.DataSource = ofd.FileName;
            }
        }
        internal void OpenFolder_DataSource()
        {
            if (this.Type != DataBaseType.SQLite) return;

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = "C:\\";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.DataSource = dialog.SelectedPath;
            }
        }
        internal void OpenFolder_Log()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = "C:\\";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.LogDirectory = dialog.SelectedPath;
            }
        }
    }
}
