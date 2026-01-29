using DotNet.Comm;
using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using DotNetFrame.CommTester.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DotNetFrame.CommTester.View
{
    public partial class FrmCommTester : Form
    {
        public delegate void UIUpdateHandler(string cmd);
        public delegate void BytesLogHandler(string type, params byte[] data);

        #region UI Controls

        private GroupBox gbx_config_port = new GroupBox();
        private Button btn_config_port_connection = new Button();
        private Label lbl_config_port_connection_status = new Label();
        private Panel pnl_config_port_porttype = new Panel();
        /// <summary>
        /// PortType별 설정 Control Cash(보관함)
        /// </summary>
        /// <remarks>
        /// 지속적으로 new로 생성하면 부하가 걸리기 때문에
        /// 초기 1회만 생성하고 그 외에는 불러오도록 보관함
        /// </remarks>
        private Dictionary<PortType, QYUserControl> cash_config_port_porttype_view = new Dictionary<PortType, QYUserControl>();

        private GroupBox gbx_config_comm = new GroupBox();
        private Label lbl_config_comm_protocol_type = new Label();
        private CheckBox chk_config_comm_protocol_errorcode_add = new CheckBox();
        private CheckBox chk_config_comm_repeat_enable = new CheckBox();
        private NumericUpDown num_config_comm_repeat_count = new NumericUpDown();
        private CheckBox chk_config_comm_repeat_infinity = new CheckBox();
        private Label lbl_config_comm_request_description = new Label();
        private TextBox txt_config_comm_request = new TextBox();
        private Button btn_config_comm_request = new Button();


        private GroupBox gbx_log = new GroupBox();
        private Label lbl_log_count_comm_trycount_title = new Label();
        private Label lbl_log_count_comm_success_title = new Label();
        private Label lbl_log_count_comm_none_title = new Label();
        private Label lbl_log_count_comm_stop_title = new Label();
        private Label lbl_log_count_comm_long_title = new Label();
        private Label lbl_log_count_protocol_errorcode_title = new Label();
        private Label lbl_log_count_protocol_frame_title = new Label();
        private Label lbl_log_count_comm_trycount_value = new Label();
        private Label lbl_log_count_comm_success_value = new Label();
        private Label lbl_log_count_comm_none_value = new Label();
        private Label lbl_log_count_comm_stop_value = new Label();
        private Label lbl_log_count_comm_long_value = new Label();
        private Label lbl_log_count_protocol_errorcode_value = new Label();
        private Label lbl_log_count_protocol_frame_value = new Label();

        private DataGridView gvDataLog = new DataGridView();
        private TextBox txtLog = new TextBox();

        #endregion UI Controls

        private CommTesterVM _viewmodel = new CommTesterVM();

        private DataTable _dtDataLog = new DataTable();
        private BackgroundWorker BgWorker = new BackgroundWorker();
        private const int RESULT_COLUMN_COUNT = 512;

        public FrmCommTester()
        {
            InitializeComponent();
            this.InitText();
            this.InitUI();
            this.InitComponent();

        }
        private void InitText()
        {
            this.gbx_config_port.Text = AppData.Lang("commtester.portproperty.title");
            this.btn_config_port_connection.Text = AppData.Lang("commtester.portproperty.connect.text");

            this.gbx_config_comm.Text = AppData.Lang("commtester.commproperty.title");
            this.lbl_config_comm_protocol_type.Text = AppData.Lang("commtester.protocolproperty.title");
            this.chk_config_comm_protocol_errorcode_add.Text = AppData.Lang("commtester.protocolproperty.errorcode.create.title");
            this.chk_config_comm_repeat_enable.Text = AppData.Lang("commtester.commproperty.repeat.enable.title");
            this.chk_config_comm_repeat_infinity.Text = AppData.Lang("commtester.commproperty.repeat.infinity.title");
            this.lbl_config_comm_request_description.Text = AppData.Lang("commtester.commproperty.tooltip.text");
            this.btn_config_comm_request.Text = AppData.Lang("commtester.commproperty.send.text");

            this.gbx_log.Text = AppData.Lang("commtester.log.title");
            this.lbl_log_count_comm_trycount_title.Text = AppData.Lang("commtester.log.commresult.trycount");
            this.lbl_log_count_comm_success_title.Text = AppData.Lang("commtester.log.commresult.success");
            this.lbl_log_count_comm_none_title.Text = AppData.Lang("commtester.log.commresult.none");
            this.lbl_log_count_comm_stop_title.Text = AppData.Lang("commtester.log.commresult.stop");
            this.lbl_log_count_comm_long_title.Text = AppData.Lang("commtester.log.commresult.infinity");
            this.lbl_log_count_protocol_errorcode_title.Text = AppData.Lang("commtester.log.protocolresult.errorcode");
            this.lbl_log_count_protocol_frame_title.Text = AppData.Lang("commtester.log.protocolresult.protocolerror");
        }

        private void InitUI()
        {
            Panel pnl_config = new Panel();
            pnl_config.Dock = DockStyle.Top;
            pnl_config.Padding = new Padding(3);
            pnl_config.Height = 230;

            this.gbx_config_port.Dock = DockStyle.Left;
            this.gbx_config_port.Width = 319;
            this.InitUI_Port(this.gbx_config_port);

            this.gbx_config_comm.Dock = DockStyle.Fill;
            this.InitUI_Comm(this.gbx_config_comm);

            this.gbx_log.Dock = DockStyle.Fill;
            this.InitUI_Log(this.gbx_log);

            pnl_config.Controls.Add(this.gbx_config_comm);
            pnl_config.Controls.Add(this.gbx_config_port);
            this.Controls.Add(this.gbx_log);
            this.Controls.Add(pnl_config);
        }

        private void InitUI_Port(GroupBox gbx)
        {
            Panel pnl_config_port = new Panel();
            pnl_config_port.Dock = DockStyle.Top;
            pnl_config_port.Height = 26;

            ComboBox cbo_config_port_list = new ComboBox();
            cbo_config_port_list.Location = new Point(3, 3);
            cbo_config_port_list.DataSource = this._viewmodel.PortTypeList;
            cbo_config_port_list.ValueMember = nameof(QYItem.Value);
            cbo_config_port_list.DisplayMember = nameof(QYItem.DisplayText);
            cbo_config_port_list.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo_config_port_list.DataBindings.Add("SelectedValue", this._viewmodel, nameof(CommTesterVM.PortType), true, DataSourceUpdateMode.OnPropertyChanged);

            this.btn_config_port_connection.Left = cbo_config_port_list.Right + 3;
            this.btn_config_port_connection.Top = cbo_config_port_list.Top - 1;
            this.btn_config_port_connection.Click += Btn_config_port_connection_Click;

            
            this.lbl_config_port_connection_status.Left = this.btn_config_port_connection.Right + 3;
            this.lbl_config_port_connection_status.Top = this.btn_config_port_connection.Top;
            this.lbl_config_port_connection_status.Height = this.btn_config_port_connection.Height;
            this.lbl_config_port_connection_status.Width = this.lbl_config_port_connection_status.Height;

            this.pnl_config_port_porttype.Dock = DockStyle.Fill;

            pnl_config_port.Controls.Add(cbo_config_port_list);
            pnl_config_port.Controls.Add(this.btn_config_port_connection);
            pnl_config_port.Controls.Add(this.lbl_config_port_connection_status);
            gbx.Controls.Add(this.pnl_config_port_porttype);
            gbx.Controls.Add(pnl_config_port);
        }

        private void Btn_config_port_connection_Click(object sender, EventArgs e)
        {
            this._viewmodel.Connection();
        }

        private void InitUI_Comm(GroupBox gbx)
        {
            this.lbl_config_comm_protocol_type.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.lbl_config_comm_protocol_type.TextAlign = ContentAlignment.MiddleLeft;
            ComboBox cbo_config_comm_protocol_type = new ComboBox();
            cbo_config_comm_protocol_type.Left = this.lbl_config_comm_protocol_type.Right + 3;
            cbo_config_comm_protocol_type.Top = this.lbl_config_comm_protocol_type.Top;
            cbo_config_comm_protocol_type.DataSource = this._viewmodel.ProtocolList;
            cbo_config_comm_protocol_type.ValueMember = nameof(QYItem.Value);
            cbo_config_comm_protocol_type.DisplayMember = nameof(QYItem.DisplayText);
            cbo_config_comm_protocol_type.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo_config_comm_protocol_type.DataBindings.Add("SelectedValue", this._viewmodel, nameof(CommTesterVM.ProtocolType), true, DataSourceUpdateMode.OnPropertyChanged);
            cbo_config_comm_protocol_type.SelectedValue = ProtocolType.None;

            this.chk_config_comm_protocol_errorcode_add.Left = this.lbl_config_comm_protocol_type.Left;
            this.chk_config_comm_protocol_errorcode_add.Top = this.lbl_config_comm_protocol_type.Bottom + 3;
            this.chk_config_comm_protocol_errorcode_add.Width = this.lbl_config_comm_protocol_type.Width + 20;
            this.chk_config_comm_protocol_errorcode_add.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_config_comm_protocol_errorcode_add.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_config_comm_protocol_errorcode_add.Enabled = false;
            this.chk_config_comm_protocol_errorcode_add.DataBindings.Add("Checked", this._viewmodel, nameof(CommTesterVM.ErrCodeEnable), true, DataSourceUpdateMode.OnPropertyChanged);


            this.chk_config_comm_repeat_enable.Left = this.chk_config_comm_protocol_errorcode_add.Left;
            this.chk_config_comm_repeat_enable.Top = this.chk_config_comm_protocol_errorcode_add.Bottom + 3;
            this.chk_config_comm_repeat_enable.Width = this.chk_config_comm_protocol_errorcode_add.Width;
            this.chk_config_comm_repeat_enable.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_config_comm_repeat_enable.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_config_comm_repeat_enable.DataBindings.Add("Checked", this._viewmodel, nameof(CommTesterVM.RepeatEnable), true, DataSourceUpdateMode.OnPropertyChanged);

            this.num_config_comm_repeat_count.Left = this.chk_config_comm_repeat_enable.Right + 3;
            this.num_config_comm_repeat_count.Top = this.chk_config_comm_repeat_enable.Top;
            this.num_config_comm_repeat_count.Width = 50;
            this.num_config_comm_repeat_count.DecimalPlaces = 0;
            this.num_config_comm_repeat_count.Increment = 1;
            this.num_config_comm_repeat_count.Minimum = 1;
            this.num_config_comm_repeat_count.Maximum = int.MaxValue;
            this.num_config_comm_repeat_count.TextAlign = HorizontalAlignment.Right;
            this.num_config_comm_repeat_count.Enabled = false;
            this.num_config_comm_repeat_count.DataBindings.Add("Value", this._viewmodel, nameof(CommTesterVM.RepeatCount), true, DataSourceUpdateMode.OnValidation);

            this.chk_config_comm_repeat_infinity.Left = this.chk_config_comm_repeat_enable.Left;
            this.chk_config_comm_repeat_infinity.Top = this.chk_config_comm_repeat_enable.Bottom + 3;
            this.chk_config_comm_repeat_infinity.Width = this.chk_config_comm_repeat_enable.Width;
            this.chk_config_comm_repeat_infinity.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_config_comm_repeat_infinity.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_config_comm_repeat_infinity.Enabled = false;
            this.chk_config_comm_repeat_infinity.DataBindings.Add("Checked", this._viewmodel,nameof(CommTesterVM.RepeatInfinity), true, DataSourceUpdateMode.OnPropertyChanged);


            this.btn_config_comm_request.Left = gbx.Right - (this.btn_config_port_connection.Width + 3);
            this.btn_config_comm_request.Top = gbx.Bottom - (this.btn_config_comm_request.Height + 4);
            this.btn_config_comm_request.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btn_config_comm_request.Click += Btn_config_comm_request_Click;

            this.txt_config_comm_request.Left = this.chk_config_comm_repeat_infinity.Left;
            this.txt_config_comm_request.Top = this.btn_config_comm_request.Top + 1;
            this.txt_config_comm_request.Width = this.btn_config_comm_request.Left - (txt_config_comm_request.Left + 3);
            this.txt_config_comm_request.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.txt_config_comm_request.KeyDown += Txt_config_comm_request_KeyDown;

            this.lbl_config_comm_request_description.Left = txt_config_comm_request.Left;
            this.lbl_config_comm_request_description.Top = txt_config_comm_request.Top - (this.lbl_config_comm_request_description.Height);
            this.lbl_config_comm_request_description.Width = 400;
            this.lbl_config_comm_request_description.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.lbl_config_comm_request_description.TextAlign = ContentAlignment.MiddleLeft;


            gbx.Controls.Add(this.lbl_config_comm_protocol_type);
            gbx.Controls.Add(cbo_config_comm_protocol_type);
            gbx.Controls.Add(this.chk_config_comm_protocol_errorcode_add);
            gbx.Controls.Add(this.chk_config_comm_repeat_enable);
            gbx.Controls.Add(this.num_config_comm_repeat_count);
            gbx.Controls.Add(this.chk_config_comm_repeat_infinity);
            gbx.Controls.Add(this.lbl_config_comm_request_description);
            gbx.Controls.Add(this.btn_config_comm_request);
            gbx.Controls.Add(txt_config_comm_request);
        }

        private void Btn_config_comm_request_Click(object sender, EventArgs e)
        {
            TextBox txt = this.txt_config_comm_request;

            this.Runstop(txt.Text);
        }
        private void Txt_config_comm_request_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;

            if (e.KeyCode == Keys.Enter)
                this.Runstop(txt.Text);
        }

        private void Runstop(string txt)
        {
            this._dtDataLog.Rows.Clear();

            this._viewmodel.RunStop(txt);
        }

        private void InitUI_Log(GroupBox gbx)
        {
            TableLayoutPanel layout_log = new TableLayoutPanel();
            layout_log.Dock = DockStyle.Fill;
            layout_log.RowCount = 3;
            layout_log.RowStyles.Add(new RowStyle() { SizeType = SizeType.Absolute, Height = 55 });
            layout_log.RowStyles.Add(new RowStyle() { SizeType = SizeType.Percent, Height = 100 });
            layout_log.ColumnCount = 8;
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            layout_log.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 8 });
            gbx.Controls.Add(layout_log);

            TableLayoutPanel layout_log_count_comm = new TableLayoutPanel();
            layout_log_count_comm.Dock = DockStyle.Fill;
            layout_log_count_comm.RowCount = 2;
            layout_log_count_comm.RowStyles.Add(new RowStyle() { SizeType = SizeType.Absolute, Height = 23 });
            layout_log_count_comm.RowStyles.Add(new RowStyle() { SizeType = SizeType.Absolute, Height = 23 });
            layout_log_count_comm.ColumnCount = 5;
            layout_log_count_comm.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 5 });
            layout_log_count_comm.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 5 });
            layout_log_count_comm.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 5 });
            layout_log_count_comm.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 5 });
            layout_log_count_comm.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 5 });
            layout_log_count_comm.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            layout_log.Controls.Add(layout_log_count_comm, 0, 0);
            layout_log.SetColumnSpan(layout_log_count_comm, 3);

            this.lbl_log_count_comm_trycount_title.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_trycount_title.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_trycount_title.AutoSize = false;
            this.lbl_log_count_comm_success_title.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_success_title.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_success_title.AutoSize = false;
            this.lbl_log_count_comm_none_title.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_none_title.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_none_title.AutoSize = false;
            this.lbl_log_count_comm_stop_title.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_stop_title.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_stop_title.AutoSize = false;
            this.lbl_log_count_comm_long_title.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_long_title.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_long_title.AutoSize = false;
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_trycount_title, 0, 0);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_success_title, 1, 0);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_none_title, 2, 0);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_stop_title, 3, 0);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_long_title, 4, 0);

            this.lbl_log_count_comm_trycount_value.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_trycount_value.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_trycount_value.AutoSize = false;
            this.lbl_log_count_comm_success_value.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_success_value.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_success_value.AutoSize = false;
            this.lbl_log_count_comm_none_value.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_none_value.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_none_value.AutoSize = false;
            this.lbl_log_count_comm_stop_value.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_stop_value.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_stop_value.AutoSize = false;
            this.lbl_log_count_comm_long_value.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_comm_long_value.Dock = DockStyle.Fill;
            this.lbl_log_count_comm_long_value.AutoSize = false;

            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_trycount_value, 0, 1);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_success_value, 1, 1);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_none_value, 2, 1);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_stop_value, 3, 1);
            layout_log_count_comm.Controls.Add(this.lbl_log_count_comm_long_value, 4, 1);




            TableLayoutPanel layout_log_count_protocol = new TableLayoutPanel();
            layout_log_count_protocol.Dock = DockStyle.Fill;
            layout_log_count_protocol.RowCount = 2;
            layout_log_count_protocol.RowStyles.Add(new RowStyle() { SizeType = SizeType.Absolute, Height = 23 });
            layout_log_count_protocol.RowStyles.Add(new RowStyle() { SizeType = SizeType.Absolute, Height = 23 });
            layout_log_count_protocol.ColumnCount = 2;
            layout_log_count_protocol.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 2 });
            layout_log_count_protocol.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent, Width = 100 / 2 });
            layout_log_count_protocol.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            layout_log.Controls.Add(layout_log_count_protocol, 3, 0);
            layout_log.SetColumnSpan(layout_log_count_protocol, 2);

            this.lbl_log_count_protocol_errorcode_title.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_protocol_errorcode_title.Dock = DockStyle.Fill;
            this.lbl_log_count_protocol_errorcode_title.AutoSize = false;
            this.lbl_log_count_protocol_frame_title.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_protocol_frame_title.Dock = DockStyle.Fill;
            this.lbl_log_count_protocol_frame_title.AutoSize = false;

            layout_log_count_protocol.Controls.Add(this.lbl_log_count_protocol_errorcode_title, 0, 0);
            layout_log_count_protocol.Controls.Add(this.lbl_log_count_protocol_frame_title, 1, 0);


            this.lbl_log_count_protocol_errorcode_value.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_protocol_errorcode_value.Dock = DockStyle.Fill;
            this.lbl_log_count_protocol_errorcode_value.AutoSize = false;
            this.lbl_log_count_protocol_frame_value.TextAlign = ContentAlignment.MiddleCenter;
            this.lbl_log_count_protocol_frame_value.Dock = DockStyle.Fill;
            this.lbl_log_count_protocol_frame_value.AutoSize = false;

            layout_log_count_protocol.Controls.Add(this.lbl_log_count_protocol_errorcode_value, 0, 1);
            layout_log_count_protocol.Controls.Add(this.lbl_log_count_protocol_frame_value, 1, 1);


            this.InitUI_Log_Grid(this.gvDataLog);
            layout_log.Controls.Add(this.gvDataLog, 0, 1);
            layout_log.SetColumnSpan(this.gvDataLog, 5);

            this.txtLog.Dock = DockStyle.Fill;
            this.txtLog.AutoSize = false;
            this.txtLog.BorderStyle = BorderStyle.FixedSingle;
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = ScrollBars.Vertical;

            layout_log.Controls.Add(this.txtLog, 5, 0);
            layout_log.SetColumnSpan(this.txtLog, 3);
            layout_log.SetRowSpan(this.txtLog, 2);
        }

        private void InitUI_Log_Grid(DataGridView gv)
        {
            this._dtDataLog = new DataTable();
            this._dtDataLog.Columns.Add(new DataColumn("Time", typeof(DateTime)));
            this._dtDataLog.Columns.Add(new DataColumn("Type", typeof(string)));
            this._dtDataLog.Columns.Add(new DataColumn("Color", typeof(Color)) { DefaultValue = Color.Transparent});

            gv.Dock = DockStyle.Fill;
            gv.AutoSize = false;
            gv.AutoGenerateColumns = false;
            gv.RowHeadersVisible = false;
            gv.AllowUserToAddRows = false;
            gv.AllowUserToResizeColumns = false;
            gv.AllowUserToResizeRows = false;
            gv.DataSource = this._dtDataLog;
            gv.RowsAdded += GvDataLog_RowsAdded;

            DataGridViewTextBoxColumn colTime = new DataGridViewTextBoxColumn();
            colTime.DataPropertyName = "Time";
            colTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTime.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTime.HeaderText = "Time";
            colTime.ReadOnly = true;
            colTime.Width = 130;
            colTime.DisplayIndex = 0;
            gv.Columns.Add(colTime);

            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn();
            colType.Name = "Type";
            colType.DataPropertyName = "Type";
            colType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colType.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colType.HeaderText = "Type";
            colType.ReadOnly = true;
            colType.Width = 40;
            colType.DisplayIndex = 1;
            gv.Columns.Add(colType);

            for (int i = 0; i < RESULT_COLUMN_COUNT; i++)
            {
                string colName = $"{i}";

                this._dtDataLog.Columns.Add(new DataColumn(colName, typeof(string)));

                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.DataPropertyName = colName;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.Width = 30;
                col.HeaderText = $"{i + 1}";
                col.ReadOnly = true;
                col.DisplayIndex = 2 + i;

                gv.Columns.Add(col);
            }

            gv.CellFormatting += Gv_CellFormatting;
        }

        private void Gv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView gv = sender as DataGridView;

            if (gv.Columns[e.ColumnIndex].Name == "Type")
            {
                Color color = (Color)this._dtDataLog.Rows[e.RowIndex]["Color"];

                if(color != Color.Transparent)
                    e.CellStyle.BackColor = color;
            }
        }

        private void GvDataLog_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridView gv = sender as DataGridView;

            if (gv.Rows.Count > 0)
                gv.FirstDisplayedScrollingRowIndex = gv.Rows.Count - 1;
        }

        private void InitComponent()
        {
            this.Load += (s, e) =>
            {
                this._viewmodel.PortType = PortType.Ethernet;
                this._viewmodel.PortType = PortType.Serial;
            };
            this.FormClosing += (s, e) => this._viewmodel.Disconnect();

            this._viewmodel.PropertyChanged += _viewmodel_PropertyChanged;
            this._viewmodel.GetResult += _viewmodel_GetResult;

            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += BgWorker_DoWork;
            this.BgWorker.RunWorkerAsync();
        }

        private void _viewmodel_GetResult(CommResult obj)
        {
            this.Invoke(new Action(() => {
                this.AddResult(obj);
                this.WriteResult(obj);
            }));
        }

        private void AddResult(CommResult rst)
        {
            DataRow dr = this._dtDataLog.NewRow();

            dr["Time"] = DateTime.Now;
            dr["Type"] = rst.Type == "Write" ? "Req" : "Rcv";
            dr["Color"] = rst.Type == "Write" || rst.Type == "Read" ? Color.Transparent : Color.LightCoral;

            if (rst.Data != null)
            {
                for (int i = 0; i < rst.Data.Length; i++)
                    dr[$"{i}"] = $"{rst.Data[i]:X2}";
            }

            this._dtDataLog.Rows.Add(dr);
        }
        private void WriteResult(CommResult rst)
        {
            string log = string.Empty;

            log = $"{rst.Type}: {ByteToString(rst.Data)}";

            log += Environment.NewLine;
            log += Environment.NewLine;
            this.txtLog.AppendText(log);
        }

        private string ByteToString(byte[] bytes)
        {
            string str = string.Empty;
            if (bytes == null) return str;

            for (int i = 0; i < bytes.Length; i++)
                str += $"{bytes[i]:X2} ";

            return str;
        }

        private void _viewmodel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CommTesterVM.IsAppOpen))
            {
                if (this._viewmodel.IsAppOpen)
                    this.btn_config_port_connection.Text = AppData.Lang("commtester.portproperty.disconnect.text");
                else
                    this.btn_config_port_connection.Text = AppData.Lang("commtester.portproperty.connect.text");
            }
            else if (e.PropertyName == nameof(CommTesterVM.IsSending))
            {
                if (this._viewmodel.IsSending)
                    this.btn_config_comm_request.Text = AppData.Lang("commtester.commproperty.stop.text");
                else
                    this.btn_config_comm_request.Text = AppData.Lang("commtester.commproperty.send.text");
            }
            else if (e.PropertyName == nameof(CommTesterVM.PortType))
            {
                this.pnl_config_port_porttype.Controls.Clear();

                QYUserControl ctrl = null;
                if (this.cash_config_port_porttype_view.ContainsKey(this._viewmodel.PortType))
                    ctrl = this.cash_config_port_porttype_view[this._viewmodel.PortType];
                else
                {
                    if (this._viewmodel.PortType == DotNet.Comm.PortType.Serial)
                        ctrl = new UcSerial();
                    else if (this._viewmodel.PortType == DotNet.Comm.PortType.Ethernet)
                        ctrl = new UcEthernet();

                    ctrl.Dock = DockStyle.Fill;
                }


                if (ctrl != null)
                {
                    ctrl.BindViewModel(this._viewmodel.OSPort_VM);

                    this.pnl_config_port_porttype.Controls.Add(ctrl);
                }
            }
            else if (e.PropertyName == nameof(CommTesterVM.ProtocolType))
            {
                if (this._viewmodel.ProtocolType == ProtocolType.None)
                    this.chk_config_comm_protocol_errorcode_add.Enabled = false;
                else
                    this.chk_config_comm_protocol_errorcode_add.Enabled = true;
            }
            else if (e.PropertyName == nameof(CommTesterVM.RepeatEnable)
                || e.PropertyName == nameof(CommTesterVM.RepeatInfinity))
            {
                if (this._viewmodel.RepeatEnable)
                {
                    if (this._viewmodel.RepeatInfinity)
                        this.num_config_comm_repeat_count.Enabled = false;
                    else
                        this.num_config_comm_repeat_count.Enabled = true;

                    this.chk_config_comm_repeat_infinity.Enabled = true;
                }
                else
                    this.chk_config_comm_repeat_infinity.Enabled = false;
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this.BgWorker.CancellationPending) break;

                    this.Invoke(new Action(() =>
                    {
                        //UI 정기적 업데이트
                        if (this._viewmodel.IsOSPortOpen)
                            this.lbl_config_port_connection_status.BackColor = Color.Green;
                        else
                            this.lbl_config_port_connection_status.BackColor = Color.Red;

                        if(this._viewmodel.IsSending)
                            this.btn_config_comm_request.Text = AppData.Lang("commtester.commproperty.stop.text");
                        else
                            this.btn_config_comm_request.Text = AppData.Lang("commtester.commproperty.send.text");

                        this.lbl_log_count_comm_trycount_value.Text = this._viewmodel.SendingCount.ToString("#,0");
                        this.lbl_log_count_comm_success_value.Text = this._viewmodel.SucessCount.ToString("#,0");
                        this.lbl_log_count_comm_none_value.Text = this._viewmodel.Error_Timeout_None_Count.ToString("#,0");
                        this.lbl_log_count_comm_stop_value.Text = this._viewmodel.Error_Timeout_Long_Count.ToString("#,0");
                        this.lbl_log_count_comm_long_value.Text = this._viewmodel.Error_Timeout_Stop_Count.ToString("#,0");
                        this.lbl_log_count_protocol_errorcode_value.Text = this._viewmodel.Error_Protocol_ErrorCode_Count.ToString("#,0");
                        this.lbl_log_count_protocol_frame_value.Text = this._viewmodel.Error_Protocol_Frame_Count.ToString("#,0");
                    }));
                }
                catch
                {

                }
                finally
                {
                    System.Threading.Thread.Sleep(1000 / 60);
                }
            }
        }
    }
}
