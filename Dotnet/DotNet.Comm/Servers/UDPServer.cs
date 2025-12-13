using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Comm.Servers
{
    public class UDPServer : ServerBase
    {
        public enum ServerStyle
        {
            OnlyRead,
            Request_Response,
            OnlyWrite,
        }

        private UdpClient _server = null;
        private Dictionary<IPEndPoint, Thread> _clientList = new Dictionary<IPEndPoint, Thread>();

        private int _portNo = 5000;
        private string _ip = IPAddress.Any.MapToIPv4().ToString();
        private ServerStyle _style = ServerStyle.OnlyRead;

        /// <summary>
        /// Port 번호
        /// </summary>
        public int PortNo { get => _portNo; set => _portNo = value; }
        /// <summary>
        /// IP 주소
        /// </summary>
        public string IP { get => _ip; set => _ip = value; }
        /// <summary>
        /// 송수신 Style
        /// </summary>
        public ServerStyle Style { get => _style; set => _style = value; }
        private IPAddress IPAddress => IPAddress.Parse(this._ip);


        public override void Open()
        {
            if(base.IsOpen == false)
            {
                this._server = new UdpClient(this.PortNo, this.IPAddress.AddressFamily);
                this._server.Client.ReceiveTimeout = 5;
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
            byte[] sendBytes = null;
            IPEndPoint lastEndPoint = null;

            while (base.IsOpen)
            {
                try
                {
                    if(this._style == ServerStyle.OnlyRead)
                    {
                        UdpReceiveResult rst = await this._server.ReceiveAsync();

                        base.RunCreateResponse(rst.Buffer);
                    }
                    else if (this._style == ServerStyle.Request_Response)
                    {
                        UdpReceiveResult rst = await this._server.ReceiveAsync();

                        if (rst.Buffer != null || rst.Buffer.Length > 0)
                        {
                            sendBytes = base.RunCreateResponse(rst.Buffer);

                            await this._server.SendAsync(rst.Buffer, rst.Buffer.Length, rst.RemoteEndPoint);

                            sendBytes = null;
                        }
                    }
                    else if (this._style == ServerStyle.OnlyWrite)
                    {
                        if(lastEndPoint == null)
                        {
                            UdpReceiveResult rst = await this._server.ReceiveAsync();
                            lastEndPoint = rst.RemoteEndPoint;
                        }
                        else
                        {
                            sendBytes = RunPeriodicSend();

                            if (sendBytes != null && sendBytes.Length > 0)
                            {
                                await this._server.SendAsync(sendBytes, sendBytes.Length, lastEndPoint);
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public override void Close()
        {
            try
            {
                if(base.IsOpen == true)
                {
                    this._server.Close();

                    base.IsOpen = false;
                    base.RunLog("UDP Server Close");
                }
                else
                {
                    base.RunLog("Already Close");
                }
            }
            catch
            {

            }
        }
    }
}
