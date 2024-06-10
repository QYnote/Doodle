using Dnf.Communication.Controls;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Communication.Frm
{
    internal partial class Frm_LogCommunication : TabPageBase
    {
        Port TestPort { get; set; }

        #region Controls

        ucControlBox CboPort = new ucControlBox(CtrlType.ComboBox);
        ucControlBox TxtSendData = new ucControlBox(CtrlType.TextBox);
        ucControlBox TxtLog = new ucControlBox(CtrlType.TextBox);
        Button BtnSend = new Button();
        Button BtnClear = new Button();

        #endregion Controls End

        internal Frm_LogCommunication(string PageName, string Caption) : base(PageName, Caption)
        {
            InitializeComponent();
            InitializeControl();

            base.BeforeRemovePageHandler += PageRemove;
        }

        internal void InitializeControl()
        {
            CboPort.Location = new Point(3, 3);
            CboPort.Size = new Size(250, 30);
            CboPort.LblWidth = 100;
            CboPort.LblText = "Port";
            (CboPort.ctrl as ComboBox).Items.AddRange(RuntimeData.Ports.Keys.ToArray());
            (CboPort.ctrl as ComboBox).SelectedIndexChanged += PortSelectChanged;

            TxtSendData.Location = new Point(3, CboPort.Location.Y + CboPort.Height + 3);
            TxtSendData.Size = new Size(400, 30);
            TxtSendData.LblWidth = 100;
            TxtSendData.LblText = "전송 데이터";

            BtnSend.Location = new Point(3, TxtSendData.Location.Y + TxtSendData.Height + 3);
            BtnSend.Size = new Size(80, 30);
            BtnSend.Text = "데이터 전송";

            BtnClear.Location = new Point(BtnSend.Location.X + BtnSend.Width + 3, BtnSend.Location.Y);
            BtnClear.Size = new Size(80, 30);
            BtnClear.Text = "Clear";

            TxtLog.Location = new Point(3, BtnSend.Location.Y + BtnSend.Height + 3);
            TxtLog.Size = new Size(this.ClientSize.Width - 6, 300);
            TxtLog.LblWidth = 100;
            TxtLog.LblText = "데이터 로그";
            TxtLog.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TextBox TxtLogCtrl = TxtLog.ctrl as TextBox;
            TxtLogCtrl.ReadOnly = true;
            TxtLogCtrl.Multiline = true;

            this.Controls.Add(CboPort);
            this.Controls.Add(TxtSendData);
            this.Controls.Add(BtnSend);
            this.Controls.Add(BtnClear);
            this.Controls.Add(TxtLog);

            BtnSend.Click += DataSend;
            BtnClear.Click += DataClear;
        }

        private void PortSelectChanged(object sender, EventArgs e)
        {
            ComboBox cbo = sender as ComboBox;

            //기존Port 이벤트 삭제
            if(this.TestPort != null)
            {
                if(this.TestPort.IsOpen == true)
                {
                    this.TestPort.Close();
                }
                this.TestPort.PortLogHandler -= WriteLog;
            }

            //변경Port 설정
            this.TestPort = RuntimeData.Ports[cbo.SelectedItem.ToString()];
            this.TestPort.PortLogHandler += WriteLog;
        }

        private void DataSend(object sender, EventArgs e)
        {
            if(this.TestPort.IsUserOpen == false)
            {
                this.TestPort.Open();
            }

            string sendText = TxtSendData.Value.ToString().Trim();

            if (this.TestPort.IsOpen == true)
            {
                if (sendText == "") return;

                CommFrame data = new CommFrame();
                data.ReqDataBytes = Encoding.UTF8.GetBytes(sendText);
                TestPort.SendingQueue.Enqueue(data);

                string str = string.Empty;
                foreach (byte b in data.ReqDataBytes)
                {
                    str += b + " ";
                }

                WriteLog(string.Format("Send Message byte : {0}", str));
            }

        }

        private void DataClear(object sender, EventArgs e)
        {
            if (this.TestPort.IsOpen == true)
            {
                this.TestPort.Close();
            }

            TxtLog.Value = string.Empty;
        }

        private delegate void SetLogUI(string Msg);
        private void WriteLog(string Msg)
        {
            if (this.InvokeRequired)
                this.Invoke(new SetLogUI(WriteLog), new object[] { Msg });
            else
            {
                TextBox TxtLogCtrl = TxtLog.ctrl as TextBox;
                if (TestPort != null)
                {
                    TxtLogCtrl.AppendText(string.Format("{0} - {1} - {2}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), this.TestPort.PortName, Msg));
                }
            }
        }

        private void PageRemove()
        {
            if (this.TestPort != null)
            {
                this.TestPort.PortLogHandler -= WriteLog;
            }

            if (TestPort.IsOpen == true)
            {
                TestPort.Close();
            }
        }
    }
}
