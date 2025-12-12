using DotNet.Comm;
using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using DotNetFrame.ViewModel;
using DotNetFrame.ViewModel.CommTester;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DotNetFrame.ViewModel.CommTester.CommTester;

namespace DotNetFrame.View.CommTester
{
    public partial class FrmCommTester : Form
    {
        public delegate void UIUpdateHandler(string cmd);
        public delegate void BytesLogHandler(string type, params byte[] data);

        #region UI Controls

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

        private int[] _baudrateList = new int[] { 9600, 19200, 38400, 57600, 115200, 921600 };
        private byte[] _databitsList = new byte[] { 7, 8 };
        private DataTable _dtDataLog = new DataTable();
        private DataTable _dtDataResult = new DataTable();
        private DataTable _dtProtocolResult = new DataTable();
        private DataTable _dtBuffer = new DataTable();
        private ViewModel.CommTester.CommTester _tester = new ViewModel.CommTester.CommTester();
        private BackgroundWorker BgWorker = new BackgroundWorker();
        private const int BUFFER_COLUMN_COUNT = 15;
        private const int RESULT_COLUMN_COUNT = 512;

        private QYSerialPort Serial
        {
            get
            {
                if (this._tester != null
                    && this._tester.OSPort is QYSerialPort)
                    return this._tester.OSPort as QYSerialPort;
                else
                    return null;
            }
        }
        private QYEthernet Ethernet
        {
            get
            {
                if (this._tester != null
                    && this._tester.OSPort is QYEthernet)
                    return this._tester.OSPort as QYEthernet;
                else
                    return null;
            }
        }

        public FrmCommTester()
        {
            InitializeComponent();
            InitUI();
            InitText();

            InitPort();
            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += BgWorker_DoWork;
            this.BgWorker.RunWorkerAsync();

            this.FormClosing += (s, e) =>
            {
                if (this._tester.IsOpen)
                {
                    this._tester.Disconnect();
                }
            };
        }

        private void InitUI()
        {
            this.InitUI_Log();
            this.InitUI_Property();
        }

        private void InitUI_Property()
        {
            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;
            pnl.Height = 200;
            pnl.Padding = new Padding(3);

            this.gbxPortSet.Dock = DockStyle.Left;
            this.gbxPortSet.Padding = new Padding(3);
            this.gbxPortSet.Width = 281;
            InitUI_Property_Port(this.gbxPortSet);

            this.gbxProtocolSet.Dock = DockStyle.Left;
            this.gbxProtocolSet.Padding = new Padding(3);
            this.gbxProtocolSet.Width = 110;
            InitUI_Property_Protocol(this.gbxProtocolSet);

            this.gbxCommSet.Dock = DockStyle.Fill;
            this.gbxCommSet.Padding = new Padding(3);
            InitUI_Protperty_Send(this.gbxCommSet);

            pnl.Controls.Add(this.gbxCommSet);
            pnl.Controls.Add(this.gbxProtocolSet);
            pnl.Controls.Add(this.gbxPortSet);
            this.Controls.Add(pnl);
        }

        #region Port 설정

        private void InitUI_Property_Port(GroupBox gbx)
        {
            Panel pnlPort = new Panel();
            pnlPort.Dock = DockStyle.Top;
            pnlPort.Height = 23;

            this.cboPortType.Width = 80;
            this.cboPortType.DataSource = QYUtils.GetEnumItems<CommType>();
            this.cboPortType.ValueMember = "Value";
            this.cboPortType.DisplayMember = "Name";
            this.cboPortType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboPortType.SelectedValueChanged += CboPortType_SelectedValueChanged;

            Panel pnlCommon = new Panel();
            pnlCommon.Dock = DockStyle.Fill;

            this.InitUI_Property_Port_Serial();
            this.InitUI_Property_Port_Ethernet();

            Panel pnlConnect = new Panel();
            pnlConnect.Location = new Point(this.cboPortType.Right + 3, this.cboPortType.Top - 1);
            pnlConnect.Height = pnlPort.Height - 1;

            this.btnConnect.Dock = DockStyle.Left;
            this.btnConnect.Click += BtnConnect_Click;

            this.lblConnState.Dock = DockStyle.Left;
            this.lblConnState.Width = pnlConnect.Height;
            this.lblConnState.BackColor = Color.Red;

            pnlConnect.Controls.Add(this.lblConnState);
            pnlConnect.Controls.Add(this.btnConnect);
            pnlPort.Controls.Add(pnlConnect);
            pnlPort.Controls.Add(this.cboPortType);

            gbx.Controls.Add(this.pnlPort_Serial);
            gbx.Controls.Add(this.pnlPort_Ethernet);
            gbx.Controls.Add(pnlPort);
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (this._tester.IsOpen)
            {
                this._tester.Disconnect();

                this.cboPortType.Enabled = true;
                this.txtEthernetIP.Enabled = true;
                this.cboPortList.Enabled = true;

                this.btnConnect.Text = AppData.Lang("commtester.portproperty.connect.text");
                this.lblConnState.BackColor = Color.Red;
            }
            else
            {
                this._tester.Connect();

                this.cboPortType.Enabled = false;
                this.txtEthernetIP.Enabled = false;
                this.cboPortList.Enabled = false;

                this.btnConnect.Text = AppData.Lang("commtester.portproperty.disconnect.text");
                this.lblConnState.BackColor = Color.Green;
            }
        }

        private void CboPortType_SelectedValueChanged(object sender, EventArgs e)
        {
            CommType type = (CommType)(sender as ComboBox).SelectedValue;

            if(type == CommType.Serial)
            {
                this._tester.PortType = CommType.Serial;

                if (this.cboPortList.Items.Count > 0)
                    this.Serial.PortName = (string)this.cboPortList.Items[0];
                this.Serial.BaudRate = (int)this._baudrateList[0];
                this.Serial.Parity = (Parity)QYUtils.EnumToItems<Parity>()[0];
                this.Serial.StopBits = (StopBits)QYUtils.EnumToItems<StopBits>()[0];
                this.Serial.DataBits = (int)this._databitsList[1];

                this.pnlPort_Serial.Visible = true;
                this.pnlPort_Ethernet.Visible = false;
            }
            else if(type == CommType.Ethernet)
            {
                this._tester.PortType = CommType.Ethernet;

                this.Ethernet.IP = this.txtEthernetIP.Text;
                this.Ethernet.PortNo = Convert.ToInt32(this.txtPortNo.Value);

                this.pnlPort_Serial.Visible = false;
                this.pnlPort_Ethernet.Visible = true;
            }
        }

        private void InitUI_Property_Port_Serial()
        {
            this.pnlPort_Serial.Dock = DockStyle.Fill;

            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;
            pnl.Height = 23;

            this.cboPortList.Dock = DockStyle.Left;
            this.cboPortList.Items.AddRange(SerialPort.GetPortNames());
            this.cboPortList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboPortList.SelectedValueChanged += CboPortList_SelectedValueChanged;

            //BaudRate
            this.gbxBaudRate.Dock = DockStyle.Left;
            this.gbxBaudRate.Width = 80;
            this.gbxBaudRate.Text = "Baudrate";
            foreach (var baudrate in this._baudrateList)
            {
                RadioButton rdo = CreateRdo(baudrate);
                rdo.Text = baudrate.ToString("#,#");
                rdo.TextAlign = ContentAlignment.MiddleRight;
                rdo.CheckedChanged += Rdo_CheckedChanged_BaudRate;

                this.gbxBaudRate.Controls.Add(rdo);
                rdo.BringToFront();
            }

            //Parity
            this.gbxParity.Dock = DockStyle.Left;
            this.gbxParity.Width = 65;
            this.gbxParity.AutoSize = false;
            this.gbxParity.Text = "Parity";
            foreach (Parity parity in Enum.GetValues(typeof(Parity)))
            {
                RadioButton rdo = CreateRdo(parity);
                rdo.CheckedChanged += Rdo_CheckedChanged_Parity;

                this.gbxParity.Controls.Add(rdo);
                rdo.BringToFront();
            }

            //StopBits
            this.gbxStopBits.Dock = DockStyle.Left;
            this.gbxStopBits.Width = 65;
            this.gbxStopBits.AutoSize = false;
            this.gbxStopBits.Text = "StopBits";
            foreach (StopBits stopbit in Enum.GetValues(typeof(StopBits)))
            {
                if (stopbit == StopBits.None
                    || stopbit == StopBits.OnePointFive
                    ) continue;

                RadioButton rdo = CreateRdo(stopbit);
                rdo.CheckedChanged += Rdo_CheckedChanged_StopBits;

                this.gbxStopBits.Controls.Add(rdo);
                rdo.BringToFront();
            }

            //DataBits
            this.gbxDataBits.Dock = DockStyle.Left;
            this.gbxDataBits.Width = 65;
            this.gbxDataBits.AutoSize = false;
            this.gbxDataBits.Text = "DataBits";
            foreach (var databit in this._databitsList)
            {
                RadioButton rdo = CreateRdo(databit);
                rdo.CheckedChanged += Rdo_CheckedChanged_DataBits;

                this.gbxDataBits.Controls.Add(rdo);
                rdo.BringToFront();
            }

            this.pnlPort_Serial.Controls.Add(this.gbxBaudRate);
            this.pnlPort_Serial.Controls.Add(this.gbxParity);
            this.pnlPort_Serial.Controls.Add(this.gbxStopBits);
            this.pnlPort_Serial.Controls.Add(this.gbxDataBits);
            pnl.Controls.Add(this.cboPortList);
            this.pnlPort_Serial.Controls.Add(pnl);

            this.gbxBaudRate.BringToFront();
            this.gbxParity.BringToFront();
            this.gbxStopBits.BringToFront();
            this.gbxDataBits.BringToFront();
        }

        private void CboPortList_SelectedValueChanged(object sender, EventArgs e)
        {
            string name = (string)(sender as ComboBox).SelectedItem;

            if (this._tester.IsOpen == false)
            {
                if (this.Serial == null) return;

                this.Serial.PortName = name;
            }
        }

        private void Rdo_CheckedChanged_BaudRate(object sender, EventArgs e)
        {
            if (this.Serial == null) return;
            RadioButton rdo = sender as RadioButton;
            int baudRate = (int)rdo.Tag;

            this.Serial.BaudRate = baudRate;
        }

        private void Rdo_CheckedChanged_Parity(object sender, EventArgs e)
        {
            if (this.Serial == null) return;
            RadioButton rdo = sender as RadioButton;
            Parity parity = (Parity)rdo.Tag;

            this.Serial.Parity = parity;
        }

        private void Rdo_CheckedChanged_StopBits(object sender, EventArgs e)
        {
            if (this.Serial == null) return;
            RadioButton rdo = sender as RadioButton;
            StopBits stopBits = (StopBits)rdo.Tag;

            this.Serial.StopBits = stopBits;
        }

        private void Rdo_CheckedChanged_DataBits(object sender, EventArgs e)
        {
            if (this.Serial == null) return;
            RadioButton rdo = sender as RadioButton;
            byte databit = (byte)rdo.Tag;

            this.Serial.DataBits = databit;
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

        private void InitUI_Property_Port_Ethernet()
        {
            this.pnlPort_Ethernet.Dock = DockStyle.Fill;
            this.pnlPort_Ethernet.Visible = false;

            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Fill;
            pnl.Width = 100;

            this.lblEthernetIP.TextAlign = ContentAlignment.MiddleLeft;
            this.lblEthernetIP.Width = 80;

            this.txtEthernetIP.Left = this.lblEthernetIP.Right + 3;
            this.txtEthernetIP.Top = this.lblEthernetIP.Top;
            this.txtEthernetIP.TextAlign = HorizontalAlignment.Center;
            this.txtEthernetIP.Text = "127.0.0.1";
            this.txtEthernetIP.KeyPress += QYUtils.Event_KeyPress_IP;
            this.txtEthernetIP.TextChanged += TxtEthernetIP_TextChanged;


            this.lblPortNo.Left = this.lblEthernetIP.Left;
            this.lblPortNo.Top = this.lblEthernetIP.Bottom + 3;
            this.lblPortNo.TextAlign = ContentAlignment.MiddleLeft;
            this.lblPortNo.Width = 80;

            this.txtPortNo.Left = this.lblPortNo.Right + 3;
            this.txtPortNo.Top = this.lblPortNo.Top;
            this.txtPortNo.DecimalPlaces = 0;
            this.txtPortNo.TextAlign = HorizontalAlignment.Right;
            this.txtPortNo.Minimum = 0;
            this.txtPortNo.Maximum = int.MaxValue;
            this.txtPortNo.Value = 5000;
            this.txtPortNo.Width = this.txtEthernetIP.Width;
            this.txtPortNo.ValueChanged += TxtPortNo_ValueChanged;

            pnl.Controls.Add(this.lblEthernetIP);
            pnl.Controls.Add(this.txtEthernetIP);
            pnl.Controls.Add(this.txtPortNo);
            pnl.Controls.Add(this.lblPortNo);
            this.pnlPort_Ethernet.Controls.Add(pnl);
        }

        private void TxtEthernetIP_TextChanged(object sender, EventArgs e)
        {
            if (this.Ethernet == null) return;

            this.Ethernet.IP = (sender as Control).Text;
        }

        private void TxtPortNo_ValueChanged(object sender, EventArgs e)
        {
            if (this.Ethernet == null) return;

            this.Ethernet.PortNo = Convert.ToInt32((sender as NumericUpDown).Value);
        }

        #endregion Port 설정
        #region Protocol 설정

        private void InitUI_Property_Protocol(GroupBox gbx)
        {
            ComboBox cboProtocolList = new ComboBox();
            cboProtocolList.Dock = DockStyle.Top;
            cboProtocolList.DropDownStyle = ComboBoxStyle.DropDownList;
            cboProtocolList.DataSource = QYUtils.GetEnumItems<ProtocolType>();
            cboProtocolList.ValueMember = "Value";
            cboProtocolList.DisplayMember = "Name";
            var items = cboProtocolList.DataSource as QYUtils.EnumItem<ProtocolType>[];
            float maxTextWidth = -1;
            Graphics graphics = cboProtocolList.CreateGraphics();
            for (int i = 0; i < items.Length; i++)
            {
                float width = graphics.MeasureString(items[i].Name, cboProtocolList.Font).Width;

                if (maxTextWidth < width) maxTextWidth = width;
            }
            if (maxTextWidth > 0)
                cboProtocolList.DropDownWidth = (int)maxTextWidth;
            cboProtocolList.SelectedValueChanged += CboProtocolList_SelectedValueChanged;

            this.chkAddErrChk.Dock = DockStyle.Top;
            this.chkAddErrChk.CheckAlign = ContentAlignment.MiddleRight;
            this.chkAddErrChk.Checked = false;
            this.chkAddErrChk.CheckedChanged += ChkAddErrChk_CheckedChanged;

            gbx.Controls.Add(this.chkAddErrChk);
            gbx.Controls.Add(cboProtocolList);
        }

        private void CboProtocolList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (this._tester == null) return;
            ComboBox cbo = sender as ComboBox;
            ProtocolType protocol = (ProtocolType)cbo.SelectedValue;

            this._tester.ProtocolType = protocol;

            if (protocol != ProtocolType.None)
                this.gvProtocolResult.Visible = true;
            else
                this.gvProtocolResult.Visible = false;
        }

        private void ChkAddErrChk_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            this._tester.ErrCode_add = chk.Checked;
        }

        #endregion Protocol 설정
        #region 전송 설정

        private void InitUI_Protperty_Send(GroupBox gbx)
        {
            InitUI_Property_Send_Repeat();

            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Bottom;
            pnl.Height = 45;

            this.lblWriteTooltip.Dock = DockStyle.Top;
            this.lblWriteTooltip.TextAlign = ContentAlignment.MiddleLeft;

            this.btnSend.Dock = DockStyle.Right;
            this.btnSend.Width = 60;
            this.btnSend.Click += BtnSend_Click;

            this.txtWrite.Dock = DockStyle.Fill;

            pnl.Controls.Add(this.txtWrite);
            pnl.Controls.Add(this.btnSend);
            pnl.Controls.Add(this.lblWriteTooltip);

            gbx.Controls.Add(pnl);
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            InitDataLogGrid();

            this._tester.Register_Data(this.txtWrite.Text);
        }

        private void InitUI_Property_Send_Repeat()
        {
            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;
            pnl.Height = 78;

            this.chkRewrite.CheckAlign = ContentAlignment.MiddleRight;
            this.chkRewrite.Checked = false;
            this.chkRewrite.Width = 100;
            this.chkRewrite.CheckedChanged += ChkRewrite_CheckedChanged;

            //반복전송 횟수
            this.lblRewrite.Left = this.chkRewrite.Left;
            this.lblRewrite.Top = this.chkRewrite.Bottom + 3;
            this.lblRewrite.Width = 80;
            this.lblRewrite.TextAlign = ContentAlignment.MiddleLeft;

            this.numRewrite.Left = this.lblRewrite.Right + 3;
            this.numRewrite.Top = this.lblRewrite.Top;
            this.numRewrite.Width = 60;
            this.numRewrite.Minimum = 2;
            this.numRewrite.Enabled = false;
            this.numRewrite.Value = 3;
            this.numRewrite.ValueChanged += NumRewrite_ValueChanged;

            this.lblRewriteUnit.Left = this.numRewrite.Right + 3;
            this.lblRewriteUnit.Top = this.numRewrite.Top;
            this.lblRewriteUnit.Width = 20;
            this.lblRewriteUnit.TextAlign = ContentAlignment.MiddleLeft;

            //무한전송
            this.chkRewriteInfi.Left = this.lblRewrite.Left;
            this.chkRewriteInfi.Top = this.lblRewrite.Bottom + 3;
            this.chkRewriteInfi.Width = 100;
            this.chkRewriteInfi.CheckAlign = ContentAlignment.MiddleRight;
            this.chkRewriteInfi.Checked = false;
            this.chkRewriteInfi.Enabled = false;
            this.chkRewriteInfi.CheckedChanged += ChkRewriteInfi_CheckedChanged;

            pnl.Controls.Add(this.lblRewriteUnit);
            pnl.Controls.Add(this.numRewrite);
            pnl.Controls.Add(this.lblRewrite);
            pnl.Controls.Add(this.chkRewrite);
            pnl.Controls.Add(this.chkRewriteInfi);
            this.gbxCommSet.Controls.Add(pnl);
        }

        private void ChkRewrite_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            this._tester.Do_repeat = chk.Checked;

            this.numRewrite.Enabled = chk.Checked;
            this.chkRewriteInfi.Enabled = chk.Checked;
        }

        private void NumRewrite_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._tester.Do_repeat_count = Convert.ToInt32(num.Value);
        }

        private void ChkRewriteInfi_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            this._tester.Do_repeat_infinity = chk.Checked;
            this.numRewrite.Enabled = !chk.Checked;
        }

        #endregion 전송 설정

        private void InitUI_Log()
        {
            this.gbxLog.Dock = DockStyle.Fill;

            this.InitUI_Log_Result();

            Panel pnlRight = new Panel();
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Width = 378;

            this.InitUI_Log_Buffer(pnlRight);
            this.InitUI_Log_Text(pnlRight);

            this.InitUI_Log_Data();

            this.gbxLog.Controls.Add(pnlRight);
            this.Controls.Add(this.gbxLog);
        }

        #region 통신결과 수

        private void InitUI_Log_Result()
        {
            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;
            pnl.Height = 47;

            this.InitUI_Log_Result_Comm();
            this.InitUI_Log_Result_Protocol();

            pnl.Controls.Add(this.gvCommResult);
            pnl.Controls.Add(this.gvProtocolResult);
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
            this.gvProtocolResult.Visible = false;

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

        #endregion 통신결과 수
        #region 통신결과 Data

        private void InitUI_Log_Buffer(Panel pnl)
        {
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

            pnl.Controls.Add(this.gvBuffer);
        }

        private void InitUI_Log_Text(Panel pnl)
        {
            this.txtLog.Dock = DockStyle.Fill;
            this.txtLog.AutoSize = false;
            this.txtLog.BorderStyle = BorderStyle.FixedSingle;
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = ScrollBars.Vertical;

            pnl.Controls.Add(this.txtLog);
        }

        private void InitUI_Log_Data()
        {
            this._dtDataLog.Columns.Add(new DataColumn("Type", typeof(string)) { DefaultValue = string.Empty });
            this._dtDataLog.Columns.Add(new DataColumn("Time", typeof(string)));

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

        #endregion 통신결과 Data

        private void InitText()
        {
            this.gbxPortSet.Text = AppData.Lang("commtester.portproperty.title");
            this.btnConnect.Text = AppData.Lang("commtester.portproperty.connect.text");

            this.lblEthernetIP.Text = AppData.Lang("commtester.portproperty.ethernet.ip.title");
            this.lblPortNo.Text = AppData.Lang("commtester.portproperty.ethernet.portno.title");

            this.gbxProtocolSet.Text = AppData.Lang("commtester.protocolproperty.title");
            this.chkAddErrChk.Text = AppData.Lang("commtester.protocolproperty.errorcode.create.title");

            this.gbxCommSet.Text = AppData.Lang("commtester.commproperty.title");
            this.chkRewrite.Text = AppData.Lang("commtester.commproperty.repeat.enable.title");
            this.lblRewrite.Text = AppData.Lang("commtester.commproperty.repeat.count.title");
            this.lblRewriteUnit.Text = AppData.Lang("commtester.commproperty.repeat.countunit.text");
            this.chkRewriteInfi.Text = AppData.Lang("commtester.commproperty.repeat.infinity.title");
            this.lblWriteTooltip.Text = AppData.Lang("commtester.commproperty.tooltip.text");
            this.btnSend.Text = AppData.Lang("commtester.commproperty.send.text");


            this.gbxLog.Text = AppData.Lang("commtester.log.title");
            this.gvCommResult.Columns["TryCount"].HeaderText = AppData.Lang("commtester.log.commresult.trycount");
            this.gvCommResult.Columns["Success"].HeaderText = AppData.Lang("commtester.log.commresult.success");
            this.gvCommResult.Columns["None Receive"].HeaderText = AppData.Lang("commtester.log.commresult.none");
            this.gvCommResult.Columns["Receive Stop"].HeaderText = AppData.Lang("commtester.log.commresult.stop");
            this.gvCommResult.Columns["Receive Too Long"].HeaderText = AppData.Lang("commtester.log.commresult.infinity");

            this.gvProtocolResult.Columns["ErrChk"].HeaderText = AppData.Lang("commtester.log.protocolresult.errorcode");
            this.gvProtocolResult.Columns["ProtocolErr"].HeaderText = AppData.Lang("commtester.log.protocolresult.protocolerror");
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
            if (this._tester.PortType == CommType.Serial)
            {
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
            else if(this._tester.PortType == CommType.Ethernet)
            {
                this.txtEthernetIP.Text = "192.168.2.133";
                this.txtPortNo.Value = 0502;
            }

            this._tester.AppPort.ComPortLog += this.UpdateUI_ComPortLog;
            this._tester.AfterSendRequest += this.UpdateUI_AfterSendRequest;
            this._tester.PortCurrentBuffer += this.UpdateUI_PortCurrentBuffer;
            this._tester.Error_ErrorCode += this.UpdateUI_Error_ErrorCode;
            this._tester.Error_Protocol += this.UpdateUI_Error_Protocol;
            this._tester.RequestComplete += this.UpdateUI_RequestComplete;
            this._tester.RequestTimeout += this.UpdateUI_RequestTimeout;
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
                        if (this._tester.IsWriting)
                            UpdateUI("Sending");
                        else
                            UpdateUI("SendComplete");
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
                DotNetFrame.ViewModel.CommTester.CommFrame frame = obj[0] as DotNetFrame.ViewModel.CommTester.CommFrame;
                //송신 Grid Log Update
                DataRow dr = this._dtDataLog.NewRow();
                dr["Type"] = "Req";
                dr["Time"] = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");
                for (int i = 0; i < frame.ReqBytes.Length; i++)
                {
                    if ((i != 0) && ((i % RESULT_COLUMN_COUNT) == 0))
                    {
                        this._dtDataLog.Rows.Add(dr);
                        dr = this._dtDataLog.NewRow();
                    }

                    dr[string.Format("Col{0}", i)] = frame.ReqBytes[i].ToString("X2");
                }
                this._dtDataLog.Rows.Add(dr);

                //시도횟수 증가
                this._dtDataResult.Rows[0]["TryCount"] = frame.TryCount;

                //TextLog Update
                this.txtLog.AppendText($"Req:{ByteToString(frame.ReqBytes)}\r\n");
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
                    if (obj[0] as string == "Sending")
                        this.btnSend.Text = AppData.Lang("commtester.commproperty.stop.text");
                    else if(obj[0] as string == "SendComplete")
                        this.btnSend.Text = AppData.Lang("commtester.commproperty.send.text");
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
                if ((i != 0) && ((i % RESULT_COLUMN_COUNT) == 0))
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
