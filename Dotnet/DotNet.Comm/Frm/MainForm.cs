using DotNet.Comm.Structures.AppPort;
using DotNet.Comm.Structures.CustomStruct.HYNux;
using DotNet.Comm.Structures.PCPorts;
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

namespace DotNet.Comm.Frm
{

    public partial class MainForm : Form
    {
        public delegate void UIUpdateHandler(string cmd);
        public delegate void BytesLogHandler(string type, params byte[] data);

        #region UI Controls

        private GroupBox gbxPortSet = new GroupBox();
        private GroupBox gbxProtocolSet = new GroupBox();
        private GroupBox gbxCommSet = new GroupBox();
        private GroupBox gbxLog = new GroupBox();

        private ComboBox cboPortList = new ComboBox();
        private Button btnConnect = new Button();
        private Label lblConnState = new Label();

        private GroupBox gbxBaudRate = new GroupBox();
        private GroupBox gbxParity = new GroupBox();
        private GroupBox gbxStopBits = new GroupBox();
        private GroupBox gbxDataBits = new GroupBox();

        private ComboBox cboProtocolList = new ComboBox();

        private DataGridView gvCommResult = new DataGridView();
        private DataGridView gvProtocolResult = new DataGridView();
        private DataGridView gvDataLog = new DataGridView();
        private DataGridView gvBuffer = new DataGridView();

        private CheckBox chkRewrite = new CheckBox();
        private NumericUpDown numRewrite = new NumericUpDown();
        private CheckBox chkRewriteInfi = new CheckBox();
        private CheckBox chkAddErrChk = new CheckBox();

        private TextBox txtWrite = new TextBox();
        private Button btnSend = new Button();
        private Label lblWriteTooltip = new Label();

        private TextBox txtLog = new TextBox();

        #endregion UI Controls

        private int[] _baudrateList = new int[] { 9600, 19200, 38400, 57600, 115200 };
        private byte[] _databitsList = new byte[] { 7, 8 };
        private DataTable _dtDataLog = new DataTable();
        private DataTable _dtDataResult = new DataTable();
        private DataTable _dtProtocolResult = new DataTable();
        private DataTable _dtBuffer = new DataTable();
        private HYPort _port = new HYPort(PortType.Serial);
        private BackgroundWorker BgWorker = new BackgroundWorker();
        private int _bufferColCount = 10;

        private QYSerialPort Serial
        {
            get
            {
                if (this._port != null
                    && this._port.PCPort is QYSerialPort)
                    return this._port.PCPort as QYSerialPort;
                else
                    return null;
            }
        }

        private byte[] _recycleData;
        private int _maxReq = 0;
        private int _curReq = 0;

        public MainForm()
        {
            InitializeComponent();
            InitUI();
            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += BgWorker_DoWork;

            this.FormClosing += (s, e) =>
            {
                if (this._port.IsOpen)
                {
                    this._port.Close();
                }
            };
        }

        private void InitUI()
        {
            #region Serial Port 설정

            //Port 목록
            this.cboPortList.Location = new Point(3, 18);
            this.cboPortList.Width = 100;
            this.cboPortList.Items.AddRange(SerialPort.GetPortNames());
            this.cboPortList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboPortList.SelectedIndexChanged += (s, e) =>
            {
                if (this._port.IsOpen == false)
                {
                    this.Serial.PortName = this.cboPortList.SelectedItem.ToString();
                }
            };
            if(this.cboPortList.Items.Count > 0) this.cboPortList.SelectedIndex = 0;

            //Port 연결버튼
            this.btnConnect.Width = this.cboPortList.Width - 23;
            this.btnConnect.Location = new Point(this.cboPortList.Location.X, this.cboPortList.Location.Y + this.cboPortList.Height + 3);
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += (s, e) =>
            {
                if(this.btnConnect.Text == "Connect")
                {
                    if (this._port.IsOpen == false
                        && this._port.Open())
                    {
                        this.cboPortList.Enabled = false;
                        this.btnConnect.Text = "Disconnect";
                        this.lblConnState.BackColor = Color.Green;
                    }
                }
                else
                {
                    if (this._port.Close())
                    {
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

            //BaudRate
            this.gbxBaudRate.Location = new Point(this.cboPortList.Location.X + this.cboPortList.Width + 3, this.cboPortList.Location.Y - 8);
            this.gbxBaudRate.Width = 80;
            this.gbxBaudRate.Text = "Baudrate";
            foreach (var baudrate in this._baudrateList)
            {
                RadioButton rdo = CreateRdo(baudrate);
                rdo.CheckedChanged += (s, e) =>
                {
                    this.Serial.BaudRate = baudrate;
                };

                this.gbxBaudRate.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbxBaudRate.Height = this.gbxBaudRate.Controls.Count * (18 + 3) + 6;
            (this.gbxBaudRate.Controls[this.gbxBaudRate.Controls.Count - 1] as RadioButton).Checked = true;

            //Parity
            this.gbxParity.Location = new Point(this.gbxBaudRate.Location.X + this.gbxBaudRate.Width + 3, this.gbxBaudRate.Location.Y);
            this.gbxParity.AutoSize = false;
            this.gbxParity.Text = "Parity";
            foreach (Parity parity in DotNet.Utils.Controls.QYUtils.EnumToItems<Parity>())
            {
                RadioButton rdo = CreateRdo(parity);
                rdo.CheckedChanged += (s, e) =>
                {
                    this.Serial.Parity = parity;
                };

                this.gbxParity.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbxParity.Size = this.gbxBaudRate.Size;
            (this.gbxParity.Controls[this.gbxParity.Controls.Count - 1] as RadioButton).Checked = true;

            //StopBits
            this.gbxStopBits.Location = new Point(this.gbxParity.Location.X + this.gbxParity.Width + 3, this.gbxParity.Location.Y);
            this.gbxStopBits.AutoSize = false;
            this.gbxStopBits.Text = "StopBits";
            foreach (StopBits stopbit in DotNet.Utils.Controls.QYUtils.EnumToItems<StopBits>())
            {
                if (stopbit == StopBits.None
                    || stopbit == StopBits.OnePointFive
                    ) continue;

                RadioButton rdo = CreateRdo(stopbit);
                rdo.CheckedChanged += (s, e) =>
                {
                    this.Serial.StopBits = stopbit;
                };

                this.gbxStopBits.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbxStopBits.Size = this.gbxBaudRate.Size;
            (this.gbxStopBits.Controls[this.gbxStopBits.Controls.Count - 1] as RadioButton).Checked = true;

            //DataBits
            this.gbxDataBits.Location = new Point(this.gbxStopBits.Location.X + this.gbxStopBits.Width + 3, this.gbxStopBits.Location.Y);
            this.gbxDataBits.AutoSize = false;
            this.gbxDataBits.Text = "DataBits";
            foreach (var databit in this._databitsList)
            {
                RadioButton rdo = CreateRdo(databit);
                rdo.CheckedChanged += (s, e) =>
                {
                    this.Serial.DataBits = databit;
                };

                this.gbxDataBits.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbxDataBits.Size = this.gbxBaudRate.Size;
            (this.gbxDataBits.Controls[this.gbxDataBits.Controls.Count - 1] as RadioButton).Checked = true;

            this.gbxPortSet.Location = new Point(3, 3);
            this.gbxPortSet.Width = this.gbxDataBits.Location.X + this.gbxDataBits.Width + 3;
            this.gbxPortSet.Height = this.gbxDataBits.Location.Y + this.gbxDataBits.Height + 3;
            this.gbxPortSet.Text = "Port Settings";

            #endregion Serial Port 설정
            #region Protocol 설정

            this.cboProtocolList.Location = new Point(3, 18);
            this.cboProtocolList.Width = this.cboPortList.Width;
            this.cboProtocolList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboProtocolList.DropDownWidth = (int)(this.cboProtocolList.CreateGraphics().MeasureString("PCLink_SUM_TD300500", this.cboProtocolList.Font).Width);
            this.cboProtocolList.Items.AddRange(new string[]
            {
                "None",
                "ModbusRTU",
                "ModbusRTU_EXP",
                "ModbusAscii",
                "ModbusAscii_EXP",
                "PCLink_STD",
                "PCLink_STD_TH300500",
                "PCLink_SUM",
                "PCLink_SUM_TD300500",
                "PCLink_SUM_TH300500",
            });
            if (this.cboProtocolList.Items.Count > 0) this.cboProtocolList.SelectedIndex = 0;
            this.cboProtocolList.SelectedIndexChanged += (s, e) =>
            {
                string item = (string)this.cboProtocolList.SelectedItem;

                switch (item)
                {
                    case "ModbusRTU":
                        this._port.Protocol = new HYModbus(0);
                        this._port.ErrorCheck = new ModbusRTUErrorCheck();
                        break;
                    case "ModbusAscii":
                        this._port.Protocol = new HYModbus(1);
                        this._port.ErrorCheck = new ModbusAsciiErrorCheck();
                        break;
                    case "ModbusRTU_EXP":
                        this._port.Protocol = new HYModbus(2);
                        this._port.ErrorCheck = new ModbusRTUErrorCheck();
                        break;
                    case "ModbusAscii_EXP":
                        this._port.Protocol = new HYModbus(3);
                        this._port.ErrorCheck = new ModbusAsciiErrorCheck();
                        break;
                    case "PCLink_STD":
                        this._port.Protocol = new PCLink(0);
                        this._port.ErrorCheck = null;
                        break;
                    case "PCLink_SUM":
                        this._port.Protocol = new PCLink(1);
                        this._port.ErrorCheck = new PCLinkErrorCheck();
                        break;
                    case "PCLink_STD_TH300500":
                        this._port.Protocol = new PCLink(2);
                        this._port.ErrorCheck = null;
                        break;
                    case "PCLink_SUM_TD300500":
                        this._port.Protocol = new PCLink(3);
                        this._port.ErrorCheck = new PCLinkErrorCheck();
                        break;
                    case "PCLink_SUM_TH300500":
                        this._port.Protocol = new PCLink(4);
                        this._port.ErrorCheck = new PCLinkTHErrorCheck();
                        break;
                    default:
                        this._port.Protocol = null;
                        this._port.ErrorCheck = null;
                        break;
                }

                if (this._port.Protocol != null)
                {
                    this._port.ProtocolName = item;
                    this.gvProtocolResult.Visible = true;
                }
                else
                {
                    this.gvProtocolResult.Visible = false;
                }
            };

            this.chkAddErrChk.Location = new Point(this.cboProtocolList.Location.X, this.cboProtocolList.Location.Y + this.cboProtocolList.Height + 3);
            this.chkAddErrChk.Width = 115;
            this.chkAddErrChk.Text = "ErrorCheck 생성";
            this.chkAddErrChk.CheckAlign = ContentAlignment.MiddleRight;
            this.chkAddErrChk.Checked = false;
            this.chkAddErrChk.CheckedChanged += (s, e) => {
                this._port.CreErrCheck = this.chkAddErrChk.Checked;
            };

            this.gbxProtocolSet.Location = new Point(this.gbxPortSet.Location.X + this.gbxPortSet.Width + 3, this.gbxPortSet.Location.Y);
            this.gbxProtocolSet.Width = this.chkAddErrChk.Location.X + this.chkAddErrChk.Width + 3;
            this.gbxProtocolSet.Height = this.gbxPortSet.Height;
            this.gbxProtocolSet.Text = "Protocol Settings";

            #endregion Protocol 설정
            #region 전송 설정

            #region 반복설정
            
            //반복전송 체크박스
            this.chkRewrite.Location = new Point(3, 15);
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

            this.gbxCommSet.Location = new Point(this.gbxProtocolSet.Location.X + this.gbxProtocolSet.Width + 3, this.gbxProtocolSet.Location.Y);
            this.gbxCommSet.Text = "Comm Settings";
            this.gbxCommSet.Width = this.btnSend.Location.X + this.btnSend.Width + 3;
            this.gbxCommSet.Height = this.btnSend.Location.Y + this.btnSend.Height + 5;

            #endregion 전송 설정

            #region 통신 결과

            //Comm Result
            this.gvCommResult.Location = new Point(3, 18);
            this.gvCommResult.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.gvCommResult.Height = 47;
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

            this.gvProtocolResult.Location = new Point(this.gvCommResult.Location.X + this.gvCommResult.Width + 3, this.gvCommResult.Location.Y);
            this.gvProtocolResult.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            this.gvProtocolResult.Height = this.gvCommResult.Height;
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

            this.gbxLog.Location = new Point(this.gbxPortSet.Location.X, this.gbxPortSet.Location.Y + this.gbxPortSet.Height + 3);
            this.gbxLog.Width = this.ClientSize.Width - 6;
            this.gbxLog.Height = this.ClientSize.Height - (this.gbxLog.Location.Y + 2);
            this.gbxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this._bufferColCount = 15;

            this.gvBuffer.Width = (this._bufferColCount * 25) + 3;
            this.gvBuffer.Location = new Point(this.gbxLog.Width - (this.gvBuffer.Width + 4), this.gvCommResult.Location.Y + this.gvCommResult.Height + 3);
            this.gvBuffer.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.gvBuffer.AutoSize = false;
            this.gvBuffer.DataSource = this._dtBuffer;
            this.gvBuffer.Height = 140;
            this.gvBuffer.RowHeadersVisible = false;
            this.gvBuffer.AllowUserToAddRows = false;
            this.gvBuffer.AllowUserToResizeColumns = false;
            this.gvBuffer.AllowUserToResizeRows = false;

            //Request, Receive Log GridView
            this.gvDataLog.Location = new Point(this.gvCommResult.Location.X, this.gvCommResult.Location.Y + this.gvCommResult.Height + 3);
            this.gvDataLog.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            this.gvDataLog.AutoSize = false;
            this.gvDataLog.DataSource = this._dtDataLog;
            this.gvDataLog.Width = this.gbxLog.Width - (this.gvBuffer.Width + 10);
            this.gvDataLog.Height = this.gbxLog.Height - (this.gvCommResult.Location.Y + this.gvCommResult.Height + 8); 
            this.gvDataLog.RowHeadersVisible = false;
            this.gvDataLog.AllowUserToAddRows = false;
            this.gvDataLog.AllowUserToResizeColumns = false;
            this.gvDataLog.AllowUserToResizeRows = false;


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
            this.txtLog.Location = new Point(this.gvBuffer.Location.X, this.gvBuffer.Location.Y + this.gvBuffer.Height + 3);
            this.txtLog.AutoSize = false;   
            this.txtLog.Width = this.gvBuffer.Width;
            this.txtLog.Height = this.gbxLog.ClientSize.Height - (this.txtLog.Location.Y + 5);
            this.txtLog.BorderStyle = BorderStyle.FixedSingle;
            this.txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = ScrollBars.Vertical;

            
            this.gbxLog.Text = "Log";

            #endregion Log Grid


            this.Controls.Add(this.gbxPortSet);
            this.Controls.Add(this.gbxProtocolSet);
            this.Controls.Add(this.gbxCommSet);
            this.Controls.Add(this.gbxLog);
            this.gbxPortSet.Controls.Add(this.cboPortList);
            this.gbxPortSet.Controls.Add(this.btnConnect);
            this.gbxPortSet.Controls.Add(this.lblConnState);
            this.gbxPortSet.Controls.Add(this.gbxBaudRate);
            this.gbxPortSet.Controls.Add(this.gbxParity);
            this.gbxPortSet.Controls.Add(this.gbxStopBits);
            this.gbxPortSet.Controls.Add(this.gbxDataBits);
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
            this.gbxLog.Controls.Add(this.gvCommResult);
            this.gbxLog.Controls.Add(this.gvProtocolResult);
            this.gbxLog.Controls.Add(this.gvDataLog);
            this.gbxLog.Controls.Add(this.gvBuffer);
            this.gbxLog.Controls.Add(this.txtLog);

            this._port.CommLog += (title, data) =>
            {
                WriteLog(title, data);
            };
            this._port.StackBuff += (title, data) =>
            {
                WriteLog(title, data);
            };
        }

        private void InitDataLogGrid()
        {
            this._dtDataResult.Rows.Clear();
            this._dtProtocolResult.Rows.Clear();
            this._dtDataLog.Rows.Clear();
            this._dtDataLog.Columns.Clear();
            this.gvDataLog.Columns.Clear();
            this.txtLog.Text = string.Empty;

            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn();
            colType.Name = "Type";
            colType.DataPropertyName = "Type";
            colType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colType.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colType.HeaderText = "Type";
            colType.ReadOnly = true;
            colType.Width = 40;

            DataGridViewTextBoxColumn colTime = new DataGridViewTextBoxColumn();
            colTime.DataPropertyName = "Time";
            colTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTime.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colTime.HeaderText = "Time";
            colTime.ReadOnly = true;
            colTime.Width = 130;

            this.gvDataLog.Columns.Add(colType);
            this.gvDataLog.Columns.Add(colTime);

            this._dtDataLog.Columns.Add(new DataColumn("Type", typeof(string)) { DefaultValue = string.Empty });
            this._dtDataLog.Columns.Add(new DataColumn("Time", typeof(string)));

            this._dtDataResult.Rows.Add();
            this._dtProtocolResult.Rows.Add();
            this.gvCommResult.ClearSelection();
            this.gvProtocolResult.ClearSelection();

            this.gvCommResult.EndEdit();
            this.gvDataLog.EndEdit();
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

            this._port.IsRequesting = false;
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

                if (this.chkRewrite.Checked)
                    this.btnSend.Text = "Stop";
                
            }
        }

        private void RequestData(byte[] data)
        {
            if (data == null || data.Length == 0) return;

            this._port.Write(data);
            this._curReq++;
            this._dtDataResult.Rows[0]["TryCount"] = (uint)(this._dtDataResult.Rows[0]["TryCount"]) + 1;
        }

        private void WriteLog(string type, params byte[] data)
        {
            try
            {
                if (this.InvokeRequired)
                    this.Invoke(new BytesLogHandler(WriteLog), type, data);
                else
                {
                    string dataStr = string.Empty;
                    if (data != null)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            dataStr += string.Format(" {0:X2}", data[i]);
                        }
                    }

                    switch (type)
                    {
                        case "After Write":
                            InsertData(type, data);
                            this.txtLog.AppendText(string.Format("Req:{0}\r\n", dataStr));
                            break;
                        case "None Receive":
                        case "Receive Stop":
                        case "Receive Too Long":
                            InsertData(type, data);
                            this._dtDataResult.Rows[0][type] = (uint)(this._dtDataResult.Rows[0][type]) + 1;
                            this.txtLog.AppendText(string.Format("Rcv Timeover:{0}\r\n\r\n", dataStr));
                            break;
                        case "Receive Success":
                            InsertData(type, data);
                            this._dtDataResult.Rows[0]["Success"] = (uint)(this._dtDataResult.Rows[0]["Success"]) + 1;
                            this.txtLog.AppendText(string.Format("Rcv:{0}\r\n\r\n", dataStr));
                            break;
                        case "ErrorCheck Dismatch":
                            InsertData(type, data);
                            this._dtProtocolResult.Rows[0]["ErrChk"] = (uint)(this._dtProtocolResult.Rows[0]["ErrChk"]) + 1;
                            //에러코드 확인
                            {
                                byte[] temp,
                                    calcErrCode = new byte[this._port.ErrorCheck.CheckLen],
                                    getErrCode = new byte[this._port.ErrorCheck.CheckLen];
                                string calcErrStr = string.Empty,
                                    getErrStr = string.Empty;
                                if (this._port.ErrorCheck is ModbusAsciiErrorCheck
                                    || this._port.ErrorCheck is PCLinkErrorCheck)
                                    temp = new byte[data.Length - 2 - this._port.ErrorCheck.CheckLen];
                                else
                                    temp = new byte[data.Length - this._port.ErrorCheck.CheckLen];

                                //받은 Error Code
                                Buffer.BlockCopy(data, temp.Length - 1, getErrCode, 0, getErrCode.Length);
                                for (int i = 0; i < getErrCode.Length; i++)
                                {
                                    getErrStr += string.Format(" {0:X2}", getErrCode[i]);
                                }

                                //계산식 Error Code
                                Buffer.BlockCopy(data, 0, temp, 0, temp.Length);
                                calcErrCode = this._port.ErrorCheck.CreateCheckBytes(temp);
                                for (int i = 0; i < calcErrCode.Length; i++)
                                {
                                    calcErrStr += string.Format(" {0:X2}", calcErrCode[i]);
                                }

                                this.txtLog.AppendText(string.Format("ErrorCheck Dismatch:{0}\r\n" +
                                    "계산된 ErrCode :{1} / 받은 ErrCode :{2}\r\n\r\n",
                                    dataStr, calcErrStr, getErrStr));
                            }
                            break;
                        case "Protocol NG":
                            InsertData(type, data);
                            this._dtProtocolResult.Rows[0]["ProtocolErr"] = (uint)(this._dtProtocolResult.Rows[0]["ProtocolErr"]) + 1;
                            this.txtLog.AppendText(string.Format("Protocol Error:{0}\r\n", dataStr));
                            break;
                        case "Stack Buff":
                            WriteBuffer(data);
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        private void InsertData(string type, byte[] data)
        {
            DataRow dr = this._dtDataLog.NewRow();
            switch (type)
            {
                case "After Write":
                    dr["Type"] = "Req";
                    break;
                case "None Receive":
                case "Receive Stop":
                case "Receive Too Long":
                case "Receive Success":
                case "ErrorCheck Dismatch":
                case "Protocol NG":
                    dr["Type"] = "Rcv";
                    break;
                default: dr["Type"] = type; break;
            }
            dr["Time"] = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");

            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    string colName = string.Format("Col{0}", i);

                    //데이터 길이만큼 Column 추가
                    if (this._dtDataLog.Columns.Contains(colName) == false)
                    {
                        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                        col.DataPropertyName = colName;
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        col.Width = 25;
                        col.HeaderText = (i + 1).ToString();
                        col.ReadOnly = true;

                        this.gvDataLog.Columns.Add(col);

                        this._dtDataLog.Columns.Add(new DataColumn(colName, typeof(string)) { DefaultValue = string.Empty });
                    }

                    dr[colName] = data[i].ToString("X2");
                }
            }

            this._dtDataLog.Rows.Add(dr);

            if (type == "None Receive"
                || type == "Receive Stop"
                || type == "Receive Too Long"
                || type == "ErrorCheck Dismatch"
                || type == "Protocol NG"
                )
            {
                //Error시 Type BackColor 지정
                this.gvDataLog.Rows[this._dtDataLog.Rows.IndexOf(dr)].Cells["Type"].Style.BackColor = Color.Red;
            }

            this.gvDataLog.FirstDisplayedScrollingRowIndex = this.gvDataLog.Rows.Count - 1;
        }

        private void WriteBuffer(byte[] data)
        {
            this._dtBuffer.Clear();
            if (data == null) return;

            DataRow dr = null;
            for (int i = 0; i < data.Length; i++)
            {
                string fieldName = string.Format("col{0}", i % this._bufferColCount);

                if (i % this._bufferColCount == 0)
                {
                    dr = this._dtBuffer.NewRow();

                    this._dtBuffer.Rows.Add(dr);
                }

                dr[fieldName] = data[i].ToString("X2");
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (this.BgWorker.CancellationPending)
                {
                    UpdateUI("EndSending");
                    break;
                }
                else
                {
                    try
                    {
                        if (this._port.IsRequesting)
                        {
                            this._port.Read();
                        }
                        else
                        {
                            if (this._maxReq <= this._curReq)
                            {
                                UpdateUI("EndSending");
                                break;
                            }

                            RequestData(this._recycleData);
                        }

                        System.Threading.Thread.Sleep(200);
                    }
                    catch
                    {

                    }
                }
            }
        }


        private void UpdateUI(string cmd)
        {
            try
            {
                if (this.InvokeRequired)
                {

                    this.Invoke(new UIUpdateHandler(UpdateUI), cmd);
                }
                else
                {
                    switch (cmd)
                    {
                        case "EndSending":
                            this.btnSend.Text = "Send";
                            break;
                    }
                }
            }
            catch
            {

            }
        }
    }
}
