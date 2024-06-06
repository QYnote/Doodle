using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    internal class Custom_EthernetPort : Port
    {
        /// <summary>
        /// Port IP번호
        /// </summary>
        internal IPAddress IPAddr;
        /// <summary>
        /// Port번호
        /// </summary>
        internal ushort PortNo;
        internal Socket clientSocket;
        internal int MaxBufferSize {  get; set; }

        internal Custom_EthernetPort(string ipAddress, uProtocolType type, ushort portNo) : base(ipAddress, type)
        {
            this.IPAddr = IPAddress.Parse(ipAddress);
            this.PortNo = portNo;
            this.MaxBufferSize = 1024;

            base.UserPortOpenFlag = false;
        }

        internal override bool IsOpen
        {
            get
            {
                if (this.clientSocket == null || this.clientSocket.Connected == false)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// 연결된 Port 열기
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        internal override bool Open()
        {
            if (this.IsOpen == false)
            {
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                try
                {
                    var asyncResult = this.clientSocket.BeginConnect(IPAddr, PortNo, null, this.clientSocket);  //연결 시도
                    bool connectResult = asyncResult.AsyncWaitHandle.WaitOne(3000); //최대 3초간 연결 대기

                    if (connectResult == true)
                    {
                        this.clientSocket.EndConnect(asyncResult);

                        //비동기 이벤트 설정
                        SocketAsyncEventArgs asyncEvent = new SocketAsyncEventArgs();
                        asyncEvent.SetBuffer(new byte[MaxBufferSize], 0, MaxBufferSize);
                        asyncEvent.UserToken = this.clientSocket;
                        asyncEvent.Completed -= AsyncEvent_Completed;
                        asyncEvent.Completed += AsyncEvent_Completed;

                        this.clientSocket.ReceiveAsync(asyncEvent);
                    }
                }
                catch
                {
                    base.PortLogHandler?.Invoke("[ERROR]Port Open Fail");
                    return false;
                }

                base.BgWorker.RunWorkerAsync();
            }
            else
            {
                base.PortLogHandler?.Invoke("[Alart]Port Already Open");
            }

            return true;
        }

        /// <summary>
        /// Receive 종료 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsyncEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (this.IsOpen == true)
            {
                if (e.BytesTransferred > 0)
                {
                    if (base.ReadingBuffer == null)
                        base.ReadingBuffer = e.Buffer;
                    else
                        base.ReadingBuffer.BytesAppend(e.Buffer);

                    this.clientSocket.ReceiveAsync(e);
                }
            }
        }

        internal override bool Close()
        {
            if(this.IsOpen == true)
            {
                this.clientSocket.Close();
                base.BgWorker.CancelAsync();
            }
            else
            {
                base.PortLogHandler?.Invoke("[Alart]Port Already Close");

                return false;
            }

            return true;
        }

        internal override void Write(byte[] bytes)
        {
            if(this.IsOpen == true)
            {
                this.clientSocket.Send(bytes);
            }

            return ;
        }

        /// <summary>[미사용]AsyncEvent_Completed이벤트가 대신 데이터 읽어줌</summary>
        internal override void Read(){}
    }
}
