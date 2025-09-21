using DotNet.Comm.Protocols;
using DotNet.Server.Servers;
using DotNet.Utils.Controls;
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
            this.txtIP_Address.KeyPress += QYUtils.TextBox_IP;

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

        #region HY Device

        private void HYDevice()
        {
            this._server = new TCPServer();
            (this._server as TCPServer).IP = "127.0.0.1";
            (this._server as TCPServer).PortNo = 5000;
            (this._server as TCPServer).CreateResponseEvent += ClientActiveEvent_HYDevice; ;
            this._protocol = new DotNet.Comm.Protocols.Customs.HYNux.HYModbus(false);
            (this._protocol as DotNet.Comm.Protocols.Customs.HYNux.HYModbus).IsTCP = true;
        }


        Dictionary<int, object> Register = new Dictionary<int, object>();
        private byte[] ClientActiveEvent_HYDevice(byte[] data)
        {
            byte[] req = this._protocol.Request_ExtractFrame(data);
            //byte[] res = this._protocol.CreateResponse(Register, req);

            return null;
        }

        #endregion End HY Device
        #region HY TeraFast Device 역할

        int SensorCount = 64;
        int framewidth = 8;
        

        private void InfinityWrite()
        {
            this._server = new TCPServer();
            (this._server as TCPServer).IP = "127.0.0.1";
            (this._server as TCPServer).PortNo = 5000;
            (this._server as TCPServer).CreateResponseEvent += FrmServer_ClientActiveEvent;
            this._protocol = new DotNet.Comm.Protocols.Customs.HYNux.PCLink(false);
        }

        private byte[] FrmServer_ClientActiveEvent(byte[] data)
        {
            if (data == null)
            {
                byte[] returnBytes = new byte[SensorCount * framewidth * sizeof(short)];
                short[] shortFrameLength = new short[SensorCount * framewidth];

                Random rnd = new Random();

                for (int i = 0; i < SensorCount; i++)
                {
                    for (int j = 0; j < framewidth; j++)
                    {
                        float value = (0x7FFF * (i / (float)SensorCount)) * (rnd.Next(9000, 11000) / 10000f);
                        if (value < 0) value = 0;
                        else if (value > 0x7FFF) value = 0x7FFF;

                        shortFrameLength[i + (j * SensorCount)] = Convert.ToInt16(value);
                    }
                }

                Buffer.BlockCopy(shortFrameLength, 0, returnBytes, 0, returnBytes.Length);

                System.Threading.Thread.Sleep(5);

                return returnBytes;
            }
            else
            {
                DotNet.Comm.Protocols.Customs.HYNux.PCLink pcLink = this._protocol as DotNet.Comm.Protocols.Customs.HYNux.PCLink;

                int startIdx = -1;
                if (startIdx < 0) return null;
                int endIdx = data.Find(pcLink.TailBytes, startIdx);

                byte[] frame = new byte[endIdx - (startIdx + 1)];
                Buffer.BlockCopy(data, 1, frame, 0, frame.Length);
                string strFrame = Encoding.ASCII.GetString(frame);
                string cmd = strFrame.Substring(0, strFrame.IndexOf(','));

                if(cmd == "FrameLength")
                {
                    int valueIdx = strFrame.IndexOf(',') + 1;
                    //framewidth = Convert.ToInt32(strFrame.Substring(valueIdx, strFrame.Length - valueIdx));

                    byte[] cmdByte = Encoding.ASCII.GetBytes(string.Format("{0},OK", cmd));
                    byte[] returnFrame = new byte[cmdByte.Length + 3];

                    Buffer.BlockCopy(new byte[] { 0x02 }, 0, returnFrame, 0, 1);
                    Buffer.BlockCopy(cmdByte, 0, returnFrame, 1, cmdByte.Length);
                    Buffer.BlockCopy(new byte[] { 0x0D, 0x0A }, 0, returnFrame, cmdByte.Length + 1, 2);

                    return null;
                }

                return null;
            }
        }

        #endregion 무한 전송 서버

    }
}
