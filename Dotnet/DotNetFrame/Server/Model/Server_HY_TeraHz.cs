using DotNet.Comm.Servers;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Server.Model
{
    internal class Server_HY_TeraHz
    {
        internal const int DEFAULT_SENSOR_COUNT = 64;
        internal const int DEFAULT_SENSOR_OFFSET_OBJECT = 500;
        internal const int DEFAULT_SENSOR_OFFSET_MAX = 1000;
        internal const int DEFAULT_SENSOR_OFFSET_BOUNDSCALE = 200;
        internal const int SENSOR_PER_CHIP = 16;

        public event EventHandler<string> ServerLog;
        private TCPServer _server = new TCPServer();
        
        //1. Fields - Communication
        private object _write_lock = new object();
        private byte[] _read_buffer = null;
        private byte[] _write_buffer = null;
        private DateTime _write_time_last = DateTime.Now;


        private int _write_interval = 1000 / 240;

        //1. Fields - Port Item
        private bool _send_allow = false;
        //private bool _cali_allow = false;
        private bool _cmd_send_cali_allow = false;

        public bool IsOpen => this._server.IsOpen;
        public string IP { get => this._server.IP; set => this._server.IP = value; }
        public int PortNo { get => this._server.PortNo; set => this._server.PortNo = value; }
        public byte[] WriteBuffer
        {
            get => this._write_buffer;
            set { lock (this._write_lock) { this._write_buffer = value; } }
        }
        public int Interval
        {
            get => this._write_interval;
            set => this._write_interval = value;
        }


        internal Server_HY_TeraHz()
        {
            this.IP = "127.0.0.1";
            this.PortNo = 5000;
            this._server.Log += (msg) => { this.ServerLog?.Invoke(this, msg); };
            this._server.PeriodicSendEvent += _server_PeriodicSendEvent;
            this._server.CreateResponseEvent += _server_CreateResponseEvent;
        }

        private byte[] _server_PeriodicSendEvent()
        {
            if (this._server.IsOpen == false || this._send_allow == false || this._write_buffer == null) return null;

            byte[] buffer = null;
            lock (this._write_lock)
            {
                buffer = this.WriteBuffer;
            }

            System.Threading.Thread.Sleep(1);

            return buffer;
        }

        private byte[] _server_CreateResponseEvent(byte[] request)
        {
            if (request == null) return null;

            //0. Test용 수신 Buffer 표기
            this.ServerLog?.Invoke(this, $"Buffer: {ByteToString(request)}");

            //1. Buffer 보관
            if (this._read_buffer == null)
                this._read_buffer = request;
            else
            {
                byte[] temp = new byte[this._read_buffer.Length + request.Length];
                Buffer.BlockCopy(this._read_buffer, 0, temp, 0, this._read_buffer.Length);
                Buffer.BlockCopy(request, 0, temp, this._read_buffer.Length, request.Length);

                this._read_buffer = temp;
            }


            //2. Reqeust Frame 추출
            if (this._read_buffer != null)
            {
                if (this._read_buffer.Length < 6) return null;

                int handle = this._read_buffer.Find(Encoding.ASCII.GetBytes("TS"));
                if (handle < 0)
                {
                    this._read_buffer = null;
                    this.ServerLog?.Invoke(this, $"비정상 Request: {this.ByteToString(this._read_buffer)}");

                    this._read_buffer = null;
                    return null;
                }

                byte[] reqBytes = new byte[6];
                Buffer.BlockCopy(this._read_buffer, handle, reqBytes, 0, reqBytes.Length);
                string reqStr = Encoding.ASCII.GetString(reqBytes);
                string cmdG = reqStr.Substring(0, 3);
                int cmdN = Convert.ToInt32(reqStr.Substring(4, 2));
                string resStr = string.Empty;

                this.ServerLog?.Invoke(this, $"Request Bytes: {this.ByteToString(reqBytes)}");

                if (cmdG == "TSN")
                {
                    if (cmdN == 0)
                    {
                        resStr = $"TSN,OK,01,{(this._cmd_send_cali_allow ? "0001" : "0000")}";
                    }
                    else if (cmdN == 1)
                    {
                        resStr = "TSN,OK";
                        this._send_allow = true;
                    }
                    else if (cmdN == 2)
                    {
                        resStr = "TSN,OK";
                        this._send_allow = false;
                    }
                    else if (cmdN == 3)
                    {
                        this._cmd_send_cali_allow = true;
                        resStr = "TSN,OK";
                    }
                    else if (cmdN == 4)
                    {
                        this._cmd_send_cali_allow = false;
                        resStr = "TSN,OK";
                    }
                    else if (cmdN == 5 || cmdN == 6)
                    {
                        //resStr = $"TSN,OK,{this._device_sensor_count.ToString("D4")}";
                        //for (int i = 0; i < this._device_sensor_count; i++)
                        //    resStr += string.Format(",0000");
                    }
                }
                else if (cmdG == "TST")
                {
                    resStr = "TST,OK";
                }

                if (resStr != string.Empty)
                {
                    byte[] resBytes = Encoding.ASCII.GetBytes(resStr);
                    this.ServerLog?.Invoke(this, $"Response Bytes: {ByteToString(resBytes)}");

                    this._read_buffer = null;
                    return resBytes;
                }

                //초기화
                this._read_buffer = null;
            }

            return null;
        }

        private string ByteToString(byte[] bytes)
        {
            string str = string.Empty;

            if (bytes != null && bytes.Length != 0)
            {
                foreach (var b in bytes)
                    str += string.Format(" {0:X2}", b);
            }

            return str;
        }

        internal void Open() => this._server.Open();
        internal void Close() => this._server.Close();
    }
}
