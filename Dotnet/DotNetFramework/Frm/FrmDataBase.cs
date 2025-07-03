using DotNet.Database;
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

namespace DotNetFramework.Frm
{
    public partial class FrmDataBase : Form
    {
        private SQLCe _SQL = new SQLCe(
            "C:\\WorkerFile\\업무자료\\TCS\\TCS\\01_source\\01_TCS\\01_TCS\\TCS\\TCS.Data\\Products\\tcsdr.sdf",
            "admin123");

        private DBCommon _DB;

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
        }

        private void InitUI()
        {
            #region Database 설정

            this.lblDBType.Location = new Point(3, 18);
            this.lblDBType.Width = 55;
            this.lblDBType.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDBType.Text = "DB 종류";
            this.cboDBType.Location = new Point(this.lblDBType.Location.X + this.lblDBType.Width, this.lblDBType.Location.Y);
            this.cboDBType.Height = this.lblDBType.Height;
            this.cboDBType.Items.AddRange(new string[] { "SQL CE ~3.0" });
            this.cboDBType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboDBType.SelectedIndexChanged += (s, e) =>
            {
                if(this.cboDBType.SelectedIndex == 0)
                {
                    this.txtDBPath.ReadOnly = true;
                    this.btnSelectPath.Visible = true;
                    this.txtID.ReadOnly = true;
                }
            };

            this.lblDBPath.Location = new Point(this.lblDBType.Location.X, this.lblDBType.Location.Y + this.lblDBType.Height + 3);
            this.lblDBPath.Width = this.lblDBType.Width;
            this.lblDBPath.TextAlign = ContentAlignment.MiddleCenter;
            this.lblDBPath.Text = "경로";
            this.txtDBPath.Location = new Point(this.lblDBPath.Location.X + this.lblDBPath.Width, this.lblDBPath.Location.Y);
            this.txtDBPath.Height = this.lblDBPath.Height;
            this.btnSelectPath.Location = new Point(this.txtDBPath.Location.X + this.txtDBPath.Width, this.txtDBPath.Location.Y - 1);
            this.btnSelectPath.Height = this.txtDBPath.Height + 2;
            this.btnSelectPath.Width = this.btnSelectPath.Height;
            this.btnSelectPath.Text = "...";
            this.btnSelectPath.Click += (s, e) =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Multiselect = false;
                ofd.DefaultExt = "sdf";
                ofd.Filter = "SQL Server Compact Edition Database Files(*.sdf)|*.sdf";
                ofd.RestoreDirectory = true;

                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    this.txtDBPath.Text = ofd.FileName;
                }
            };

            this.lblID.Location = new Point(this.lblDBType.Location.X, this.lblDBPath.Location.Y + this.lblDBPath.Height + 3);
            this.lblID.Width = this.lblDBType.Width;
            this.lblID.TextAlign = ContentAlignment.MiddleCenter;
            this.lblID.Text = "ID";
            this.txtID.Location = new Point(this.lblID.Location.X + this.lblID.Width, this.lblID.Location.Y);
            this.txtID.Height = this.lblID.Height;

            this.lblPassword.Location = new Point(this.lblDBType.Location.X, this.lblID.Location.Y + this.lblID.Height + 3);
            this.lblPassword.Width = this.lblDBType.Width;
            this.lblPassword.TextAlign = ContentAlignment.MiddleCenter;
            this.lblPassword.Text = "PW";
            this.txtPassword.Location = new Point(this.lblPassword.Location.X + this.lblPassword.Width, this.lblPassword.Location.Y);
            this.txtPassword.Height = this.lblPassword.Height;

            this.btnConnect.Location = new Point(this.txtPassword.Location.X - 1, this.txtPassword.Location.Y + this.txtPassword.Height + 3);
            this.btnConnect.Width = (this.btnSelectPath.Location.X + this.btnSelectPath.Width) - this.btnConnect.Location.X;
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += (s, e) =>
            {
                if(this.cboDBType.SelectedIndex == 0)
                {
                    this._DB = new SQLCe(this.txtDBPath.Text, this.txtPassword.Text);
                }

                this._DB.LogEvent += (log) => { MessageBox.Show(log); };
            };

            this.gbxDatabase.Location = new Point(3, 3);
            this.gbxDatabase.Text = "Database 설정";
            this.gbxDatabase.Width = this.btnSelectPath.Location.X + this.btnSelectPath.Width + 3;
            this.gbxDatabase.Height = this.btnConnect.Location.Y + this.btnConnect.Height + 4;

            #endregion Database 설정

            this.lblSavePath.Location = new Point(this.gbxDatabase.Location.X + this.gbxDatabase.Width + 3, this.lblDBType.Location.Y + 2);
            this.lblSavePath.Width = 80;
            this.lblSavePath.TextAlign = ContentAlignment.MiddleCenter;
            this.lblSavePath.Text = "Log파일 경로";
            this.txtSavepath.Location = new Point(this.lblSavePath.Location.X + this.lblSavePath.Width, this.lblSavePath.Location.Y);
            this.txtSavepath.Height = this.lblDBPath.Height;
            this.txtSavepath.ReadOnly = true;
            this.btnSelectSavepath.Location = new Point(this.txtSavepath.Location.X + this.txtSavepath.Width, this.txtSavepath.Location.Y - 1);
            this.btnSelectSavepath.Height = this.txtSavepath.Height + 2;
            this.btnSelectSavepath.Width = this.btnSelectSavepath.Height;
            this.btnSelectSavepath.Text = "...";
            this.btnSelectSavepath.Click += (s, e) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.SelectedPath = "C:\\";
                dialog.ShowNewFolderButton = true;

                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtSavepath.Text = dialog.SelectedPath;
                }
            };



            this.txtQuery.Dock = DockStyle.Fill;
            this.txtQuery.Multiline = true;
            this.txtQuery.WordWrap = false;
            this.txtQuery.ScrollBars = ScrollBars.Both;

            this.btnQuery.Dock = DockStyle.Bottom;
            this.btnQuery.Text = "Query";
            this.btnQuery.Click += (s, e) => { SendQuery(); };



            this.cboTable.Location = new Point(3, 3);
            this.cboTable.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboTable.SelectedIndexChanged += (s, e) =>
            {
                ChangeTable();
            };

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
            this.txtDBPath.Text = "C:\\WorkerFile\\업무자료\\TCS\\TCS\\01_source\\01_TCS\\01_TCS\\TCS\\TCS.Data\\Products\\tcsdr.sdf";
            this.txtDBPath.ReadOnly = true;
            this.txtID.ReadOnly = true;
            this.txtPassword.Text = "admin123";
            this.txtSavepath.Text = "C:\\WorkerFile\\업무자료\\TCS\\TCS\\01_source\\01_TCS\\01_TCS\\TCS\\TCS.Data\\Products";
        }

        private void SendQuery()
        {
            if(this._DB == null)
            {
                MessageBox.Show(string.Format("DB 미연결"));
                return;
            }

            try
            {
                string txt = this.txtQuery.Text;

                this.cboTable.Items.Clear();
                this.gvTable.DataSource = null;

                if (txt.ToUpper().Contains("INSERT")
                    || txt.ToUpper().Contains("UPDATE")
                    || txt.ToUpper().Contains("DELETE"))
                {
                    bool result = false;
                    this._DB.BeginTransaction();

                    result = this._DB.ExcuteNonQuery(txt);

                    this._DB.EndTransaction(result);

                    if (result)
                    {
                        //Query 로그 저장
                        string logText = $"{DateTime.Now}{Environment.NewLine}{txt}{Environment.NewLine}{Environment.NewLine}";
                        string logpath = string.Format("{0}\\{1}.txt", this.txtSavepath.Text, "sqlLog");

                        Directory.CreateDirectory(Path.GetDirectoryName(logpath));

                        File.AppendAllText(logpath, logText);

                        MessageBox.Show("Query 완료");
                    }
                }
                else if (txt.ToUpper().Contains("SELECT"))
                {
                    this._ds = this._DB.ExcuteQuery<DataSet>(txt);

                    if (this._ds != null)
                    {
                        foreach (DataTable dt in this._ds.Tables)
                        {
                            this.cboTable.Items.Add(dt.TableName);
                        }
                    }

                    if (this.cboTable.Items.Count > 0)
                    {
                        if (this.cboTable.SelectedIndex == 0)
                            ChangeTable();
                        else
                            this.cboTable.SelectedIndex = 0;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("[Error]{0}\r\nTract:{1}", ex.Message, ex.StackTrace));
            }
        }

        private void ChangeTable()
        {
            this.gvTable.Columns.Clear();

            if(this._ds != null)
                this.gvTable.DataSource = this._ds.Tables[this.cboTable.SelectedItem.ToString()];
        }

        private void UpdateGridColumns(string tableName)
        {
            string query = $@"
            SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,
            	CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = '{tableName}';
            SELECT * FROM {tableName};
            ";

            DataSet ds = this._SQL.ExcuteQuery<DataSet>(query);
            this.gvTable.Columns.Clear();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = dr["COLUMN_NAME"].ToString();
                col.DataPropertyName = col.HeaderText;
                switch (dr["DATA_TYPE"].ToString())
                {
                    case "nvarchar": col.MaxInputLength = dr["CHARACTER_MAXIMUM_LENGTH"] == null ? 100 : Convert.ToInt32(dr["CHARACTER_MAXIMUM_LENGTH"].ToString()); break;
                    case "numeric": col.ValueType = typeof(Int64); break;
                    case "int": col.ValueType = typeof(Int32); break;
                    case "smallint": col.ValueType = typeof(Int16); break;
                    case "tinyint": col.ValueType = typeof(byte); break;
                    case "bit": col.ValueType = typeof(bool); break;
                }
                
                this.gvTable.Columns.Add(col);
            }

            this.gvTable.DataSource = ds.Tables[1];
        }
    }
}
