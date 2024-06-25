using Dnf.Comm.Controls;
using Dnf.Comm.Controls.PCPorts;
using Dnf.Comm.Data;
using Dnf.Utils.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Comm.Frm
{
    internal partial class FrmPort : Form
    {
        /// <summary>
        /// Form Open 형태, New : 신규생성, Edit : 수정
        /// </summary>
        FrmEditType OpenType { get; set; }
        /// <summary>
        /// Form 내부 Port
        /// </summary>
        ProgramPort frmPort {  get; set; }

        #region Controls

        //저장 정보
        Panel pnlButton = new Panel();
        Button BtnOK = new Button();        //저장    
        Button BtnCancel = new Button();    //취소

        //Port 정보
        Panel pnlControlBox = new Panel();
        ucControlBox cboPortName = new ucControlBox(CtrlType.ComboBox);      //연결된 포트
        ucControlBox cboProtocolType = new ucControlBox(CtrlType.ComboBox);  //통신방법 구분
        ucControlBox cboBaudRate = new ucControlBox(CtrlType.ComboBox);      //BaudRate
        ucControlBox numDataBits = new ucControlBox(CtrlType.Numberic);      //Data Bits
        ucControlBox cboStopBit = new ucControlBox(CtrlType.ComboBox);       //StopBit
        ucControlBox cboParity = new ucControlBox(CtrlType.ComboBox);        //ParityBit

        ucControlBox txtIPaddr = new ucControlBox(CtrlType.TextBox);        //IP
        ucControlBox txtPortNo = new ucControlBox(CtrlType.MaskedTextBox);  //Port번호

        #endregion Controls End

        /// <summary>
        /// Port 관리 Form
        /// </summary>
        /// <param name="port">Edit일 때 수정할 Port</param>
        internal FrmPort(ProgramPort port = null)
        {
            if(port == null)
            {
                this.OpenType = FrmEditType.New;
            }
            else
            {
                this.OpenType = FrmEditType.Edit;
                this.frmPort = port;
            }

            InitializeComponent();
            InitialForm();
        }

        private void InitialForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.Size = new Size(250, 213);
            this.Text = RuntimeData.String("F01");

            InitializeButton();
            InitializeControlBox();
            InitializeDockIndex();

            SetText();
            SetDefaultValue();
            SetVisible();
        }

        /// <summary>
        /// Button Control 생성
        /// </summary>
        private void InitializeButton()
        {
            pnlButton.Dock = DockStyle.Bottom;
            pnlButton.Size = new Size(pnlButton.Width, 30);

            //Button 정의
            BtnOK.Dock = DockStyle.Right;
            BtnCancel.Dock = DockStyle.Right;

            BtnOK.Size = new Size(100, BtnOK.Height);
            BtnCancel.Size = new Size(100, BtnCancel.Height);

            //Button 추가
            pnlButton.Controls.Add(BtnOK);
            pnlButton.Controls.Add(BtnCancel);
            //이벤트
            BtnOK.Click += ClickButton_OK;
            BtnCancel.Click += ClickButton_Cancel;
            //정렬
            BtnCancel.BringToFront();
            BtnOK.BringToFront();

            this.Controls.Add(pnlButton);
        }

        /// <summary>
        /// 조작 Control 생성
        /// </summary>
        private void InitializeControlBox()
        {
            pnlControlBox.Dock = DockStyle.Fill;
            //pnlControlBox.Size = new Size(pnlControlBox.Width, pnlControlBox.Height);
            pnlControlBox.MinimumSize = new Size(pnlControlBox.Width, pnlControlBox.Height);

            //Control 명(Control Type - Item구분 - 담당Property)
            cboPortName.ctrl.Name = "CboPortName";
            cboProtocolType.ctrl.Name = "CboProtocolType";
            cboBaudRate.ctrl.Name = "cboBaudRate";
            numDataBits.ctrl.Name = "numDataBits";
            cboStopBit.ctrl.Name = "cboStopBit";
            cboParity.ctrl.Name = "cboParity";
            txtIPaddr.ctrl.Name = "txtIPaddr";
            txtPortNo.ctrl.Name = "txtPortNo";

            //Items
            (cboPortName.ctrl as ComboBox).Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            (cboProtocolType.ctrl as ComboBox).Items.AddRange(UtilCustom.EnumToItems<uProtocolType>());
            (cboBaudRate.ctrl as ComboBox).Items.AddRange(EnumCustom.BaudRate);
            (cboStopBit.ctrl as ComboBox).Items.AddRange(UtilCustom.EnumToItems<StopBits>());
            (cboParity.ctrl as ComboBox).Items.AddRange(UtilCustom.EnumToItems<Parity>());

            (numDataBits.ctrl as ucNumeric).MaxValue = 8;
            (numDataBits.ctrl as ucNumeric).MinValue = 7;

            (txtPortNo.ctrl as MaskedTextBox).ValidatingType = typeof(short);
            (txtPortNo.ctrl as MaskedTextBox).Mask = "####";

            (txtIPaddr.ctrl as TextBox).KeyPress += UtilCustom.TextBox_IP;

            cboPortName.Dock = DockStyle.Top;
            cboProtocolType.Dock = DockStyle.Top;
            cboBaudRate.Dock = DockStyle.Top;
            numDataBits.Dock = DockStyle.Top;
            cboStopBit.Dock = DockStyle.Top;
            cboParity.Dock = DockStyle.Top;
            txtIPaddr.Dock = DockStyle.Top;
            txtPortNo.Dock = DockStyle.Top;

            //Label Width
            int portLabelWidth = 80;
            cboPortName.LblWidth = portLabelWidth;
            cboProtocolType.LblWidth = portLabelWidth;
            cboBaudRate.LblWidth = portLabelWidth;
            numDataBits.LblWidth = portLabelWidth;
            cboStopBit.LblWidth = portLabelWidth;
            cboParity.LblWidth = portLabelWidth;
            txtIPaddr.LblWidth = portLabelWidth;
            txtPortNo.LblWidth = portLabelWidth;

            (cboProtocolType.ctrl as ComboBox).SelectedValueChanged += (sender, e) => { SetVisible(); };

            Label splitLine1 = UtilCustom.CreateSplitLine(DockStyle.Top);

            pnlControlBox.Controls.Add(splitLine1);
            pnlControlBox.Controls.Add(cboPortName);
            pnlControlBox.Controls.Add(cboProtocolType);
            pnlControlBox.Controls.Add(cboBaudRate);
            pnlControlBox.Controls.Add(numDataBits);
            pnlControlBox.Controls.Add(cboStopBit);
            pnlControlBox.Controls.Add(cboParity);
            pnlControlBox.Controls.Add(txtIPaddr);
            pnlControlBox.Controls.Add(txtPortNo);

            //Control 정렬
            cboProtocolType.BringToFront();
            splitLine1.BringToFront();
            cboPortName.BringToFront();
            cboBaudRate.BringToFront();
            numDataBits.BringToFront();
            cboStopBit.BringToFront();
            cboParity.BringToFront();
            txtIPaddr.BringToFront();
            txtPortNo.BringToFront();

            this.Controls.Add(pnlControlBox);
        }

        /// <summary>
        /// Dock 순서 조정
        /// </summary>
        private void InitializeDockIndex()
        {
            pnlControlBox.BringToFront();
            pnlButton.BringToFront();
        }

        /// <summary>
        /// Controls 기본값 지정
        /// </summary>
        private void SetDefaultValue()
        {
            if(OpenType == FrmEditType.New)
            {
                //Serial 정보
                if((cboPortName.ctrl as ComboBox).Items.Count != 0)
                {
                    (cboPortName.ctrl as ComboBox).SelectedIndex = 0;
                }
                (cboProtocolType.ctrl as ComboBox).SelectedIndex = 0;
                (cboBaudRate.ctrl as ComboBox).SelectedIndex = 0;
                (cboStopBit.ctrl as ComboBox).SelectedIndex = 0;
                (cboParity.ctrl as ComboBox).SelectedIndex = 0;
                (numDataBits.ctrl as ucNumeric).Value = (numDataBits.ctrl as ucNumeric).MaxValue;

                //Ethernet 정보
                (txtPortNo.ctrl as MaskedTextBox).Text = "5000";
                (txtIPaddr.ctrl as TextBox).Text = "127.0.0.1";
            }
            else if(OpenType == FrmEditType.Edit)
            {
                cboProtocolType.Value = frmPort.ProtocolType;

                //Serial Port일경우
                if (frmPort.ProtocolType == uProtocolType.ModBusRTU || frmPort.ProtocolType == uProtocolType.ModBusAscii)
                {
                    PortSerial serial = this.frmPort.PCPort as PortSerial;

                    (cboPortName.ctrl as ComboBox).Text = serial.COMName;
                    cboProtocolType.Value = this.frmPort.ProtocolType;
                    cboBaudRate.Value = serial.BaudRate;
                    cboStopBit.Value = serial.StopBit;
                    cboParity.Value = serial.Parity;
                    (numDataBits.ctrl as ucNumeric).Value = serial.DataBits;
                }
                //Ethernet Port인경우
                else if(frmPort.ProtocolType == uProtocolType.ModBusTcpIp)
                {
                    PortEthernet ethernet = this.frmPort.PCPort as PortEthernet;

                    (txtPortNo.ctrl as MaskedTextBox).Text = ethernet.PortNo.ToString("D4");
                    (txtIPaddr.ctrl as TextBox).Text = ethernet.IP.ToString();
                }
            }
        }

        /// <summary>
        /// Controls Text 지정
        /// </summary>
        private void SetText()
        {
            BtnOK.Text     = RuntimeData.String("F010100");
            BtnCancel.Text = RuntimeData.String("F010101");

            //Label 표기 Text
            cboPortName.LblText     = RuntimeData.String("F010200");
            cboProtocolType.LblText = RuntimeData.String("F010201");
            cboBaudRate.LblText     = RuntimeData.String("F010202");
            numDataBits.LblText     = RuntimeData.String("F010203");
            cboStopBit.LblText      = RuntimeData.String("F010204");
            cboParity.LblText       = RuntimeData.String("F010205");

            txtIPaddr.LblText = RuntimeData.String("F010300");
            txtPortNo.LblText = RuntimeData.String("F010301");
        }

        #region Event

        private void SetVisible()
        {
            //Protocol Type에 따라서 Visible 처리
            uProtocolType type = (uProtocolType)cboProtocolType.Value;

            if (type == uProtocolType.ModBusTcpIp)
            {
                cboPortName.Visible = false;
                cboBaudRate.Visible = false;
                numDataBits.Visible = false;
                cboStopBit.Visible = false;
                cboParity.Visible = false;

                txtIPaddr.Visible = true;
                txtPortNo.Visible = true;
            }
            //Default
            else
            {
                cboPortName.Visible = true;
                cboBaudRate.Visible = true;
                numDataBits.Visible = true; 
                cboStopBit.Visible = true;
                cboParity.Visible = true;

                txtIPaddr.Visible = false;
                txtPortNo.Visible = false;
            }
        }

        /// <summary>
        /// 저장 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickButton_OK(object sender, EventArgs e)
        {
            //Port 중복 확인
            if (ConfirmPort() == false) return;

            //생성 or 변경절차 실행
            if (this.OpenType == FrmEditType.New)
            {
                //신규생성 Process
                uProtocolType protocolType = (uProtocolType)cboProtocolType.Value;

                if (protocolType == uProtocolType.ModBusRTU
                    || protocolType == uProtocolType.ModBusAscii)
                {
                    //Serial Port일경우
                    string portName = cboPortName.Value.ToString();
                    string baudRate = cboBaudRate.Value.ToString();
                    int dataBits = Convert.ToInt32(numDataBits.Value);
                    Parity parity = (Parity)cboParity.Value;
                    StopBits stopBits = (StopBits)cboStopBit.Value;

                    this.frmPort = new ProgramPort(portName, baudRate, dataBits, parity, stopBits);
                    this.frmPort.ProtocolType = protocolType;
                }
                else if (protocolType == uProtocolType.ModBusTcpIp)
                {
                    //TCP Port일경우
                    string ip = (txtIPaddr.ctrl as TextBox).Text;
                    int portNo = int.Parse((txtPortNo.ctrl as MaskedTextBox).Text);

                    this.frmPort = new ProgramPort(ip, portNo);
                    this.frmPort.ProtocolType = protocolType;
                }

                if (frmPort != null)
                {
                    FrmUnit frmUnit = new FrmUnit(FrmEditType.New, frmPort);

                    //정상처리시 Runtime에 추가
                    if (frmUnit.ShowDialog() == DialogResult.OK)
                    {
                        RuntimeData.Ports.Add(this.frmPort.PortName, this.frmPort);
                        this.DialogResult = DialogResult.OK;
                    }
                }
            }
            else if (this.OpenType == FrmEditType.Edit)
            {
                //수정 Process
                string BfPortName = this.frmPort.PortName;
                uProtocolType protocolType = (uProtocolType)cboProtocolType.Value;

                //BasePort가 바뀌는 Protocol로 바뀌었을 경우
                if (CheckBaseProtocolChange() == true)
                {
                    if (protocolType == uProtocolType.ModBusRTU
                        || protocolType == uProtocolType.ModBusAscii)
                    {
                        //Serial Port일경우
                        string portName = cboPortName.Value.ToString();
                        string baudRate = cboBaudRate.Value.ToString();
                        int dataBits = Convert.ToInt32(numDataBits.Value);
                        Parity parity = (Parity)cboParity.Value;
                        StopBits stopBits = (StopBits)cboStopBit.Value;

                        this.frmPort.PCPort = new PortSerial(portName, baudRate, dataBits, parity, stopBits);
                    }
                    else if (protocolType == uProtocolType.ModBusTcpIp)
                    {
                        //TCP Port일경우
                        string ip = (txtIPaddr.ctrl as TextBox).Text;
                        int portNo = int.Parse((txtPortNo.ctrl as MaskedTextBox).Text);

                        this.frmPort.PCPort = new PortEthernet(ip, portNo);
                    }
                }
                else
                {
                    if (protocolType == uProtocolType.ModBusRTU
                        || protocolType == uProtocolType.ModBusAscii)
                    {
                        PortSerial serial = this.frmPort.PCPort as PortSerial;
                        //Serial Port일경우
                        serial.COMName = cboPortName.Value.ToString();
                        serial.BaudRate = cboBaudRate.Value.ToString();
                        serial.DataBits = Convert.ToInt32(numDataBits.Value);
                        serial.Parity = (Parity)cboParity.Value;
                        serial.StopBit = (StopBits)cboStopBit.Value;
                    }
                    else if (protocolType == uProtocolType.ModBusTcpIp)
                    {
                        PortEthernet ethernet = this.frmPort.PCPort as PortEthernet;

                        //TCP Port일경우
                        ethernet.IP = (txtIPaddr.ctrl as TextBox).Text;
                        ethernet.PortNo = int.Parse((txtPortNo.ctrl as MaskedTextBox).Text);
                    }
                }


                UtilCustom.DictKeyChange(RuntimeData.Ports, BfPortName, this.frmPort.PortName);
                this.DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// BaseProtocol 변경 여부
        /// </summary>
        /// <returns>true : 변경안됨 / false : 변경됨</returns>
        private bool CheckBaseProtocolChange()
        {
            uProtocolType BfProtocol = this.frmPort.ProtocolType;   //기존 Protocol
            uProtocolType AfProtocol = (uProtocolType)cboProtocolType.Value;    //변경된 Protocol

            if (BfProtocol == uProtocolType.ModBusRTU
                || BfProtocol == uProtocolType.ModBusAscii)
            {
                if (AfProtocol == uProtocolType.ModBusTcpIp)
                    return true;
                else
                    return false;
            }
            else if(BfProtocol == uProtocolType.ModBusTcpIp)
            {
                if (AfProtocol == uProtocolType.ModBusTcpIp)
                    return false;
                else
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Port 확인 절차
        /// </summary>
        /// <returns>true : OK / false : Error</returns>
        private bool ConfirmPort()
        {
            //사전 검사
            //Port명 중복 검사
            uProtocolType type = (uProtocolType)cboProtocolType.Value;
            string portName = "";

            if(type == uProtocolType.ModBusTcpIp)
            {
                IPAddress ip = IPAddress.Parse((txtIPaddr.ctrl as TextBox).Text);
                ushort no = ushort.Parse((txtPortNo.ctrl as MaskedTextBox).Text);
                portName = ip.ToString() + ":" + no.ToString();
            }
            else
            {
                portName = cboPortName.Value == null ? "" : cboPortName.Value.ToString();
            }

            if (portName == "") return false;

            if (RuntimeData.Ports.ContainsKey(portName))
            {
                MessageBox.Show(RuntimeData.String("F010000"));
                return false;
            }


            return true;
        }

        private void ClickButton_Cancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        #endregion Event End
    }
}
