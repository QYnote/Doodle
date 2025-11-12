using DotNet.Comm;
using DotNet.Comm.ClientPorts;
using DotNet.Utils.Controls;
using DotNet.Utils.Controls.Utils;
using DotNetFrame.CustomComm.HYNux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Frm
{

    public partial class FrmCommTester : Form
    {
        public delegate void UIUpdateHandler(string cmd);
        public delegate void BytesLogHandler(string type, params byte[] data);

        #region UI Controls

        private SplitContainer pnlSplit = new SplitContainer();

        private Panel pnlPortSet_Common = new Panel();
        private GroupBox gbxPortSet = new GroupBox();
        private ComboBox cboPortType = new ComboBox();
        private ComboBox cboPortList = new ComboBox();
        private TextBox txtEthernetIP = new TextBox();
        private NumericUpDown txtPortNo = new NumericUpDown();
        private Button btnConnect = new Button();
        private Label lblConnState = new Label();
        private GroupBox gbxBaudRate = new GroupBox();
        private GroupBox gbxParity = new GroupBox();
        private GroupBox gbxStopBits = new GroupBox();
        private GroupBox gbxDataBits = new GroupBox();

        private GroupBox gbxProtocolSet = new GroupBox();
        private ComboBox cboProtocolList = new ComboBox();
        private CheckBox chkAddErrChk = new CheckBox();

        private GroupBox gbxCommSet = new GroupBox();
        private CheckBox chkRewrite = new CheckBox();
        private NumericUpDown numRewrite = new NumericUpDown();
        private CheckBox chkRewriteInfi = new CheckBox();
        private TextBox txtWrite = new TextBox();
        private Button btnSend = new Button();
        private Label lblWriteTooltip = new Label();

        private GroupBox gbxLog = new GroupBox();
        private Panel pnlResult = new Panel();
        private DataGridView gvCommResult = new DataGridView();
        private DataGridView gvProtocolResult = new DataGridView();
        private Panel pnlLog = new Panel();
        private DataGridView gvDataLog = new DataGridView();
        private DataGridView gvBuffer = new DataGridView();
        private TextBox txtLog = new TextBox();

        #endregion UI Controls

        private int[] _baudrateList = new int[] { 9600, 19200, 38400, 57600, 115200, 921600 };
        private byte[] _databitsList = new byte[] { 7, 8 };
        private DataTable _dtDataLog = new DataTable();
        private DataTable _dtDataResult = new DataTable();
        private DataTable _dtProtocolResult = new DataTable();
        private DataTable _dtBuffer = new DataTable();
        private HYCommTesterPort _port = new HYCommTesterPort();
        private BackgroundWorker BgWorker = new BackgroundWorker();
        private int _bufferColCount = 10;
        private int rstColCount = 512;

        private QYSerialPort Serial
        {
            get
            {
                if (this._port != null
                    && this._port.ComPort is QYSerialPort)
                    return this._port.ComPort as QYSerialPort;
                else
                    return null;
            }
        }
        private QYEthernet Ethernet
        {
            get
            {
                if (this._port != null
                    && this._port.ComPort is QYEthernet)
                    return this._port.ComPort as QYEthernet;
                else
                    return null;
            }
        }

        private byte[] _recycleData;
        private int _maxReq = 0;
        private int _curReq = 0;

        private bool _isInit = false;
        private bool _isRequesting = false;

        public FrmCommTester()
        {
            InitializeComponent();
            InitUI();
            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += BgWorker_DoWork;
            this.Text = "CommTester";
            this._isInit = true;

            this.FormClosing += (s, e) =>
            {
                if (this._port.IsUserOpen)
                {
                    this._port.Disconnect();
                }
            };
        }

        private void InitUI()
        {
            this.pnlSplit.Location = new Point(3, 3);
            this.pnlSplit.Orientation = Orientation.Horizontal;
            this.pnlSplit.Dock = DockStyle.Fill;
            this.pnlSplit.Panel1.Padding = new Padding(3);
            this.pnlSplit.Panel2.Padding = this.pnlSplit.Panel1.Padding;
            this.pnlSplit.Panel1MinSize = 20;
            this.pnlSplit.SplitterDistance = 22;

            #region Port 설정
            this.gbxPortSet.Dock = DockStyle.Left;
            this.gbxPortSet.Padding = new Padding(3);
            this.gbxPortSet.Width = 432;
            this.gbxPortSet.Text = "Port Settings";

            this.pnlPortSet_Common.Dock = DockStyle.Left;
            this.pnlPortSet_Common.Width = 106;

            this.cboPortType.Location = new Point(3, 8);
            this.cboPortType.Width = this.pnlPortSet_Common.Width - 6;
            this.cboPortType.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.cboPortType.Items.AddRange(new string[] { "Serial", "Ethernet" });
            this.cboPortType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboPortType.SelectedIndex = 0;
            this.cboPortType.SelectedIndexChanged += (s, e) =>
            {
                if (this._isInit == false) return;
                string selectedItem = (string)this.cboPortType.SelectedItem;

                if(selectedItem == "Serial")
                {
                    this._port.CommType = CommType.Serial;
                    this.cboPortList.Visible = true;
                    this.txtEthernetIP.Visible = false;
                    this.txtPortNo.Visible = false;
                    this.gbxBaudRate.Visible = true;
                    this.gbxParity.Visible = true;
                    this.gbxStopBits.Visible = true;
                    this.gbxDataBits.Visible = true;
                    this.gbxPortSet.Width = 432;
                }
                else if(selectedItem == "Ethernet")
                {
                    this._port.CommType = CommType.Ethernet;
                    this.cboPortList.Visible = false;
                    this.txtEthernetIP.Visible = true;
                    this.txtPortNo.Visible = true;
                    this.gbxBaudRate.Visible = false;
                    this.gbxParity.Visible = false;
                    this.gbxStopBits.Visible = false;
                    this.gbxDataBits.Visible = false;
                    this.gbxPortSet.Width = 112;
                }

                InitPort();
            };

            #region Serial Port 설정

            //Port 목록
            this.cboPortList.Location = new Point(this.cboPortType.Location.X, this.cboPortType.Location.Y + this.cboPortType.Height + 3);
            this.cboPortList.Width = this.cboPortType.Width;
            this.cboPortList.Items.AddRange(SerialPort.GetPortNames());
            this.cboPortList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboPortList.SelectedIndexChanged += (s, e) =>
            {
                if (this._isInit == false) return;
                if (this._port.IsUserOpen == false)
                {
                    this.Serial.PortName = (string)this.cboPortList.SelectedItem;
                }
            };

            //BaudRate
            this.gbxBaudRate.Dock = DockStyle.Left;
            this.gbxBaudRate.Width = 80;
            this.gbxBaudRate.Text = "Baudrate";
            foreach (var baudrate in this._baudrateList)
            {
                RadioButton rdo = CreateRdo(baudrate);
                rdo.CheckedChanged += (s, e) =>
                {
                    if (this._isInit == false) return;

                    this.Serial.BaudRate = baudrate;
                };

                this.gbxBaudRate.Controls.Add(rdo);
                rdo.BringToFront();
            }

            //Parity
            this.gbxParity.Dock = DockStyle.Left;
            this.gbxParity.AutoSize = false;
            this.gbxParity.Text = "Parity";
            foreach (Parity parity in Enum.GetValues(typeof(Parity)))
            {
                RadioButton rdo = CreateRdo(parity);
                rdo.CheckedChanged += (s, e) =>
                {
                    if (this._isInit == false) return;

                    this.Serial.Parity = parity;
                };

                this.gbxParity.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbxParity.Size = this.gbxBaudRate.Size;

            //StopBits
            this.gbxStopBits.Dock = DockStyle.Left;
            this.gbxStopBits.AutoSize = false;
            this.gbxStopBits.Text = "StopBits";
            foreach (StopBits stopbit in Enum.GetValues(typeof(StopBits)))
            {
                if (stopbit == StopBits.None
                    || stopbit == StopBits.OnePointFive
                    ) continue;

                RadioButton rdo = CreateRdo(stopbit);
                rdo.CheckedChanged += (s, e) =>
                {
                    if (this._isInit == false) return;

                    this.Serial.StopBits = stopbit;
                };

                this.gbxStopBits.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbxStopBits.Size = this.gbxBaudRate.Size;

            //DataBits
            this.gbxDataBits.Dock = DockStyle.Left;
            this.gbxDataBits.AutoSize = false;
            this.gbxDataBits.Text = "DataBits";
            foreach (var databit in this._databitsList)
            {
                RadioButton rdo = CreateRdo(databit);
                rdo.CheckedChanged += (s, e) =>
                {
                    if (this._isInit == false) return;

                    this.Serial.DataBits = databit;
                };

                this.gbxDataBits.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbxDataBits.Size = this.gbxBaudRate.Size;

            #endregion Serial Port 설정
            #region Ethernet Port 설정

            //IP Address
            this.txtEthernetIP.Location = new Point(this.cboPortType.Location.X, this.cboPortType.Location.Y + this.cboPortType.Height + 3);
            this.txtEthernetIP.Width = this.cboPortType.Width;
            this.txtEthernetIP.TextAlign = HorizontalAlignment.Center;
            this.txtEthernetIP.Text = "127.0.0.1";
            this.txtEthernetIP.KeyPress += QYUtils.Event_KeyPress_IP;
            this.txtEthernetIP.TextChanged += (s, e) =>
            {
                System.Net.IPAddress ip = null;
                if (System.Net.IPAddress.TryParse(this.txtEthernetIP.Text, out ip))
                {
                    this.Ethernet.IP = this.txtEthernetIP.Text;
                }
            };

            //Port No
            this.txtPortNo.Location = new Point(this.txtEthernetIP.Location.X, this.txtEthernetIP.Location.Y + this.txtEthernetIP.Height + 3);
            this.txtPortNo.Width = this.txtEthernetIP.Width;
            this.txtPortNo.DecimalPlaces = 0;
            this.txtPortNo.TextAlign = HorizontalAlignment.Right;
            this.txtPortNo.Minimum = 0;
            this.txtPortNo.Maximum = int.MaxValue;
            this.txtPortNo.Value = 5000;
            this.txtPortNo.ValueChanged += (s, e) =>
            {
                this.Ethernet.PortNo = Convert.ToInt32(this.txtPortNo.Value);
            };

            #endregion

            //Port 연결버튼
            this.btnConnect.Location = new Point(this.txtPortNo.Location.X, this.txtPortNo.Location.Y + this.txtPortNo.Height + 3);
            this.btnConnect.Width = this.cboPortList.Width - 23;
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += (s, e) =>
            {
                if (this.btnConnect.Text == "Connect")
                {
                    if (this._port.IsUserOpen == false)
                    {
                        this._port.Connect();
                        this.cboPortType.Enabled = false;
                        this.txtEthernetIP.Enabled = false;
                        this.cboPortList.Enabled = false;
                        this.btnConnect.Text = "Disconnect";
                        this.lblConnState.BackColor = Color.Green;
                    }
                }
                else
                {
                    if (this._port.IsUserOpen == true)
                    {
                        this._port.Disconnect();
                        this.cboPortType.Enabled = true;
                        this.txtEthernetIP.Enabled = true;
                        this.cboPortList.Enabled = true;
                        this.btnConnect.Text = "Connect";
                        this.lblConnState.BackColor = Color.Red;
                    }
                }
            };

            this.lblConnState.Location = new Point(this.btnConnect.Location.X + this.btnConnect.Width + 3, this.btnConnect.Location.Y + 2);
            this.lblConnState.Height = 19;
            this.lblConnState.Width = 19;
            this.lblConnState.BackColor = Color.Red;


            #endregion Port 설정
            #region Protocol 설정

            this.cboProtocolList.Location = new Point(3, 26);
            this.cboProtocolList.Width = this.cboPortList.Width;
            this.cboProtocolList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboProtocolList.DropDownWidth = (int)(this.cboProtocolList.CreateGraphics().MeasureString("PCLink_SUM_TD300500", this.cboProtocolList.Font).Width);
            this.cboProtocolList.Items.AddRange(Enum.GetValues(typeof(ProtocolType)).OfType<object>().ToArray());
            if (this.cboProtocolList.Items.Count > 0) this.cboProtocolList.SelectedIndex = 0;
            this.cboProtocolList.SelectedIndexChanged += (s, e) =>
            {
                ProtocolType protocol = (ProtocolType)this.cboProtocolList.SelectedItem;
                this._port.ProtocolType = protocol;

                if (protocol != ProtocolType.None)
                    this.gvProtocolResult.Visible = true;
                else
                    this.gvProtocolResult.Visible = false;
            };

            this.chkAddErrChk.Location = new Point(this.cboProtocolList.Location.X, this.cboProtocolList.Location.Y + this.cboProtocolList.Height + 3);
            this.chkAddErrChk.Width = 115;
            this.chkAddErrChk.Text = "ErrorCheck 생성";
            this.chkAddErrChk.CheckAlign = ContentAlignment.MiddleRight;
            this.chkAddErrChk.Checked = false;

            this.gbxProtocolSet.Dock = DockStyle.Left;
            this.gbxProtocolSet.Width = this.chkAddErrChk.Location.X + this.chkAddErrChk.Width + 3;
            this.gbxProtocolSet.Text = "Protocol Settings";

            #endregion Protocol 설정
            #region 전송 설정

            #region 반복설정
            
            //반복전송 체크박스
            this.chkRewrite.Location = new Point(3, 26);
            this.chkRewrite.Width = 80;
            this.chkRewrite.Text = "반복 전송";
            this.chkRewrite.CheckAlign = ContentAlignment.MiddleRight;
            this.chkRewrite.Checked = false;
            this.chkRewrite.CheckedChanged += (s, e) => {
                this.numRewrite.Enabled = this.chkRewrite.Checked;
                this.chkRewriteInfi.Enabled = this.chkRewrite.Checked;
            };

            //반복전송 횟수 TextBox
            this.numRewrite.Location = new Point(this.chkRewrite.Location.X + this.chkRewrite.Size.Width + 3, this.chkRewrite.Location.Y + 3);
            this.numRewrite.Width = 60;
            this.numRewrite.Minimum = 2;
            this.numRewrite.Enabled = false;
            this.numRewrite.Value = 100;

            Label lblRewriteUnit = new Label();
            lblRewriteUnit.Location = new Point(this.numRewrite.Location.X + this.numRewrite.Size.Width + 3, this.numRewrite.Location.Y);
            lblRewriteUnit.Width = 20;
            lblRewriteUnit.Text = "회";
            lblRewriteUnit.TextAlign = ContentAlignment.MiddleLeft;

            //반복전송 무한 체크박스
            this.chkRewriteInfi.Location = new Point(this.chkRewrite.Location.X, this.chkRewrite.Location.Y + this.chkRewrite.Height + 3);
            this.chkRewriteInfi.Width = 80;
            this.chkRewriteInfi.Text = "무한반복";
            this.chkRewriteInfi.CheckAlign = ContentAlignment.MiddleRight;
            this.chkRewriteInfi.Checked = false;
            this.chkRewriteInfi.Enabled = false;
            this.chkRewriteInfi.CheckedChanged += (s, e) => {
                this.numRewrite.Enabled = !this.chkRewriteInfi.Checked;
            };

            #endregion 반복설정

            //TextBox 작성요령 설명
            this.lblWriteTooltip.Location = new Point(this.chkRewriteInfi.Location.X, this.chkRewriteInfi.Location.Y + this.chkRewriteInfi.Height + 3);
            this.lblWriteTooltip.Text = "10진수: @000, 16진수: #00  ex)입력값: 30 - 10진수:@030 / 16진수:#26";
            this.lblWriteTooltip.Width = 400;
            this.lblWriteTooltip.TextAlign = ContentAlignment.MiddleLeft;

            //TextBox데이터 전송 버튼
            this.btnSend.Width = 60;
            this.btnSend.Location = new Point(this.lblWriteTooltip.Width - this.btnSend.Width + 3, this.lblWriteTooltip.Location.Y + this.lblWriteTooltip.Height + 3);
            this.btnSend.Text = "Send";
            this.btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnSend.Click += (s, e) => { WriteData(); };

            //전송 Write 텍스트 Box
            this.txtWrite.Location = new Point(this.lblWriteTooltip.Location.X, this.lblWriteTooltip.Location.Y + this.lblWriteTooltip.Height + 4);
            this.txtWrite.Width = this.lblWriteTooltip.Width - (this.btnSend.Width + 3);
            this.txtWrite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.gbxCommSet.Dock = DockStyle.Left;
            this.gbxCommSet.Text = "Comm Settings";
            this.gbxCommSet.Width = this.btnSend.Location.X + this.btnSend.Width + 3;

            #endregion 전송 설정

            #region 통신 결과

            this.pnlResult.Dock = DockStyle.Top;
            this.pnlResult.Height = 47;
            //this.pnlResult.BorderStyle = BorderStyle.FixedSingle;

            //Comm Result
            this.gvCommResult.Dock = DockStyle.Left;
            this.gvCommResult.Width = 270 + 3;
            this.gvCommResult.DataSource = this._dtDataResult;
            this.gvCommResult.RowHeadersVisible = false;
            this.gvCommResult.AllowUserToAddRows = false;
            this.gvCommResult.ReadOnly = true;

            DataGridViewTextBoxColumn colTry = new DataGridViewTextBoxColumn();
            colTry.DataPropertyName = "TryCount";
            colTry.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTry.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTry.HeaderText = "시도수";
            colTry.ReadOnly = true;
            colTry.Width = 50;

            DataGridViewTextBoxColumn colSuccess = new DataGridViewTextBoxColumn();
            colSuccess.DataPropertyName = "Success";
            colSuccess.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSuccess.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSuccess.HeaderText = "성공";
            colSuccess.ReadOnly = true;
            colSuccess.Width = 50;

            DataGridViewTextBoxColumn colNoneReceive = new DataGridViewTextBoxColumn();
            colNoneReceive.DataPropertyName = "None Receive";
            colNoneReceive.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colNoneReceive.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colNoneReceive.HeaderText = "무응답";
            colNoneReceive.ReadOnly = true;
            colNoneReceive.Width = 50;

            DataGridViewTextBoxColumn colReceiveStop = new DataGridViewTextBoxColumn();
            colReceiveStop.DataPropertyName = "Receive Stop";
            colReceiveStop.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveStop.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveStop.HeaderText = "응답멈춤";
            colReceiveStop.ReadOnly = true;
            colReceiveStop.Width = 60;

            DataGridViewTextBoxColumn colReceiveLong = new DataGridViewTextBoxColumn();
            colReceiveLong.DataPropertyName = "Receive Too Long";
            colReceiveLong.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveLong.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colReceiveLong.HeaderText = "무한응답";
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

            #endregion 통신 결과
            #region Protocol 결과

            this.gvProtocolResult.Dock = DockStyle.Left;
            this.gvProtocolResult.DataSource = this._dtProtocolResult;
            this.gvProtocolResult.RowHeadersVisible = false;
            this.gvProtocolResult.AllowUserToAddRows = false;
            this.gvProtocolResult.ReadOnly = true;
            this.gvProtocolResult.Visible = false;

            DataGridViewTextBoxColumn colErrorCheck = new DataGridViewTextBoxColumn();
            colErrorCheck.DataPropertyName = "ErrChk";
            colErrorCheck.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colErrorCheck.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colErrorCheck.HeaderText = "무결성검사";
            colErrorCheck.ReadOnly = true;
            colErrorCheck.Width = 72;

            DataGridViewTextBoxColumn colProtocolErr = new DataGridViewTextBoxColumn();
            colProtocolErr.DataPropertyName = "ProtocolErr";
            colProtocolErr.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colProtocolErr.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colProtocolErr.HeaderText = "Protocol NG";
            colProtocolErr.ReadOnly = true;
            colProtocolErr.Width = 80;

            this.gvProtocolResult.Columns.Add(colErrorCheck);
            this.gvProtocolResult.Columns.Add(colProtocolErr);

            int gvWidth = 0;
            foreach (DataGridViewColumn col in this.gvProtocolResult.Columns)
            {
                gvWidth += col.Width;
            }
            this.gvProtocolResult.Width = gvWidth + 3;

            this._dtProtocolResult.Columns.Add(new DataColumn("ErrChk", typeof(uint)) { DefaultValue = 0 });
            this._dtProtocolResult.Columns.Add(new DataColumn("ProtocolErr", typeof(uint)) { DefaultValue = 0 });

            #endregion
            #region Log Grid
            
            this.gbxLog.Dock = DockStyle.Fill;

            this.pnlLog.Dock = DockStyle.Right;
            this.pnlLog.Width = (this._bufferColCount * 38) - 2;
            this._bufferColCount = 15;

            this.gvBuffer.Dock = DockStyle.Top;
            this.gvBuffer.AutoSize = false;
            this.gvBuffer.DataSource = this._dtBuffer;
            this.gvBuffer.Height = 140;
            this.gvBuffer.RowHeadersVisible = false;
            this.gvBuffer.AllowUserToAddRows = false;
            this.gvBuffer.AllowUserToResizeColumns = false;
            this.gvBuffer.AllowUserToResizeRows = false;

            //Request, Receive Log GridView
            this._dtDataLog.Columns.Add(new DataColumn("Type", typeof(string)) { DefaultValue = string.Empty });
            this._dtDataLog.Columns.Add(new DataColumn("Time", typeof(string)));
            

            this.gvDataLog.Dock = DockStyle.Fill;
            this.gvDataLog.AutoSize = false;
            this.gvDataLog.DataSource = this._dtDataLog;
            this.gvDataLog.Width = this.gbxLog.Width - (this.gvBuffer.Width + 10);
            this.gvDataLog.Height = this.gbxLog.Height - (this.gvCommResult.Location.Y + this.gvCommResult.Height + 8); 
            this.gvDataLog.RowHeadersVisible = false;
            this.gvDataLog.AllowUserToAddRows = false;
            this.gvDataLog.AllowUserToResizeColumns = false;
            this.gvDataLog.AllowUserToResizeRows = false;
            this.gvDataLog.RowsAdded += (s, e) =>
            {
                if (this.gvDataLog.Rows.Count > 0)
                    this.gvDataLog.FirstDisplayedScrollingRowIndex = this.gvDataLog.Rows.Count - 1;
            };

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

            for (int i = 0; i < rstColCount; i++)
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


            for (int i = 0; i < this._bufferColCount; i++)
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

            //Text Log
            this.txtLog.Dock = DockStyle.Fill;
            this.txtLog.AutoSize = false;   
            this.txtLog.BorderStyle = BorderStyle.FixedSingle;
            //this.txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = ScrollBars.Vertical;

            
            this.gbxLog.Text = "Log";
            

            #endregion Log Grid

            #region Control Add

            this.Controls.Add(this.pnlSplit);
            this.pnlSplit.Panel1.Controls.Add(this.gbxPortSet);
            this.gbxPortSet.Controls.Add(this.pnlPortSet_Common);
            this.pnlPortSet_Common.Controls.Add(this.cboPortType);
            this.pnlPortSet_Common.Controls.Add(this.cboPortList);
            this.pnlPortSet_Common.Controls.Add(this.txtEthernetIP);
            this.pnlPortSet_Common.Controls.Add(this.txtPortNo);
            this.pnlPortSet_Common.Controls.Add(this.btnConnect);
            this.pnlPortSet_Common.Controls.Add(this.lblConnState);
            this.gbxPortSet.Controls.Add(this.gbxBaudRate);
            this.gbxPortSet.Controls.Add(this.gbxParity);
            this.gbxPortSet.Controls.Add(this.gbxStopBits);
            this.gbxPortSet.Controls.Add(this.gbxDataBits);
            this.pnlSplit.Panel1.Controls.Add(this.gbxProtocolSet);
            this.pnlSplit.Panel1.Controls.Add(this.gbxCommSet);
            this.pnlSplit.Panel2.Controls.Add(this.gbxLog);
            this.gbxProtocolSet.Controls.Add(this.cboProtocolList);
            this.gbxProtocolSet.Controls.Add(this.chkAddErrChk);
            this.gbxCommSet.Controls.Add(this.chkRewrite);
            this.gbxCommSet.Controls.Add(this.numRewrite);
            this.gbxCommSet.Controls.Add(lblRewriteUnit);
            this.gbxCommSet.Controls.Add(this.chkRewriteInfi);
            this.gbxCommSet.Controls.Add(this.txtWrite);
            this.gbxCommSet.Controls.Add(this.btnSend);
            this.gbxCommSet.Controls.Add(this.lblWriteTooltip);
            this.gbxCommSet.Controls.Add(this.txtWrite);
            this.gbxLog.Controls.Add(this.pnlResult);
            this.pnlResult.Controls.Add(this.gvCommResult);
            this.pnlResult.Controls.Add(this.gvProtocolResult);
            this.gbxLog.Controls.Add(this.gvDataLog);
            this.gbxLog.Controls.Add(this.pnlLog);
            this.pnlLog.Controls.Add(this.gvBuffer);
            this.pnlLog.Controls.Add(this.txtLog);

            #endregion Control Add
            #region Visible Index
            
            this.pnlPortSet_Common.BringToFront();
            this.gbxBaudRate.BringToFront();
            this.gbxParity.BringToFront();
            this.gbxStopBits.BringToFront();
            this.gbxDataBits.BringToFront();

            this.gbxPortSet.BringToFront();
            this.gbxProtocolSet.BringToFront();
            this.gbxCommSet.BringToFront();

            this.gvCommResult.BringToFront();
            this.gvProtocolResult.BringToFront();

            this.pnlResult.BringToFront();
            this.pnlLog.BringToFront();
            this.gvDataLog.BringToFront();
            this.gvBuffer.BringToFront();
            this.txtLog.BringToFront();

            #endregion Visible Index

            InitPort();
            this._port.ComPortLog += this.UpdateUI_ComPortLog;
            this._port.AfterSendRequest += this.UpdateUI_AfterSendRequest;
            this._port.PortCurrentBuffer += this.UpdateUI_PortCurrentBuffer;
            this._port.Error_ErrorCode += this.UpdateUI_Error_ErrorCode;
            this._port.Error_Protocol += this.UpdateUI_Error_Protocol;
            this._port.RequestComplete += this.UpdateUI_RequestComplete;
            this._port.RequestTimeout += this.UpdateUI_RequestTimeout;

            this.txtEthernetIP.Visible = false;
            this.txtPortNo.Visible = false;
        }

        private void InitDataLogGrid()
        {
            this._dtDataResult.Rows.Clear();
            this._dtProtocolResult.Rows.Clear();
            this._dtDataLog.Rows.Clear();
            this._dtBuffer.Rows.Clear();
            this.txtLog.Text = string.Empty;

            this._dtDataResult.Rows.Add();
            this._dtProtocolResult.Rows.Add();
            this.gvCommResult.ClearSelection();
            this.gvProtocolResult.ClearSelection();
        }

        private void InitPort()
        {
            if (this._port.CommType == CommType.Serial)
            {
                if(this.cboPortList.Items.Count > 0)
                    this.Serial.PortName = (string)this.cboPortList.Items[0];
                this.Serial.BaudRate = (int)this._baudrateList[0];
                this.Serial.Parity = (Parity)QYUtils.EnumToItems<Parity>()[0];
                this.Serial.StopBits = (StopBits)QYUtils.EnumToItems<StopBits>()[0];
                this.Serial.DataBits = (int)this._databitsList[1];

                if(this.cboPortList.Items.Count > 0)
                    this.cboPortList.SelectedIndex = 0;
                foreach (RadioButton rdo in this.gbxBaudRate.Controls)
                {
                    if((int)rdo.Tag == this.Serial.BaudRate)
                    {
                        rdo.Checked = true;
                        break;
                    }
                }
                foreach (RadioButton rdo in this.gbxParity.Controls)
                {
                    if ((Parity)rdo.Tag == this.Serial.Parity)
                    {
                        rdo.Checked = true;
                        break;
                    }
                }
                foreach (RadioButton rdo in this.gbxStopBits.Controls)
                {
                    if ((StopBits)rdo.Tag == this.Serial.StopBits)
                    {
                        rdo.Checked = true;
                        break;
                    }
                }
                foreach (RadioButton rdo in this.gbxDataBits.Controls)
                {
                    if ((byte)rdo.Tag == this.Serial.DataBits)
                    {
                        rdo.Checked = true;
                        break;
                    }
                }
            }
            else if(this._port.CommType == CommType.Ethernet)
            {
                this.txtEthernetIP.Text = "192.168.2.133";
                this.txtPortNo.Value = 0502;

                this.Ethernet.IP = this.txtEthernetIP.Text;
                this.Ethernet.PortNo = Convert.ToInt32(this.txtPortNo.Value);
            }
        }

        private RadioButton CreateRdo(object data)
        {
            RadioButton rdo = new RadioButton();
            rdo.AutoSize = false;
            rdo.Height = 18;
            rdo.Width = (int)this.CreateGraphics().MeasureString(data.ToString(), rdo.Font).Width;
            rdo.Dock = DockStyle.Top;
            rdo.Text = data.ToString();
            rdo.Checked = false;
            rdo.Tag = data;

            return rdo;
        }

        private void WriteData()
        {
            if (this.BgWorker.IsBusy)
            {
                this.BgWorker.CancelAsync();
                return;
            }
            InitDataLogGrid();
            this._port.WriteQueue.Clear();


            int handle = 0;
            string splitStr;
            byte b = 0;
            List<byte> bytes = new List<byte>();

            while (handle < this.txtWrite.TextLength)
            {
                char c = this.txtWrite.Text[handle];
                int len;
                bool boolean = false;
                if (c == '@') len = 3;
                else if (c == '#') len = 2;
                else
                {
                    if (++handle > this.txtWrite.TextLength)
                        break;
                    else
                        continue;
                }

                if (++handle + len > this.txtWrite.TextLength)
                    break;

                splitStr = this.txtWrite.Text.Substring(handle, len);
                if(c == '@')
                    boolean = byte.TryParse(splitStr, out b);
                else if(c == '#')
                    boolean = byte.TryParse(splitStr,
                        System.Globalization.NumberStyles.HexNumber,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out b);

                if (boolean)
                {
                    bytes.Add(b);
                    handle += len;
                }
            }

            this._recycleData = bytes.ToArray();
            if (this.chkAddErrChk.Checked)
            {
                this._recycleData = QYUtils.Comm.BytesAppend(this._recycleData, this._port.Protocol.CreateErrCode(this._recycleData));
            }

            this._isRequesting = true;
            this._curReq = 0;
            if (this.chkRewrite.Checked)
            {
                if (this.chkRewriteInfi.Checked)
                    this._maxReq = int.MaxValue;
                else
                    this._maxReq = (int)this.numRewrite.Value;
            }
            else
            {
                this._maxReq = 1;
            }

            if(this.BgWorker.IsBusy == false)
            {
                this.BgWorker.RunWorkerAsync();

                this.btnSend.Text = "Stop";
            }
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
                        if (this._isRequesting
                            && this._port.WriteQueue.Count == 0)
                        {
                            if (this._maxReq <= this._curReq)
                                break;
                            else
                            {
                                this._curReq++;
                                this._port.WriteQueue.Enqueue(this._recycleData);
                            }
                        }
                    }
                }
                catch
                {

                }
            }

            this._isRequesting = false;
            UpdateUI("SendComplete");
        }

        private void UpdateUI_ComPortLog(params object[] obj)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UpdateUI_WithParam(UpdateUI_ComPortLog), new object[] { obj });
            else
            {
                this.txtLog.AppendText($"Port Log: {obj[0] as string}\r\n");
            }
        }

        private void UpdateUI_AfterSendRequest(params object[] obj)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UpdateUI_WithParam(UpdateUI_AfterSendRequest), new object[] { obj });
            else
            {
                byte[] req = obj[0] as byte[];
                //송신 Grid Log Update
                DataRow dr = this._dtDataLog.NewRow();
                dr["Type"] = "Req";
                dr["Time"] = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");
                for (int i = 0; i < req.Length; i++)
                {
                    if ((i != 0) && ((i % rstColCount) == 0))
                    {
                        this._dtDataLog.Rows.Add(dr);
                        dr = this._dtDataLog.NewRow();
                    }

                    dr[string.Format("Col{0}", i)] = req[i].ToString("X2");
                }
                this._dtDataLog.Rows.Add(dr);

                //시도횟수 증가
                this._dtDataResult.Rows[0]["TryCount"] = this._curReq;

                //TextLog Update
                this.txtLog.AppendText($"Req:{ByteToString(req)}\r\n");
            }
        }

        private void UpdateUI_PortCurrentBuffer(params object[] obj)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UpdateUI_WithParam(UpdateUI_PortCurrentBuffer), new object[] { obj });
            else
            {
                byte[] buffer = obj[0] as byte[];
                this._dtBuffer.Rows.Clear();

                DataRow dr = null;
                for (int i = 0; i < buffer.Length; i++)
                {
                    string fieldName = string.Format("col{0}", i % this._bufferColCount);
                    if (i % this._bufferColCount == 0)
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
        }

        private void UpdateUI_Error_ErrorCode(params object[] obj)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UpdateUI_WithParam(UpdateUI_Error_ErrorCode), new object[] { obj });
            else
            {
                byte[] frame = obj[0] as byte[];
                //수신 Grid Log Update
                AddRcvDataLog(frame, true);

                //TextLog Update
                this.txtLog.AppendText($"Res Error - ErrorCode:{ByteToString(obj[0] as byte[])}\r\n");

                //Result Grid Update
                this._dtProtocolResult.Rows[0]["ErrChk"] = (uint)(this._dtProtocolResult.Rows[0]["ErrChk"]) + 1;
            }
        }

        private void UpdateUI_Error_Protocol(params object[] obj)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UpdateUI_WithParam(UpdateUI_Error_Protocol), new object[] { obj });
            else
            {
                byte[] frame = obj[0] as byte[];
                //수신 Grid Log Update
                AddRcvDataLog(frame, true);

                //TextLog Update
                this.txtLog.AppendText($"Res Error - Protocol:{ByteToString(obj[0] as byte[])}\r\n");

                //Result Grid Update
                this._dtProtocolResult.Rows[0]["ProtocolErr"] = (uint)(this._dtProtocolResult.Rows[0]["ProtocolErr"]) + 1;
            }
        }

        private void UpdateUI_RequestComplete(params object[] obj)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UpdateUI_WithParam(UpdateUI_RequestComplete), new object[] { obj });
            else
            {
                byte[] frame = obj[1] as byte[];
                //수신 Grid Log Update
                AddRcvDataLog(frame, false);
                //TextLog Update
                this.txtLog.AppendText($"Res:{ByteToString(frame)}\r\n\r\n");

                //Result Grid Update
                this._dtDataResult.Rows[0]["Success"] = (uint)(this._dtDataResult.Rows[0]["Success"]) + 1;
            }
        }

        private void UpdateUI_RequestTimeout(params object[] obj)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UpdateUI_WithParam(UpdateUI_RequestTimeout), new object[] { obj });
            else
            {
                string type = obj[0] as string;

                if(type == "None Response")
                {
                    //수신 Grid Log Update
                    DataRow dr = this._dtDataLog.NewRow();
                    dr["Type"] = "Rcv";
                    dr["Time"] = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");
                    this._dtDataLog.Rows.Add(dr);
                    this.gvDataLog.Rows[this._dtDataLog.Rows.IndexOf(dr)].Cells["Type"].Style.BackColor = Color.Salmon;

                    //TextLog Update
                    this.txtLog.AppendText(string.Format("Res Timeover - None: -\r\n"));

                    //Result Grid Update
                    this._dtDataResult.Rows[0]["None Receive"] = (uint)(this._dtDataResult.Rows[0]["None Receive"]) + 1;
                }
                else if(type == "Stop Response")
                {
                    byte[] frame = obj[1] as byte[];
                    //수신 Grid Log Update
                    AddRcvDataLog(frame, true);

                    //TextLog Update
                    this.txtLog.AppendText($"Res Timeover - Stop:{ByteToString(frame)}\r\n");

                    //Result Grid Update
                    this._dtDataResult.Rows[0]["Receive Stop"] = (uint)(this._dtDataResult.Rows[0]["Receive Stop"]) + 1;
                }
                else if (type == "Long Response")
                {
                    byte[] frame = obj[1] as byte[];
                    //수신 Grid Log Update
                    AddRcvDataLog(frame, true);

                    //TextLog Update
                    this.txtLog.AppendText($"Res Timeover - Long:{ByteToString(frame)}\r\n");

                    //Result Grid Update
                    this._dtDataResult.Rows[0]["Receive Too Long"] = (uint)(this._dtDataResult.Rows[0]["Receive Too Long"]) + 1;
                }
            }
        }

        private void UpdateUI(params object[] obj)
        {
            try
            {
                if (this.InvokeRequired)
                    this.BeginInvoke(new UpdateUI_WithParam(UpdateUI), new object[] { obj });
                else
                {
                    if(obj[0] as string == "SendComplete")
                        this.btnSend.Text = "Send";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format(
                    "FrmCommTester.cs - UpdateUI()\r\n" +
                    "{0}\r\n\r\n" +
                    "{1}",
                    ex.Message, ex.StackTrace));
            }
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

        private void AddRcvDataLog(byte[] bytes, bool isError = false)
        {
            DataRow dr = this._dtDataLog.NewRow();
            dr["Type"] = "Rcv";
            dr["Time"] = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");
            for (int i = 0; i < bytes.Length; i++)
            {
                if ((i != 0) && ((i % rstColCount) == 0))
                {
                    this._dtDataLog.Rows.Add(dr);
                    if(isError)
                    {
                        this.gvDataLog.Rows[this._dtDataLog.Rows.IndexOf(dr)].Cells["Type"].Style.BackColor = Color.Crimson;
                        this.gvDataLog.Rows[this._dtDataLog.Rows.IndexOf(dr)].Cells["Type"].Style.ForeColor = Color.White;
                    }
                    dr = this._dtDataLog.NewRow();
                }

                dr[string.Format("Col{0}", i)] = bytes[i].ToString("X2");
            }
            this._dtDataLog.Rows.Add(dr);
            if(isError)
            {
                this.gvDataLog.Rows[this._dtDataLog.Rows.IndexOf(dr)].Cells["Type"].Style.BackColor = Color.Crimson;
                this.gvDataLog.Rows[this._dtDataLog.Rows.IndexOf(dr)].Cells["Type"].Style.ForeColor = Color.White;
            }

        }
    }
}
