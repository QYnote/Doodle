using System.IO.Ports;

namespace DotNet.Comm.Transport
{
    public class QYSerialPort : ITransport
    {
        private SerialPort _serialPort = new SerialPort();

        public string PortName
        {
            get { return this._serialPort.PortName; }
            set
            {
                if (this.IsOpen == false)
                    this._serialPort.PortName = value;
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
        public bool IsOpen
        {
            get { return this._serialPort.IsOpen; }
        }

        public QYSerialPort()
        {
            this.Parity = Parity.None;
            this.StopBits = StopBits.One;
            this.DataBits = 8;
            this.BaudRate = 9600;
        }

        public void Open()
        {
            if (this.IsOpen == false)
                this._serialPort.Open();
        }

        public void Close()
        {
            if(this.IsOpen)
                this._serialPort.Close();
        }

        public byte[] Read()
        {
            byte[] readBytes = null;

            if (this.IsOpen)
            {
                if (this._serialPort.BytesToRead > 0)
                {
                    readBytes = new byte[this._serialPort.BytesToRead];

                    this._serialPort.Read(readBytes, 0, readBytes.Length);
                }
            }

            return readBytes;
        }
        public void Write(byte[] bytes)
        {
            if (this.IsOpen)
            {
                this._serialPort.Write(bytes, 0, bytes.Length);
            }
        }

        public void Initialize()
        {
            if (this._serialPort != null)
                this._serialPort.Close();
        }
    }
}
