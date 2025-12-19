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
        private Label lbl_server_ip = new Label();
        private TextBox txt_server_ip = new TextBox();
        private Label lbl_server_portno = new Label();
        private NumericUpDown num_server_portno = new NumericUpDown();
        private Button btn_server_connection = new Button();

        private GroupBox gbx_sensor = new GroupBox();
        private TrackBar track_sensor_count = new TrackBar();
        private Label lbl_sensor_count = new Label();


        private GroupBox gbx_data = new GroupBox();
        private CheckBox chk_data_object_offset = new CheckBox();
        private NumericUpDown num_data_offset_Object = new NumericUpDown();
        private CheckBox chk_data_span_offset = new CheckBox();
        private NumericUpDown num_data_offset = new NumericUpDown();
        private CheckBox chk_data_randomvalue_scale = new CheckBox();
        private NumericUpDown num_data_randomvalue_scale = new NumericUpDown();

        private TextBox txtLog = null;

        #endregion UI Controls

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
            this.gbx_server.Text = AppData.Lang("server.terahz.server");
            this.lbl_server_ip.Text = AppData.Lang("server.terahz.server.ip");
            this.lbl_server_portno.Text = AppData.Lang("server.terahz.server.portno");
            this.btn_server_connection.Text = AppData.Lang("server.terahz.server.connection.connect");


            this.gbx_sensor.Text = AppData.Lang("server.terahz.sensor");
            this.lbl_sensor_count.Text = AppData.Lang("server.terahz.sensor.count");

            this.gbx_data.Text = AppData.Lang("server.terahz.data");
            this.chk_data_span_offset.Text = AppData.Lang("server.terahz.data.span.offset");
            this.chk_data_object_offset.Text = AppData.Lang("server.terahz.data.object.offset");
            this.chk_data_randomvalue_scale.Text = AppData.Lang("server.terahz.data.randomvalue.scale");
        }

        private void InitUI()
        {
            this.gbx_server.Dock = DockStyle.Top;
            this.InitUI_Server(this.gbx_server);

            this.gbx_sensor.Dock = DockStyle.Top;
            this.InitUI_HW(this.gbx_sensor);

            this.gbx_data.Dock = DockStyle.Top;
            this.InitUI_Data(this.gbx_data);

            this.Height = this.gbx_data.Bottom + 3;

            this.Controls.Add(this.gbx_data);
            this.Controls.Add(this.gbx_sensor);
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
            this.txt_server_ip.DataBindings.Add("Text", this._teraHzHandler, nameof(this._teraHzHandler.Server_IPAddress), true, DataSourceUpdateMode.OnPropertyChanged);

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
            this.num_server_portno.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Server_PortNo), true, DataSourceUpdateMode.OnPropertyChanged);

            
            this.btn_server_connection.Left = this.num_server_portno.Left;
            this.btn_server_connection.Top = this.num_server_portno.Bottom + 3;
            this.btn_server_connection.TextAlign = ContentAlignment.MiddleCenter;
            this.btn_server_connection.Tag = false;
            this.btn_server_connection.Click += Btn_server_connection_Click; ;

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
                this._teraHzHandler.Close();

                this.txt_server_ip.Enabled = true;
                this.num_server_portno.Enabled = true;
                this.track_sensor_count.Enabled = true;
                btn.Text = AppData.Lang("server.terahz.server.connection.connect");

                btn.Tag = false;
            }
            else
            {
                this.txtLog.Text = string.Empty;

                this._teraHzHandler.Open();

                this.txt_server_ip.Enabled = false;
                this.num_server_portno.Enabled = false;
                this.track_sensor_count.Enabled = false;
                btn.Text = AppData.Lang("server.terahz.server.connection.disconnect");

                btn.Tag = true;
            }
        }

        private void InitUI_HW(GroupBox gbx)
        {
            this.lbl_sensor_count.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.lbl_sensor_count.TextAlign = ContentAlignment.MiddleLeft;

            this.track_sensor_count.Left = this.lbl_sensor_count.Left;
            this.track_sensor_count.Top = this.lbl_sensor_count.Bottom;
            this.track_sensor_count.Width = 200;
            this.track_sensor_count.Minimum = 6;
            this.track_sensor_count.Maximum = 9;
            this.track_sensor_count.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Sensor_Count), true, DataSourceUpdateMode.OnPropertyChanged);

            Label lblmin = new Label();
            lblmin.Width = 27;
            lblmin.Height = 18;
            lblmin.Left = this.track_sensor_count.Left;
            lblmin.Top = this.track_sensor_count.Bottom - (lblmin.Height);
            lblmin.Text = Math.Pow(2, this.track_sensor_count.Minimum).ToString();
            lblmin.TextAlign = ContentAlignment.MiddleCenter;

            Label lblmax = new Label();
            lblmax.Size = lblmin.Size;
            lblmax.Left = this.track_sensor_count.Right - (lblmax.Width);
            lblmax.Top = lblmin.Top;
            lblmax.Text = Math.Pow(2, this.track_sensor_count.Maximum).ToString();
            lblmax.TextAlign = ContentAlignment.MiddleCenter;

            gbx.Height = lblmax.Bottom + 3;

            gbx.Controls.Add(lblmin);
            gbx.Controls.Add(lblmax);
            gbx.Controls.Add(this.lbl_sensor_count);
            gbx.Controls.Add(this.track_sensor_count);
        }

        private void InitUI_Data(GroupBox gbx)
        {
            this.chk_data_span_offset.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.chk_data_span_offset.Width = this.lbl_sensor_count.Width;
            this.chk_data_span_offset.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_data_span_offset.Checked = false;
            this.chk_data_span_offset.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_data_span_offset.DataBindings.Add("Checked", this._teraHzHandler, nameof(this._teraHzHandler.Data_Span_Run), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_offset.Left = this.chk_data_span_offset.Right + 3;
            this.num_data_offset.Top = this.chk_data_span_offset.Top;
            this.num_data_offset.Width = this.txt_server_ip.Width;
            this.num_data_offset.DecimalPlaces = 0;
            this.num_data_offset.TextAlign = HorizontalAlignment.Right;
            this.num_data_offset.Minimum = 0;
            this.num_data_offset.Maximum = UInt16.MaxValue;
            this.num_data_offset.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Data_Span_Offset), true, DataSourceUpdateMode.OnPropertyChanged);

            this.chk_data_object_offset.Left = this.chk_data_span_offset.Left;
            this.chk_data_object_offset.Top = this.chk_data_span_offset.Bottom + 3;
            this.chk_data_object_offset.Width = this.lbl_sensor_count.Width;
            this.chk_data_object_offset.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_data_object_offset.Checked = false;
            this.chk_data_object_offset.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_data_object_offset.DataBindings.Add("Checked", this._teraHzHandler, nameof(this._teraHzHandler.Data_Object_Run), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_offset_Object.Left = this.chk_data_object_offset.Right + 3;
            this.num_data_offset_Object.Top = this.chk_data_object_offset.Top;
            this.num_data_offset_Object.Width = this.txt_server_ip.Width;
            this.num_data_offset_Object.DecimalPlaces = 0;
            this.num_data_offset_Object.TextAlign = HorizontalAlignment.Right;
            this.num_data_offset_Object.Minimum = 0;
            this.num_data_offset_Object.Maximum = UInt16.MaxValue;
            this.num_data_offset_Object.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Data_Object_Offset), true, DataSourceUpdateMode.OnPropertyChanged);


            this.chk_data_randomvalue_scale.Left = this.chk_data_object_offset.Left;
            this.chk_data_randomvalue_scale.Top = this.chk_data_object_offset.Bottom + 3;
            this.chk_data_randomvalue_scale.Width = this.lbl_sensor_count.Width;
            this.chk_data_randomvalue_scale.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_data_randomvalue_scale.Checked = true;
            this.chk_data_randomvalue_scale.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_data_randomvalue_scale.DataBindings.Add("Checked", this._teraHzHandler, nameof(this._teraHzHandler.Data_RandomValue_Run), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_data_randomvalue_scale.Left = this.chk_data_randomvalue_scale.Right + 3;
            this.num_data_randomvalue_scale.Top = this.chk_data_randomvalue_scale.Top;
            this.num_data_randomvalue_scale.Width = this.txt_server_ip.Width;
            this.num_data_randomvalue_scale.DecimalPlaces = 0;
            this.num_data_randomvalue_scale.TextAlign = HorizontalAlignment.Right;
            this.num_data_randomvalue_scale.Minimum = 0;
            this.num_data_randomvalue_scale.Maximum = UInt16.MaxValue;
            this.num_data_randomvalue_scale.DataBindings.Add("Value", this._teraHzHandler, nameof(this._teraHzHandler.Data_RandomValue_Offset), true, DataSourceUpdateMode.OnPropertyChanged);

            gbx.Controls.Add(this.chk_data_object_offset);
            gbx.Controls.Add(this.num_data_offset_Object);
            gbx.Controls.Add(this.chk_data_span_offset);
            gbx.Controls.Add(this.num_data_offset);
            gbx.Controls.Add(this.chk_data_randomvalue_scale);
            gbx.Controls.Add(this.num_data_randomvalue_scale);
        }

        private void InitComponet()
        {
            this._teraHzHandler.PropertyChanged += _teraHzHandler_PropertyChanged;
            this._teraHzHandler.ServerLog += _teraHzHandler_ServerLog;

            this.VisibleChanged += UcTeraHzProperty_VisibleChanged;

            _teraHzHandler_PropertyChanged(this._teraHzHandler, new PropertyChangedEventArgs(nameof(this._teraHzHandler.Data_Span_Run)));
            _teraHzHandler_PropertyChanged(this._teraHzHandler, new PropertyChangedEventArgs(nameof(this._teraHzHandler.Data_Object_Run)));
            _teraHzHandler_PropertyChanged(this._teraHzHandler, new PropertyChangedEventArgs(nameof(this._teraHzHandler.Data_RandomValue_Run)));
        }

        private void _teraHzHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(this._teraHzHandler.Data_Span_Run))
            {
                this.chk_data_object_offset.Enabled = !this._teraHzHandler.Data_Span_Run;

                this.num_data_offset.Enabled = this._teraHzHandler.Data_Span_Run;
            }
            else if (e.PropertyName == nameof(this._teraHzHandler.Data_Object_Run))
            {
                this.chk_data_span_offset.Enabled = !this._teraHzHandler.Data_Object_Run;

                this.num_data_offset_Object.Enabled = this._teraHzHandler.Data_Object_Run;
            }
            else if (e.PropertyName == nameof(this._teraHzHandler.Data_RandomValue_Run))
            {
                this.num_data_randomvalue_scale.Enabled = this._teraHzHandler.Data_RandomValue_Run;
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
                this._teraHzHandler.Close();
            }
        }
    }
}
