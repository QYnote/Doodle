using DotNet.Comm.Transport;
using DotNet.Utils.ViewModel;
using DotNetFrame.CommTester.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel.Port
{
    internal class SerialVM : TransportVM
    {
        private BindingList<string> _list_port = new BindingList<string>();
        private List<QYItem> _list_parity = new List<QYItem>();
        private List<QYItem> _list_stopbits = new List<QYItem>();

        private QYSerialPort Serial { get; }
        public BindingList<string> PortList => this._list_port;
        public List<QYItem> ParityList => this._list_parity;
        public List<QYItem> StopbitsList => this._list_stopbits;

        public string PortName
        {
            get => this.Serial.PortName;
            set => this.Serial.PortName = value;
        }
        public int BaudRate
        {
            get => this.Serial.BaudRate;
            set => this.Serial.BaudRate = value;
        }
        public int DataBits
        {
            get => this.Serial.DataBits;
            set => this.Serial.DataBits = value;
        }
        public Parity Parity
        {
            get => this.Serial.Parity;
            set => this.Serial.Parity = value;
        }
        public StopBits StopBits
        {
            get => this.Serial.StopBits;
            set => this.Serial.StopBits = value;
        }
        public bool IsConnected => this.Serial.IsOpen;
        
        internal SerialVM(QYSerialPort port)
        {
            this.Serial = port;

            this.RefreshPortList();
            this._list_parity = QYUtils_ViewModel.GetEnumItems<Parity>().ToList();
            this._list_stopbits = QYUtils_ViewModel.GetEnumItems<StopBits>().ToList();
        }

        internal void RefreshPortList()
        {
            this._list_port.Clear();
            foreach (var name in SerialPort.GetPortNames())
                this._list_port.Add(name);
        }
    }
}
