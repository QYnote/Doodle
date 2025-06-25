using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls.DevicePort
{
    public class QYSerailPort : DevicePortBase
    {
        private SerialPort _port = new SerialPort();

        public override string PortName
        {
            get
            {
                return this._port.PortName;
            }
            set
            {
                this._port.PortName = value;
            }
        }

        public override bool IsOpen
        {
            get
            {
                return this._port.IsOpen;
            }
        }

        /// <summary>
        /// SerialPort Baudrate
        /// </summary>
        public int Baudrate
        {
            get
            {
                return this._port.BaudRate;
            }
            set
            {
                this._port.BaudRate = value;
            }
        }

        /// <summary>
        /// SerialPort DataBits
        /// </summary>
        public int DataBits
        {
            get
            {
                return this._port.DataBits;
            }
            set
            {
                this._port.DataBits = value;
            }
        }

        /// <summary>
        /// SerialPort Parity
        /// </summary>
        public Parity Parity
        {
            get
            {
                return this._port.Parity;
            }
            set
            {
                this._port.Parity = value;
            }
        }

        /// <summary>
        /// SerialPort StopBits
        /// </summary>
        public StopBits StopBits
        {
            get
            {
                return this._port.StopBits;
            }
            set
            {
                this._port.StopBits = value;
            }
        }

        public QYSerailPort() { }

        public override bool Open()
        {
            try
            {
                if (this.PortName != string.Empty && this.IsOpen == false)
                {
                    this._port.Open();

                    return true;
                }
            }
            catch
            {
                Debug.WriteLine("[Error]Port Open Fail");
            }

            return false;
        }

        public override bool Close()
        {
            try
            {
                if (this.PortName != string.Empty
                    && this.IsOpen == true)
                {
                    this._port.Close();

                    return true;
                }
            }
            catch
            {
                Debug.WriteLine("[Error]Port Close Fail");
            }

            return false;
        }

        public override byte[] Read()
        {
            try
            {
                if (this.PortName != string.Empty
                    && this.IsOpen == true
                    && this._port.BytesToRead > 0)
                {
                    byte[] buffer = new byte[this._port.BytesToRead];

                    this._port.Read(buffer, 0, buffer.Length);

                    return buffer;
                }
            }
            catch
            {
                Debug.WriteLine("[Error]Port Read Fail");
            }

            return null;
        }

        public override bool Write(byte[] data)
        {
            try
            {
                if (this.PortName != string.Empty
                    && this.IsOpen == true)
                {
                    this._port.Write(data, 0, data.Length);

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
