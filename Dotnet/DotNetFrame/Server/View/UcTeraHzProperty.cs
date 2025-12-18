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
    public partial class UcTeraHzProperty : UserControl
    {
        #region UI Controls

        private GroupBox gbx_server = new GroupBox();
        private Label lbl_Server_IP = new Label();
        private TextBox txt_Server_IP = new TextBox();
        private Label lbl_Server_PortNo = new Label();
        private NumericUpDown num_Server_PortNo = new NumericUpDown();
        private Button btn_server_connection = new Button();

        private GroupBox gbx_hw = new GroupBox();
        private TrackBar track_hw_sensor_count = new TrackBar();
        private Label lbl_data_Count = new Label();


        private GroupBox gbx_data = new GroupBox();
        private NumericUpDown num_data_Count = new NumericUpDown();
        private CheckBox chk_data_offset_Object = new CheckBox();
        private NumericUpDown num_data_offset_Object = new NumericUpDown();
        private CheckBox chk_data_offset_Max = new CheckBox();
        private NumericUpDown num_data_offset_Max = new NumericUpDown();
        private CheckBox chk_data_offset_Random = new CheckBox();
        private NumericUpDown num_data_offset_BoundScale = new NumericUpDown();

        private Button btnConnection = new Button();

        private TextBox txtLog = null;

        #endregion UI Controls

        private VM_Server_HY_TeraHz _teraHz = new VM_Server_HY_TeraHz();
        private TeraHzHandler _teraHzHandler = new TeraHzHandler();

        internal UcTeraHzProperty(TextBox txtLog)
        {
            this.txtLog = txtLog;

            InitializeComponent();
            this.InitText();
            this.InitUI();
            this.InitComponet();

        }

        private void InitText()
        {
            this.gbx_server.Text = AppData.Lang("server.terahz.port");
            this.lbl_Server_IP.Text = AppData.Lang("server.terahz.ip");
            this.lbl_Server_PortNo.Text = AppData.Lang("server.terahz.portno");
            this.btn_server_connection.Text = AppData.Lang("server.terahz.connection.connect");


            this.gbx_hw.Text = AppData.Lang("server.terahz.hw");
            this.lbl_data_Count.Text = AppData.Lang("server.terahz.sensor.count");

            this.gbx_data.Text = AppData.Lang("server.terahz.data");
            this.chk_data_offset_Object.Text = AppData.Lang("server.terahz.sensor.offset.object");
            this.chk_data_offset_Max.Text = AppData.Lang("server.terahz.sensor.offset.max");
            this.chk_data_offset_Random.Text = AppData.Lang("server.terahz.sensor.offset.random");
        }

        private void InitUI()
        {
            this.gbx_server.Dock = DockStyle.Top;
            this.InitUI_Server(this.gbx_server);

            this.gbx_hw.Dock = DockStyle.Top;
            this.InitUI_HW(this.gbx_hw);

            this.num_data_Count.Left = this.lbl_data_Count.Right + 3;
            this.num_data_Count.Top = this.lbl_data_Count.Top;
            this.num_data_Count.Width = this.txt_Server_IP.Width;
            this.num_data_Count.DecimalPlaces = 0;
            this.num_data_Count.TextAlign = HorizontalAlignment.Right;
            this.num_data_Count.Minimum = 0;
            this.num_data_Count.Maximum = UInt64.MaxValue;
            this.num_data_Count.Value = VM_Server_HY_TeraHz.DEFAULT_SENSOR_COUNT;

            this.gbx_data.Dock = DockStyle.Top;
            this.InitUI_Data(this.gbx_data);

            this.btnConnection.Click += BtnConnection_Click;

            this.Height = this.btnConnection.Bottom + 3;

            this.Controls.Add(this.gbx_data);
            this.Controls.Add(this.gbx_hw);
            this.Controls.Add(this.gbx_server);
        }

        private void InitUI_Server(GroupBox gbx)
        {
            this.lbl_Server_IP.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.lbl_Server_IP.TextAlign = ContentAlignment.MiddleLeft;
            this.txt_Server_IP.Left = this.lbl_Server_IP.Right + 3;
            this.txt_Server_IP.Top = this.lbl_Server_IP.Top;
            this.txt_Server_IP.Width = 80;
            this.txt_Server_IP.TextAlign = HorizontalAlignment.Center;
            this.txt_Server_IP.Text = this._teraHz.IP;
            this.txt_Server_IP.KeyPress += QYUtils.Event_KeyPress_IP;
            this.txt_Server_IP.DataBindings.Add("Text", this._teraHzHandler, nameof(this._teraHzHandler.Server_IPAddress), true, DataSourceUpdateMode.OnPropertyChanged);

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
            this.num_Server_PortNo.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Server_PortNo), true, DataSourceUpdateMode.OnPropertyChanged);

            
            this.btn_server_connection.Left = this.num_Server_PortNo.Left;
            this.btn_server_connection.Top = this.num_Server_PortNo.Bottom + 3;
            this.btn_server_connection.TextAlign = ContentAlignment.MiddleCenter;
            this.btn_server_connection.Tag = false;
            this.btn_server_connection.Click += BtnConnection_Click;

            gbx.Height = this.btn_server_connection.Bottom + 3;

            gbx.Controls.Add(this.lbl_Server_IP);
            gbx.Controls.Add(this.txt_Server_IP);
            gbx.Controls.Add(this.lbl_Server_PortNo);
            gbx.Controls.Add(this.num_Server_PortNo);
            gbx.Controls.Add(this.btn_server_connection);
        }

        private void BtnConnection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if ((bool)btn.Tag)
            {
                this._teraHz.Close();

                this.txt_Server_IP.Enabled = true;
                this.num_Server_PortNo.Enabled = true;
                this.num_data_Count.Enabled = true;
                btn.Text = AppData.Lang("server.terahz.connection.connect");

                btn.Tag = false;
            }
            else
            {
                this.txtLog.Text = string.Empty;

                this._teraHz.Open();

                this.txt_Server_IP.Enabled = false;
                this.num_Server_PortNo.Enabled = false;
                this.num_data_Count.Enabled = false;
                btn.Text = AppData.Lang("server.terahz.connection.disconnect");

                btn.Tag = true;
            }
        }

        private void InitUI_HW(GroupBox gbx)
        {
            this.lbl_data_Count.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.lbl_data_Count.TextAlign = ContentAlignment.MiddleLeft;

            this.track_hw_sensor_count.Left = this.lbl_data_Count.Left;
            this.track_hw_sensor_count.Top = this.lbl_data_Count.Bottom;
            this.track_hw_sensor_count.Width = 200;
            this.track_hw_sensor_count.Minimum = 6;
            this.track_hw_sensor_count.Maximum = 9;
            this.track_hw_sensor_count.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.HW_SensorCount), true, DataSourceUpdateMode.OnPropertyChanged);

            Label lblmin = new Label();
            lblmin.Width = 27;
            lblmin.Height = 18;
            lblmin.Left = this.track_hw_sensor_count.Left;
            lblmin.Top = this.track_hw_sensor_count.Bottom - (lblmin.Height);
            lblmin.Text = Math.Pow(2, this.track_hw_sensor_count.Minimum).ToString();
            lblmin.TextAlign = ContentAlignment.MiddleCenter;

            Label lblmax = new Label();
            lblmax.Size = lblmin.Size;
            lblmax.Left = this.track_hw_sensor_count.Right - (lblmax.Width);
            lblmax.Top = lblmin.Top;
            lblmax.Text = Math.Pow(2, this.track_hw_sensor_count.Maximum).ToString();
            lblmax.TextAlign = ContentAlignment.MiddleCenter;

            gbx.Height = lblmax.Bottom + 3;

            gbx.Controls.Add(lblmin);
            gbx.Controls.Add(lblmax);
            gbx.Controls.Add(this.lbl_data_Count);
            gbx.Controls.Add(this.track_hw_sensor_count);
        }

        private void InitUI_Data(GroupBox gbx)
        {
            this.chk_data_offset_Max.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.chk_data_offset_Max.Width = this.lbl_data_Count.Width;
            this.chk_data_offset_Max.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_data_offset_Max.Checked = false;
            this.chk_data_offset_Max.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_data_offset_Max.DataBindings.Add("Checked", this._teraHzHandler, nameof(this._teraHzHandler.Data_Span_Run), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_offset_Max.Left = this.chk_data_offset_Max.Right + 3;
            this.num_data_offset_Max.Top = this.chk_data_offset_Max.Top;
            this.num_data_offset_Max.Width = this.txt_Server_IP.Width;
            this.num_data_offset_Max.DecimalPlaces = 0;
            this.num_data_offset_Max.TextAlign = HorizontalAlignment.Right;
            this.num_data_offset_Max.Minimum = 0;
            this.num_data_offset_Max.Maximum = UInt16.MaxValue;
            this.num_data_offset_Max.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Data_Span_Offset), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_offset_Max.Enabled = false;

            this.chk_data_offset_Object.Left = this.chk_data_offset_Max.Left;
            this.chk_data_offset_Object.Top = this.chk_data_offset_Max.Bottom + 3;
            this.chk_data_offset_Object.Width = this.lbl_data_Count.Width;
            this.chk_data_offset_Object.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_data_offset_Object.Checked = false;
            this.chk_data_offset_Object.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_data_offset_Object.DataBindings.Add("Checked", this._teraHzHandler, nameof(this._teraHzHandler.Data_Object_Run), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_offset_Object.Left = this.chk_data_offset_Object.Right + 3;
            this.num_data_offset_Object.Top = this.chk_data_offset_Object.Top;
            this.num_data_offset_Object.Width = this.txt_Server_IP.Width;
            this.num_data_offset_Object.DecimalPlaces = 0;
            this.num_data_offset_Object.TextAlign = HorizontalAlignment.Right;
            this.num_data_offset_Object.Minimum = 0;
            this.num_data_offset_Object.Maximum = UInt16.MaxValue;
            this.num_data_offset_Object.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Data_Object_Offset), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_offset_Object.Enabled = false;


            this.chk_data_offset_Random.Left = this.chk_data_offset_Object.Left;
            this.chk_data_offset_Random.Top = this.chk_data_offset_Object.Bottom + 3;
            this.chk_data_offset_Random.Width = this.lbl_data_Count.Width;
            this.chk_data_offset_Random.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_data_offset_Random.Checked = true;
            this.chk_data_offset_Random.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_data_offset_Random.DataBindings.Add("Checked", this._teraHzHandler, nameof(this._teraHzHandler.Data_RandomValue_Run), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_offset_BoundScale.Left = this.chk_data_offset_Random.Right + 3;
            this.num_data_offset_BoundScale.Top = this.chk_data_offset_Random.Top;
            this.num_data_offset_BoundScale.Width = this.txt_Server_IP.Width;
            this.num_data_offset_BoundScale.DecimalPlaces = 0;
            this.num_data_offset_BoundScale.TextAlign = HorizontalAlignment.Right;
            this.num_data_offset_BoundScale.Minimum = 0;
            this.num_data_offset_BoundScale.Maximum = UInt16.MaxValue;
            this.num_data_offset_BoundScale.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Data_RandomValue_Offset), true, DataSourceUpdateMode.OnPropertyChanged);

            gbx.Controls.Add(this.chk_data_offset_Object);
            gbx.Controls.Add(this.num_data_offset_Object);
            gbx.Controls.Add(this.chk_data_offset_Max);
            gbx.Controls.Add(this.num_data_offset_Max);
            gbx.Controls.Add(this.chk_data_offset_Random);
            gbx.Controls.Add(this.num_data_offset_BoundScale);
        }

        private void InitComponet()
        {
            this._teraHzHandler.PropertyChanged += _teraHzHandler_PropertyChanged;
            this._teraHzHandler.ServerLog += _teraHzHandler_ServerLog;

            this.VisibleChanged += UcTeraHzProperty_VisibleChanged;
        }

        private void _teraHzHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(this._teraHzHandler.Data_Span_Run))
            {
                this.chk_data_offset_Object.Enabled = !this._teraHzHandler.Data_Span_Run;

                this.num_data_offset_Max.Enabled = this._teraHzHandler.Data_Span_Run;
            }
            else if (e.PropertyName == nameof(this._teraHzHandler.Data_Object_Run))
            {
                this.chk_data_offset_Max.Enabled = !this._teraHzHandler.Data_Object_Run;

                this.num_data_offset_Object.Enabled = this._teraHzHandler.Data_Object_Run;
            }
            else if (e.PropertyName == nameof(this._teraHzHandler.Data_RandomValue_Run))
            {
                this.num_data_offset_BoundScale.Enabled = this._teraHzHandler.Data_RandomValue_Run;
            }
        }

        private void _teraHzHandler_ServerLog(object sender, string e)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
                this.BeginInvoke(new EventHandler<string>(_teraHzHandler_ServerLog), sender, e);
            else
            {
                this.txtLog.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"), e));
            }
        }

        private void UcTeraHzProperty_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == false)
            {
                this._teraHz.Close();
            }
        }
    }
}
