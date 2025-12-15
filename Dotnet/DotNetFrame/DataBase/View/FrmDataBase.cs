using DotNet.Database;
using DotNet.Utils.Controls.Utils;
using DotNetFrame.Base.Model;
using DotNetFrame.DataBase.ViewModel;
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

namespace DotNetFrame.DataBase.View
{
    public partial class FrmDataBase : Form
    {
        private VM_DataBase _db = new VM_DataBase();

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

        private Label lblSavePath = new Label();
        private TextBox txtSavepath = new TextBox();
        private Button btnLogPath = new Button();

        private SplitContainer spltPnl = new SplitContainer();

        private TextBox txtQuery = new TextBox();
        private Button btnQuery = new Button();

        private ComboBox cboTable = new ComboBox();
        private DataGridView gvTable = new DataGridView();

        #endregion UI Controls

        public FrmDataBase()
        {
            InitializeComponent();
            InitUI();
            InitText();

            this.Load += (s, e) =>
            {
                this.cboDBType.SelectedValue = DataBaseType.SQLite;
            };
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
        }

        private void TxtQuery_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.F5)
                this.BtnQuery_Click(sender, e);
        }

        private void BtnQuery_Click(object sender, EventArgs e)
        {
            string query = this.txtQuery.Text;

            try
            {
                if (query.ToUpper().Contains("SELECT"))
                {
                    if (this._db.SendQuery(query))
                    {
                        this.cboTable.Items.Clear();

                        for (int i = 0; i < this._db.DataSet.Tables.Count; i++)
                            this.cboTable.Items.Add(this._db.DataSet.Tables[i].TableName);

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
                    if (this._db.SendQuery(query))
                    {
                        MessageBox.Show("Query 완료");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Message:{ex.Message}\r\n{ex.StackTrace}");
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
            this.txtDBPath.TextChanged += txtDBPath_TextChanged;

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

            this.gbxDatabase.Width = this.btnLogPath.Right + 3;
            this.gbxDatabase.Height = this.lblSavePath.Bottom + 9;

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
            pnl.Controls.Add(this.gbxDatabase);
            this.Controls.Add(pnl);
        }

        private void txtDBPath_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;

            try
            {
                this._db.DataSource = txt.Text;
            }
            catch(NotImplementedException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void CboDBType_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            if (cbo.SelectedValue is DataBaseType type == false) return;

            if (type == DataBaseType.SQLCe)
            {
                this._db.Type = DataBaseType.SQLCe;

                this.txtDBPath.Text = $"{SQLCe.DEFAULT_SQLCE_DIR}\\{SQLCe.DEFAULT_SQLCE_DIR_FILENAME}";
                this.txtSavepath.Text = SQLCe.DEFAULT_SQLCE_DIR;
                this.txtPassword.Text = SQLCe.DEFAULT_SQLCE_PASSWORD;

                this.txtDBPath.ReadOnly = true;
                this.btnSourcePath.Visible = true;
                this.txtID.ReadOnly = true;
            }
            else if (type == DataBaseType.SQLite)
            {
                this._db.Type = DataBaseType.SQLite;

                string path = string.Empty;
                this.txtDBPath.Text = $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite\\{SQLite.DEFAULT_SQLITE_DIR_FILENAME}";
                this.txtSavepath.Text = $"{new DirectoryInfo(SQLite.DEFAULT_SQLITE_DIR).Parent.Parent.Parent.FullName}\\DotNet.Database\\Resources\\SQLite";
                this.txtPassword.Text = SQLite.DEFAULT_SQLITE_PASSWORD;

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
        }

        #region Method

        private void UpdateGrid(int tableIndex)
        {
            this.gvTable.Columns.Clear();

            if (this._db.DataSet != null)
                this.gvTable.DataSource = this._db.DataSet.Tables[this.cboTable.SelectedIndex];
        }

        #endregion Run Method
    }
}
