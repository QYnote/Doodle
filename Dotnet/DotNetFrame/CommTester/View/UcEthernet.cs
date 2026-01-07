using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using DotNetFrame.CommTester.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.CommTester.View
{
    internal class UcEthernet : UserControl
    {
        private Label lbl_ip = new Label();
        private Label lbl_portno = new Label();

        EthernetHandler _handler;

        internal UcEthernet(EthernetHandler handler)
        {
            this._handler = handler;
            this.InitText();
            this.InitUI();
        }

        private void InitText()
        {
            this.lbl_ip.Text = AppData.Lang("commtester.portproperty.ethernet.ip.title");
            this.lbl_portno.Text = AppData.Lang("commtester.portproperty.ethernet.portno.title");
        }

        private void InitUI()
        {
            this.lbl_ip.Location = new Point(3, 3);
            this.lbl_ip.TextAlign = ContentAlignment.MiddleLeft;
            TextBox txt_ip = new TextBox();
            txt_ip.Left = this.lbl_ip.Right + 3;
            txt_ip.Top = this.lbl_ip.Top;
            txt_ip.Width = 80;
            txt_ip.TextAlign = HorizontalAlignment.Center;
            txt_ip.KeyPress += QYUtils.Event_KeyPress_IP;
            txt_ip.DataBindings.Add("Text", this._handler, nameof(this._handler.IP), true, DataSourceUpdateMode.OnPropertyChanged);

            this.lbl_portno.Left = this.lbl_ip.Left;
            this.lbl_portno.Top = this.lbl_ip.Bottom + 3;
            this.lbl_portno.TextAlign = ContentAlignment.MiddleLeft;
            NumericUpDown num_portno = new NumericUpDown();
            num_portno.Left = this.lbl_portno.Right + 3;
            num_portno.Top = this.lbl_portno.Top;
            num_portno.Width = txt_ip.Width;
            num_portno.DecimalPlaces = 0;
            num_portno.TextAlign = HorizontalAlignment.Right;
            num_portno.Minimum = 0;
            num_portno.Maximum = int.MaxValue;
            num_portno.DataBindings.Add("Value", this._handler, $"{nameof(this._handler.PortNo)}", true, DataSourceUpdateMode.OnPropertyChanged);

            this.Controls.Add(this.lbl_ip);
            this.Controls.Add(txt_ip);
            this.Controls.Add(this.lbl_portno);
            this.Controls.Add(num_portno);
        }
    }
}
