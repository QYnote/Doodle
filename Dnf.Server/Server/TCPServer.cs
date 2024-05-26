using Dnf.Utils.Controls;
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

namespace Dnf.Server.Server
{
    internal class TCPServer : ServerBase
    {
        private TcpListener server = null;
        private List<TcpClient> clientList = new List<TcpClient>();
        /// <summary>Data Receive 처리 이벤트 / byte[] Data, int bytesLength / return : 보낸 Client에게 되보내줄 Data</summary>
        internal ReceiveHandler ReceiveActiveEvent;
        /// <summary>
        /// Data Receive 처리 delegate
        /// </summary>
        /// <param name="data">읽어들인 Bytes</param>
        /// <param name="bytesLength">읽은 Bytes 길이</param>
        /// <returns>보낸 Client에게 보내줄 Data</returns>
        internal delegate byte[] ReceiveHandler(byte[] data, int bytesLength);

        internal TCPServer(TcpListener listener)
        {
            base.IsOpen = false;
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
            }
            else
            {
                UtilCustom.DebugWrite("Already Open");
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
                }
                catch
                {
                    UtilCustom.DebugWrite("Error");
                }
            }
        }

        /// <summary>접속한 Client에서 데이터를 Receive 받아 그에따른 처리 진행</summary>
        /// <param name="client">연결된 TCP Client</param>
        private void ClientMethod(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            byte[] returnBytes = null;
            int bytesLength;

            try
            {
                //읽을때마다 새로 읽은 Data buffer에 덮어씌우기
                while((bytesLength = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    //Data Receive에 따른 무언가 처리
                    returnBytes = this.ReceiveActiveEvent?.Invoke(buffer, bytesLength);

                    //보낸 Client에게 되돌려주는 Data
                    if (returnBytes != null)
                    { 
                        stream.Write(returnBytes, 0, returnBytes.Length);
                    }
                }
            }
            catch
            {
                UtilCustom.DebugWrite(string.Format("Error : {0}", new Exception().Message));
            }
            finally
            {
                client.Close();
                this.clientList.Remove(client);
                UtilCustom.DebugWrite("Client Receive End");
            }
        }

        /// <summary>서버 닫기</summary>
        internal override void Close()
        {
            if (base.IsOpen == true)
            {
                base.IsOpen = false;
                this.server.Stop();
            }
            else
            {
                UtilCustom.DebugWrite("Already Close");
            }
        }
    }
}
