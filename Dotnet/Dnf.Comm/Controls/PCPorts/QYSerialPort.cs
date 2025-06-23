using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls.PCPorts
{
    public class QYSerialPort : PCPortBase
    {
        private SerialPort _serialPort = new SerialPort();
        private string _comName;

        public override string PortName
        {
            get { return this._comName; }
            set
            { 
                this._comName = value;
                this._serialPort.PortName = this._comName;
            }
        }

        public int BaudRate
        {
            get { return this._serialPort.BaudRate; }
            set { this._serialPort.BaudRate = value; }
        }

        public int DataBits
        {
            get { return this._serialPort.DataBits; }
            set { this._serialPort.DataBits = value; }
        }
        public Parity Parity
        {
            get { return this._serialPort.Parity; }
            set { this._serialPort.Parity = value; }
        }
        public StopBits StopBits
        {
            get { return this._serialPort.StopBits; }
            set
            {
                if (value == StopBits.None
                    || value == StopBits.OnePointFive)
                    this._serialPort.StopBits = StopBits.One;
                else
                    this._serialPort.StopBits = value;
            }
        }

        public override bool IsOpen
        {
            get { return this._serialPort.IsOpen; }
        }

        public QYSerialPort() : base(PortType.Serial)
        {
            this.Parity = Parity.None;
            this.StopBits = StopBits.One;
            this.DataBits = 8;
            this.BaudRate = 9600;
        }

        public override bool Open()
        {
            if (this.IsOpen == false)
            {
                try
                {
                    this._serialPort.Open();

                    return true;
                }
                catch
                {
                    Debug.WriteLine("[Error]Port Open Fail");
                }
            }
            else
            {
                Debug.WriteLine("[Alart]Port already Open");
            }

            return false;
        }

        public override bool Close()
        {
            if(this.IsOpen)
            {
                try
                {
                    this._serialPort.Close();

                    return true;
                }
                catch
                {
                    Debug.WriteLine("[Error]Port Close Fail");
                }
            }
            else
            {
                Debug.WriteLine("[Alart]Port already Close");
            }

            return false;
        }

        public override byte[] Read()
        {
            byte[] readBytes = null;

            try
            {
                if (this.IsOpen)
                {
                    if(this._serialPort.BytesToRead > 0)
                    {
                        readBytes = new byte[this._serialPort.BytesToRead];

                        this._serialPort.Read(readBytes, 0, readBytes.Length);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("[Error]Port Read Fail");
            }

            return readBytes;
        }
        public override bool Write(byte[] bytes)
        {
            try
            {
                if (this.IsOpen)
                {
                    this._serialPort.Write(bytes, 0, bytes.Length);

                    return true;
                }
            }
            catch
            {
                Debug.WriteLine("[Error]Port Write Fail");
            }

            return false;
        }
    }
}
