using DotNet.Comm.Protocols;
using DotNet.Comm.Servers;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Server.Model
{
    internal class Server_Modbus
    {
        public event EventHandler<string> ServerLog;

        private TCPServer _server = new TCPServer();
        private ProtocolBase _protocol = null;

        private byte[] _read_buffer = null;

        private Dictionary<int, object> _server_items = new Dictionary<int, object>();

        public bool IsOpen => this._server.IsOpen;
        public string IP { get => this._server.IP; set => this._server.IP = value; }
        public int PortNo { get => this._server.PortNo; set => this._server.PortNo = value; }

        internal Server_Modbus()
        {
            this.IP = "127.0.0.1";
            this.PortNo = 5000;
            this._server.Log += (msg) => { this.ServerLog?.Invoke(this, msg); };
            this._server.CreateResponseEvent += _server_CreateResponseEvent;

            this._protocol = new Modbus(false);
        }

        private byte[] _server_CreateResponseEvent(byte[] request)
        {
            if (request == null) return null;

            //0. Test용 수신 Buffer 표기
            this.ServerLog?.Invoke(this, $"buffer: {ByteToString(request)}");

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
                byte[] reqFrame = this._protocol.Request_ExtractFrame(this._read_buffer);

                if (reqFrame != null)
                {
                    this.ServerLog?.Invoke(this, $"Request Frame: {ByteToString(reqFrame)}");
                    //3. Response Frame 생성
                    byte[] resFrame = this._protocol.Request_CreateResponse(reqFrame, this._server_items);

                    if (resFrame != null)
                    {
                        this.ServerLog?.Invoke(this, $"Response Frame: {ByteToString(resFrame)}");

                        this._read_buffer = null;
                        return resFrame;
                    }
                }
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
