using DotNet.Server.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dotnet.Comm.Servers
{
    internal class UDPServer : ServerBase
    {
        private UdpClient _server = null;

        private int _portNo = 5000;
        private string _ip = IPAddress.Any.MapToIPv4().ToString();

        /// <summary>
        /// Port 번호
        /// </summary>
        public int PortNo { get => _portNo; set => _portNo = value; }
        /// <summary>
        /// IP 주소
        /// </summary>
        public string Ip { get => _ip; set => _ip = value; }

        private IPAddress IPAddress => IPAddress.Parse(this._ip);

        public override void Open()
        {
            if(base.IsOpen == false)
            {
                this._server = new UdpClient(this.PortNo, this.IPAddress.AddressFamily);
                base.IsOpen = true;


                Thread clientThread = new Thread(() => AcceptClient());
                clientThread.Start();

                base.RunLog(string.Format("UDP Server Open - IP : {0} / PortNo : {1}", this.IPAddress, this.PortNo));
            }
            else
            {
                base.RunLog("Already Open");
            }
        }

        private async void AcceptClient()
        {
            while (base.IsOpen)
            {
                try
                {
                    UdpReceiveResult rst = await this._server.ReceiveAsync();

                    _ = ClientMethod(rst.RemoteEndPoint, rst.Buffer);
                }
                catch
                {

                }
            }
        }

        private async Task ClientMethod(IPEndPoint client, byte[] buffer)
        {
            if(buffer != null || buffer.Length > 0)
            {

            }
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }
    }
}
