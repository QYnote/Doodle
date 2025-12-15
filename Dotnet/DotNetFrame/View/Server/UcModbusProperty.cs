using DotNet.Utils.Controls.Utils;
using DotNetFrame.ViewModel;
using DotNetFrame.ViewModel.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.View.Server
{
    public partial class UcModbusProperty : UserControl
    {
        #region UI Controls

        private Label lbl_Server_IP = new Label();
        private TextBox txt_Server_IP = new TextBox();
        private Label lbl_Server_PortNo = new Label();
        private NumericUpDown num_Server_PortNo = new NumericUpDown();

        private Button btnConnection = new Button();

        private TextBox txtLog = null;

        #endregion UI Controls

        private VM_Server_Modbus _modbus = new VM_Server_Modbus();

        public UcModbusProperty(TextBox txtLog)
        {
            this.txtLog = txtLog;

            InitializeComponent();
            InitUI();
            this.InitText();

            this._modbus.ServerLog += ServerLog;
            this.VisibleChanged += UcModbusProperty_VisibleChanged;
        }

        private void InitUI()
        {
            this.Dock = DockStyle.Top;
            this.Hide();

            this.lbl_Server_IP.Location = new Point(3, 3);
            this.lbl_Server_IP.TextAlign = ContentAlignment.MiddleLeft;
            this.txt_Server_IP.Left = this.lbl_Server_IP.Right + 3;
            this.txt_Server_IP.Top = this.lbl_Server_IP.Top;
            this.txt_Server_IP.Width = 80;
            this.txt_Server_IP.TextAlign = HorizontalAlignment.Center;
            this.txt_Server_IP.Text = this._modbus.IP;
            this.txt_Server_IP.KeyPress += QYUtils.Event_KeyPress_IP;
            this.txt_Server_IP.TextChanged += TxtIP_TextChanged;

            this.lbl_Server_PortNo.Left = this.lbl_Server_IP.Left;
            this.lbl_Server_PortNo.Top = this.lbl_Server_IP.Bottom + 3;
            this.lbl_Server_PortNo.TextAlign = ContentAlignment.MiddleLeft;
            this.num_Server_PortNo.Left = this.lbl_Server_PortNo.Right + 3;
            this.num_Server_PortNo.Top = this.lbl_Server_PortNo.Top;
            this.num_Server_PortNo.Width = this.txt_Server_IP.Width;
            this.num_Server_PortNo.DecimalPlaces = 0;
            this.num_Server_PortNo.TextAlign = HorizontalAlignment.Right;
            this.num_Server_PortNo.Minimum = 0;
            this.num_Server_PortNo.Maximum = int.MaxValue;
            this.num_Server_PortNo.Value = this._modbus.PortNo;
            this.num_Server_PortNo.ValueChanged += NumPortNo_ValueChanged;

            this.btnConnection.Left = this.lbl_Server_PortNo.Left;
            this.btnConnection.Top = this.lbl_Server_PortNo.Bottom + 3;
            this.btnConnection.TextAlign = ContentAlignment.MiddleCenter;
            this.btnConnection.Tag = false;
            this.btnConnection.Click += BtnConnection_Click;

            this.Height = this.btnConnection.Bottom + 3;

            this.Controls.Add(this.lbl_Server_IP);
            this.Controls.Add(this.txt_Server_IP);
            this.Controls.Add(this.lbl_Server_PortNo);
            this.Controls.Add(this.num_Server_PortNo);
            this.Controls.Add(this.btnConnection);
        }

        private void InitText()
        {
            this.lbl_Server_IP.Text = AppData.Lang("server.modbus.ip");
            this.lbl_Server_PortNo.Text = AppData.Lang("server.modbus.portno");
            this.btnConnection.Text = AppData.Lang("server.modbus.connection.connect");
        }

        private void TxtIP_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;

            this._modbus.IP = txt.Text;
        }

        private void NumPortNo_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._modbus.PortNo = Convert.ToInt32(num.Value);
        }
        private void BtnConnection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if ((bool)btn.Tag)
            {
                this._modbus.Close();

                this.txt_Server_IP.Enabled = true;
                this.num_Server_PortNo.Enabled = true;
                btn.Text = AppData.Lang("server.modbus.connection.connect");

                btn.Tag = false;
            }
            else
            {
                this.txtLog.Text = string.Empty;

                this._modbus.Open();

                this.txt_Server_IP.Enabled = false;
                this.num_Server_PortNo.Enabled = false;
                btn.Text = AppData.Lang("server.modbus.connection.disconnect");

                btn.Tag = true;
            }
        }

        private void ServerLog(params object[] obj)
        {
            if (obj[0] is string == false || this.IsDisposed) return;

            if (this.InvokeRequired)
                this.BeginInvoke(new Update_WithParam(ServerLog), new object[] { obj });
            else
            {
                string txt = obj[0] as string;

                this.txtLog.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"), txt));
            }
        }

        private void UcModbusProperty_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == false)
            {
                this._modbus.Close();
            }
        }
    }
}
