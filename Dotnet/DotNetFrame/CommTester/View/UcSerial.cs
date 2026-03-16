using DotNet.Utils.ViewModel;
using DotNet.Utils.Views;
using DotNetFrame.CommTester.ViewModel.Port;
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
        private ComboBox cbo_portname = new ComboBox();
        private NumericUpDown num_baudrate = new NumericUpDown();
        private NumericUpDown num_databits = new NumericUpDown();
        private ComboBox cbo_parity = new ComboBox();
        private ComboBox cbo_stopbits = new ComboBox();

        private SerialVM VM { get; }

        internal UcSerial(SerialVM viewmodel)
        {
            this.VM = viewmodel;

            this.BindingControl();
            InitUI();
        }

        private void BindingControl()
        {
            this.cbo_portname.DataSource = this.VM.PortList;
            this.cbo_portname.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbo_portname.DataBindings.Add(nameof(ComboBox.SelectedValue), this.VM, nameof(SerialVM.PortName), true, DataSourceUpdateMode.OnPropertyChanged);

            this.num_baudrate.DecimalPlaces = 0;
            this.num_baudrate.TextAlign = HorizontalAlignment.Right;
            this.num_baudrate.Minimum = 0;
            this.num_baudrate.Maximum = int.MaxValue;
            this.num_baudrate.DataBindings.Add(nameof(NumericUpDown.Value), this.VM, nameof(SerialVM.BaudRate), true, DataSourceUpdateMode.OnPropertyChanged);

            this.num_databits.DecimalPlaces = 0;
            this.num_databits.TextAlign = HorizontalAlignment.Right;
            this.num_databits.Minimum = 7;
            this.num_databits.Maximum = 8;
            this.num_databits.DataBindings.Add(nameof(NumericUpDown.Value), this.VM, nameof(SerialVM.DataBits), true, DataSourceUpdateMode.OnPropertyChanged);

            this.cbo_parity.ValueMember = nameof(QYItem.Value);
            this.cbo_parity.DisplayMember = nameof(QYItem.DisplayText);
            this.cbo_parity.DataSource = this.VM.ParityList;
            this.cbo_parity.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbo_parity.DataBindings.Add(nameof(ComboBox.SelectedValue), this.VM, nameof(SerialVM.Parity), true, DataSourceUpdateMode.OnPropertyChanged);

            this.cbo_stopbits.ValueMember = nameof(QYItem.Value);
            this.cbo_stopbits.DisplayMember = nameof(QYItem.DisplayText);
            this.cbo_stopbits.DataSource = this.VM.StopbitsList;
            this.cbo_stopbits.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cbo_stopbits.DataBindings.Add(nameof(ComboBox.SelectedValue), this.VM, nameof(SerialVM.StopBits), true, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void InitUI()
        {
            this.Dock = DockStyle.Top;
            this.Height = 134;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.Percent));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent));

            Label lbl_portName = new Label();
            lbl_portName.Dock = DockStyle.Fill;
            lbl_portName.TextAlign = ContentAlignment.MiddleLeft;
            lbl_portName.Text = "Port 목록";

            Label lbl_baudrate = new Label();
            lbl_baudrate.Dock = DockStyle.Fill;
            lbl_baudrate.TextAlign = ContentAlignment.MiddleLeft;
            lbl_baudrate.Text = "Baudrate";

            Label lbl_datbits = new Label();
            lbl_datbits.Dock = DockStyle.Fill;
            lbl_datbits.TextAlign = ContentAlignment.MiddleLeft;
            lbl_datbits.Text = "DataBits";

            Label lbl_parity = new Label();
            lbl_parity.Dock = DockStyle.Fill;
            lbl_parity.TextAlign = ContentAlignment.MiddleLeft;
            lbl_parity.Text = "Parity";

            Label lbl_stopbits = new Label();
            lbl_stopbits.Dock = DockStyle.Fill;
            lbl_stopbits.TextAlign = ContentAlignment.MiddleLeft;
            lbl_stopbits.Text = "StopBits";

            layout.Controls.Add(lbl_portName, 0, 0);
            layout.Controls.Add(lbl_baudrate, 0, 1);
            layout.Controls.Add(lbl_datbits , 0, 2);
            layout.Controls.Add(lbl_parity  , 0, 3);
            layout.Controls.Add(lbl_stopbits, 0, 4);

            layout.Controls.Add(this.cbo_portname, 1, 0);
            layout.Controls.Add(this.num_baudrate, 1, 1);
            layout.Controls.Add(this.num_databits, 1, 2);
            layout.Controls.Add(this.cbo_parity, 1, 3);
            layout.Controls.Add(this.cbo_stopbits, 1, 4);

            this.Controls.Add(layout);
        }
    }
}
