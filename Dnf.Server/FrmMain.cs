using Dnf.Server.Server;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Server
{
    public partial class FrmMain : Form
    {
        #region Controls

        private ToolStrip TextMenu = new ToolStrip();   //상단 아이콘 메뉴
        private ToolStripMenuItem TextMenu_Option = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_Option_Settings = new ToolStripMenuItem();

        private ToolStrip IconMenu = new ToolStrip();   //상단 아이콘 메뉴
        private ToolStripButton IconMenu_Connect_Open = new ToolStripButton();
        private ToolStripButton IconMenu_Connect_Close = new ToolStripButton();

        private TextBox TxtLog = new TextBox();

        #endregion Controls

        ServerBase server = null;
        private bool ServerUserOpen = false;
        internal string ServerType = "TCP Server";

        public FrmMain()
        {
            InitializeComponent();
            InitContolBase();

            this.Text = "서버 생성기";
        }

        private void InitContolBase()
        {
            InitTextMenu();
            InitIconMenu();

            TxtLog.Location = new Point(0, 64);
            TxtLog.AutoSize = false;
            TxtLog.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 64);
            TxtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TxtLog.ReadOnly = true;
            TxtLog.BorderStyle = BorderStyle.FixedSingle;
            TxtLog.Text = "FrmOpen";
            TxtLog.Multiline = true;

            this.Controls.Add(IconMenu);
            this.Controls.Add(TextMenu);
            this.Controls.Add(TxtLog);
        }

        private void InitTextMenu()
        {
            TextMenu_Option.Name = "TextMenu_Option";
            TextMenu_Option_Settings.Name = "TextMenu_Option_Settings";

            TextMenu_Option.Text = "옵션";
            TextMenu_Option_Settings.Text = "설정";

            TextMenu_Option.DropDownItems.AddRange(new ToolStripItem[] { TextMenu_Option_Settings });
            TextMenu.Items.AddRange(new ToolStripItem[] {
                TextMenu_Option
                //new ToolStripSeparator(),
                //new ToolStripSeparator(),
            });

            TextMenu_Option_Settings.Click += (sender, e) => { new FrmSettings(this).ShowDialog(); };
        }

        private void InitIconMenu()
        {
            IconMenu.ImageScalingSize = new Size(32, 32);

            IconMenu_Connect_Open.Name = "IconMenu_Connect_Open";
            IconMenu_Connect_Close.Name = "IconMenu_Connect_Close";

            IconMenu_Connect_Open.DisplayStyle = ToolStripItemDisplayStyle.Image;
            IconMenu_Connect_Close.DisplayStyle = ToolStripItemDisplayStyle.Image;

            IconMenu_Connect_Open.Image = Dnf.Utils.Properties.Resources.Connect_Green_32x32;
            IconMenu_Connect_Close.Image = Dnf.Utils.Properties.Resources.Connect_Red_32x32;

            IconMenu.Items.AddRange(new ToolStripItem[] {
                IconMenu_Connect_Open,
                IconMenu_Connect_Close,
                new ToolStripSeparator()
            });

            IconMenu_Connect_Close.Visible = false;

            IconMenu_Connect_Open.Click += (sender, e) => { ServerOpen(); };
            IconMenu_Connect_Close.Click += (sender, e) => { ServerClose(); };
        }

        private void ServerOpen()
        {
            if (ServerUserOpen == true)
            {
                return;
            }

            //Open Server 구분

            if (ServerType == "TCP Server")
            {
                this.server = new TCPServer(new System.Net.Sockets.TcpListener(IPAddress.Parse("127.0.0.1"), 5000));
                (this.server as TCPServer).ReceiveActiveEvent += DataReceive;  //Receive 이벤트 지정
                this.server.SendMsg += (msg) => { UpdateUI("ServerLog", new object[] { msg }); };
            }

            //없는 서버타입이면 취소
            if (this.server == null) return;

            this.server.Open();

            ServerUserOpen = true;
            IconMenu_Connect_Open.Visible = false;
            IconMenu_Connect_Close.Visible = true;
        }

        private void ServerClose()
        {
            if (ServerUserOpen == false)
            {
                return;
            }

            this.server.Close();

            //후처리
            ServerUserOpen = false;
            IconMenu_Connect_Open.Visible = true;
            IconMenu_Connect_Close.Visible = false;
        }

        private delegate void UpdateUIdelegate(string type, object[] obj);
        private void UpdateUI(string type, object[] obj = null)
        {
            if (this.InvokeRequired)
                this.Invoke(new UpdateUIdelegate(UpdateUI), new object[] { type, obj });
            else
            {
                if (type == "ReceiveLog")
                {
                    //데이터 Log 기록
                    TxtLog.AppendText(string.Format("\r\nData Receive : {0}", obj[0]));
                    TxtLog.AppendText(string.Format("\r\nData Send : {0}", obj[1]));
                }
                else if (type == "ServerLog")
                {
                    TxtLog.AppendText(string.Format("\r\n{0} ServerLog - {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), obj[0]));

                }
            }
        }

        byte[] StackBytes = new byte[0];
        /// <summary>
        /// 데이터 Receive 이벤트
        /// </summary>
        /// <param name="readBytes">읽어들인 Bytes</param>
        /// <param name="bytesLength">읽은 Bytes 길이</param>
        /// <returns>보낸 Client에게 보내줄 Data</returns>
        private byte[] DataReceive(byte[] readBytes, int bytesLength)
        {
            byte[] writeBytes = null;

            StackBytes.BytesAppend(readBytes);

            UpdateUI("ReceiveLog", new object[] { readBytes, writeBytes });
            return writeBytes;
        }

    }
}
