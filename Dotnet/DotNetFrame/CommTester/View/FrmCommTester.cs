using DotNet.Comm;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using DotNetFrame.CommTester.Model;
using DotNetFrame.CommTester.ViewModel;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private UcSerial uc_config_port_serial;
        private UcEthernet uc_config_port_ethernet;

        private GroupBox gbx_config_comm = new GroupBox();
        private Label lbl_config_comm_protocol_type = new Label();
        private CheckBox chk_config_comm_protocol_errorcode_add = new CheckBox();
        private CheckBox chk_config_comm_repeat_enable = new CheckBox();
        private NumericUpDown num_config_comm_repeat_count = new NumericUpDown();
        private CheckBox chk_config_comm_repeat_infinity = new CheckBox();
        private Label lbl_config_comm_request_description = new Label();
        private Button btn_config_comm_request = new Button();


        private GroupBox gbx_log = new GroupBox();

        private GroupBox gbxPortSet = new GroupBox();
        private ComboBox cboPortType = new ComboBox();

        private Panel pnlPort_Serial = new Panel();
        private ComboBox cboPortList = new ComboBox();
        private Button btnConnect = new Button();
        private Label lblConnState = new Label();
        private GroupBox gbxBaudRate = new GroupBox();
        private GroupBox gbxParity = new GroupBox();
        private GroupBox gbxStopBits = new GroupBox();
        private GroupBox gbxDataBits = new GroupBox();

        private Panel pnlPort_Ethernet = new Panel();
        private Label lblEthernetIP = new Label();
        private TextBox txtEthernetIP = new TextBox();
        private Label lblPortNo = new Label();
        private NumericUpDown txtPortNo = new NumericUpDown();

        private GroupBox gbxProtocolSet = new GroupBox();
        private CheckBox chkAddErrChk = new CheckBox();

        private GroupBox gbxCommSet = new GroupBox();
        private CheckBox chkRewrite = new CheckBox();
        private Label lblRewrite = new Label();
        private NumericUpDown numRewrite = new NumericUpDown();
        private Label lblRewriteUnit = new Label();
        private CheckBox chkRewriteInfi = new CheckBox();
        private TextBox txtWrite = new TextBox();
        private Button btnSend = new Button();
        private Label lblWriteTooltip = new Label();

        private GroupBox gbxLog = new GroupBox();
        private DataGridView gvCommResult = new DataGridView();
        private DataGridView gvProtocolResult = new DataGridView();
        private DataGridView gvDataLog = new DataGridView();
        private DataGridView gvBuffer = new DataGridView();
        private TextBox txtLog = new TextBox();

        #endregion UI Controls

        private CommTesterHandler _handler = new CommTesterHandler();

        private int[] _baudrateList = new int[] { 9600, 19200, 38400, 57600, 115200, 921600 };
        private byte[] _databitsList = new byte[] { 7, 8 };
        private DataTable _dtDataLog = new DataTable();
        private DataTable _dtDataResult = new DataTable();
        private DataTable _dtProtocolResult = new DataTable();
        private DataTable _dtBuffer = new DataTable();
        private BackgroundWorker BgWorker = new BackgroundWorker();
        private const int BUFFER_COLUMN_COUNT = 15;
        private const int RESULT_COLUMN_COUNT = 512;

        public FrmCommTester()
        {
            InitializeComponent();
            this.InitText();
            this.InitUI();
            this.InitText_AfterUI();
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
        }

        private void InitText_AfterUI()
        {
            this.gvCommResult.Columns["TryCount"].HeaderText = AppData.Lang("commtester.log.commresult.trycount");
            this.gvCommResult.Columns["Success"].HeaderText = AppData.Lang("commtester.log.commresult.success");
            this.gvCommResult.Columns["None Receive"].HeaderText = AppData.Lang("commtester.log.commresult.none");
            this.gvCommResult.Columns["Receive Stop"].HeaderText = AppData.Lang("commtester.log.commresult.stop");
            this.gvCommResult.Columns["Receive Too Long"].HeaderText = AppData.Lang("commtester.log.commresult.infinity");

            this.gvProtocolResult.Columns["ErrChk"].HeaderText = AppData.Lang("commtester.log.protocolresult.errorcode");
            this.gvProtocolResult.Columns["ProtocolErr"].HeaderText = AppData.Lang("commtester.log.protocolresult.protocolerror");
        }

        private void InitUI()
        {
            Panel pnl_config = new Panel();
            pnl_config.Dock = DockStyle.Top;
            pnl_config.Padding = new Padding(3);
            pnl_config.Height = 218;

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
            cbo_config_port_list.DataSource = this._handler.Port_Type_List;
            cbo_config_port_list.DisplayMember = "DisplayText";
            cbo_config_port_list.ValueMember = "Value";
            cbo_config_port_list.DataBindings.Add("SelectedValue", this._handler, nameof(this._handler.Port_Type), true, DataSourceUpdateMode.OnPropertyChanged);
            cbo_config_port_list.DropDownStyle = ComboBoxStyle.DropDownList;

            this.btn_config_port_connection.Left = cbo_config_port_list.Right + 3;
            this.btn_config_port_connection.Top = cbo_config_port_list.Top - 1;
            this.btn_config_port_connection.Click += Btn_config_port_connection_Click;

            
            this.lbl_config_port_connection_status.Left = this.btn_config_port_connection.Right + 3;
            this.lbl_config_port_connection_status.Top = this.btn_config_port_connection.Top;
            this.lbl_config_port_connection_status.Height = this.btn_config_port_connection.Height;
            this.lbl_config_port_connection_status.Width = this.lbl_config_port_connection_status.Height;

            this.uc_config_port_serial = new UcSerial(this._handler.Serial);
            this.uc_config_port_serial.Dock = DockStyle.Fill;

            this.uc_config_port_ethernet = new UcEthernet(this._handler.Ethernet);
            this.uc_config_port_ethernet.Dock = DockStyle.Fill;

            pnl_config_port.Controls.Add(cbo_config_port_list);
            pnl_config_port.Controls.Add(this.btn_config_port_connection);
            pnl_config_port.Controls.Add(this.lbl_config_port_connection_status);
            gbx.Controls.Add(this.uc_config_port_serial);
            gbx.Controls.Add(this.uc_config_port_ethernet);
            gbx.Controls.Add(pnl_config_port);
        }

        private void Btn_config_port_connection_Click(object sender, EventArgs e)
        {
            this._handler.Port_IsUserOpen = !this._handler.Port_IsUserOpen;
        }

        private void InitUI_Comm(GroupBox gbx)
        {
            this.lbl_config_comm_protocol_type.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.lbl_config_comm_protocol_type.TextAlign = ContentAlignment.MiddleLeft;
            ComboBox cbo_config_comm_protocol_type = new ComboBox();
            cbo_config_comm_protocol_type.Left = this.lbl_config_comm_protocol_type.Right + 3;
            cbo_config_comm_protocol_type.Top = this.lbl_config_comm_protocol_type.Top;
            cbo_config_comm_protocol_type.DataSource = this._handler.Port_protocol_type_list;
            cbo_config_comm_protocol_type.DisplayMember = "DisplayText";
            cbo_config_comm_protocol_type.ValueMember = "Value";
            cbo_config_comm_protocol_type.DataBindings.Add("SelectedValue", this._handler, nameof(this._handler.Port_Protocol_Type), true, DataSourceUpdateMode.OnPropertyChanged);
            cbo_config_comm_protocol_type.DropDownStyle = ComboBoxStyle.DropDownList;

            this.chk_config_comm_protocol_errorcode_add.Left = this.lbl_config_comm_protocol_type.Left;
            this.chk_config_comm_protocol_errorcode_add.Top = this.lbl_config_comm_protocol_type.Bottom + 3;
            this.chk_config_comm_protocol_errorcode_add.Width = this.lbl_config_comm_protocol_type.Width + 20;
            this.chk_config_comm_protocol_errorcode_add.DataBindings.Add("Checked", this._handler, nameof(this._handler.Port_Protocol_ErrorCode_Add), true, DataSourceUpdateMode.OnPropertyChanged);
            this.chk_config_comm_protocol_errorcode_add.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_config_comm_protocol_errorcode_add.TextAlign = ContentAlignment.MiddleLeft;


            this.chk_config_comm_repeat_enable.Left = this.chk_config_comm_protocol_errorcode_add.Left;
            this.chk_config_comm_repeat_enable.Top = this.chk_config_comm_protocol_errorcode_add.Bottom + 3;
            this.chk_config_comm_repeat_enable.Width = this.chk_config_comm_protocol_errorcode_add.Width;
            this.chk_config_comm_repeat_enable.DataBindings.Add("Checked", this._handler, nameof(this._handler.Port_Comm_Repeat_Enable), true, DataSourceUpdateMode.OnPropertyChanged);
            this.chk_config_comm_repeat_enable.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_config_comm_repeat_enable.TextAlign = ContentAlignment.MiddleLeft;

            this.num_config_comm_repeat_count.Left = this.chk_config_comm_repeat_enable.Right + 3;
            this.num_config_comm_repeat_count.Top = this.chk_config_comm_repeat_enable.Top;
            this.num_config_comm_repeat_count.Width = 50;
            this.num_config_comm_repeat_count.DecimalPlaces = 0;
            this.num_config_comm_repeat_count.Increment = 1;
            this.num_config_comm_repeat_count.Minimum = 1;
            this.num_config_comm_repeat_count.Maximum = int.MaxValue;
            this.num_config_comm_repeat_count.TextAlign = HorizontalAlignment.Right;
            this.num_config_comm_repeat_count.DataBindings.Add("Value", this._handler, nameof(this._handler.Port_Comm_Repeat_Count), true, DataSourceUpdateMode.OnPropertyChanged);

            this.chk_config_comm_repeat_infinity.Left = this.chk_config_comm_repeat_enable.Left;
            this.chk_config_comm_repeat_infinity.Top = this.chk_config_comm_repeat_enable.Bottom + 3;
            this.chk_config_comm_repeat_infinity.Width = this.chk_config_comm_repeat_enable.Width;
            this.chk_config_comm_repeat_infinity.DataBindings.Add("Checked", this._handler, nameof(this._handler.Port_Comm_Repeat_Infinity), true, DataSourceUpdateMode.OnPropertyChanged);
            this.chk_config_comm_repeat_infinity.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_config_comm_repeat_infinity.TextAlign = ContentAlignment.MiddleLeft;


            this.btn_config_comm_request.Left = gbx.Right - (this.btn_config_port_connection.Width + 3);
            this.btn_config_comm_request.Top = gbx.Bottom - (this.btn_config_comm_request.Height + 4);
            this.btn_config_comm_request.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btn_config_comm_request.Click += Btn_config_comm_request_Click;

            TextBox txt_config_comm_request = new TextBox();
            txt_config_comm_request.Left = this.chk_config_comm_repeat_infinity.Left;
            txt_config_comm_request.Top = this.btn_config_comm_request.Top + 1;
            txt_config_comm_request.Width = this.btn_config_comm_request.Left - (txt_config_comm_request.Left + 3);
            txt_config_comm_request.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txt_config_comm_request.DataBindings.Add("Text", this._handler, nameof(this._handler.Port_Comm_Request), true, DataSourceUpdateMode.OnPropertyChanged);
            txt_config_comm_request.KeyDown += Txt_config_comm_request_KeyDown;

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
            this._handler.Data_Register();
        }
        private void Txt_config_comm_request_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
                this._handler.Data_Register();
        }


        private void InitUI_Log(GroupBox gbx)
        {
            Panel pnl_log_buffer = new Panel();
            pnl_log_buffer.Dock = DockStyle.Right;
            pnl_log_buffer.Width = 378;

            this.gvBuffer.Dock = DockStyle.Top;
            this.gvBuffer.AutoSize = false;
            this.gvBuffer.DataSource = this._dtBuffer;
            this.gvBuffer.Height = 140;
            this.gvBuffer.RowHeadersVisible = false;
            this.gvBuffer.AllowUserToAddRows = false;
            this.gvBuffer.AllowUserToResizeColumns = false;
            this.gvBuffer.AllowUserToResizeRows = false;

            for (int i = 0; i < BUFFER_COLUMN_COUNT; i++)
            {
                string fieldName = string.Format("col{0}", i);
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.DataPropertyName = fieldName;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderText = (i + 1).ToString();
                col.ReadOnly = true;
                col.Width = 25;

                this.gvBuffer.Columns.Add(col);

                this._dtBuffer.Columns.Add(new DataColumn(fieldName, typeof(string)) { DefaultValue = string.Empty });
            }

            this.txtLog.Dock = DockStyle.Fill;
            this.txtLog.AutoSize = false;
            this.txtLog.BorderStyle = BorderStyle.FixedSingle;
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = ScrollBars.Vertical;

            Panel pnl_log_result = new Panel();
            pnl_log_result.Dock = DockStyle.Top;
            pnl_log_result.Height = 47;
            this.InitUI_Log_Result(pnl_log_result);

            this.InitUI_Log_Grid();

            gbx.Controls.Add(this.gvDataLog);
            pnl_log_buffer.Controls.Add(this.gvBuffer);
            pnl_log_buffer.Controls.Add(this.txtLog);
            gbx.Controls.Add(pnl_log_buffer);
            gbx.Controls.Add(pnl_log_result);
        }

        private void InitUI_Log_Result(Panel pnl)
        {
            InitUI_Log_Result_Comm();
            InitUI_Log_Result_Protocol();

            pnl.Controls.Add(this.gvProtocolResult);
            pnl.Controls.Add(this.gvCommResult);
        }

        private void InitUI_Log_Result_Comm()
        {
            //Comm Result
            this.gvCommResult.Dock = DockStyle.Left;
            this.gvCommResult.Width = 270 + 3;
            this.gvCommResult.DataSource = this._dtDataResult;
            this.gvCommResult.RowHeadersVisible = false;
            this.gvCommResult.AllowUserToAddRows = false;
            this.gvCommResult.ReadOnly = true;

            DataGridViewTextBoxColumn colTry = new DataGridViewTextBoxColumn();
            colTry.Name = colTry.DataPropertyName = "TryCount";
            colTry.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTry.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTry.ReadOnly = true;
            colTry.Width = 50;

            DataGridViewTextBoxColumn colSuccess = new DataGridViewTextBoxColumn();
            colSuccess.Name = colSuccess.DataPropertyName = "Success";
            colSuccess.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSuccess.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSuccess.ReadOnly = true;
            colSuccess.Width = 50;

            DataGridViewTextBoxColumn colNoneReceive = new DataGridViewTextBoxColumn();
            colNoneReceive.Name = colNoneReceive.DataPropertyName = "None Receive";
            colNoneReceive.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colNoneReceive.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colNoneReceive.ReadOnly = true;
            colNoneReceive.Width = 50;

            DataGridViewTextBoxColumn colReceiveStop = new DataGridViewTextBoxColumn();
            colReceiveStop.Name = colReceiveStop.DataPropertyName = "Receive Stop";
            colReceiveStop.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveStop.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveStop.ReadOnly = true;
            colReceiveStop.Width = 60;

            DataGridViewTextBoxColumn colReceiveLong = new DataGridViewTextBoxColumn();
            colReceiveLong.Name = colReceiveLong.DataPropertyName = "Receive Too Long";
            colReceiveLong.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveLong.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveLong.ReadOnly = true;
            colReceiveLong.Width = 60;

            this.gvCommResult.Columns.Add(colTry);
            this.gvCommResult.Columns.Add(colSuccess);
            this.gvCommResult.Columns.Add(colNoneReceive);
            this.gvCommResult.Columns.Add(colReceiveStop);
            this.gvCommResult.Columns.Add(colReceiveLong);

            this._dtDataResult.Columns.Add(new DataColumn("TryCount", typeof(uint)) { DefaultValue = 0 });
            this._dtDataResult.Columns.Add(new DataColumn("Success", typeof(uint)) { DefaultValue = 0 });
            this._dtDataResult.Columns.Add(new DataColumn("None Receive", typeof(uint)) { DefaultValue = 0 });
            this._dtDataResult.Columns.Add(new DataColumn("Receive Stop", typeof(uint)) { DefaultValue = 0 });
            this._dtDataResult.Columns.Add(new DataColumn("Receive Too Long", typeof(uint)) { DefaultValue = 0 });
        }

        private void InitUI_Log_Result_Protocol()
        {
            this.gvProtocolResult.Dock = DockStyle.Left;
            this.gvProtocolResult.DataSource = this._dtProtocolResult;
            this.gvProtocolResult.RowHeadersVisible = false;
            this.gvProtocolResult.AllowUserToAddRows = false;
            this.gvProtocolResult.ReadOnly = true;

            DataGridViewTextBoxColumn colErrorCheck = new DataGridViewTextBoxColumn();
            colErrorCheck.Name = colErrorCheck.DataPropertyName = "ErrChk";
            colErrorCheck.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colErrorCheck.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colErrorCheck.ReadOnly = true;
            colErrorCheck.Width = 72;

            DataGridViewTextBoxColumn colProtocolErr = new DataGridViewTextBoxColumn();
            colProtocolErr.Name = colProtocolErr.DataPropertyName = "ProtocolErr";
            colProtocolErr.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colProtocolErr.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colProtocolErr.ReadOnly = true;
            colProtocolErr.Width = 80;

            this.gvProtocolResult.Columns.Add(colErrorCheck);
            this.gvProtocolResult.Columns.Add(colProtocolErr);

            int gvWidth = 0;
            foreach (DataGridViewColumn col in this.gvProtocolResult.Columns)
                gvWidth += col.Width;
            this.gvProtocolResult.Width = gvWidth + 3;

            this._dtProtocolResult.Columns.Add(new DataColumn("ErrChk", typeof(uint)) { DefaultValue = 0 });
            this._dtProtocolResult.Columns.Add(new DataColumn("ProtocolErr", typeof(uint)) { DefaultValue = 0 });
        }

        private void InitUI_Log_Grid()
        {
            this.gvDataLog.Dock = DockStyle.Fill;
            this.gvDataLog.AutoSize = false;
            this.gvDataLog.DataSource = this._dtDataLog;
            this.gvDataLog.RowHeadersVisible = false;
            this.gvDataLog.AllowUserToAddRows = false;
            this.gvDataLog.AllowUserToResizeColumns = false;
            this.gvDataLog.AllowUserToResizeRows = false;
            this.gvDataLog.RowsAdded += GvDataLog_RowsAdded;

            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn();
            colType.Name = "Type";
            colType.DataPropertyName = "Type";
            colType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colType.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colType.HeaderText = "Type";
            colType.ReadOnly = true;
            colType.Width = 40;
            colType.DisplayIndex = 0;
            this.gvDataLog.Columns.Add(colType);

            DataGridViewTextBoxColumn colTime = new DataGridViewTextBoxColumn();
            colTime.DataPropertyName = "Time";
            colTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTime.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTime.HeaderText = "Time";
            colTime.ReadOnly = true;
            colTime.Width = 130;
            colTime.DisplayIndex = 1;
            this.gvDataLog.Columns.Add(colTime);

            for (int i = 0; i < RESULT_COLUMN_COUNT; i++)
            {
                string colName = string.Format("Col{0}", i);

                this._dtDataLog.Columns.Add(new DataColumn(colName, typeof(string)));

                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.DataPropertyName = colName;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.Width = 25;
                col.HeaderText = (i + 1).ToString();
                col.ReadOnly = true;
                col.DisplayIndex = 2 + i;

                this.gvDataLog.Columns.Add(col);
            }

            this.gbxLog.Controls.Add(this.gvDataLog);
        }

        private void GvDataLog_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridView gv = sender as DataGridView;

            if (gv.Rows.Count > 0)
                gv.FirstDisplayedScrollingRowIndex = gv.Rows.Count - 1;
        }

        private string ByteToString(byte[] bytes)
        {
            string str = string.Empty;

            if(bytes != null && bytes.Length != 0)
            {
                foreach (var b in bytes)
                    str += string.Format(" {0:X2}", b);
            }

            return str;
        }

        private void AddRow_Frame(byte[] bytes, FrameStatus status)
        {
            DataRow dr = this._dtDataLog.NewRow();
            dr["Type"] = status == FrameStatus.Requesting ? "Req" : "Rcv";
            dr["Time"] = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");

            for (int i = 0; i < bytes.Length; i++)
            {
                if ((i != 0) && ((i % RESULT_COLUMN_COUNT) == 0))
                {
                    this._dtDataLog.Rows.Add(dr);
                    dr = this._dtDataLog.NewRow();
                }

                dr[string.Format("Col{0}", i)] = bytes[i].ToString("X2");
            }
            this._dtDataLog.Rows.Add(dr);

            if ((status == FrameStatus.Requesting || status == FrameStatus.Result_Comm_OK) == false)
                this.gvDataLog.Rows[this._dtDataLog.Rows.IndexOf(dr)].Cells["Type"].Style.BackColor = Color.Crimson;
        }

        private void InitComponent()
        {
            this._handler.PropertyChanged += _handler_PropertyChanged;
            this._handler.FrameUpated += _handler_FrameUpated;
            _handler_PropertyChanged(this._handler, new PropertyChangedEventArgs(nameof(this._handler.Port_Type)));
            _handler_PropertyChanged(this._handler, new PropertyChangedEventArgs(nameof(this._handler.Port_Comm_Repeat_Enable)));

            this.FormClosing += (s, e) => this._handler.Port_IsUserOpen = false;

            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += BgWorker_DoWork;
            this.BgWorker.RunWorkerAsync();
        }

        private void _handler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CommTesterHandler handler = sender as CommTesterHandler;

            if (e.PropertyName == nameof(handler.Port_Type))
            {
                if (handler.Port_Type == CommType.Serial)
                {
                    this.uc_config_port_serial.Show();
                    this.uc_config_port_ethernet.Hide();
                }
                else if (handler.Port_Type == CommType.Ethernet)
                {
                    this.uc_config_port_serial.Hide();
                    this.uc_config_port_ethernet.Show();
                }
            }
            else if (e.PropertyName == nameof(handler.Port_IsUserOpen))
            {
                this.uc_config_port_serial.Enabled = !handler.Port_IsUserOpen;
                this.uc_config_port_ethernet.Enabled = !handler.Port_IsUserOpen;

                if (handler.Port_IsUserOpen)
                    this.btn_config_port_connection.Text = AppData.Lang("commtester.portproperty.disconnect.text");
                else
                    this.btn_config_port_connection.Text = AppData.Lang("commtester.portproperty.connect.text");
            }
            else if (e.PropertyName == nameof(handler.Port_Comm_Repeat_Enable))
            {
                this.num_config_comm_repeat_count.Enabled = handler.Port_Comm_Repeat_Enable;
                this.chk_config_comm_repeat_infinity.Enabled = handler.Port_Comm_Repeat_Enable;
            }
            else if (e.PropertyName == nameof(handler.Port_Comm_Repeat_Infinity))
            {
                this.num_config_comm_repeat_count.Enabled = !handler.Port_Comm_Repeat_Infinity;
            }
        }

        private void _handler_FrameUpated(object sender, TestDataFrame e)
        {
            if (this.IsDisposed || this.Disposing) return;

            if (this.InvokeRequired)
                this.BeginInvoke((MethodInvoker)delegate { _handler_FrameUpated(sender, e); });
            else
            {
                switch (e.Status)
                {
                    case FrameStatus.Reading: FrameUpdate_Reading(e); break;
                    case FrameStatus.Requesting: FrameUpdate_Requesting(e); break;
                    case FrameStatus.Result_Comm_None:
                    case FrameStatus.Result_Comm_Stop:
                    case FrameStatus.Result_Comm_Long: this.FrameUpdate_Timeout(e); break;
                    case FrameStatus.Result_Comm_OK: 
                    case FrameStatus.Result_Protocol_ErrorCode:
                    case FrameStatus.Result_Protocol_NG: this.FrameUpdate_End(e); break;
                }
            }
        }

        private void FrameUpdate_Reading(TestDataFrame frame)
        {
            byte[] buffer = frame.Comm.Buffer;

            this._dtBuffer.Rows.Clear();

            DataRow dr = null;
            for (int i = 0; i < buffer.Length; i++)
            {
                string fieldName = string.Format("col{0}", i % BUFFER_COLUMN_COUNT);
                if (i % BUFFER_COLUMN_COUNT == 0)
                {
                    if (i != 0)
                        this._dtBuffer.Rows.Add(dr);

                    dr = this._dtBuffer.NewRow();
                }

                dr[fieldName] = buffer[i].ToString("X2");
            }

            if (dr != null)
                this._dtBuffer.Rows.Add(dr);
        }
        private void FrameUpdate_Requesting(TestDataFrame frame)
        {
            this.AddRow_Frame(frame.Comm.ReqBytes, frame.Status);

            //시도횟수
            this._dtDataResult.Rows[0]["TryCount"] = frame.Comm.TryCount_Cur;

            //TextLog Update
            this.txtLog.AppendText($"Req:{ByteToString(frame.Comm.ReqBytes)}\r\n");
        }
        private void FrameUpdate_Timeout(TestDataFrame frame)
        {
            string text = string.Empty;

            this.AddRow_Frame(frame.Comm.Buffer, frame.Status);

            if (frame.Status == FrameStatus.Result_Comm_None)
            {
                text = "Res Timeover - None: -\r\n";
                this._dtDataResult.Rows[0]["None Receive"] = frame.Result.Comm.None;
            }
            else if (frame.Status == FrameStatus.Result_Comm_Stop)
            {
                text = $"Res Timeover - Stop:{ByteToString(frame.Comm.Buffer)}\r\n";
                this._dtDataResult.Rows[0]["Receive Stop"] = frame.Result.Comm.Stop;
            }
            else if (frame.Status == FrameStatus.Result_Comm_Long)
            {
                text = $"Res Timeover - Long:{ByteToString(frame.Comm.Buffer)}\r\n";
                this._dtDataResult.Rows[0]["Receive Too Long"] = frame.Result.Comm.Long;
            }

            //TextLog Update
            this.txtLog.AppendText(text);
        }
        private void FrameUpdate_End(TestDataFrame frame)
        {
            string text = string.Empty;

            this.AddRow_Frame(frame.Comm.RcvBytes, frame.Status);
            if (frame.Status == FrameStatus.Result_Comm_OK)
            {
                text = $"Res:{ByteToString(frame.Comm.RcvBytes)}\r\n\r\n";
                this._dtDataResult.Rows[0]["Success"] = frame.Result.Comm.OK;
            }
            else if (frame.Status == FrameStatus.Result_Protocol_ErrorCode)
            {
                text = $"Res Error - ErrorCode:{ByteToString(frame.Comm.RcvBytes)}\r\n";
                this._dtProtocolResult.Rows[0]["ErrChk"] = frame.Result.Protocol.ErrorCode;
            }
            else if (frame.Status == FrameStatus.Result_Protocol_NG)
            {
                text = $"Res Error - Protocol:{ByteToString(frame.Comm.RcvBytes)}\r\n";
                this._dtDataResult.Rows[0]["ProtocolErr"] = frame.Result.Protocol.NG;
            }

            //TextLog Update
            this.txtLog.AppendText(text);
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this.BgWorker.CancellationPending)
                        break;
                    else
                    {
                        this.UpdateUI();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format(
                        "FrmCommTester.cs - BgWorker_Dowork()\r\n" +
                        "{0}\r\n\r\n" +
                        "{1}",
                        ex.Message, ex.StackTrace));
                }

                System.Threading.Thread.Sleep(5);
            }
        }
        private void UpdateUI()
        {
            if (this.IsDisposed || this.Disposing) return;

            if (this.InvokeRequired)
                this.BeginInvoke((MethodInvoker)delegate { UpdateUI(); });
            else
            {
                //연결 상태
                if (this._handler.Port_IsPortOpen)
                    this.lbl_config_port_connection_status.BackColor = Color.Green;
                else
                    this.lbl_config_port_connection_status.BackColor = Color.Red;
            }
        }
    }
}
