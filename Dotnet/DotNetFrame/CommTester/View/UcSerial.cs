using DotNet.Utils.Views;
using DotNet.CommTester.ViewModel;
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
    public partial class UcSerial : QYUserControl
    {
        internal UcSerial()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;

            ComboBox cbo_portname = new ComboBox();
            cbo_portname.Location = new Point(3, 3);
            cbo_portname.ValueMember = nameof(QYItem.Value);
            cbo_portname.DisplayMember = nameof(QYItem.DisplayText);
            cbo_portname.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo_portname.DataBindings.Add(nameof(ComboBox.DataSource), base.BindingSource, nameof(SerialVM.PortNameList));
            cbo_portname.DataBindings.Add(nameof(ComboBox.SelectedValue), base.BindingSource, nameof(SerialVM.PortName), true, DataSourceUpdateMode.OnPropertyChanged);
            cbo_portname.DataBindings.Add(nameof(ComboBox.Enabled), base.BindingSource, nameof(SerialVM.PortName_EditEnable), true, DataSourceUpdateMode.OnPropertyChanged);

            pnl.Height = cbo_portname.Bottom + 3;

            QYRadioGroup rdo_baudrate = new QYRadioGroup();
            rdo_baudrate.Dock = DockStyle.Left;
            rdo_baudrate.Width = 80;
            rdo_baudrate.Caption = "BaudRate";
            rdo_baudrate.ValueMember = nameof(QYItem.Value);
            rdo_baudrate.DisplayMember = nameof(QYItem.DisplayText);
            rdo_baudrate.DataBindings.Add("DataSource", base.BindingSource, nameof(SerialVM.BaudRateList));
            rdo_baudrate.DataBindings.Add("SelectedValue", base.BindingSource, nameof(SerialVM.BaudRate), true, DataSourceUpdateMode.OnPropertyChanged);

            QYRadioGroup rdo_parity = new QYRadioGroup();
            rdo_parity.Dock = DockStyle.Left;
            rdo_parity.Width = 65;
            rdo_parity.Caption = "Parity";
            rdo_parity.ValueMember = nameof(QYItem.Value);
            rdo_parity.DisplayMember = nameof(QYItem.DisplayText);
            rdo_parity.DataBindings.Add("DataSource", base.BindingSource, nameof(SerialVM.ParityList));
            rdo_parity.DataBindings.Add("SelectedValue", base.BindingSource, nameof(SerialVM.Parity), true, DataSourceUpdateMode.OnPropertyChanged);

            QYRadioGroup rdo_stopbits = new QYRadioGroup();
            rdo_stopbits.Dock = DockStyle.Left;
            rdo_stopbits.Width = 103;
            rdo_stopbits.Caption = "StopBits";
            rdo_stopbits.ValueMember = nameof(QYItem.Value);
            rdo_stopbits.DisplayMember = nameof(QYItem.DisplayText);
            rdo_stopbits.DataBindings.Add("DataSource", base.BindingSource, nameof(SerialVM.StopBitsList));
            rdo_stopbits.DataBindings.Add("SelectedValue", base.BindingSource, nameof(SerialVM.StopBits), true, DataSourceUpdateMode.OnPropertyChanged);

            QYRadioGroup rdo_databits = new QYRadioGroup();
            rdo_databits.Dock = DockStyle.Left;
            rdo_databits.Width = 65;
            rdo_databits.Caption = "DataBits";
            rdo_databits.ValueMember = nameof(QYItem.Value);
            rdo_databits.DisplayMember = nameof(QYItem.DisplayText);
            rdo_databits.DataBindings.Add("DataSource", base.BindingSource, nameof(SerialVM.DataBitsList));
            rdo_databits.DataBindings.Add("SelectedValue", base.BindingSource, nameof(SerialVM.DataBits), true, DataSourceUpdateMode.OnPropertyChanged);

            pnl.Controls.Add(cbo_portname);
            this.Controls.Add(rdo_databits);
            this.Controls.Add(rdo_stopbits);
            this.Controls.Add(rdo_parity);
            this.Controls.Add(rdo_baudrate);
            this.Controls.Add(pnl);
        }
    }
}
