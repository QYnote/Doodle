using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Comm.Servers
{
    public class TCPServer : ServerBase
    {
        private TcpListener _server = null;
        private List<TcpClient> _clientList = new List<TcpClient>();

        public int PortNo { get; set; } = 5000;
        public string IP { get; set; } = IPAddress.Any.MapToIPv4().ToString();

        #region 편의성 Property

        public IPAddress IPAddress
        {
            get
            {
                return IPAddress.Parse(this.IP);
            }
        }

        #endregion 편의성 Property

        /// <summary>서버 열기</summary>
        public override void Open()
        {
            if (base.IsOpen == false)
            {
                this._server = new TcpListener(this.IPAddress, this.PortNo);
                this._server.Start();

                base.IsOpen = true;

                //접속 확인용 Thread를 생성하여 접속시도하는 client 확인
                Thread ServerThread = new Thread(AcceptClient);
                ServerThread.Start();

                base.RunLog(string.Format("TCP Server Open - IP : {0} / PortNo : {1}", this.IPAddress, this.PortNo));
            }
            else
            {
                base.RunLog("Already Open");
            }
        }

        /// <summary>접속시도하는 Client 확인</summary>
        private void AcceptClient()
        {
            while (base.IsOpen == true)
            {
                try
                {
                    TcpClient client = this._server.AcceptTcpClient();    //접속시도하는 Client 확인
                    this._clientList.Add(client);

                    //접속한 Client 데이터 Receive 처리해주는 Thread 생성
                    Thread clientThread = new Thread(() => ClientMethod(client));
                    clientThread.Start();
                    base.RunLog(string.Format("Client Accepted - IP : {0} / Port : {1}", (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), (client.Client.RemoteEndPoint as IPEndPoint).Port));
                }
                catch(Exception ex)
                {
                    if (ex.HResult != -2147467259)
                    {
                        base.RunLog("Client Accept Error");
                    }
                }
            }
        }

        /// <summary>
        /// 접속한 Client에서 데이터를 Receive 받아 그에따른 처리 진행
        /// </summary>
        /// <param name="client">연결된 TCP Client</param>
        private void ClientMethod(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] bufferFull = new byte[1024];
            byte[] sendBytes = null;
            int bytesLength;

            while (client.Connected)
            {
                try
                {
                    //연결 검사
                    if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                    {
                        base.RunLog(string.Format("Client Receive End - IP : {0} / Port : {1}", (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), (client.Client.RemoteEndPoint as IPEndPoint).Port));
                        client.Client.Shutdown(SocketShutdown.Both);
                        client.Close();
                        this._clientList.Remove(client);
                        break;
                    }

                    //수신된 Data 검사
                    if (stream.DataAvailable)
                    {
                        bytesLength = stream.Read(bufferFull, 0, bufferFull.Length);

                        if (bytesLength > 0)
                        {
                            //Socket Buffer 최대치 검사
                            if (bufferFull.Length < bytesLength)
                                bufferFull = new byte[bufferFull.Length * 2];

                            byte[] buffer = new byte[bytesLength];
                            Buffer.BlockCopy(bufferFull, 0, buffer, 0, bytesLength);

                            //Client Active Event에 읽어들인 Buffer 전송
                            sendBytes = base.RunCreateResponse(buffer);

                            //보낸 Client에게 되돌려주는 Data
                            if (sendBytes != null && sendBytes.Length != 0)
                            {
                                stream.Write(sendBytes, 0, sendBytes.Length);
                                sendBytes = null;   //전송 후 초기화
                            }
                        }
                    }

                    //주기적으로 전송하는 Data
                    sendBytes = RunPeriodicSend();
                    if (sendBytes != null && sendBytes.Length != 0)
                    {
                        stream.Write(sendBytes, 0, sendBytes.Length);
                    }
                }
                catch (Exception ex)
                {
                    base.RunLog(string.Format("Error : {0}\r\nTrace:{1}", ex.Message, ex.StackTrace));
                }
            }//End While
        }

        /// <summary>서버 닫기</summary>
        public override void Close()
        {
            try
            {
                if (base.IsOpen == true)
                {
                    base.IsOpen = false;

                    this._server.Stop();
                    foreach (TcpClient client in this._clientList)
                    {
                        client.Client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    this._clientList.Clear();

                    base.RunLog("TCP Server Close");
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
