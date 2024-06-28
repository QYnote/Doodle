using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Comm.Controls.PCPorts
{
    internal class PortEthernet : PCPortBase
    {
        /// <summary>
        /// Port 연결 상태
        /// </summary>
        internal override bool IsOpen
        {
            get
            {
                if (clientSocket == null || clientSocket.Connected == false)
                    return false;
                else
                    return true;
            }
        }
        internal override string PortName { get { return this.IP + ":" + this.PortNo; } }
        /// <summary>
        /// 서버와 연결할 Socket
        /// </summary>
        private Socket clientSocket { get; set; }
        /// <summary>
        /// Port IP번호
        /// </summary>
        internal string IP {  get; set; }
        /// <summary>
        /// Port번호
        /// </summary>
        internal int PortNo;
        /// <summary>
        /// Port가 담당할 Buffer 최대 크기
        /// </summary>
        internal int MaxBufferSize { get; set; }
        /// <summary>
        /// Port에서 Receive한 Buffer
        /// </summary>
        private byte[] ReadingBuffer { get; set; }

        internal PortEthernet(string ipAddress, int portNo)
        {
            this.IP = ipAddress;
            this.PortNo = portNo;
            this.MaxBufferSize = 1024;
        }
        /// <summary>
        /// Ethernet Port 열기
        /// </summary>
        /// <returns>true : 정상 열림 / false : 열기 실패</returns>
        internal override bool Open()
        {
            if (this.IsOpen == false)
            {
                IPAddress ipAddr = IPAddress.Parse(this.IP);
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                try
                {
                    var asyncResult = this.clientSocket.BeginConnect(ipAddr, PortNo, null, this.clientSocket);  //연결 시도
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
                    UtilCustom.DebugWrite("[ERROR]Port Open Fail");
                    return false;
                }
            }
            else
            {
                UtilCustom.DebugWrite("[Alart]Port Already Open");
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
                    byte[] buffer = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, buffer, 0, buffer.Length);

                    if (this.ReadingBuffer == null)
                        this.ReadingBuffer = buffer;
                    else
                        this.ReadingBuffer.BytesAppend(buffer);

                    this.clientSocket.ReceiveAsync(e);
                }
            }
        }
        /// <summary>
        /// Port 닫기
        /// </summary>
        /// <returns>true : 정상 닫기 / false : 닫기 실패</returns>
        internal override bool Close()
        {
            if (this.IsOpen == true)
            {
                this.clientSocket.Close();      //Socket 닫기
            }
            else
            {
                UtilCustom.DebugWrite("[Alart]Port Already Close");

                return false;
            }

            return true;
        }
        /// <summary>
        /// Port Data 읽어서 PortClass의 ReadingData에 쌓기
        /// </summary>
        /// <param name="buffer">담아갈 byte Array</param>
        internal override byte[] Read(byte[] buffer)
        {
            //읽은 Buffer 없으면 기존꺼 return
            if (this.ReadingBuffer == null || this.ReadingBuffer.Length == 0)
                return buffer;

            byte[] returnBuffer = null;
            if(buffer == null || buffer.Length == 0)
                returnBuffer = this.ReadingBuffer;
            else
                returnBuffer = buffer.BytesAppend(this.ReadingBuffer);

            this.ReadingBuffer = null;

            return returnBuffer;
        }
        /// <summary>
        /// Port Data 전송
        /// </summary>
        /// <param name="bytes">전송할 Data byte Array</param>
        internal override void Write(byte[] bytes)
        {
            if (this.IsOpen == true)
            {
                this.clientSocket.Send(bytes);
            }

            return;
        }
    }
}
