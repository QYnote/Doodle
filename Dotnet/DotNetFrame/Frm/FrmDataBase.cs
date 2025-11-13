using DotNet.Database;
using DotNet.Utils.Controls.Utils;
using DotNetFrame.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Frm
{
    public partial class FrmDataBase : Form
    {
        private string defaultDir = "C:\\WorkerFile\\업무자료\\01_TCS\\01_source\\TCS.Data\\Products";
        private string defaultFileName = "tcsdr.sdf";

        private DBCommon _DB = null;

        #region UI Controls

        private GroupBox gbxDatabase = new GroupBox();
        private Label lblDBType = new Label();
        private Label lblDBPath = new Label();
        private Label lblID = new Label();
        private Label lblPassword = new Label();
        private ComboBox cboDBType = new ComboBox();
        private TextBox txtDBPath = new TextBox();
        private TextBox txtID = new TextBox();
        private TextBox txtPassword = new TextBox();
        private Button btnSelectPath = new Button();
        private Button btnConnect = new Button();

        private Label lblSavePath = new Label();
        private TextBox txtSavepath = new TextBox();
        private Button btnSelectSavepath = new Button();

        private SplitContainer spltPnl = new SplitContainer();

        private TextBox txtQuery = new TextBox();
        private Button btnQuery = new Button();

        private ComboBox cboTable = new ComboBox();
        private DataGridView gvTable = new DataGridView();

        #endregion UI Controls

        private DataSet _ds;

        public FrmDataBase()
        {
            InitializeComponent();
            InitUI();

            this._ds = new DataSet();
        }

        private void InitUI()
        {
            #region Database 설정

            this.lblDBType.Location = new Point(3, 18);
            this.lblDBType.Width = 55;
            this.lblDBType.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDBType.Text = AppData.Lang("frmdb.dbproperty.type");
            this.cboDBType.Location = new Point(this.lblDBType.Location.X + this.lblDBType.Width, this.lblDBType.Location.Y);
            this.cboDBType.Height = this.lblDBType.Height;
            this.cboDBType.DataSource = QYUtils.GetEnumItems<DataBaseType>();
            this.cboDBType.ValueMember = "Value";
            this.cboDBType.DisplayMember = "Name";
            this.cboDBType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDBType.SelectedValueChanged += CboDBType_SelectedValueChanged;

            this.lblDBPath.Location = new Point(this.lblDBType.Location.X, this.lblDBType.Location.Y + this.lblDBType.Height + 3);
            this.lblDBPath.Width = this.lblDBType.Width;
            this.lblDBPath.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDBPath.Text = AppData.Lang("frmdb.dbproperty.filepath");
            this.txtDBPath.Location = new Point(this.lblDBPath.Location.X + this.lblDBPath.Width, this.lblDBPath.Location.Y);
            this.txtDBPath.Height = this.lblDBPath.Height;
            this.btnSelectPath.Location = new Point(this.txtDBPath.Location.X + this.txtDBPath.Width, this.txtDBPath.Location.Y - 1);
            this.btnSelectPath.Height = this.txtDBPath.Height + 2;
            this.btnSelectPath.Width = this.btnSelectPath.Height;
            this.btnSelectPath.Text = "...";
            this.btnSelectPath.Click += BtnSelectPath_Click;

            this.lblID.Location = new Point(this.lblDBType.Location.X, this.lblDBPath.Location.Y + this.lblDBPath.Height + 3);
            this.lblID.Width = this.lblDBType.Width;
            this.lblID.TextAlign = ContentAlignment.MiddleCenter;
            this.lblID.Text = AppData.Lang("frmdb.dbproperty.id");
            this.txtID.Location = new Point(this.lblID.Location.X + this.lblID.Width, this.lblID.Location.Y);
            this.txtID.Height = this.lblID.Height;

            this.lblPassword.Location = new Point(this.lblDBType.Location.X, this.lblID.Location.Y + this.lblID.Height + 3);
            this.lblPassword.Width = this.lblDBType.Width;
            this.lblPassword.TextAlign = ContentAlignment.MiddleCenter;
            this.lblPassword.Text = AppData.Lang("frmdb.dbproperty.pw");
            this.txtPassword.Location = new Point(this.lblPassword.Location.X + this.lblPassword.Width, this.lblPassword.Location.Y);
            this.txtPassword.Height = this.lblPassword.Height;

            this.btnConnect.Location = new Point(this.txtPassword.Location.X - 1, this.txtPassword.Location.Y + this.txtPassword.Height + 3);
            this.btnConnect.Width = (this.btnSelectPath.Location.X + this.btnSelectPath.Width) - this.btnConnect.Location.X;
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += BtnConnect_Click;

            this.gbxDatabase.Location = new Point(3, 3);
            this.gbxDatabase.Text = AppData.Lang("frmdb.dbproperty.title");
            this.gbxDatabase.Width = this.btnSelectPath.Location.X + this.btnSelectPath.Width + 3;
            this.gbxDatabase.Height = this.btnConnect.Location.Y + this.btnConnect.Height + 4;

            #endregion Database 설정

            this.lblSavePath.Location = new Point(this.gbxDatabase.Location.X + this.gbxDatabase.Width + 3, this.lblDBType.Location.Y + 2);
            this.lblSavePath.Width = 80;
            this.lblSavePath.TextAlign = ContentAlignment.MiddleCenter;
            this.lblSavePath.Text = AppData.Lang("frmdb.dbproperty.logpath");
            this.txtSavepath.Location = new Point(this.lblSavePath.Location.X + this.lblSavePath.Width, this.lblSavePath.Location.Y);
            this.txtSavepath.Height = this.lblDBPath.Height;
            this.txtSavepath.ReadOnly = true;
            this.btnSelectSavepath.Location = new Point(this.txtSavepath.Location.X + this.txtSavepath.Width, this.txtSavepath.Location.Y - 1);
            this.btnSelectSavepath.Height = this.txtSavepath.Height + 2;
            this.btnSelectSavepath.Width = this.btnSelectSavepath.Height;
            this.btnSelectSavepath.Text = "...";
            this.btnSelectSavepath.Click += BtnSelectSavepath_Click;



            this.txtQuery.Dock = DockStyle.Fill;
            this.txtQuery.Multiline = true;
            this.txtQuery.WordWrap = false;
            this.txtQuery.ScrollBars = ScrollBars.Both;

            this.btnQuery.Dock = DockStyle.Bottom;
            this.btnQuery.Text = "Query";
            this.btnQuery.Click += BtnQuery_Click;



            this.cboTable.Location = new Point(3, 3);
            this.cboTable.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboTable.SelectedIndexChanged += CboTable_SelectedIndexChanged;

            this.spltPnl.Location = new Point(this.gbxDatabase.Location.X, this.gbxDatabase.Location.Y + this.gbxDatabase.Height + 3);
            this.spltPnl.Width = this.ClientSize.Width - (this.spltPnl.Location.X + 3);
            this.spltPnl.Height = this.ClientSize.Height - (this.spltPnl.Location.Y + 3);
            this.spltPnl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            this.gvTable.Location = new Point(this.cboTable.Location.X, this.cboTable.Location.Y + this.cboTable.Height + 3);
            this.gvTable.Width = this.spltPnl.Panel2.ClientSize.Width - 6;
            this.gvTable.Height = this.spltPnl.Panel2.ClientSize.Height - (this.cboTable.Height + 9);
            this.gvTable.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            this.gvTable.AllowUserToAddRows = false;
            this.gvTable.AllowUserToDeleteRows = false;
            this.gvTable.AllowUserToResizeRows = false;
            this.gvTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.gvTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.gvTable.RowHeadersVisible = false;
            this.gvTable.ReadOnly = true;


            this.gbxDatabase.Controls.Add(this.lblDBType);
            this.gbxDatabase.Controls.Add(this.cboDBType);
            this.gbxDatabase.Controls.Add(this.lblDBPath);
            this.gbxDatabase.Controls.Add(this.txtDBPath);
            this.gbxDatabase.Controls.Add(this.btnSelectPath);
            this.gbxDatabase.Controls.Add(this.lblID);
            this.gbxDatabase.Controls.Add(this.txtID);
            this.gbxDatabase.Controls.Add(this.lblPassword);
            this.gbxDatabase.Controls.Add(this.txtPassword);
            this.gbxDatabase.Controls.Add(this.btnConnect);
            this.spltPnl.Panel1.Controls.Add(this.txtQuery);
            this.spltPnl.Panel1.Controls.Add(this.btnQuery);
            this.spltPnl.Panel2.Controls.Add(this.cboTable);
            this.spltPnl.Panel2.Controls.Add(this.gvTable);
            this.Controls.Add(this.gbxDatabase);
            this.Controls.Add(this.lblSavePath);
            this.Controls.Add(this.txtSavepath);
            this.Controls.Add(this.btnSelectSavepath);
            this.Controls.Add(this.spltPnl);


            if (this.cboDBType.Items.Count > 0) this.cboDBType.SelectedIndex = 0;
            this.txtDBPath.Text = $"{defaultDir}\\{defaultFileName}";
            this.txtDBPath.ReadOnly = true;
            this.txtID.ReadOnly = true;
            this.txtPassword.Text = "admin123";
            this.txtSavepath.Text = $"{defaultDir}";
        }

        #region UI Event
        //이벤트 모음
        //UI 변경요소만 이곳에서 진행
        //UI외적으로 다른 class나 계산 Method 같은 경우는 따로 Method를 빼서 UI값을 던져주도록 작성

        private void CboDBType_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo.SelectedValue is DataBaseType type == false) return;

            if (type == DataBaseType.SQLCe)
            {
                this.txtDBPath.ReadOnly = true;
                this.btnSelectPath.Visible = true;
                this.txtID.ReadOnly = true;
            }
            else if (type == DataBaseType.SQLite)
            {
                DirectoryInfo runDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                DirectoryInfo binDir = runDir.Parent;
                DirectoryInfo runProjectDir = binDir.Parent;
                DirectoryInfo solutionDir = runProjectDir.Parent;
                string path = $"{solutionDir.FullName}\\DotNet.Database\\Resources\\SQLite";

                this.txtDBPath.Text = $"{path}\\QYDB.sqlite";
                this.txtSavepath.Text = path;
                this.txtPassword.Text = string.Empty;

                this.txtDBPath.ReadOnly = true;
                this.btnSelectPath.Visible = true;
                this.txtID.ReadOnly = true;
            }
        }

        private void BtnSelectPath_Click(object sender, EventArgs e)
        {
            if (this.cboDBType.SelectedValue is DataBaseType dbType == false) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            if (dbType == DataBaseType.SQLCe)
            {
                ofd.DefaultExt = "sdf";
                ofd.Filter = "SQL Server Compact Edition Database Files(*.sdf)|*.sdf";
            }
            else if(dbType == DataBaseType.SQLite)
            {
                ofd.DefaultExt = "sqlite";
                ofd.Filter = "SQLite Database Files(*.sqlite)|*.sqlite" +
                            "|SQLite Database Files(*.db)|*.db";
            }

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.txtDBPath.Text = ofd.FileName;
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (this.cboDBType.SelectedValue is DataBaseType dbType == false) return;
            string dbPath = this.txtDBPath.Text,
                   id = this.txtID.Text,
                   pw = this.txtPassword.Text;
            string extention = dbPath.Split('.').Last();
            bool enable = false;

            switch (dbType)
            {
                case DataBaseType.SQLCe:
                    if (extention == "sdf") enable = true;
                    break;
                case DataBaseType.SQLite:
                    if (extention == "sqlite" || extention == "db" || extention == string.Empty) enable = true;
                    break;
            }

            if(enable == false)
            {
                MessageBox.Show("호환하는 파일이 아닙니다.");
                return;
            }

            this._DB = null;

            switch (dbType)
            {
                case DataBaseType.SQLCe:
                    this._DB = new SQLCe(dbPath, pw);
                    break;
                case DataBaseType.SQLite:
                    this._DB = new SQLite(dbPath, pw);
                    break;
                //case DataBaseType.MySQL:
                //    this._DB = new MySQL(dbPath, id, pw);
                //    break;
            }

            if(this._DB != null)
            {
                this._DB.LogEvent += (msg) => { MessageBox.Show(msg); };
            }
        }

        private void BtnSelectSavepath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = "C:\\";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.txtSavepath.Text = dialog.SelectedPath;
            }
        }

        private void BtnQuery_Click(object sender, EventArgs e)
        {
            if (this._DB == null)
            {
                MessageBox.Show(string.Format("DB 미연결"));
                return;
            }

            string query = this.txtQuery.Text;

            if ((query.ToUpper().Contains("INSERT") || query.ToUpper().Contains("UPDATE") || query.ToUpper().Contains("DELETE"))
                    && query.ToUpper().Contains("SELECT"))
            {
                MessageBox.Show("SELECT와 INSERT, UPDATE, DELETE는 동시에 사용 불가능" +
                    "-기술 부족");
                return;
            }

            if (query.ToUpper().Contains("SELECT"))
            {
                if (this.SendQuery(query))
                {
                    this.cboTable.Items.Clear();

                    for (int i = 0; i < this._ds.Tables.Count; i++)
                        this.cboTable.Items.Add(this._ds.Tables[i].TableName);

                    if(this.cboTable.Items.Count > 0)
                    {
                        if (this.cboTable.SelectedIndex == 0)
                            this.UpdateGrid(0);
                        else
                            this.cboTable.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                if (this.SendQuery(query))
                {
                    MessageBox.Show("Query 완료");
                }
            }
        }

        private void CboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cbo = sender as ComboBox;

            this.UpdateGrid(cbo.SelectedIndex);
        }

        private void UpdateGrid(int tableIndex)
        {
            this.gvTable.Columns.Clear();

            if (this._ds != null)
                this.gvTable.DataSource = this._ds.Tables[this.cboTable.SelectedIndex];
        }

        #endregion UI UI EventEvent End

        #region Run Method

        private bool SendQuery(string query)
        {
            bool result = false;
            
            try
            {
                if (query.ToUpper().Contains("CREATE") ||
                    query.ToUpper().Contains("ALTER") ||
                    query.ToUpper().Contains("DROP") ||
                    query.ToUpper().Contains("INSERT") ||
                    query.ToUpper().Contains("UPDATE") ||
                    query.ToUpper().Contains("DELETE"))
                {
                    this._DB.BeginTransaction();

                    result = this._DB.ExecuteNonQuery(query);

                    this._DB.EndTransaction(result);

                    if (result)
                    {
                        //Query 로그 저장
                        string logText =
                            $"============================================================{Environment.NewLine}" +
                            $"{DateTime.Now}{Environment.NewLine}" +
                            $"{query}{Environment.NewLine}" +
                            $"============================================================{Environment.NewLine}" +
                            $"{Environment.NewLine}";
                        string logpath = $"{this.txtSavepath.Text}\\sqlLog.txt";

                        Directory.CreateDirectory(Path.GetDirectoryName(logpath));

                        File.AppendAllText(logpath, logText);
                    }
                }
                else if (query.ToUpper().Contains("SELECT"))
                {
                    DataSet ds = null;

                    ds = this._DB.ExecuteQuery(query);

                    if(ds != null)
                    {
                        this._ds = ds;

                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                MessageBox.Show($"[Error]{ex.Message}\r\nTrace:{ex.StackTrace}");
                this._DB.EndTransaction(result);
            }

            return result;
        }

        #endregion Run Method
    }
}
