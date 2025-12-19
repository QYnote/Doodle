using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel
{
    internal class SerialHandler : QYViewModelHandler
    {
        private OSPortBase _base;

        private List<string> _portname_list;
        private List<int> _baudrate_list = new List<int>()
        {
            9600, 19200, 38400, 57600, 115200, 921600
        };
        private List<int> _databit_list = new List<int>()
        {
            7, 8
        };

        private QYSerialPort Serial => this._base as QYSerialPort;

        public List<string> PortList => this._portname_list;
        public List<int> BaudRateList => _baudrate_list;
        public List<int> DatabitList => _databit_list;

        public bool IsOpen => this.Serial?.IsOpen ?? false;
        public string PortName
        {
            get => this.Serial?.PortName ?? string.Empty;
            set
            {
                if (this.Serial == null) return;

                if (this.Serial.PortName != value
                    && this.Serial.IsOpen == false)
                {
                    this.Serial.PortName = value;
                }
            }
        }
        public int BaudRate
        {
            get => this.Serial?.BaudRate ?? 9600;
            set
            {
                if (this.Serial == null) return;

                if (this.Serial.BaudRate != value
                    && this.Serial.IsOpen == false)
                {
                    this.Serial.BaudRate = value;
                }
            }
        }
        public System.IO.Ports.Parity Parity
        {
            get => this.Serial?.Parity ?? System.IO.Ports.Parity.None;
            set
            {
                if (this.Serial == null) return;

                if (this.Serial.Parity != value
                    && this.Serial.IsOpen == false)
                {
                    this.Serial.Parity = value;
                }
            }
        }
        public System.IO.Ports.StopBits StopBits
        {
            get => this.Serial?.StopBits ?? System.IO.Ports.StopBits.One;
            set
            {
                if (this.Serial == null) return;

                if (this.Serial.StopBits != value
                    && this.Serial.IsOpen == false)
                {
                    this.Serial.StopBits = value;
                }
            }
        }
        public int DataBits
        {
            get => this.Serial?.DataBits ?? 8;
            set
            {
                if (this.Serial == null) return;

                if (this.Serial.DataBits != value
                    && this.Serial.IsOpen == false)
                {
                    this.Serial.DataBits = value;
                }
            }
        }

        

        internal SerialHandler(OSPortBase osport) 
        {
            this._base = osport;

            this._portname_list = System.IO.Ports.SerialPort.GetPortNames().ToList();
            if (this._portname_list.Count > 0)
                this.PortName = this._portname_list[0];
        }
    }
}
