using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using DotNetFrame.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Server.View
{
    public partial class FrmServer : Form
    {
        #region UI Controls

        private SplitContainer pnlSplit = new SplitContainer();

        private GroupBox gbx_server_property = new GroupBox();
        private Label lbl_server_type = new Label();

        private GroupBox gbx_property_custom = new GroupBox();
        private UcTeraHzProperty teraHz = null;
        private UcModbusProperty modbus = null;

        private GroupBox gbx_server_log = new GroupBox();
        private TextBox txt_server_log = new TextBox();

        #endregion UI Controls

        private ServerHandler _severhandler = new ServerHandler();

        public FrmServer()
        {
            InitializeComponent();

            this.InitText();
            this.InitUI();
            this.InitComponent();
        }

        private void InitText()
        {
            this.gbx_server_property.Text = AppData.Lang("server.property.title");
            this.lbl_server_type.Text = AppData.Lang("server.property.type.text");
            this.gbx_property_custom.Text = AppData.Lang("server.property.type.property");
            this.gbx_server_log.Text = AppData.Lang("server.log");
        }

        private void InitUI()
        {
            this.pnlSplit.Dock = DockStyle.Fill;
            this.pnlSplit.Panel1.Padding = this.pnlSplit.Panel2.Padding = new Padding(3);

            this.gbx_server_property.Dock = DockStyle.Fill;
            this.gbx_server_property.Padding = new Padding(3);
            this.gbx_server_property.Text = "Server Settings";

            Panel pnl_server = new Panel();
            pnl_server.Dock = DockStyle.Top;
            this.InitUI_Server(pnl_server);

            this.gbx_property_custom.Dock = DockStyle.Fill;
            this.InitUI_Custom(this.gbx_property_custom);

            this.gbx_server_log.Dock = DockStyle.Fill;
            this.txt_server_log.Dock = DockStyle.Fill;
            this.txt_server_log.ReadOnly = true;
            this.txt_server_log.Multiline = true;
            this.txt_server_log.BorderStyle = BorderStyle.None;
            this.txt_server_log.ScrollBars = new ScrollBars();


            this.gbx_server_property.Controls.Add(this.gbx_property_custom);
            this.gbx_server_property.Controls.Add(pnl_server);
            this.pnlSplit.Panel1.Controls.Add(this.gbx_server_property);

            this.gbx_server_log.Controls.Add(this.txt_server_log);
            this.pnlSplit.Panel2.Controls.Add(this.gbx_server_log);

            this.Controls.Add(this.pnlSplit);
        }

        private void InitUI_Server(Panel pnl)
        {
            this.lbl_server_type.Location = new Point(3, 3);
            this.lbl_server_type.TextAlign = ContentAlignment.MiddleLeft;
            ComboBox cbo_server_type = new ComboBox();
            cbo_server_type.Left = this.lbl_server_type.Right + 3;
            cbo_server_type.Top = this.lbl_server_type.Top;
            cbo_server_type.DataSource = this._severhandler.ServerList;
            cbo_server_type.ValueMember = "Value";
            cbo_server_type.DisplayMember = "DisplayText";
            cbo_server_type.DataBindings.Add("SelectedValue", this._severhandler, nameof(this._severhandler.Server_Current), true, DataSourceUpdateMode.OnPropertyChanged);
            cbo_server_type.DropDownStyle = ComboBoxStyle.DropDownList;

            pnl.Height = this.lbl_server_type.Bottom + 3;

            pnl.Controls.Add(this.lbl_server_type);
            pnl.Controls.Add(cbo_server_type);
        }

        private void InitUI_Custom(GroupBox gbx)
        {
            this.teraHz = new UcTeraHzProperty(this.txt_server_log);
            this.teraHz.Dock = DockStyle.Fill;

            this.modbus = new UcModbusProperty(this.txt_server_log);
            this.modbus.Dock = DockStyle.Fill;
            this.modbus.Visible = false;

            gbx.Controls.Add(this.teraHz);
            gbx.Controls.Add(this.modbus);
        }

        private void InitComponent()
        {
            this._severhandler.PropertyChanged += _severhandler_PropertyChanged;
        }

        private void _severhandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(this._severhandler.Server_Current))
            {
                if(this._severhandler.Server_Current == ServerType.TeraHz)
                {
                    this.teraHz.Show();

                    this.modbus.Hide();
                }
                else if (this._severhandler.Server_Current == ServerType.Modbus)
                {
                    this.modbus.Show();

                    this.teraHz.Hide();
                }
            }
        }
    }
}
