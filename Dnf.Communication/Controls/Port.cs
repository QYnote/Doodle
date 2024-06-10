using Dnf.Communication.Controls.Protocols;
using Dnf.Communication.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    /// <summary>
    /// 송,수신 세트 구조
    /// </summary>
    internal struct CommFrame
    {
        /// <summary>전송받은(Receive) Byte Array</summary>
        internal byte[] RcvDataBytes { get; set; }
        /// <summary>전송 될(Request) Byte Array</summary>
        internal byte[] ReqDataBytes { get; set; }

        //Data Receive관련 Property
        /// <summary>Data Read 종료 시간(DateTime Tick), Device에서 Receive 처리가 안될 때 제거용</summary>
        internal long ReadEndTimeTick { get; set; }
        /// <summary>Timeout 되었는지에대한 Flag</summary>
        internal bool IsTimeout { get; set; }

        //Data Send관련 Property
        /// <summary>Data 전송 보낸 시간(DateTime Tick), 데이터가 오지않는 Receive Timeout에 사용</summary>
        internal long SendTimeTick { get; set; }
        /// <summary>Data 전송 시도 최대 회수</summary>
        internal byte MaxSendTryCount { get; set; }
        /// <summary>Data 전송 시도 현재 회수</summary>
        internal byte CurSendTryCount { get; set; }
    }

    internal class Port
    {
        internal delegate void PortLogDelegate(string Msg);
        /// <summary>Port Log 작성용 이벤트</summary>
        internal PortLogDelegate PortLogHandler { get; set; }
        /// <summary>
        /// SerialPort 생성 Class
        /// </summary>
        /// <param name="COMName">Port 명</param>
        /// <param name="BaudRate">통신속도</param>
        /// <param name="DataBits">데이터길이</param>
        /// <param name="Parity">Parity</param>
        /// <param name="StopBits">StopBit</param>
        /// <param name="Protocol">통신규격</param>
        internal Port(string COMName, string BaudRate, int DataBits, Parity Parity, StopBits StopBits, uProtocolType Protocol)
        {
            this.PortBase = new PortSerial(COMName, BaudRate, DataBits, Parity, StopBits);
            this.PortName = COMName;
            this.ProtocolType = Protocol;

            CommInitializePort();
        }
        /// <summary>
        /// Ethernet Port 생성 Class
        /// </summary>
        /// <param name="IpAddress"></param>
        /// <param name="PortNo"></param>
        /// <param name="Protocol"></param>
        internal Port(string IpAddress, int PortNo, uProtocolType Protocol)
        {
            this.PortBase = new PortEthernet(IpAddress, PortNo);
            this.PortName = IpAddress + ":" + PortNo;
            this.ProtocolType = Protocol;

            CommInitializePort();
        }
        /// <summary>
        /// Port 공통 초기화 Method
        /// </summary>
        private void CommInitializePort()
        {
            this.Units = new Dictionary<int, Unit>();

            this.BgWorker = new BackgroundWorker();
            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += PortWorker;

            //Port 전송처리 Property
            this.SendingQueue = new Queue<CommFrame>();
            this.ReceiveQueue = new Queue<CommFrame>();
            this.RemainBufferLength = 0;
            this.ReadingTimeout = 6000;
            this.NoneReceiveTimeout = 6000;
        }
        /// <summary>
        /// User가 Open했는지에대한 확인
        /// </summary>
        internal bool IsUserOpen { get; set; }
        /// <summary>
        /// Port 연결 상태
        /// </summary>
        internal bool IsOpen
        {
            get
            {
                if(PortBase == null || PortBase.IsOpen == false)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// Port 종류
        /// </summary>
        internal PortBase PortBase { get; set; }
        /// <summary>
        /// 통신 방법
        /// </summary>
        internal uProtocolType ProtocolType { get; set; }
        /// <summary>
        /// Port 이름
        /// </summary>
        internal string PortName { get; set; }
        /// <summary>
        /// Port에 연결된 하위 Unit들(ex. PLC, 센서 등), (SlaveAddr, Unit)
        /// </summary>
        internal Dictionary<int, Unit> Units { get; set; }
        /// <summary>
        /// 읽어들인 Buffer 처리방법 Class
        /// </summary>
        private ProtocolBase Processor { get; set; }
        /// <summary>
        /// Data 송,수신 처리할 Background Thread
        /// </summary>
        internal BackgroundWorker BgWorker { get; set; }
        /// <summary>
        /// 전송할 Data List보관함
        /// </summary>
        internal Queue<CommFrame> SendingQueue { get; set; }
        /// <summary>
        /// Receive 처리된 Data List 보관함
        /// </summary>
        internal Queue<CommFrame> ReceiveQueue { get; set; }
        /// <summary>
        /// 읽고있는 중인 Class에 보관된 Buffer
        /// </summary>
        private byte[] ReadingBuffer = null;
        /// <summary>
        /// 남은 Buffer 길이, 읽은 Buffer 확인용
        /// </summary>
        private int RemainBufferLength {  get; set; }
        /// <summary>
        /// 미수신 Timeout 시간, 단위(ms), Default 6000
        /// </summary>
        internal int NoneReceiveTimeout { get; set; }
        /// <summary>
        /// 읽기 최대 시간, 단위(ms), Default 6000
        /// </summary>
        internal int ReadingTimeout { get; set; }
        /// <summary>
        /// 최근 보낸 Frame, 
        /// </summary>
        private CommFrame RecentSendFrame { get; set; }
        /// <summary>
        /// 통신 열기
        /// </summary>
        internal void Open()
        {
            if (this.IsUserOpen == false)
            {
                if (this.ProtocolType == uProtocolType.ModBusTcpIp)
                    this.Processor = (ProtocolBase)null;
                else
                    this.Processor = (ProtocolBase)null;

                this.PortBase.Open();

                this.IsUserOpen = true;
                this.BgWorker.RunWorkerAsync();
            }
        }
        /// <summary>
        /// 통신 닫기
        /// </summary>
        internal void Close()
        {
            if(this.IsUserOpen == true)
            {
                this.PortBase.Close();

                this.IsUserOpen = false;
                this.BgWorker.CancelAsync();
            }
        }
        /// <summary>
        /// 통신 데이터 쓰기
        /// </summary>
        /// <param name="commFrame">ReqDataBytes가 입력된 CommFrame</param>
        private void Write(CommFrame commFrame)
        {
            if (commFrame.ReqDataBytes == null) return;

            commFrame.SendTimeTick = DateTime.Now.Ticks;    //전송시간
            commFrame.IsTimeout = false;    //Timeout 여부

            this.PortBase.Write(commFrame.ReqDataBytes);    //Data 전송

            this.RecentSendFrame = commFrame;   //최근 보낸 Data
        }
        /// <summary>
        /// Background Worker 진행 Event, 송,수신 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PortWorker(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (this.BgWorker.CancellationPending == true)
                    return;
                else
                {
                    if (this.IsUserOpen == false) return;

                    //Port 끊켜있으면 재연결
                    if (this.IsOpen == false)
                    {
                        //Port 초기화
                        this.SendingQueue.Clear();

                        //Port 재연결
                        this.PortBase.Open();
                        continue;
                    }

                    //전송할 Data List가 있을 경우
                    while (SendingQueue.Count > 0)
                    {
                        Thread.Sleep(50);
                        if (this.IsOpen == false) break;

                        CommFrame commFrame = this.SendingQueue.Peek();

                        if (commFrame.ReqDataBytes.Equals(this.RecentSendFrame.ReqDataBytes) == false)
                        {
                            //최근 보낸 Data와 Queue에 마지막에있는 Data가 다를경우 Data 전송
                            this.Write(commFrame);
                        }
                        else
                        {
                            //Data Receive Process 실행
                            long readStartTimeTick = DateTime.Now.Ticks;

                            //1. Port Open 재검사
                            if (this.IsOpen == false) break;

                            //2. Timeout 검사
                            if (TimeoutCheck(commFrame, readStartTimeTick) == true) continue;

                            this.PortBase.Read(ref ReadingBuffer);

                            //3. 마지막 Read한 버퍼 량에서 추가적으로 더 들어오지 않았다면 재실행
                            //2개가 연속으로 들어왔을 경우에는 어떻게?
                            if (this.ReadingBuffer == null || this.ReadingBuffer.Length == this.RemainBufferLength) continue;

                            //4. Protocol에따른 Bytes Frame 추출
                            int tailEndIdx = ReadingBuffer.Length - 1;//-1;
                            byte[] frameBytes = ReadingBuffer;   //Processor.Frame();

                            //5. 데이터가 다 안들어왔거나 비정상 데이터 Receive 확인
                            if (frameBytes == null) continue;

                            //6. Receive 종료시간 기록
                            commFrame.ReadEndTimeTick = DateTime.Now.Ticks;
                            commFrame.RcvDataBytes = frameBytes;

                            //7. 기존 Port에 있던 Bytes 표기
                            string str = "RecevingBytes : ";
                            foreach (byte b in this.ReadingBuffer)
                            {
                                str += b + " ";
                            }
                            this.PortLogHandler?.Invoke(str);

                            //8. 추출한 Bytes표기
                            str = "FrameBytes : ";
                            foreach (byte b in frameBytes)
                            {
                                str += b + " ";
                            }
                            this.PortLogHandler?.Invoke(str);

                            //9. 추출하고 남은 Bytes 표기
                            byte[] remainBytes = null;
                            int remainLen = this.ReadingBuffer.Length - tailEndIdx - 1;
                            if (remainLen - 1 > 0)
                            {
                                remainBytes = new byte[remainLen];
                                Buffer.BlockCopy(this.ReadingBuffer, tailEndIdx + 1, remainBytes, 0, remainLen); //끝에 잘려서 가져오는지 확인
                            }
                            this.ReadingBuffer = remainBytes;
                            this.RemainBufferLength = remainLen;

                            str = "RemainBytes : ";
                            if (remainLen > 0)
                            {
                                foreach (byte b in remainBytes)
                                {
                                    str += b + " ";
                                }
                            }
                            str += "\r\n";
                            this.PortLogHandler?.Invoke(str);

                            this.ReceiveQueue.Enqueue(commFrame);
                            this.SendingQueue.Dequeue();
                        }
                    }//while(SendQueue.Count) End
                }
            }//while(true) End
        }
        /// <summary>
        /// Timeout 검사
        /// </summary>
        /// <param name="sendingFrame">보냈던 Frame 정보</param>
        /// <param name="readStartTime">Read 시작시간, Receive가 너무 오래하는 Timeout에 이용</param>
        /// <returns>true : Timeout, false : 아직 Timeout 안됨</returns>
        private bool TimeoutCheck(CommFrame sendingFrame, long readStartTime)
        {
            long curTimeTick = DateTime.Now.Ticks;
            if (this.RemainBufferLength <= (ReadingBuffer == null ? 0 : ReadingBuffer.Length))
            {
                //데이터가 너무 오래들어오는 Timeout
                if (curTimeTick - readStartTime > this.ReadingTimeout)
                {
                    if (sendingFrame.CurSendTryCount > sendingFrame.MaxSendTryCount)
                    {
                        CommFrame frame = this.SendingQueue.Dequeue();
                        frame.IsTimeout = true;

                        this.ReceiveQueue.Enqueue(frame);
                    }
                    else
                    {
                        //Send 재시도
                        sendingFrame.CurSendTryCount++;
                        this.RecentSendFrame = new CommFrame();
                    }

                    this.ReadingBuffer = null;

                    return true;
                }
            }
            else
            {
                //데이터가 들어오지 않는 Timeout
                if (curTimeTick - sendingFrame.SendTimeTick > this.NoneReceiveTimeout)
                {
                    if (sendingFrame.CurSendTryCount > sendingFrame.MaxSendTryCount)
                    {
                        CommFrame frame = this.SendingQueue.Dequeue();
                        frame.IsTimeout = true;

                        this.ReceiveQueue.Enqueue(frame);
                    }
                    else
                    {
                        //Send 재시도
                        sendingFrame.CurSendTryCount++;
                        this.RecentSendFrame = new CommFrame();
                    }

                    this.ReadingBuffer = null;

                    return true;
                }
            }

            return false;
        }
    }
}
