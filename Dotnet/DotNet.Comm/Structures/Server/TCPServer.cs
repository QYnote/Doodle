using DotNet.Utils.Controls;
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

namespace DotNet.Server.Server
{
    internal class TCPServer : ServerBase
    {
        private TcpListener server = null;
        private List<TcpClient> clientList = new List<TcpClient>();
        /// <summary>Data Receive 처리 이벤트 / byte[] Data, int bytesLength / return : 보낸 Client에게 되보내줄 Data</summary>
        internal ReceiveHandler ReceiveActiveEvent { get; set; }
        /// <summary>
        /// Data Receive 처리 delegate
        /// </summary>
        /// <param name="data">읽어들인 Bytes</param>
        /// <param name="bytesLength">읽은 Bytes 길이</param>
        /// <returns>보낸 Client에게 보내줄 Data</returns>
        internal delegate byte[] ReceiveHandler(byte[] data, int bytesLength);

        internal TCPServer(ServerSendType sendType, TcpListener listener):base(sendType)
        {
            this.server = listener;
        }

        /// <summary>서버 열기</summary>
        internal override void Open()
        {
            if (base.IsOpen == false)
            {
                this.server.Start();

                base.IsOpen = true;

                //접속 확인용 Thread를 생성하여 접속시도하는 client 확인
                Thread ServerThread = new Thread(AcceptClient);
                ServerThread.Start();
                base.SendMsg?.Invoke("TCP Server Open");
            }
            else
            {
                base.SendMsg?.Invoke("Already Open");
            }
        }

        /// <summary>접속시도하는 Client 확인</summary>
        private void AcceptClient()
        {
            while (base.IsOpen == true)
            {
                try
                {
                    TcpClient client = this.server.AcceptTcpClient();    //접속시도하는 Client 확인
                    this.clientList.Add(client);

                    //접속한 Client 데이터 Receive 처리해주는 Thread 생성
                    Thread clientThread = new Thread(() => ClientMethod(client));
                    clientThread.Start();
                    base.SendMsg?.Invoke(string.Format("Client Accepted / IP : {0}, Port : {1}", (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), (client.Client.RemoteEndPoint as IPEndPoint).Port));
                }
                catch(Exception ex)
                {
                    if (ex.HResult != -2147467259)
                    {
                        base.SendMsg?.Invoke("Client Accept Error");
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

                        returnBytes = this.ReceiveActiveEvent?.Invoke(buffer, bytesLength);

                        //보낸 Client에게 되돌려주는 Data
                        if (returnBytes != null || returnBytes.Length != 0)
                        {
                            stream.Write(returnBytes, 0, returnBytes.Length);
                        }
                    }
                }
                else if(base.SendType == ServerSendType.WriteRead)
                {
                    while (true)
                    {
                        returnBytes = this.ReceiveActiveEvent?.Invoke(null, 0);

                        if (returnBytes != null || returnBytes.Length != 0)
                        {
                            stream.Write(returnBytes, 0, returnBytes.Length);

                            returnBytes = null;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                base.SendMsg?.Invoke(string.Format("Error : {0}", ex.Message));
            }
            finally
            {
                if (client.Connected == true)
                {
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    this.clientList.Remove(client);
                    base.SendMsg?.Invoke(string.Format("Client Receive End / IP : {0}, Port : {1}", (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(), (client.Client.RemoteEndPoint as IPEndPoint).Port));
                }
            }
        }

        /// <summary>서버 닫기</summary>
        internal override void Close()
        {
            if (base.IsOpen == true)
            {
                base.IsOpen = false;

                this.server.Stop();
                foreach(TcpClient client in this.clientList)
                {
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                this.clientList.Clear();

                base.SendMsg?.Invoke("TCP Server Close");
            }
            else
            {
                base.SendMsg?.Invoke("Already Close");
            }
        }
    }
}
