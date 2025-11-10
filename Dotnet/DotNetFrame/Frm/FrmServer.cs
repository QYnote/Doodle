using DotNet.Comm.Protocols;
using DotNet.Comm.Servers;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Frm
{
    public partial class FrmServer : Form
    {
        #region UI Controls

        private SplitContainer pnlSplit = new SplitContainer();
        private GroupBox gbxSetting = new GroupBox();
        private TextBox txtIP_Address = new TextBox();
        private NumericUpDown txtIP_PortNo = new NumericUpDown();

        private ComboBox cboProtocol = new ComboBox();


        private Button btnOpenClose = new Button();
        private TextBox txtServerLog = new TextBox();

        #endregion UI Controls

        private ServerBase _server;
        private ProtocolFrame _protocol;

        private Dictionary<int, object> _regModbus = new Dictionary<int, object>()
        {
            { 0, (Int16)0x0001 },
            { 1, (Int16)0x1672 },
        };
        private byte[] _stackBuffer = null;


        public FrmServer()
        {
            InitializeComponent();
            InitUI();

            this.FormClosing += (s, e) =>
            {
                if (this._server.IsOpen)
                {
                    this._server.Close();
                }
            };
        }

        private void InitUI()
        {
            this.pnlSplit.Dock = DockStyle.Fill;
            this.pnlSplit.Panel1.Padding = this.pnlSplit.Panel2.Padding = new Padding(3);

            #region 설정 Panel

            this.gbxSetting.Dock = DockStyle.Fill;
            this.gbxSetting.Padding = new Padding(3);
            this.gbxSetting.Text = "Server Settings";

            this.txtIP_Address.Location = new Point(3, (int)(this.CreateGraphics().MeasureString(this.gbxSetting.Text, this.gbxSetting.Font).Height) + 3);
            this.txtIP_Address.Text = "127.0.0.1";
            this.txtIP_Address.TextAlign = HorizontalAlignment.Center;
            this.txtIP_Address.KeyPress += DotNet.Utils.Controls.Utils.QYUtils.Event_KeyPress_IP;

            this.txtIP_PortNo.Location = new Point(this.txtIP_Address.Location.X, this.txtIP_Address.Bottom + 3);
            this.txtIP_PortNo.TextAlign = HorizontalAlignment.Center;
            this.txtIP_PortNo.Minimum = 0;
            this.txtIP_PortNo.Maximum = int.MaxValue;
            this.txtIP_PortNo.Value = 5000;
            this.txtIP_PortNo.Width = this.txtIP_Address.Width;

            #endregion 설정 Panel

            this.cboProtocol.Location = new Point(this.txtIP_PortNo.Location.X, this.txtIP_PortNo.Bottom + 3);
            this.cboProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboProtocol.Items.AddRange(new string[]
            {
                "Modbus",
                "TeraHz",
            });

            this.btnOpenClose.Location = new Point(this.cboProtocol.Location.X, this.cboProtocol.Bottom + 3);
            this.btnOpenClose.Text = "Open";
            this.btnOpenClose.Click += BtnOpenClose_Click;

            this.txtServerLog.Dock = DockStyle.Fill;
            this.txtServerLog.ReadOnly = true;
            this.txtServerLog.Multiline = true;
            this.txtServerLog.Width = this.ClientSize.Width - 3;
            this.txtServerLog.Height = this.ClientSize.Height - this.txtServerLog.Location.Y;
            this.txtServerLog.ScrollBars = new ScrollBars();
            this.txtServerLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(this.pnlSplit);
            this.pnlSplit.Panel1.Controls.Add(this.gbxSetting);
            this.gbxSetting.Controls.Add(this.txtIP_Address);
            this.gbxSetting.Controls.Add(this.txtIP_PortNo);
            this.gbxSetting.Controls.Add(this.cboProtocol);
            this.gbxSetting.Controls.Add(this.btnOpenClose);
            this.pnlSplit.Panel2.Controls.Add(this.txtServerLog);
            //this.Controls.Add(this.btnOpenClose);
            //this.Controls.Add(this.txtServerLog);
        }

        private void BtnOpenClose_Click(object sender, EventArgs e)
        {

            if (this.btnOpenClose.Text == "Open")
            {
                //서버 Open 처리
                if ((string)this.cboProtocol.SelectedItem == "Modbus")
                {
                    //Server 종류 설정
                    TCPServer server = new TCPServer();
                    server.IP = this.txtIP_Address.Text;
                    server.PortNo = Convert.ToInt32(this.txtIP_PortNo.Value);
                    server.Log += (msg) => { this.UIUpdate(msg); };
                    server.CreateResponseEvent += Server_CreateResponseEvent_Modbus;

                    this._server = server;

                    //Server Protocol 지정
                    this._protocol = new Modbus(false);
                }
                else if ((string)this.cboProtocol.SelectedItem == "TeraHz")
                {
                    TCPServer server = this.CreateServer_TeraHz();

                    this._server = server;
                }
                else
                {
                    this._protocol = null;
                    this._server = null;
                }

                if (this._server != null)
                {
                    this.txtServerLog.Text = string.Empty;
                    this._server.Open();

                    //UI Update
                    this.btnOpenClose.Text = "Close";
                    this.txtIP_Address.Enabled = false;
                    this.txtIP_PortNo.Enabled = false;
                    this.cboProtocol.Enabled = false;
                }
            }
            else if(this.btnOpenClose.Text == "Close")
            {
                //서버 종료 처리
                if(this._server != null)
                    this._server.Close();

                //UI Update
                this.btnOpenClose.Text = "Open";
                this.txtIP_Address.Enabled = true;
                this.txtIP_PortNo.Enabled = true;
                this.cboProtocol.Enabled = true;

                this._stackBuffer = null;
            }
        }

        delegate void UIHandler(string text);
        private void UIUpdate(string txt)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new UIHandler(UIUpdate), txt);
            else
            {
                this.txtServerLog.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"), txt));
            }
        }

        private string ByteToString(byte[] bytes)
        {
            string str = string.Empty;

            if (bytes != null && bytes.Length != 0)
            {
                foreach (var b in bytes)
                    str += string.Format(" {0:X2}", b);
            }

            return str;
        }

        private byte[] Server_CreateResponseEvent_Modbus(byte[] buffer)
        {
            if (buffer == null) return null;

            //0. Test용 수신 Buffer 표기
            this.UIUpdate(string.Format("buffer: {0}" , ByteToString(buffer)));

            //1. Buffer 보관
            if (this._stackBuffer == null)
                this._stackBuffer = buffer;
            else
            {
                byte[] temp = new byte[this._stackBuffer.Length + buffer.Length];
                Buffer.BlockCopy(this._stackBuffer, 0, temp, 0, this._stackBuffer.Length);
                Buffer.BlockCopy(buffer, 0, temp, this._stackBuffer.Length, buffer.Length);

                this._stackBuffer = temp;
            }


            //2. Reqeust Frame 추출
            if(this._stackBuffer != null)
            {
                byte[] reqFrame = this._protocol.Request_ExtractFrame(this._stackBuffer);
                
                if (reqFrame != null)
                {
                    this.UIUpdate(string.Format("Request Frame: {0}", ByteToString(reqFrame)));
                    //3. Response Frame 생성
                    byte[] resFrame = this._protocol.Request_CreateResponse(reqFrame, this._regModbus);
                    
                    if (resFrame != null)
                    {
                        this.UIUpdate(string.Format("Response Frame: {0}", ByteToString(resFrame)));

                        this._stackBuffer = null;
                        return resFrame;
                    }
                }
            }

            return null;
        }

        #region HY TeraFast Device 역할

        const int SensorCount = 64;
        Int16[] baseValue = null;
        Int16[] caliValue = null;
        bool[] _isVectorUp = null;
        bool allowSend = false;
        bool creatingResponse = false;
        bool _isEditSend = false;

        private TCPServer CreateServer_TeraHz()
        {
            TCPServer server = new TCPServer();
            server.IP = this.txtIP_Address.Text;
            server.PortNo = Convert.ToInt32(this.txtIP_PortNo.Value);
            server.Log += (msg) => { this.UIUpdate(msg); };
            server.PeriodicSendEvent += Server_PeriodicSendEvent_TeraHz;
            server.CreateResponseEvent += Server_CreateResponseEvent_TeraHz;

            this.baseValue = new Int16[SensorCount];
            this.caliValue = new Int16[SensorCount];
            this._isVectorUp = new bool[SensorCount];
            Random rnd = new Random();
            for (int i = 0; i < SensorCount; i++)
            {
                this.baseValue[i] = (Int16)(0x7FFF * (i / (float)SensorCount));
                Int16 rndValue = Convert.ToInt16((rnd.Next(-10000, 10000) / 10000f) * 2000);
                if (this.baseValue[i] + rndValue < 0)
                    this.baseValue[i] = 0;
                else if (this.baseValue[i] + rndValue > 0x7FFF)
                    this.baseValue[i] = 0x7FFF;
                else
                    this.baseValue[i] += rndValue;
                this._isVectorUp[i] = true;

                this.caliValue[i] = (Int16)i;
                if (i == SensorCount - 1)
                    this.caliValue[i] = 0x7FFF - 0x7CCC;
            }

            return server;
        }

        private byte[] Server_PeriodicSendEvent_TeraHz()
        {
            if (this.allowSend == false || this.creatingResponse) return null;

            byte[] returnBuffer = new byte[SensorCount * sizeof(Int16)];
            int addValue = 50;

            for (int i = 0; i < SensorCount; i++)
            {
                if (_isVectorUp[i])
                {
                    if(baseValue[i] + addValue > 0x7FFF)
                    {
                        baseValue[i] = 0x7FFF;
                        _isVectorUp[i] = false;
                    }
                    else
                        baseValue[i] += (Int16)addValue;
                }
                else
                {
                    if (baseValue[i] - addValue <= 0)
                    {
                        baseValue[i] = 0;
                        _isVectorUp[i] = true;
                    }
                    else
                        baseValue[i] -= (Int16)addValue;
                }
            }

            Buffer.BlockCopy(baseValue, 0, returnBuffer, 0, returnBuffer.Length);

            System.Threading.Thread.Sleep(2);

            return returnBuffer;
        }

        private byte[] Server_CreateResponseEvent_TeraHz(byte[] request)
        {
            if (request == null) return null;
            this.creatingResponse = true;

            //0. Test용 수신 Buffer 표기
            this.UIUpdate(string.Format("buffer: {0}", ByteToString(request)));

            //1. Buffer 보관
            if (this._stackBuffer == null)
                this._stackBuffer = request;
            else
            {
                byte[] temp = new byte[this._stackBuffer.Length + request.Length];
                Buffer.BlockCopy(this._stackBuffer, 0, temp, 0, this._stackBuffer.Length);
                Buffer.BlockCopy(request, 0, temp, this._stackBuffer.Length, request.Length);

                this._stackBuffer = temp;
            }

            //2. Reqeust Frame 추출
            if (this._stackBuffer != null)
            {
                if (this._stackBuffer.Length < 6) return null;

                int handle = QYUtils.Find(this._stackBuffer, Encoding.ASCII.GetBytes("TS"));
                if (handle < 0)
                {
                    this._stackBuffer = null;
                    this.UIUpdate($"비정상 Request: {ByteToString(this._stackBuffer)}");

                    this.creatingResponse = false;
                    this._stackBuffer = null;
                    return null;
                }

                byte[] reqBytes = new byte[6];
                Buffer.BlockCopy(this._stackBuffer, handle, reqBytes, 0, reqBytes.Length);
                string reqStr = Encoding.ASCII.GetString(reqBytes);
                string cmdG = reqStr.Substring(0, 3);
                int cmdN = Convert.ToInt32(reqStr.Substring(4, 2));
                string resStr = string.Empty;

                if(cmdG == "TSN")
                {
                    if(cmdN == 0)
                    {
                        resStr = $"TSN,OK,01,{(this._isEditSend ? "0001" : "0000")}";
                    }
                    else if(cmdN == 1)
                    {
                        resStr = "TSN,OK";
                        this.allowSend = true;
                    }
                    else if (cmdN == 2)
                    {
                        resStr = "TSN,OK";
                        this.allowSend = false;
                    }
                    else if (cmdN == 3)
                    {
                        this._isEditSend = true;
                        resStr = "TSN,OK";
                    }
                    else if (cmdN == 4)
                    {
                        this._isEditSend = false;
                        resStr = "TSN,OK";
                    }
                    else if (cmdN == 5)
                    {
                        resStr = $"TSN,OK,{SensorCount.ToString("D4")}";
                        for (int i = 0; i < SensorCount; i++)
                            resStr += string.Format(",{0:X4}", this.caliValue[i]);
                    }
                    else if (cmdN == 6)
                    {
                        resStr = $"TSN,OK,{SensorCount.ToString("D4")}";
                        for (int i = 0; i < SensorCount; i++)
                            resStr += string.Format(",{0:X4}", 0x7FFF - this.caliValue[i]);
                    }
                }
                else if(cmdG == "TST")
                {
                    resStr = "TST,OK";
                }

                if(resStr != string.Empty)
                {
                    byte[] resBytes = Encoding.ASCII.GetBytes(resStr);
                    this.UIUpdate(string.Format("Response Bytes: {0}", ByteToString(resBytes)));

                    this.creatingResponse = false;
                    this._stackBuffer = null;
                    return resBytes;
                }

                //초기화
                this._stackBuffer = null;
            }

            return null;
        }

        #endregion 무한 전송 서버

    }
}
