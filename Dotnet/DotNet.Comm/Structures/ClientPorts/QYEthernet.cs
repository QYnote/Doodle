using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Structures.ClientPorts
{
    public class QYEthernet : PCPortBase
    {
        #region Fields
        /// <summary>연결 Ethernet Socket</summary>
        private Socket _clientSocket;
        /// <summary>비동기 연결 시간 체크용 Stopwatch</summary>
        private Stopwatch _sw = new Stopwatch();
        /// <summary>비동기 연결 시 읽어들인 Data</summary>
        private Queue<byte[]> _readingData = new Queue<byte[]>();

        #endregion Fields
        #region 설정용 Property

        /// <summary>Ethernet IP</summary>
        public string IP { get; set; } = "127.0.0.1";
        /// <summary>Ethernet Port번호</summary>
        public int PortNo { get; set; } = 5000;
        /// <summary>받을 Buffer 크기</summary>
        public int MaxBufferSize { get; set; } = 1024;
        /// <summary>동기/비동기 통신 여부</summary>
        /// <remarks>true : 동기통신 / false : 비동기통신</remarks>
        public bool IsSync { get; set; } = false;
        /// <summary>비동기통신 Timeout 시간[ms]</summary>
        public int AsyncTimeout { get; set; } = 3000;

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
                if (this.IsSync)
                {
                    if (this._clientSocket == null
                        || this._clientSocket.Receive(new byte[1], SocketFlags.Peek) == 0)
                        return false;
                }
                else
                {
                    if (this._clientSocket == null
                        || this._sw.ElapsedMilliseconds > this.AsyncTimeout)
                        return false;
                }

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
        /// <summary>
        /// Ethernet PCPort
        /// </summary>
        /// <param name="isSync">
        /// 동기/비동기 통신 여부<br/>
        /// true : 동기통신 / false : 비동기통신
        /// </param>
        public QYEthernet(bool isSync) : base(PortType.Ethernet)
        {
            this.IsSync = isSync;
        }

        public override bool Open()
        {
            if (this.IsOpen == false)
            {
                this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    if (this.IsSync)
                    {
                        this._clientSocket.Connect(this.IPAddress, this.PortNo);
                    }
                    else
                    {
                        var asyncResult = this._clientSocket.BeginConnect(this.IPAddress, this.PortNo, null, this._clientSocket);  //연결 시도
                        bool connectResult = asyncResult.AsyncWaitHandle.WaitOne(this.AsyncTimeout); //최대 3초간 연결 대기

                        if (connectResult == true)
                        {
                            this._clientSocket.EndConnect(asyncResult);

                            //비동기 이벤트 설정
                            SocketAsyncEventArgs asyncEvent = new SocketAsyncEventArgs();
                            asyncEvent.SetBuffer(new byte[this.MaxBufferSize], 0, this.MaxBufferSize);
                            asyncEvent.UserToken = this._clientSocket;
                            asyncEvent.Completed += AsyncEvent_Completed;
                            this._sw.Restart();

                            this._clientSocket.ReceiveAsync(asyncEvent);
                        }
                        else
                            this._clientSocket.Close();
                    }

                    return true;
                }
                catch
                {
                    this._clientSocket = null;
                    base.LogRun("[Error]Port Open Fail");
                }
            }
            else
            {
                base.LogRun("[Alart]Port already open");
            }

            return false;
        }
        /// <summary>
        /// 비동기통신 수신 종료 이벤트
        /// </summary>
        private void AsyncEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (this.IsOpen == true)
            {
                if (e.SocketError == SocketError.Success
                    && e.BytesTransferred > 0)
                {
                    try
                    {
                        byte[] buffer = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, buffer, 0, buffer.Length);

                        this._readingData.Enqueue(buffer);

                        this._clientSocket.ReceiveAsync(e);
                        this._sw.Restart();
                    }
                    catch
                    {
                        //this._readingData.Clear();
                    }
                }
                else
                {
                    //Server 닫힘 감지
                    this.Close();
                }
            }
        }
        public override bool Close()
        {
            //동기식 연결일 경우 IsOpen일 경우 Close처리
            //비동기식 연결일 경우 바로 Close처리
            if (this.IsOpen)
            {
                this._clientSocket.Shutdown(SocketShutdown.Both);
                this._clientSocket.Close();      //Socket 닫기

                this._clientSocket = null;
                this._readingData.Clear();
                this._sw.Stop();
                this._sw.Reset();
            }
            else
            {
                base.LogRun("[Alart]Port already close");
            }

            return true;
        }

        public override byte[] Read()
        {
            if (this.IsSync)
                return SyncRead();
            else
                return AsyncRead();
        }
        /// <summary>
        /// 동기통신 읽기처리
        /// </summary>
        /// <returns>PCPort에 담겨있던 Buffer</returns>
        private byte[] SyncRead()
        {
            if (this.IsOpen)
            {
                byte[] buffer = new byte[this.MaxBufferSize];
                this._clientSocket.Receive(buffer);

                return buffer;
            }

            return null;
        }
        /// <summary>
        /// 비동기통신 읽기처리
        /// </summary>
        /// <returns>PCPort에 담겨있던 Buffer</returns>
        private byte[] AsyncRead()
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
                    {
                        if (readData == null) continue;
                        byte[] containBytes = new byte[readBytes.Length + readData.Length];

                        //옮길 Array, 옮길 Array 시작 index, 넘겨받은 Array, 넘겨받을 Array index, 옮길 Array 수
                        Buffer.BlockCopy(readBytes, 0, containBytes, 0, readBytes.Length);
                        Buffer.BlockCopy(readData, 0, containBytes, readBytes.Length, readData.Length);

                        readBytes = containBytes;
                    }
                }

                return readBytes;
            }

            return null;
        }

        public override void Write(byte[] bytes)
        {
            if (this.IsSync)
                SyncWrite(bytes);
            else
                AsyncWrite(bytes);
        }
        /// <summary>
        /// 동기통신 Data 쓰기
        /// </summary>
        /// <param name="bytes">전송할 Byte Array</param>
        private void SyncWrite(byte[] bytes)
        {
            if (this.IsOpen)
                this._clientSocket.Send(bytes);
        }
        /// <summary>
        /// 비동기통신 Data 쓰기
        /// </summary>
        /// <param name="bytes">전송할 Byte Array</param>
        private void AsyncWrite(byte[] bytes)
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
