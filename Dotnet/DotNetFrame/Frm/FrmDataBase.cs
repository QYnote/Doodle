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
        private Button btnSourcePath = new Button();
        private Button btnConnect = new Button();

        private Label lblSavePath = new Label();
        private TextBox txtSavepath = new TextBox();
        private Button btnLogPath = new Button();

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
            InitText();

            this._ds = new DataSet();
        }

        private void InitUI()
        {
            InitUI_Property();
            this.txtQuery.Dock = DockStyle.Fill;
            this.txtQuery.Multiline = true;
            this.txtQuery.WordWrap = false;
            this.txtQuery.ScrollBars = ScrollBars.Both;
            this.txtQuery.KeyUp += TxtQuery_KeyUp;

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


            this.spltPnl.Panel1.Controls.Add(this.txtQuery);
            this.spltPnl.Panel1.Controls.Add(this.btnQuery);
            this.spltPnl.Panel2.Controls.Add(this.cboTable);
            this.spltPnl.Panel2.Controls.Add(this.gvTable);
            this.Controls.Add(this.spltPnl);


            if (this.cboDBType.Items.Count > 0) this.cboDBType.SelectedIndex = 0;
            this.txtDBPath.Text = $"{defaultDir}\\{defaultFileName}";
            this.txtDBPath.ReadOnly = true;
            this.txtID.ReadOnly = true;
            this.txtPassword.Text = "admin123";
            this.txtSavepath.Text = $"{defaultDir}";
        }

        private void TxtQuery_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.F5)
                this.BtnQuery_Click(sender, e);
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

                    if (this.cboTable.Items.Count > 0)
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

        private void InitUI_Property()
        {
            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;
            pnl.Padding = new Padding(3);

            this.gbxDatabase.Padding = new Padding(3);
            this.gbxDatabase.Dock = DockStyle.Left;

            this.lblDBType.Location = new Point(3, 18);
            this.lblDBType.Width = 85;
            this.lblDBType.TextAlign = ContentAlignment.MiddleCenter;

            this.cboDBType.Left= this.lblDBType.Right + 3;
            this.cboDBType.Top = this.lblDBType.Top;
            this.cboDBType.DataSource = QYUtils.GetEnumItems<DataBaseType>();
            this.cboDBType.ValueMember = "Value";
            this.cboDBType.DisplayMember = "Name";
            this.cboDBType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDBType.SelectedValueChanged += CboDBType_SelectedValueChanged;


            this.lblDBPath.Left = this.lblDBType.Left;
            this.lblDBPath.Top = this.lblDBType.Bottom + 3;
            this.lblDBPath.Width = this.lblDBType.Width;
            this.lblDBPath.TextAlign = ContentAlignment.MiddleCenter;

            this.txtDBPath.Left = this.lblDBPath.Right + 3;
            this.txtDBPath.Top = this.lblDBPath.Top;
            this.txtDBPath.Height = this.lblDBPath.Height;
            this.txtDBPath.Width = 600;

            this.btnSourcePath.Left = this.txtDBPath.Right + 3;
            this.btnSourcePath.Top = this.txtDBPath.Top - 1;
            this.btnSourcePath.Height = this.txtDBPath.Height + 2;
            this.btnSourcePath.Width = this.btnSourcePath.Height;
            this.btnSourcePath.Text = "...";
            this.btnSourcePath.Click += btnSourcePath_Click;


            this.lblID.Left = this.lblDBPath.Left;
            this.lblID.Top = this.lblDBPath.Bottom + 3;
            this.lblID.Width = this.lblDBType.Width;
            this.lblID.TextAlign = ContentAlignment.MiddleCenter;

            this.txtID.Left = this.lblID.Right + 3;
            this.txtID.Top = this.lblID.Top;
            this.txtID.Height = this.lblID.Height;

            this.lblPassword.Left = this.txtID.Right + 3;
            this.lblPassword.Top = this.txtID.Top;
            this.lblPassword.Width = this.lblDBType.Width;
            this.lblPassword.TextAlign = ContentAlignment.MiddleCenter;

            this.txtPassword.Left = this.lblPassword.Right + 3;
            this.txtPassword.Top = this.lblPassword.Top;
            this.txtPassword.Height = this.lblPassword.Height;


            this.lblSavePath.Left = this.lblID.Left;
            this.lblSavePath.Top = this.lblID.Bottom + 3;
            this.lblSavePath.Width = this.lblDBType.Width;
            this.lblSavePath.TextAlign = ContentAlignment.MiddleCenter;

            this.txtSavepath.Left = this.lblSavePath.Right + 3;
            this.txtSavepath.Top = this.lblSavePath.Top;
            this.txtSavepath.Width = this.txtDBPath.Width;
            this.txtSavepath.ReadOnly = true;

            this.btnLogPath.Left = this.txtSavepath.Right + 3;
            this.btnLogPath.Top = this.txtSavepath.Top - 1;
            this.btnLogPath.Height = this.txtSavepath.Height + 2;
            this.btnLogPath.Width = this.btnLogPath.Height;
            this.btnLogPath.Text = "...";
            this.btnLogPath.Click += btnLogPath_Click;


            this.btnConnect.Left = this.txtSavepath.Left;
            this.btnConnect.Top = this.lblSavePath.Bottom + 3;
            this.btnConnect.Width = this.cboDBType.Width;
            this.btnConnect.Click += BtnConnect_Click;

            this.gbxDatabase.Width = this.btnLogPath.Right + 3;
            this.gbxDatabase.Height = this.btnConnect.Bottom + 9;

            pnl.Height = this.gbxDatabase.Bottom + 3;

            this.gbxDatabase.Controls.Add(this.lblDBType);
            this.gbxDatabase.Controls.Add(this.cboDBType);
            this.gbxDatabase.Controls.Add(this.lblDBPath);
            this.gbxDatabase.Controls.Add(this.txtDBPath);
            this.gbxDatabase.Controls.Add(this.btnSourcePath);
            this.gbxDatabase.Controls.Add(this.lblID);
            this.gbxDatabase.Controls.Add(this.txtID);
            this.gbxDatabase.Controls.Add(this.lblPassword);
            this.gbxDatabase.Controls.Add(this.txtPassword);
            this.gbxDatabase.Controls.Add(this.lblSavePath);
            this.gbxDatabase.Controls.Add(this.txtSavepath);
            this.gbxDatabase.Controls.Add(this.btnLogPath);
            this.gbxDatabase.Controls.Add(this.btnConnect);
            pnl.Controls.Add(this.gbxDatabase);
            this.Controls.Add(pnl);
        }

        private void CboDBType_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo.SelectedValue is DataBaseType type == false) return;

            if (type == DataBaseType.SQLCe)
            {
                this.txtDBPath.ReadOnly = true;
                this.btnSourcePath.Visible = true;
                this.txtID.ReadOnly = true;
            }
            else if (type == DataBaseType.SQLite)
            {
                DirectoryInfo runDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                DirectoryInfo binDir = runDir.Parent;
                DirectoryInfo runProjectDir = binDir.Parent;
                DirectoryInfo solutionDir = runProjectDir.Parent;
                string path = string.Empty;
                if (Directory.Exists($"{solutionDir.FullName}\\DotNet.Database"))
                {
                    path = $"{solutionDir.FullName}\\DotNet.Database\\Resources\\SQLite";

                    this.txtDBPath.Text = $"{path}\\QYDB.sqlite";
                    this.txtSavepath.Text = path;
                }
                this.txtPassword.Text = string.Empty;

                this.txtDBPath.ReadOnly = false;
                this.btnSourcePath.Visible = true;
                this.txtID.ReadOnly = true;
            }
        }

        private void btnSourcePath_Click(object sender, EventArgs e)
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
            else if (dbType == DataBaseType.SQLite)
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

            if (enable == false)
            {
                MessageBox.Show("호환하는 파일이 아닙니다.");
                return;
            }

            SetDataBase(
                dbType: dbType,
                source: dbPath,
                id: id,
                password: pw);
        }

        private void btnLogPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = "C:\\";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.txtSavepath.Text = dialog.SelectedPath;
            }
        }

        private void InitText()
        {
            this.gbxDatabase.Text = AppData.Lang("frmdb.dbproperty.title");
            this.lblDBType.Text = AppData.Lang("frmdb.dbproperty.type");
            this.lblDBPath.Text = AppData.Lang("frmdb.dbproperty.filepath");
            this.lblID.Text = AppData.Lang("frmdb.dbproperty.id");
            this.lblPassword.Text = AppData.Lang("frmdb.dbproperty.pw");
            this.lblSavePath.Text = AppData.Lang("frmdb.dbproperty.logpath");
            this.btnConnect.Text = "Connect";
        }

        #region Method

        private void SetDataBase(DataBaseType dbType, string source, string id = "", string password = "")
        {
            this._DB = null;

            switch (dbType)
            {
                case DataBaseType.SQLCe:
                    this._DB = new SQLCe(source, password);
                    break;
                case DataBaseType.SQLite:
                    this._DB = new SQLite(source, password);
                    break;
                    //case DataBaseType.MySQL:
                    //    this._DB = new MySQL(dbPath, id, pw);
                    //    break;
            }

            if (this._DB != null)
            {
                this._DB.LogEvent += (msg) => { MessageBox.Show(msg); };
            }
        }

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

        private void UpdateGrid(int tableIndex)
        {
            this.gvTable.Columns.Clear();

            if (this._ds != null)
                this.gvTable.DataSource = this._ds.Tables[this.cboTable.SelectedIndex];
        }

        #endregion Run Method
    }
}
