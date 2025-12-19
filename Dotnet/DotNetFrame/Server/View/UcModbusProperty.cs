using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using DotNetFrame.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Server.View
{
    public partial class UcModbusProperty : UserControl
    {
        #region UI Controls

        private GroupBox gbx_server = new GroupBox();
        private Label lbl_server_ip = new Label();
        private TextBox txt_server_ip = new TextBox();
        private Label lbl_server_portno = new Label();
        private NumericUpDown num_server_portno = new NumericUpDown();

        private Button btn_server_connection = new Button();

        private TextBox txtLog = null;

        #endregion UI Controls

        private ModbusHandler _modbusHandler = new ModbusHandler();

        public UcModbusProperty(TextBox txtLog)
        {
            this.txtLog = txtLog;

            InitializeComponent();
            this.InitText();
            this.InitUI();
            this.InitComponet();
        }

        private void InitText()
        {
            this.gbx_server.Text= AppData.Lang("server.modbus.server");
            this.lbl_server_ip.Text = AppData.Lang("server.modbus.server.ip");
            this.lbl_server_portno.Text = AppData.Lang("server.modbus.server.portno");
            this.btn_server_connection.Text = AppData.Lang("server.modbus.server.connection.connect");
        }

        private void InitUI()
        {
            this.Dock = DockStyle.Top;
            this.Hide();

            this.gbx_server.Dock = DockStyle.Top;
            this.InitUI_Server(this.gbx_server);

            this.Height = this.gbx_server.Bottom + 3;

            this.Controls.Add(this.gbx_server);
        }

        private void InitUI_Server(GroupBox gbx)
        {
            this.lbl_server_ip.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.lbl_server_ip.TextAlign = ContentAlignment.MiddleLeft;
            this.txt_server_ip.Left = this.lbl_server_ip.Right + 3;
            this.txt_server_ip.Top = this.lbl_server_ip.Top;
            this.txt_server_ip.Width = 80;
            this.txt_server_ip.TextAlign = HorizontalAlignment.Center;
            this.txt_server_ip.KeyPress += QYUtils.Event_KeyPress_IP;
            this.txt_server_ip.DataBindings.Add("Text", this._modbusHandler, nameof(this._modbusHandler.Server_IPAddress), true, DataSourceUpdateMode.OnPropertyChanged);

            this.lbl_server_portno.Left = this.lbl_server_ip.Left;
            this.lbl_server_portno.Top = this.lbl_server_ip.Bottom + 3;
            this.lbl_server_portno.TextAlign = ContentAlignment.MiddleLeft;
            this.num_server_portno.Left = this.lbl_server_portno.Right + 3;
            this.num_server_portno.Top = this.lbl_server_portno.Top;
            this.num_server_portno.Width = this.txt_server_ip.Width;
            this.num_server_portno.DecimalPlaces = 0;
            this.num_server_portno.TextAlign = HorizontalAlignment.Right;
            this.num_server_portno.Minimum = 0;
            this.num_server_portno.Maximum = int.MaxValue;
            this.num_server_portno.DataBindings.Add("Value", this._modbusHandler, nameof(this._modbusHandler.Server_PortNo), true, DataSourceUpdateMode.OnPropertyChanged);

            this.btn_server_connection.Left = this.num_server_portno.Left;
            this.btn_server_connection.Top = this.num_server_portno.Bottom + 3;
            this.btn_server_connection.TextAlign = ContentAlignment.MiddleCenter;
            this.btn_server_connection.Tag = false;
            this.btn_server_connection.Click += Btn_server_connection_Click;

            gbx.Height = this.btn_server_connection.Bottom + 3;

            gbx.Controls.Add(this.lbl_server_ip);
            gbx.Controls.Add(this.txt_server_ip);
            gbx.Controls.Add(this.lbl_server_portno);
            gbx.Controls.Add(this.num_server_portno);
            gbx.Controls.Add(this.btn_server_connection);
        }

        private void Btn_server_connection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if ((bool)btn.Tag)
            {
                this._modbusHandler.Close();

                this.txt_server_ip.Enabled = true;
                this.num_server_portno.Enabled = true;
                btn.Text = AppData.Lang("server.modbus.server.connection.connect");

                btn.Tag = false;
            }
            else
            {
                this.txtLog.Text = string.Empty;

                this._modbusHandler.Open();

                this.txt_server_ip.Enabled = false;
                this.num_server_portno.Enabled = false;
                btn.Text = AppData.Lang("server.modbus.server.connection.disconnect");

                btn.Tag = true;
            }
        }

        private void InitComponet()
        {
            this._modbusHandler.PropertyChanged += _modbushandler_PropertyChanged;
            this._modbusHandler.ServerLog += _modbushandler_ServerLog;

            this.VisibleChanged += UcModbusProperty_VisibleChanged;
        }

        private void _modbushandler_ServerLog(object sender, string e)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
                this.BeginInvoke(new EventHandler<string>(_modbushandler_ServerLog), sender, e);
            else
            {
                this.txtLog.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"), e));
            }
        }

        private void _modbushandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }

        private void UcModbusProperty_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == false)
            {
                this._modbusHandler.Close();
            }
        }
    }
}
