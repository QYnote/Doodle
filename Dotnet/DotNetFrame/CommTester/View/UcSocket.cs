using DotNet.Utils.Controls.Utils;
using DotNet.Utils.ViewModel;
using DotNetFrame.CommTester.ViewModel.Port;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.CommTester.View
{
    internal class UcSocket : UserControl
    {
        private TextBox txt_ip = new TextBox();
        private NumericUpDown num_portno = new NumericUpDown();
        private ComboBox cbo_protocol = new ComboBox();
        //private NumericUpDown num_maxbuffer = new NumericUpDown();

        SocketVM VM { get; }

        internal UcSocket(SocketVM viewmodel)
        {
            this.VM = viewmodel;

            this.BindingControl();
            this.InitUI();
        }

        private void BindingControl()
        {
            this.txt_ip.KeyPress += QYUtils.Event_KeyPress_IP;
            this.txt_ip.DataBindings.Add(nameof(TextBox.Text), this.VM, nameof(SocketVM.IP), true, DataSourceUpdateMode.OnPropertyChanged);

            this.num_portno.DecimalPlaces = 0;
            this.num_portno.TextAlign = HorizontalAlignment.Right;
            this.num_portno.Minimum = 0;
            this.num_portno.Maximum = int.MaxValue;
            this.num_portno.DataBindings.Add(nameof(NumericUpDown.Value), this.VM, nameof(SocketVM.PortNo), true, DataSourceUpdateMode.OnPropertyChanged);

            this.cbo_protocol.ValueMember = nameof(QYItem.Value);
            this.cbo_protocol.DisplayMember = nameof(QYItem.DisplayText);
            this.cbo_protocol.DataSource = this.VM.ProtocolList;
            this.cbo_protocol.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbo_protocol.DataBindings.Add(nameof(ComboBox.SelectedValue), this.VM, nameof(SocketVM.Protocol), true, DataSourceUpdateMode.OnPropertyChanged);

            //this.num_maxbuffer.DecimalPlaces = 0;
            //this.num_maxbuffer.TextAlign = HorizontalAlignment.Right;
            //this.num_maxbuffer.Minimum = 0;
            //this.num_maxbuffer.Maximum = int.MaxValue;
            //this.num_maxbuffer.DataBindings.Add(nameof(NumericUpDown.Value), this.VM, nameof(SocketVM.MaxBufferSize), true, DataSourceUpdateMode.OnPropertyChanged);

        }

        private void InitUI()
        {
            this.Dock = DockStyle.Top;
            this.Height = 82;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.Percent));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent));

            Label lbl_ip = new Label();
            lbl_ip.Dock = DockStyle.Fill;
            lbl_ip.TextAlign = ContentAlignment.MiddleLeft;
            lbl_ip.Text = "IP Addresss";

            Label lbl_portno = new Label();
            lbl_portno.Dock = DockStyle.Fill;
            lbl_portno.TextAlign = ContentAlignment.MiddleLeft;
            lbl_portno.Text = "Port 번호";

            Label lbl_protocol = new Label();
            lbl_protocol.Dock = DockStyle.Fill;
            lbl_protocol.TextAlign = ContentAlignment.MiddleLeft;
            lbl_protocol.Text = "Protocol";

            layout.Controls.Add(lbl_ip, 0, 0);
            layout.Controls.Add(lbl_portno, 0, 1);
            layout.Controls.Add(lbl_protocol, 0, 2);

            layout.Controls.Add(this.txt_ip, 1, 0);
            layout.Controls.Add(this.num_portno, 1, 1);
            layout.Controls.Add(this.cbo_protocol, 1, 2);

            this.Controls.Add(layout);
        }
    }
}
