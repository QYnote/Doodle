using DotNet.Database;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
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
        #region UI Controls

        private GroupBox gbx_db_property = new GroupBox();
        private Label lbl_db_property_type = new Label();
        private Label lbl_db_property_datasource = new Label();
        private Button btn_db_property_datasource_file = new Button();
        private Button btn_db_property_datasource_folder = new Button();
        private Label lbl_db_property_id = new Label();
        private Label lbl_db_property_pw = new Label();
        private Label lbl_db_property_logdirectory = new Label();

        private TextBox txt_query = new TextBox();

        private DataGridView gvTable = new DataGridView();

        #endregion UI Controls

        private DatabaseHandler _dbHandler = new DatabaseHandler();

        public FrmDataBase()
        {
            InitializeComponent();
            InitText();
            InitUI();
            InitComponent();
        }


        private void InitText()
        {
            this.gbx_db_property.Text = AppData.Lang("db.property.title");
            this.lbl_db_property_type.Text = AppData.Lang("db.property.type");
            this.lbl_db_property_datasource.Text = AppData.Lang("db.property.datasource");
            this.lbl_db_property_id.Text = AppData.Lang("db.property.id");
            this.lbl_db_property_pw.Text = AppData.Lang("db.property.pw");
            this.lbl_db_property_logdirectory.Text = AppData.Lang("db.property.logdirectory");
        }

        private void InitUI()
        {
            this.Padding = new Padding(3);

            this.gbx_db_property.Dock = DockStyle.Top;
            this.InitUI_Property(this.gbx_db_property);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;

            GroupBox gbx_db_query = new GroupBox();
            gbx_db_query.Dock = DockStyle.Fill;
            gbx_db_query.Text = "Query";
            this.InitUI_Query(gbx_db_query);

            GroupBox gbx_db_view = new GroupBox();
            gbx_db_view.Dock = DockStyle.Fill;
            gbx_db_view.Text = "Tables";
            this.InitUI_Tables(gbx_db_view);

            split.Panel1.Controls.Add(gbx_db_query);
            split.Panel2.Controls.Add(gbx_db_view);
            this.Controls.Add(split);
            this.Controls.Add(this.gbx_db_property);
        }
        private void InitUI_Property(GroupBox gbx)
        {
            this.lbl_db_property_type.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx));
            this.lbl_db_property_type.TextAlign = ContentAlignment.MiddleLeft;
            ComboBox cbo_db_property_type = new ComboBox();
            cbo_db_property_type.Left = this.lbl_db_property_type.Right + 3;
            cbo_db_property_type.Top = this.lbl_db_property_type.Top;
            cbo_db_property_type.DataSource = this._dbHandler.TypeList;
            cbo_db_property_type.DisplayMember = "DisplayText";
            cbo_db_property_type.ValueMember = "Value";
            cbo_db_property_type.DataBindings.Add("SelectedValue", this._dbHandler, nameof(this._dbHandler.Type), true, DataSourceUpdateMode.OnPropertyChanged);
            cbo_db_property_type.DropDownStyle = ComboBoxStyle.DropDownList;

            this.lbl_db_property_datasource.Left = this.lbl_db_property_type.Left;
            this.lbl_db_property_datasource.Top = this.lbl_db_property_type.Bottom + 3;
            this.lbl_db_property_datasource.TextAlign = ContentAlignment.MiddleLeft;
            TextBox txt_db_property_datasource = new TextBox();
            txt_db_property_datasource.Left = this.lbl_db_property_datasource.Right + 3;
            txt_db_property_datasource.Top = this.lbl_db_property_datasource.Top;
            txt_db_property_datasource.Height = this.lbl_db_property_datasource.Height;
            txt_db_property_datasource.Width = 600;
            txt_db_property_datasource.DataBindings.Add("Text", this._dbHandler, nameof(this._dbHandler.DataSource), true, DataSourceUpdateMode.OnPropertyChanged);
            txt_db_property_datasource.DataBindings.Add("Enabled", this._dbHandler, nameof(this._dbHandler.DataSource_Enable));
            this.btn_db_property_datasource_file.Left = txt_db_property_datasource.Right + 3;
            this.btn_db_property_datasource_file.Top = txt_db_property_datasource.Top - 1;
            this.btn_db_property_datasource_file.Height = txt_db_property_datasource.Height + 2;
            this.btn_db_property_datasource_file.Width = this.btn_db_property_datasource_file.Height;
            this.btn_db_property_datasource_file.ImageAlign = ContentAlignment.MiddleCenter;
            this.btn_db_property_datasource_file.Image = DotNet.Utils.Properties.Resources.Button_Database_16x16;
            this.btn_db_property_datasource_file.FlatStyle = FlatStyle.Flat;
            this.btn_db_property_datasource_file.FlatAppearance.BorderSize = 0;
            this.btn_db_property_datasource_file.Click += Btn_db_property_datasource_file_Click;
            this.btn_db_property_datasource_folder.Left = this.btn_db_property_datasource_file.Right + 3;
            this.btn_db_property_datasource_folder.Top = this.btn_db_property_datasource_file.Top;
            this.btn_db_property_datasource_folder.Height = txt_db_property_datasource.Height + 2;
            this.btn_db_property_datasource_folder.Width = this.btn_db_property_datasource_file.Height;
            this.btn_db_property_datasource_folder.ImageAlign = ContentAlignment.MiddleCenter;
            this.btn_db_property_datasource_folder.Image = DotNet.Utils.Properties.Resources.Button_Folder_16x16;
            this.btn_db_property_datasource_folder.FlatStyle = FlatStyle.Flat;
            this.btn_db_property_datasource_folder.FlatAppearance.BorderSize = 0;
            this.btn_db_property_datasource_folder.DataBindings.Add("Visible", this._dbHandler, nameof(this._dbHandler.DataSource_Folder_Enable));
            this.btn_db_property_datasource_folder.Click += Btn_db_property_datasource_folder_Click;


            this.lbl_db_property_id.Left = this.lbl_db_property_datasource.Left;
            this.lbl_db_property_id.Top = this.lbl_db_property_datasource.Bottom + 3;
            this.lbl_db_property_id.TextAlign = ContentAlignment.MiddleLeft;
            TextBox txt_db_property_id = new TextBox();
            txt_db_property_id.Left = this.lbl_db_property_id.Right + 3;
            txt_db_property_id.Top = this.lbl_db_property_id.Top;
            txt_db_property_id.Height = this.lbl_db_property_id.Height;
            txt_db_property_id.DataBindings.Add("Text", this._dbHandler, nameof(this._dbHandler.ID), true, DataSourceUpdateMode.OnPropertyChanged);
            txt_db_property_id.DataBindings.Add("Enabled", this._dbHandler, nameof(this._dbHandler.ID_Enable));

            this.lbl_db_property_pw.Left = txt_db_property_id.Right + 3;
            this.lbl_db_property_pw.Top = txt_db_property_id.Top;
            this.lbl_db_property_pw.TextAlign = ContentAlignment.MiddleLeft;
            TextBox txt_db_property_pw = new TextBox();
            txt_db_property_pw.Left = this.lbl_db_property_pw.Right + 3;
            txt_db_property_pw.Top = this.lbl_db_property_pw.Top;
            txt_db_property_pw.Height = this.lbl_db_property_pw.Height;
            txt_db_property_pw.DataBindings.Add("Text", this._dbHandler, nameof(this._dbHandler.Password), true, DataSourceUpdateMode.OnPropertyChanged);
            txt_db_property_pw.DataBindings.Add("Enabled", this._dbHandler, nameof(this._dbHandler.Pasword_Enable));

            this.lbl_db_property_logdirectory.Left = this.lbl_db_property_id.Left;
            this.lbl_db_property_logdirectory.Top = this.lbl_db_property_id.Bottom + 3;
            this.lbl_db_property_logdirectory.TextAlign = ContentAlignment.MiddleLeft;
            TextBox txt_db_property_logdirectory = new TextBox();
            txt_db_property_logdirectory.Left = this.lbl_db_property_logdirectory.Right + 3;
            txt_db_property_logdirectory.Top = this.lbl_db_property_logdirectory.Top;
            txt_db_property_logdirectory.Height = this.lbl_db_property_logdirectory.Height;
            txt_db_property_logdirectory.Width = 600;
            txt_db_property_logdirectory.DataBindings.Add("Text", this._dbHandler, nameof(this._dbHandler.LogDirectory), true, DataSourceUpdateMode.OnPropertyChanged);
            Button btn_db_property_logdirectory = new Button();
            btn_db_property_logdirectory.Left = txt_db_property_logdirectory.Right + 3;
            btn_db_property_logdirectory.Top = txt_db_property_logdirectory.Top - 1;
            btn_db_property_logdirectory.Height = txt_db_property_logdirectory.Height + 2;
            btn_db_property_logdirectory.Width = btn_db_property_logdirectory.Height;
            btn_db_property_logdirectory.ImageAlign = ContentAlignment.MiddleCenter;
            btn_db_property_logdirectory.Image = DotNet.Utils.Properties.Resources.Button_Folder_16x16;
            btn_db_property_logdirectory.FlatStyle = FlatStyle.Flat;
            btn_db_property_logdirectory.FlatAppearance.BorderSize = 0;
            btn_db_property_logdirectory.Click += Btn_db_property_logdirectory_Click;

            gbx.Height = this.lbl_db_property_logdirectory.Bottom + 3;

            gbx.Controls.Add(this.lbl_db_property_type);
            gbx.Controls.Add(cbo_db_property_type);
            gbx.Controls.Add(this.lbl_db_property_datasource);
            gbx.Controls.Add(txt_db_property_datasource);
            gbx.Controls.Add(this.btn_db_property_datasource_file);
            gbx.Controls.Add(this.btn_db_property_datasource_folder);
            gbx.Controls.Add(this.lbl_db_property_id);
            gbx.Controls.Add(txt_db_property_id);
            gbx.Controls.Add(this.lbl_db_property_pw);
            gbx.Controls.Add(txt_db_property_pw);
            gbx.Controls.Add(this.lbl_db_property_logdirectory);
            gbx.Controls.Add(txt_db_property_logdirectory);
            gbx.Controls.Add(btn_db_property_logdirectory);
        }
        private void Btn_db_property_datasource_file_Click(object sender, EventArgs e)
        {
            this._dbHandler.Open_DataSource();
        }
        private void Btn_db_property_datasource_folder_Click(object sender, EventArgs e)
        {
            this._dbHandler.OpenFolder_DataSource();
        }
        private void Btn_db_property_logdirectory_Click(object sender, EventArgs e)
        {
            this._dbHandler.OpenFolder_Log();
        }

        private void InitUI_Query(GroupBox gbx)
        {
            this.txt_query.Dock = DockStyle.Fill;
            this.txt_query.Multiline = true;
            this.txt_query.WordWrap = false;
            this.txt_query.ScrollBars = ScrollBars.Both;
            this.txt_query.KeyUp += Txt_query_KeyUp; ;

            Button btn_query = new Button();
            btn_query.Dock = DockStyle.Bottom;
            btn_query.Text = "Query";
            btn_query.Click += Btn_query_Click; ;

            gbx.Controls.Add(this.txt_query);
            gbx.Controls.Add(btn_query);
        }
        private void Txt_query_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                this.Btn_query_Click(sender, e);
        }
        private void Btn_query_Click(object sender, EventArgs e)
        {
            string query = this.txt_query.Text;

            if (this._dbHandler.SendQuery(query))
                MessageBox.Show(AppData.Lang("Query 완료"));
        }

        private void InitUI_Tables(GroupBox gbx)
        {
            Panel pnl_db_view = new Panel();
            pnl_db_view.Dock = DockStyle.Top;
            pnl_db_view.Height = 30;

            ComboBox cbo_db_table_list = new ComboBox();
            cbo_db_table_list.Location = new Point(3, 3);
            cbo_db_table_list.DataSource = this._dbHandler.TableNames;
            cbo_db_table_list.DataBindings.Add("SelectedValue", this._dbHandler, nameof(this._dbHandler.SelectedName), true, DataSourceUpdateMode.OnPropertyChanged);
            cbo_db_table_list.DropDownStyle = ComboBoxStyle.DropDownList;

            this.gvTable.Dock = DockStyle.Fill;
            this.gvTable.AllowUserToAddRows = false;
            this.gvTable.AllowUserToDeleteRows = false;
            this.gvTable.AllowUserToResizeRows = false;
            this.gvTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.gvTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.gvTable.RowHeadersVisible = false;
            this.gvTable.ReadOnly = true;
            this.gvTable.DataBindings.Add("DataSource", this._dbHandler, nameof(this._dbHandler.CurrentTable), true, DataSourceUpdateMode.OnPropertyChanged);

            pnl_db_view.Controls.Add(cbo_db_table_list);
            gbx.Controls.Add(this.gvTable);
            gbx.Controls.Add(pnl_db_view);
        }

        private void InitComponent()
        {
            this._dbHandler.ErrorMessage += _dbHandler_ErrorMessage;
        }
        private void _dbHandler_ErrorMessage(string obj)
        {
            MessageBox.Show(obj);
        }
    }
}
