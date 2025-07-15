using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet.Server.Servers
{
    public class TCPServer : ServerBase
    {
        /// <summary>
        /// Data Client에서 처리를 위한 이벤트 Handler
        /// </summary>
        /// <param name="data">이벤트에서 사용할 Data</param>
        /// <returns>Client에게 전송할 Data</returns>
        public delegate byte[] ClientHandler(byte[] data);
        /// <summary>
        /// Data Receive 처리 이벤트
        /// </summary>
        /// <returns>Client에게 전송할 Data</returns>
        public event ClientHandler ClientActiveEvent;

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

        public TCPServer(ServerSendType sendType):base(sendType) { }

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
            byte[] returnBytes = null;
            int bytesLength;

            try
            {
                if (base.SendType == ServerSendType.ReadWrite || base.SendType == ServerSendType.ReadOnly)
                {
                    //읽을때마다 새로 읽은 Data buffer에 덮어씌우기
                    while ((bytesLength = stream.Read(bufferFull, 0, bufferFull.Length)) > 0)
                    {
                        //Data Receive에 따른 무언가 처리
                        byte[] buffer = new byte[bytesLength];
                        Buffer.BlockCopy(bufferFull, 0, buffer, 0, bytesLength);

                        returnBytes = this.ClientActiveEvent?.Invoke(buffer);

                        //보낸 Client에게 되돌려주는 Data
                        if (returnBytes != null && returnBytes.Length != 0)
                        {
                            stream.Write(returnBytes, 0, returnBytes.Length);
                        }
                    }
                }
                else if(base.SendType == ServerSendType.WriteRead)
                {
                    while (true)
                    {
                        returnBytes = this.ClientActiveEvent?.Invoke(null);

                        //보낸 Client에게 되돌려주는 Data
                        if (returnBytes != null && returnBytes.Length != 0)
                        {
                            stream.Write(returnBytes, 0, returnBytes.Length);

                            returnBytes = null;
                        }

                        //Client의 Request 요청 처리
                        byte[] receiveBytes = null;

                        while (stream.DataAvailable)
                        {
                            bytesLength = stream.Read(bufferFull, 0, bufferFull.Length);
                            if (bytesLength == 0) continue;

                            //Data Receive에 따른 무언가 처리
                            byte[] buffer = new byte[bytesLength];
                            Buffer.BlockCopy(bufferFull, 0, buffer, 0, bytesLength);

                            if (receiveBytes == null || receiveBytes.Length == 0)
                                receiveBytes = buffer;
                            else
                            {
                                byte[] temp = new byte[receiveBytes.Length + bytesLength];
                                Buffer.BlockCopy(receiveBytes, 0, temp, 0, receiveBytes.Length);
                                Buffer.BlockCopy(buffer, 0, temp, receiveBytes.Length, bytesLength);

                                receiveBytes = temp;
                            }

                            returnBytes = this.ClientActiveEvent?.Invoke(receiveBytes);

                            //보낸 Client에게 되돌려주는 Data
                            if (returnBytes != null && returnBytes.Length != 0)
                            {
                                stream.Write(returnBytes, 0, returnBytes.Length);

                                returnBytes = null;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                base.RunLog(string.Format("Error : {0}\r\nTrace:{1}", ex.Message, ex.StackTrace));
            }
            finally
            {
                if (client.Connected == true)
                {
                    base.RunLog(string.Format("Client Receive End - IP : {0} / Port : {1}", (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), (client.Client.RemoteEndPoint as IPEndPoint).Port));
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    this._clientList.Remove(client);
                }
            }
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
