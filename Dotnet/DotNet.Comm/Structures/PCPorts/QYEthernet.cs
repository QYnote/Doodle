using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotNet.Utils.Controls;

namespace DotNet.Comm.Structures.PCPorts
{
    public class QYEthernet : PCPortBase
    {
        #region Fields

        private Socket _clientSocket;

        private string _ip;
        private Queue<byte[]> _readingData = new Queue<byte[]>();

        #endregion Fields

        #region 설정용 Property

        public string IP { get; set; }
        public int PortNo { get; set; }
        public int MaxBufferSize { get; set; } = 1024;

        #endregion 설정용 Property

        #region 편의용 Property

        private IPAddress IPAddress
        {
            get
            {
                return IPAddress.Parse(this.IP);
            }
        }

        public override bool IsOpen
        {
            get
            {
                if (this._clientSocket == null || this._clientSocket.Connected == false)
                    return false;

                return true;
            }
        }

        public override string PortName
        {
            get
            {
                if (this.IPAddress == null)
                    return string.Empty;

                return string.Format("{0}:{1}", this.IPAddress.ToString(), this.PortNo);
            }
            set => throw new NotImplementedException();
        }

        #endregion 편의용 Property

        public QYEthernet() : base(PortType.Ethernet) { }

        public override bool Open()
        {
            if (this.IsOpen == false)
            {
                this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    var asyncResult = this._clientSocket.BeginConnect(this.IPAddress, this.PortNo, null, this._clientSocket);  //연결 시도
                    bool connectResult = asyncResult.AsyncWaitHandle.WaitOne(3000); //최대 3초간 연결 대기

                    if (connectResult == true)
                    {
                        this._clientSocket.EndConnect(asyncResult);

                        //비동기 이벤트 설정
                        SocketAsyncEventArgs asyncEvent = new SocketAsyncEventArgs();
                        asyncEvent.SetBuffer(new byte[this.MaxBufferSize], 0, this.MaxBufferSize);
                        asyncEvent.UserToken = this._clientSocket;
                        asyncEvent.Completed += AsyncEvent_Completed;

                        this._clientSocket.ReceiveAsync(asyncEvent);
                    }

                    return true;
                }
                catch
                {
                    Debug.WriteLine("[Error]Port Open Fail");
                }
            }
            else
            {
                Debug.WriteLine("[Alart]Port already Open");
            }

            return false;
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

                    this._readingData.Enqueue(buffer);

                    this._clientSocket.ReceiveAsync(e);
                }
                //else
                //{
                //    //Server 닫힘 감지
                //    this.Close();
                //}
            }
        }

        public override bool Close()
        {
            if (this.IsOpen == true)
            {
                this._clientSocket.Shutdown(SocketShutdown.Both);
                this._clientSocket.Close();      //Socket 닫기

                this._clientSocket = null;
            }
            else
            {
                QYUtils.DebugWrite("[Alart]Port Already Close");

                return false;
            }

            return true;
        }

        public override byte[] Read()
        {
            if (this.IsOpen)
            {
                int readCount = this._readingData.Count;

                if (readCount > 0)
                {
                    byte[] readBytes = null;
                    for (int i = 0; i < readCount; i++)
                    {
                        byte[] readData = this._readingData.Dequeue();

                        if (readBytes == null)
                            readBytes = readData;
                        else
                            readBytes = readBytes.BytesAppend(readData);
                    }

                    return readBytes;
                }
            }

            return null;
        }

        public override void Write(byte[] bytes)
        {
            if (this.IsOpen && bytes != null)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(bytes, 0, bytes.Length);
                this._clientSocket.SendAsync(args);

                args.Dispose();
            }
        }
    }
}
