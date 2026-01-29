using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel
{
    internal class SerialVM : QYViewModel
    {
        private QYSerialPort _serialPort = null;

        private BindingList<QYItem> _portname_list = new BindingList<QYItem>();
        private List<QYItem> _baudrate_list = new List<QYItem>();
        private List<QYItem> _databit_list = new List<QYItem>();
        private List<QYItem> _parity_list = new List<QYItem>();
        private List<QYItem> _stopbits_list = new List<QYItem>();

        public string PortName
        {
            get { return this._serialPort.PortName; }
            set
            {
                if (this.PortName != value
                    && this._serialPort.IsOpen == false)
                {
                    this._serialPort.PortName = value;

                    base.OnPropertyChanged(nameof(this.PortName));
                }
            }
        }
        public int BaudRate
        {
            get { return this._serialPort.BaudRate; }
            set
            {
                if (this.BaudRate != value)
                {
                    this._serialPort.BaudRate = value;

                    base.OnPropertyChanged(nameof(this.BaudRate));
                }
            }
        }
        public int DataBits
        {
            get { return this._serialPort.DataBits; }
            set
            {
                if (this.DataBits != value)
                {
                    this._serialPort.DataBits = value;

                    base.OnPropertyChanged(nameof(this.DataBits));
                }
            }
        }
        public Parity Parity
        {
            get { return this._serialPort.Parity; }
            set
            {
                if (this.Parity != value)
                {
                    this._serialPort.Parity = value;

                    base.OnPropertyChanged(nameof(this.Parity));
                }
            }
        }
        public StopBits StopBits
        {
            get { return this._serialPort.StopBits; }
            set
            {
                if (this.StopBits != value)
                {
                    this._serialPort.StopBits = value;

                    base.OnPropertyChanged(nameof(this.StopBits));
                }
            }
        }

        public BindingList<QYItem> PortNameList => this._portname_list;
        public List<QYItem> BaudRateList => this._baudrate_list;
        public List<QYItem> DataBitsList => this._databit_list;
        public List<QYItem> ParityList => this._parity_list;
        public List<QYItem> StopBitsList => this._stopbits_list;

        internal SerialVM(QYSerialPort serialPort)
        {
            this._serialPort = serialPort;

            this.InitList();
        }

        private void InitList()
        {
            List<int> baudratelist = new List<int>()
            {
                9600, 19200, 38400, 57600, 115200, 921600
            };
            List<int> databits = new List<int>()
            {
                7, 8
            };

            this.RefreshPortList();
            foreach (var baudrate in baudratelist)
            {
                QYItem item = new QYItem(baudrate);
                item.DisplayText = baudrate.ToString("#,#");

                this.BaudRateList.Add(item);
            }

            foreach (var item in databits)
                this.DataBitsList.Add(new QYItem(item));

            this._parity_list = QYViewUtils.EnumToItem<Parity>().ToList();
            this._stopbits_list = QYViewUtils.EnumToItem<StopBits>().ToList();
        }

        public void RefreshPortList()
        {
            this.PortNameList.Clear();

            foreach (var item in SerialPort.GetPortNames())
                this.PortNameList.Add(new QYItem(item));

            if (this.PortNameList.Count > 0)
                this.PortName = (string)this.PortNameList[0].Value;
        }
    }
}
