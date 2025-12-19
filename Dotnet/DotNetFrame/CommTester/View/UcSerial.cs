using DotNet.Utils.Views;
using DotNetFrame.CommTester.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.CommTester.View
{
    public partial class UcSerial : UserControl
    {
        private SerialHandler _handler;

        internal UcSerial(SerialHandler handler)
        {
            this._handler = handler;

            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;

            ComboBox cbo_portname = new ComboBox();
            cbo_portname.Location = new Point(3, 3);
            cbo_portname.DataSource = this._handler.PortList;
            cbo_portname.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo_portname.DataBindings.Add("SelectedValue", this._handler, nameof(this._handler.PortName), true, DataSourceUpdateMode.OnPropertyChanged);

            pnl.Height = cbo_portname.Bottom + 3;

            GroupBox gbx_baudrate = new GroupBox();
            gbx_baudrate.Padding = new Padding(3);
            gbx_baudrate.Dock = DockStyle.Left;
            gbx_baudrate.Width = 80;
            gbx_baudrate.Text = "BaudRate";
            for (int i = 0; i < this._handler.BaudRateList.Count; i++)
            {
                RadioButton rdo = new RadioButton();
                rdo.Dock = DockStyle.Top;
                rdo.Height = 20;
                rdo.Text = this._handler.BaudRateList[i].ToString("#,#");
                rdo.TextAlign = ContentAlignment.MiddleLeft;

                QYViewUtils.BindingRadioButton(rdo, this._handler, nameof(this._handler.BaudRate), this._handler.BaudRateList[i]);
                gbx_baudrate.Controls.Add(rdo);
                rdo.BringToFront();
            }
            gbx_baudrate.Height = gbx_baudrate.Controls[0].Bottom + 3;

            GroupBox gbx_parity = new GroupBox();
            gbx_parity.Padding = new Padding(3);
            gbx_parity.Dock = DockStyle.Left;
            gbx_parity.Width = 65;
            gbx_parity.Text = "Parity";
            RadioButton[] rdo_parity = QYViewUtils.CreateEnumRadioButton<System.IO.Ports.Parity>();
            foreach (var rdo in rdo_parity)
            {
                rdo.Dock = DockStyle.Top;
                rdo.Height = 20;
                QYViewUtils.BindingRadioButton(rdo, _handler, nameof(_handler.Parity), rdo.Tag);
                
                gbx_parity.Controls.Add(rdo);
                rdo.BringToFront();
            }
            gbx_baudrate.Height = gbx_parity.Controls[0].Bottom + 3;

            GroupBox gbx_stopbits = new GroupBox();
            gbx_stopbits.Padding = new Padding(3);
            gbx_stopbits.Dock = DockStyle.Left;
            gbx_stopbits.Width = 103;
            gbx_stopbits.Text = "StopBits";
            RadioButton[] rdo_stopbits = QYViewUtils.CreateEnumRadioButton<System.IO.Ports.StopBits>();
            foreach (var rdo in rdo_stopbits)
            {
                rdo.Dock = DockStyle.Top;
                rdo.Height = 20;
                QYViewUtils.BindingRadioButton(rdo, _handler, nameof(_handler.StopBits), rdo.Tag);

                gbx_stopbits.Controls.Add(rdo);
                rdo.BringToFront();
            }
            gbx_stopbits.Height = gbx_stopbits.Controls[0].Bottom + 3;

            GroupBox gbx_databits = new GroupBox();
            gbx_databits.Padding = new Padding(3);
            gbx_databits.Dock = DockStyle.Left;
            gbx_databits.Width = 65;
            gbx_databits.Text = "DataBits";
            for (int i = 0; i < this._handler.DatabitList.Count; i++)
            {
                RadioButton rdo = new RadioButton();
                rdo.Dock = DockStyle.Top;
                rdo.Height = 20;
                rdo.Text = this._handler.DatabitList[i].ToString();

                QYViewUtils.BindingRadioButton(rdo, this._handler, nameof(this._handler.DataBits), this._handler.DatabitList[i]);
                gbx_databits.Controls.Add(rdo);
                rdo.BringToFront();
            }
            gbx_databits.Height = gbx_databits.Controls[0].Bottom + 3;

            pnl.Controls.Add(cbo_portname);
            this.Controls.Add(gbx_databits);
            this.Controls.Add(gbx_stopbits);
            this.Controls.Add(gbx_parity);
            this.Controls.Add(gbx_baudrate);
            this.Controls.Add(pnl);
        }
    }
}
