using DotNet.Utils.Controls.Utils;
using DotNetFrame.Base.Model;
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
        private enum ServerType
        {
            TeraHz,
            Modbus,
        }


        #region UI Controls

        private SplitContainer pnlSplit = new SplitContainer();

        private GroupBox gbx_server_property = new GroupBox();
        private Label lbl_server_type = new Label();
        private ComboBox cbo_server_type = new ComboBox();

        private GroupBox gbx_property_custom = new GroupBox();
        private UcTeraHzProperty teraHz = null;
        private UcModbusProperty modbus = null;

        private GroupBox gbx_server_log = new GroupBox();
        private TextBox txt_server_log = new TextBox();

        #endregion UI Controls

        public FrmServer()
        {
            InitializeComponent();
            this.InitUI();
            this.InitText();
        }

        private void InitUI()
        {
            this.pnlSplit.Dock = DockStyle.Fill;
            this.pnlSplit.Panel1.Padding = this.pnlSplit.Panel2.Padding = new Padding(3);

            this.gbx_server_property.Dock = DockStyle.Fill;
            this.gbx_server_property.Padding = new Padding(3);
            this.gbx_server_property.Text = "Server Settings";

            Panel pnl_server_type = new Panel();
            pnl_server_type.Dock = DockStyle.Top;
            pnl_server_type.Height = 28;
            this.lbl_server_type.Location = new Point(3, 3);
            this.lbl_server_type.TextAlign = ContentAlignment.MiddleLeft;
            this.cbo_server_type.Left = this.lbl_server_type.Right + 3;
            this.cbo_server_type.Top = this.lbl_server_type.Top;
            this.cbo_server_type.DataSource = QYUtils.GetEnumItems<ServerType>();
            this.cbo_server_type.ValueMember = "Value";
            this.cbo_server_type.DisplayMember = "DisplayText";
            this.cbo_server_type.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbo_server_type.SelectedValueChanged += CboProtocol_SelectedValueChanged;

            this.gbx_property_custom.Dock = DockStyle.Fill;
            this.gbx_property_custom.Text = "종류별 설정";

            this.teraHz = new UcTeraHzProperty(this.txt_server_log);
            this.modbus = new UcModbusProperty(this.txt_server_log);

            this.gbx_server_log.Dock = DockStyle.Fill;
            this.gbx_server_log.Text = "서버 로그";

            this.txt_server_log.Dock = DockStyle.Fill;
            this.txt_server_log.ReadOnly = true;
            this.txt_server_log.Multiline = true;
            this.txt_server_log.BorderStyle = BorderStyle.None;
            this.txt_server_log.ScrollBars = new ScrollBars();


            this.Controls.Add(this.pnlSplit);
            this.pnlSplit.Panel1.Controls.Add(this.gbx_server_property);
            pnl_server_type.Controls.Add(this.lbl_server_type);
            pnl_server_type.Controls.Add(this.cbo_server_type);
            this.gbx_property_custom.Controls.Add(this.teraHz);
            this.gbx_property_custom.Controls.Add(this.modbus);
            this.gbx_server_property.Controls.Add(this.gbx_property_custom);
            this.gbx_server_property.Controls.Add(pnl_server_type);
            this.gbx_server_log.Controls.Add(this.txt_server_log);
            this.pnlSplit.Panel2.Controls.Add(this.gbx_server_log);
        }

        private void InitText()
        {
            this.gbx_server_property.Text = AppData.Lang("server.property.title");
            this.lbl_server_type.Text = AppData.Lang("server.property.type.text");
            this.gbx_property_custom.Text = AppData.Lang("server.property.type.property");
            this.gbx_server_log.Text = AppData.Lang("server.log");
        }

        private void CboProtocol_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cbo = sender as ComboBox;
            ServerType type = (ServerType)cbo.SelectedValue;

            if(type == ServerType.TeraHz)
            {
                this.teraHz.Show();

                this.modbus.Hide();
            }
            else if(type == ServerType.Modbus)
            {
                this.modbus.Show();

                this.teraHz.Hide();
            }
        }

        private void ServerLog(params object[] obj)
        {
            if (obj[0] is string == false) return;

            if (this.InvokeRequired)
                this.BeginInvoke(new Update_WithParam(ServerLog), obj);
            else
            {
                string txt = obj[0] as string;

                this.txt_server_log.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"), txt));
            }
        }
    }
}
