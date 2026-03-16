using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DotNet.Comm.Transport
{
    /*개발Log
     * 비동기 Timeout은 Application Port에서 판단하도록 한다
     * 사유: 정말로 연결하고 Timeout안주고 무한정 연결할 수도 있기때문
     */
    public class QYSocketPort : ITransport
    {
        #region Fields
        /// <summary>연결 Ethernet Socket</summary>
        private Socket _clientSocket = null;
        private object _lock_buffer = new object();
        private byte[] _asyncBuffer = null;

        private ProtocolType _socketProtocol = ProtocolType.Tcp;
        private IPAddress _ipAddress = null;
        private string _ip = "127.0.0.1";
        private int _portNo = 5000;
        private int _maxBufferSize = 64000;//UDP Packet 제한 최대치(64KB)
        private bool _isSync = false;

        #endregion Fields
        #region 설정용 Property

        /// <summary>Ethernet IP</summary>
        public string IP
        {
            get => this._ip;
            set
            {
                if (IPAddress.TryParse(value, out this._ipAddress))
                {
                    this._ip = value;
                }
            }
        }
        /// <summary>Ethernet Port번호</summary>
        public int PortNo { get => _portNo; set => _portNo = value; }
        /// <summary>
        /// Socket Protocol 종류
        /// </summary>
        public ProtocolType SocketProtocol { get => _socketProtocol; set => _socketProtocol = value; }
        /// <summary>받을 Buffer 크기</summary>
        public int MaxBufferSize { get => _maxBufferSize; set => _maxBufferSize = value; }
        /// <summary>동기/비동기 통신 여부</summary>
        /// <returns>true : 동기통신 / false : 비동기통신</returns>
        public bool IsSync { get => _isSync; set => _isSync = value; }

        #endregion 설정용 Property
        #region 편의용 Property

        public bool IsOpen
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
                    if (this._clientSocket == null || this._clientSocket.Connected == false)
                        return false;
                }

                return true;
            }
        }

        #endregion 편의용 Property
        /// <summary>
        /// Ethernet PCPort
        /// </summary>
        /// <param name="isSync">
        /// 동기/비동기 통신 여부<br/>
        /// true : 동기통신 / false : 비동기통신
        /// </param>
        public QYSocketPort(bool isSync = false)
        {
            this.IsSync = isSync;
        }

        public void Open()
        {
            if (this.IsOpen == false)
            {
                this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, this._socketProtocol);

                try
                {
                    if (this.IsSync)
                    {
                        this._clientSocket.Connect(this._ipAddress, this._portNo);
                    }
                    else
                    {
                        var asyncResult = this._clientSocket.BeginConnect(this._ipAddress, this._portNo, null, this._clientSocket);  //연결 시도
                        bool connectResult = asyncResult.AsyncWaitHandle.WaitOne(3000); //최대 3초간 연결 대기

                        if (connectResult == true)
                        {
                            this._clientSocket.EndConnect(asyncResult);//연결시도 종료
                            
                            //비동기 이벤트 설정
                            SocketAsyncEventArgs asyncEvent = new SocketAsyncEventArgs();
                            asyncEvent.SetBuffer(new byte[this.MaxBufferSize], 0, this.MaxBufferSize);
                            asyncEvent.UserToken = this._clientSocket;
                            asyncEvent.Completed += AsyncEvent_Completed;

                            this._clientSocket.ReceiveAsync(asyncEvent);
                        }
                        else
                            this._clientSocket.Close();
                    }

                    return;
                }
                catch 
                {
                    this._clientSocket = null;
                }
            }
        }
        /// <summary>
        /// 비동기통신 수신 종료 이벤트
        /// </summary>
        private void AsyncEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            //연결 끊김 확인
            Socket client = sender as Socket;
            if (client == null || client.Connected == false) return;

            if (client.Connected && e.BytesTransferred > 0)
            {
                lock (this._lock_buffer)
                {
                    if (this._asyncBuffer == null)
                    {
                        this._asyncBuffer = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, this._asyncBuffer, 0, this._asyncBuffer.Length);
                    }
                    else
                    {
                        byte[] temp = new byte[this._asyncBuffer.Length + e.BytesTransferred];
                        Buffer.BlockCopy(this._asyncBuffer, 0, temp, 0, this._asyncBuffer.Length);
                        Buffer.BlockCopy(e.Buffer, 0, temp, this._asyncBuffer.Length, e.BytesTransferred);

                        this._asyncBuffer = temp;
                    }
                }
            }

            client.ReceiveAsync(e);
        }

        public void Close()
        {
            if (this._clientSocket != null && this._clientSocket.Connected)
            {
                this._clientSocket.Shutdown(SocketShutdown.Both);
                this._clientSocket.Close();      //Socket 닫기
                this._clientSocket = null;
            }
        }

        public byte[] Read()
        {
            if (this.IsSync) return this.SyncRead();
            else return this.AsyncRead();
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
            lock (this._lock_buffer)
            {
                if (this._asyncBuffer != null && this._asyncBuffer.Length > 0)
                {
                    byte[] readBytes = new byte[this._asyncBuffer.Length];

                    Buffer.BlockCopy(this._asyncBuffer, 0, readBytes, 0, readBytes.Length);
                    this._asyncBuffer = null;

                    return readBytes;
                }
            }

            return null;
        }

        public void Write(byte[] bytes)
        {
            if (this.IsSync) this.SyncWrite(bytes);
            else this.AsyncWrite(bytes);
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
            if (this._clientSocket.Connected && bytes != null)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(bytes, 0, bytes.Length);
                this._clientSocket.SendAsync(args);
                
                args.Dispose();
            }
        }

        public void Initialize()
        {
            this.Close();
            this._clientSocket = null;
            this._asyncBuffer = null;
        }
    }
}
