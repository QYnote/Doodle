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

namespace DotNetFramework.Frm
{
    public partial class FrmServer : Form
    {
        #region UI Controls

        private TextBox txtServerLog = new TextBox();
        private Button btnOpenClose = new Button();

        #endregion UI Controls

        private ServerBase _server;


        public FrmServer()
        {
            InitializeComponent();
            InitUI();
            InitComponent();

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
            this.btnOpenClose.Location = new Point(3, 3);
            this.btnOpenClose.Text = "Open";
            this.btnOpenClose.Click += (s, e) =>
            {
                this.txtServerLog.Text = string.Empty;

                if(this.btnOpenClose.Text == "Open")
                {
                    this._server.Open();

                    this.btnOpenClose.Text = "Close";
                }
                else if(this.btnOpenClose.Text == "Close")
                {
                    this._server.Close();

                    this.btnOpenClose.Text = "Open";
                }
            };

            this.txtServerLog.Location = new Point(this.btnOpenClose.Location.X, this.btnOpenClose.Location.Y + this.btnOpenClose.Height + 3);
            this.txtServerLog.ReadOnly = true;
            this.txtServerLog.Multiline = true;
            this.txtServerLog.Width = this.ClientSize.Width - 3;
            this.txtServerLog.Height = this.ClientSize.Height - this.txtServerLog.Location.Y;
            this.txtServerLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(this.btnOpenClose);
            this.Controls.Add(this.txtServerLog);
        }

        private void InitComponent()
        {
            InfinityWrite();
            this._server.Log += _server_Log;
        }

        private void _server_Log(string msg)
        {
            UIUpdate(msg);
        }

        delegate void UIHandler(string text);
        private void UIUpdate(string txt)
        {
            if (this.InvokeRequired)
                this.Invoke(new UIHandler(UIUpdate), txt);
            else
            {
                this.txtServerLog.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now, txt));
            }
        }

        #region 무한 전송 서버

        private void InfinityWrite()
        {
            this._server = new TCPServer(ServerSendType.WriteRead);
            (this._server as TCPServer).IP = "127.0.0.1";
            (this._server as TCPServer).PortNo = 5000;
            (this._server as TCPServer).ClientActiveEvent += FrmServer_ClientActiveEvent;
        }

        int SensorCount = 64;
        int framewidth = 16;
        bool isSending = true;
        private byte[] FrmServer_ClientActiveEvent(byte[] data)
        {
            if (this.isSending == true)
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
                int startIdx = Array.IndexOf(data, 0x02);
                int endIdx = data.Find(new byte[] { 0x0D, 0x0A }, startIdx);

                byte[] frame = new byte[endIdx - (startIdx + 1)];
                string strFrame = Encoding.ASCII.GetString(frame);

                return null;
            }
        }

        #endregion 무한 전송 서버

    }
}
