using Dnf.Communication.Controls;
using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using Dnf.Utils.Views;
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

namespace Dnf.Communication.Frm
{
    internal partial class FrmPort : Form
    {
        /// <summary>
        /// Form Open 형태, New : 신규생성, Edit : 수정
        /// </summary>
        FrmEditType OpenType;
        /// <summary>
        /// Form 내부 Port
        /// </summary>
        Port frmPort;

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
        ucControlBox numDataBits = new ucControlBox(CtrlType.Numberic);     //Data Bits
        ucControlBox cboStopBit = new ucControlBox(CtrlType.ComboBox);       //StopBit
        ucControlBox cboParity = new ucControlBox(CtrlType.ComboBox);        //ParityBit

        ucControlBox txtIPaddr = new ucControlBox(CtrlType.TextBox);        //IP
        ucControlBox txtPortNo = new ucControlBox(CtrlType.MaskedTextBox);  //Port번호

        #endregion Controls End

        /// <summary>
        /// Port 관리 Form
        /// </summary>
        /// <param name="frm">상위 Form</param>
        /// <param name="type">Form 열리는 Type / New, Edit</param>
        /// <param name="port">Edit일 때 수정할 Port</param>
        internal FrmPort(FrmEditType type, Port port = null)
        {
            OpenType = type;
            frmPort = port;

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
            SetEnable();
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
            (cboBaudRate.ctrl as ComboBox).Items.AddRange(UtilCustom.EnumToItems<BaudRate>());
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
                (txtPortNo.ctrl as MaskedTextBox).Text = "0502";
                (txtIPaddr.ctrl as TextBox).Text = "192.168.0.1";
            }
            else if(OpenType == FrmEditType.Edit && frmPort != null)
            {
                cboProtocolType.Value = frmPort.ProtocolType;

                //Serial Port일경우
                if (frmPort.ProtocolType == uProtocolType.ModBusRTU || frmPort.ProtocolType == uProtocolType.ModBusAscii)
                {
                    Custom_SerialPort port = frmPort as Custom_SerialPort;

                    (cboPortName.ctrl as ComboBox).Text = port.PortName;
                    cboProtocolType.Value = port.ProtocolType;
                    cboBaudRate.Value = port.BaudRate;
                    cboStopBit.Value = port.StopBIt;
                    cboParity.Value = port.Parity;
                    (numDataBits.ctrl as ucNumeric).Value = port.DataBits;
                }
                //Ethernet Port인경우
                else if(frmPort.ProtocolType == uProtocolType.ModBusTcpIp)
                {
                    Custom_EthernetPort port = frmPort as Custom_EthernetPort;

                    (txtPortNo.ctrl as MaskedTextBox).Text = port.PortNo.ToString("D4");
                    (txtIPaddr.ctrl as TextBox).Text = port.IPAddr.ToString();
                }
            }
        }

        /// <summary>
        /// Controls 비활성화 처리
        /// </summary>
        private void SetEnable()
        {
            //수정사항일 시 비활성화 처리
            if (OpenType == FrmEditType.Edit)
            {
                uProtocolType protocolType = frmPort.ProtocolType;

                if (protocolType == uProtocolType.ModBusRTU || protocolType == uProtocolType.ModBusAscii)
                {
                    cboPortName.Enabled = false;
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
            Port port = null;
            if (OpenType == FrmEditType.New)
            {
                //Port 생성
                if (ConfirmCreate())
                {
                    port = CreatePort();
                    if (port == null)
                    {
                        MessageBox.Show(RuntimeData.String("F010001"));
                        return;
                    }

                    frmPort = port; 
                }
            }
            else if(OpenType == FrmEditType.Edit && frmPort != null)
            {
                //Port 수정
                EditPort();
            }

            //화면 종료
            if(OpenType == FrmEditType.New && frmPort != null)
            {
                FrmUnit frmUnit = new FrmUnit(FrmEditType.New, frmPort);

                //정상처리시 Runtime에 추가
                if (port != null && frmUnit.ShowDialog() == DialogResult.OK)
                {
                    RuntimeData.Ports.Add(port.PortName, port);
                    this.DialogResult = DialogResult.OK;
                }
            }
            else if(OpenType == FrmEditType.Edit)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Create Port 확인 절차
        /// </summary>
        /// <returns>true : OK / false : Error</returns>
        private bool ConfirmCreate()
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

        /// <summary>
        /// Port 생성하기
        /// </summary>
        /// <returns></returns>
        private Port CreatePort()
        {
            //Port 생성
            Port port = null;

            uProtocolType protocolType = (uProtocolType)cboProtocolType.Value;

            if (protocolType == uProtocolType.ModBusRTU || protocolType == uProtocolType.ModBusAscii)
            {
                //Serial Port일경우
                string portName = cboPortName.Value.ToString();
                BaudRate baudRate = (BaudRate)cboBaudRate.Value;
                int dataBits = Convert.ToInt32(numDataBits.Value);
                StopBits stopBits = (StopBits)cboStopBit.Value;
                Parity parity = (Parity)cboParity.Value;

                port = new Custom_SerialPort(portName, protocolType, baudRate, dataBits, stopBits, parity);
            }
            else if (protocolType == uProtocolType.ModBusTcpIp)
            {
                //TCP Port일경우
                IPAddress ip = IPAddress.Parse((txtIPaddr.ctrl as TextBox).Text);
                ushort portNo = ushort.Parse((txtPortNo.ctrl as MaskedTextBox).Text);

                port = new Custom_EthernetPort(protocolType, ip, portNo);
            }

            return port;
        }

        /// <summary>
        /// Port 수정하기
        /// </summary>
        private void EditPort()
        {
            uProtocolType protocolType = (uProtocolType)cboProtocolType.Value;
            //변경할 Protocol과 이전 Protocol이 같을경우
            if (protocolType == frmPort.ProtocolType)
            {
                if (frmPort.ProtocolType == uProtocolType.ModBusRTU || frmPort.ProtocolType == uProtocolType.ModBusAscii)
                {
                    //Serial Port
                    Custom_SerialPort port = frmPort as Custom_SerialPort;

                    port.BaudRate = (BaudRate)cboBaudRate.Value;
                    port.DataBits = Convert.ToInt32(numDataBits.Value);
                    port.StopBIt = (StopBits)cboStopBit.Value;
                    port.Parity = (Parity)cboParity.Value;
                }
                else if (frmPort.ProtocolType == uProtocolType.ModBusTcpIp)
                {
                    //Ethernet Port
                    Custom_EthernetPort port = frmPort as Custom_EthernetPort;
                    string bfPortName = port.PortName;//변경전 PortName 미리 저장

                    port.IPAddr = IPAddress.Parse((txtIPaddr.ctrl as TextBox).Text);
                    port.PortNo = ushort.Parse((txtPortNo.ctrl as TextBox).Text);
                    port.PortName = port.IPAddr.ToString() + ":" + port.PortNo;

                    //Runtime에 있는 Port Name 수정
                    UtilCustom.DictKeyChange(RuntimeData.Ports, bfPortName, port.PortName);
                }
            }
            //변경할 Protocol과 이전 Protocol이 다를경우
            else
            {
                if (protocolType == uProtocolType.ModBusRTU || protocolType == uProtocolType.ModBusAscii)
                {
                    //Serial Port
                    BaudRate baudRate = (BaudRate)cboBaudRate.Value;
                    int DataBits = Convert.ToInt32(numDataBits.Value);
                    StopBits stopBIt = (StopBits)cboStopBit.Value;
                    Parity parity = (Parity)cboParity.Value;

                    Port port = new Custom_SerialPort(frmPort.PortName, protocolType, baudRate, DataBits, stopBIt, parity);
                    RuntimeData.Ports[frmPort.PortName] = port;
                }
                else if (protocolType == uProtocolType.ModBusTcpIp)
                {
                    //Ethernet Port
                    IPAddress ip = IPAddress.Parse((txtIPaddr.ctrl as TextBox).Text);
                    ushort portNo = ushort.Parse((txtPortNo.ctrl as TextBox).Text);

                    Port port = new Custom_EthernetPort(protocolType, ip, portNo);

                    //Runtime에 있는 Port Name 수정
                    RuntimeData.Ports.Remove(frmPort.PortName);
                    RuntimeData.Ports.Add(port.PortName, port);
                }
            }
        }

        private void ClickButton_Cancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        #endregion Event End
    }
}
