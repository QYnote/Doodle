using DotNet.Utils.ViewModel;
using DotNetFrame.CommTester.Model;
using DotNetFrame.CommTester.ViewModel.Port;
using System;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.CommTester.View
{
    public partial class FrmCommTester : Form
    {
        private ComboBox cbo_port = new ComboBox();
        private ComboBox cbo_protocol = new ComboBox();
        private CheckBox chk_error_create = new CheckBox();
        private CheckBox chk_reapeat_enable = new CheckBox();
        private NumericUpDown num_repeat_count = new NumericUpDown();
        private CheckBox chk_repeat_infinity = new CheckBox();

        private Panel pnl_transport = new Panel();
        private Button btn_connection = new Button();
        private Button btn_send = new Button();
        private TextBox txt_request = new TextBox();
        private TextBox txt_log = new TextBox();

        private BindingSource _binding = new BindingSource();
        private PortVM VM = new PortVM();

        public FrmCommTester()
        {
            this.BindingControl();
            this.InitUI();

            this.VM.PropertyChanged += VM_PropertyChanged;
        }

        private void BindingControl()
        {
            this._binding.DataSource = this.VM;

            this.cbo_port.ValueMember = nameof(QYItem.Value);
            this.cbo_port.DisplayMember = nameof(QYItem.DisplayText);
            this.cbo_port.DataSource = this.VM.PortTypeList;
            this.cbo_port.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbo_port.DataBindings.Add(nameof(ComboBox.SelectedValue), this._binding, nameof(PortVM.PortType), true, DataSourceUpdateMode.OnPropertyChanged);

            this.cbo_protocol.ValueMember = nameof(QYItem.Value);
            this.cbo_protocol.DisplayMember = nameof(QYItem.DisplayText);
            this.cbo_protocol.DataSource = this.VM.ProtocolTypeList;
            this.cbo_protocol.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbo_protocol.DataBindings.Add(nameof(ComboBox.SelectedValue), this._binding, nameof(PortVM.ProtocolType), true, DataSourceUpdateMode.OnPropertyChanged);

            this.chk_error_create.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_error_create.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_error_create.CheckAlign = ContentAlignment.MiddleLeft;
            this.chk_error_create.DataBindings.Add(nameof(CheckBox.Checked), this._binding, nameof(PortVM.CreateErrorCode), true, DataSourceUpdateMode.OnPropertyChanged);
            this.chk_error_create.DataBindings.Add(nameof(CheckBox.Enabled), this._binding, nameof(PortVM.ErrorCode_Enable), true, DataSourceUpdateMode.OnPropertyChanged);

            this.chk_reapeat_enable.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_reapeat_enable.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_reapeat_enable.CheckAlign = ContentAlignment.MiddleLeft;
            this.chk_reapeat_enable.DataBindings.Add(nameof(CheckBox.Checked), this._binding, nameof(PortVM.Repeat_Enable), true, DataSourceUpdateMode.OnPropertyChanged);

            this.num_repeat_count.DecimalPlaces = 0;
            this.num_repeat_count.TextAlign = HorizontalAlignment.Right;
            this.num_repeat_count.Minimum = 0;
            this.num_repeat_count.Maximum = int.MaxValue;
            this.num_repeat_count.DataBindings.Add(nameof(NumericUpDown.Value), this._binding, nameof(PortVM.Repeat_Count), true, DataSourceUpdateMode.OnPropertyChanged);
            this.num_repeat_count.DataBindings.Add(nameof(NumericUpDown.Enabled), this._binding, nameof(PortVM.Repeat_Count_Enable), true, DataSourceUpdateMode.OnPropertyChanged);

            this.chk_repeat_infinity.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_repeat_infinity.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_repeat_infinity.CheckAlign = ContentAlignment.MiddleLeft;
            this.chk_repeat_infinity.DataBindings.Add(nameof(CheckBox.Checked), this._binding, nameof(PortVM.Repeat_Infinity), true, DataSourceUpdateMode.OnPropertyChanged);
            this.chk_repeat_infinity.DataBindings.Add(nameof(CheckBox.Enabled), this._binding, nameof(PortVM.Repeat_Enable), true, DataSourceUpdateMode.OnPropertyChanged);

            this.txt_log.Dock = DockStyle.Fill;
            this.txt_log.Multiline = true;
            this.txt_log.ReadOnly = true;
            this.txt_log.ScrollBars = ScrollBars.Vertical;
            this.txt_log.DataBindings.Add(nameof(TextBox.Text), this._binding, nameof(PortVM.LogText), true, DataSourceUpdateMode.OnPropertyChanged);
            this.txt_log.TextChanged += Txt_log_TextChanged;
        }

        private void Txt_log_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            txt.SelectionStart = txt.Text.Length;
            txt.ScrollToCaret();
        }

        private void InitUI()
        {
            Panel pnl_property = new Panel();
            pnl_property.Dock = DockStyle.Top;
            pnl_property.Height = 200;
            GroupBox gbx_config = new GroupBox();
            gbx_config.Dock = DockStyle.Left;
            gbx_config.Text = "Transport 설정";
            this.InitUI_Config(gbx_config);

            GroupBox gbx_protocol = new GroupBox();
            gbx_protocol.Dock = DockStyle.Fill;
            gbx_protocol.Text = "통신 설정";
            this.InitUI_Protocol(gbx_protocol);

            Panel pnl_text = new Panel();
            pnl_text.Dock = DockStyle.Top;
            pnl_text.Height = 23;
            this.InitUI_Request(pnl_text);

            GroupBox gbx_log = new GroupBox();
            gbx_log.Dock = DockStyle.Fill;
            gbx_log.Text = "로그";

            pnl_property.Controls.Add(gbx_protocol);
            pnl_property.Controls.Add(gbx_config);
            gbx_log.Controls.Add(this.txt_log);
            this.Controls.Add(gbx_log);
            this.Controls.Add(pnl_text);
            this.Controls.Add(pnl_property);
        }

        private void InitUI_Config(GroupBox gbx)
        {
            Panel pnl_port = new Panel();
            pnl_port.Dock = DockStyle.Top;
            pnl_port.Height = 23;
            this.cbo_port.Dock = DockStyle.Left;
            this.btn_connection.Dock = DockStyle.Left;
            this.btn_connection.Text = "연결";

            this.pnl_transport.Dock = DockStyle.Fill;

            pnl_port.Controls.Add(this.cbo_port);
            pnl_port.Controls.Add(this.btn_connection);
            gbx.Controls.Add(this.pnl_transport);
            gbx.Controls.Add(pnl_port);
        }

        private void InitUI_Protocol(GroupBox gbx)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Location = new Point(0, 18);
            layout.Width = 250;
            layout.Height = 155;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());

            Label lbl_protocol = new Label();
            lbl_protocol.Dock = DockStyle.Fill;
            lbl_protocol.TextAlign = ContentAlignment.MiddleLeft;
            lbl_protocol.Text = "Protocol 종류";

            Label lbl_error_create = new Label();
            lbl_error_create.Dock = DockStyle.Fill;
            lbl_error_create.TextAlign = ContentAlignment.MiddleLeft;
            lbl_error_create.Text = "CheckSum 생성";

            Label lbl_repeat_enable = new Label();
            lbl_repeat_enable.Dock = DockStyle.Fill;
            lbl_repeat_enable.TextAlign = ContentAlignment.MiddleLeft;
            lbl_repeat_enable.Text = "반복 진행";

            Label lbl_repeat_count = new Label();
            lbl_repeat_count.Dock = DockStyle.Fill;
            lbl_repeat_count.TextAlign = ContentAlignment.MiddleLeft;
            lbl_repeat_count.Text = "반복 횟수";

            Label lbl_repeat_infinity = new Label();
            lbl_repeat_infinity.Dock = DockStyle.Fill;
            lbl_repeat_infinity.TextAlign = ContentAlignment.MiddleLeft;
            lbl_repeat_infinity.Text = "무한 반복";

            layout.Controls.Add(lbl_protocol, 0, 0);
            layout.Controls.Add(lbl_error_create, 0, 1);
            layout.Controls.Add(lbl_repeat_enable, 0, 3);
            layout.Controls.Add(lbl_repeat_count, 0, 4);
            layout.Controls.Add(lbl_repeat_infinity, 0, 5);

            layout.Controls.Add(this.cbo_protocol, 1, 0);
            layout.Controls.Add(this.chk_error_create, 1, 1);
            layout.Controls.Add(this.chk_reapeat_enable, 1, 3);
            layout.Controls.Add(this.num_repeat_count, 1, 4);
            layout.Controls.Add(this.chk_repeat_infinity, 1, 5);

            gbx.Controls.Add(layout);
        }

        private void InitUI_Request(Panel pnl)
        {
            this.btn_send.Text = "전송";
            this.btn_send.Dock = DockStyle.Left;
            this.btn_send.Click += (s, e) =>
            {
                this.VM.Send(this.txt_request.Text);
            };

            this.txt_request.Dock = DockStyle.Fill;

            pnl.Controls.Add(this.txt_request);
            pnl.Controls.Add(this.btn_send);
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is PortVM port == false) return;

            if (e.PropertyName == nameof(PortVM.PortConfig))
            {
                this.pnl_transport.Controls.Clear();

                switch (port.PortType)
                {
                    case DotNet.Comm.Transport.PortType.Serial:
                        this.pnl_transport.Controls.Add(new UcSerial((SerialVM)this.VM.PortConfig));
                        break;
                    case DotNet.Comm.Transport.PortType.Socket:
                        this.pnl_transport.Controls.Add(new UcSocket((SocketVM)this.VM.PortConfig));
                        break;
                }
            }
            else if(e.PropertyName == nameof(PortVM.IsSending))
            {
                if (port.IsSending)
                {
                    this.btn_send.Text = "중지";
                }
                else
                {
                    this.btn_send.Text = "전송";
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.VM.PortType = DotNet.Comm.Transport.PortType.Serial;
        }
    }
}
