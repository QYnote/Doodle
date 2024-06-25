using Dnf.Comm.Controls.PCPorts;
using Dnf.Comm.Controls.Protocols;
using Dnf.Comm.Data;
using Dnf.Utils.Controls;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Dnf.Comm.Controls
{
    internal struct CommFrame
    {
        internal byte[] RcvDataBytes{ get; set; }
        internal byte[] ReqDataBytes{ get; set; }

        /// <summary>
        /// 데이터가 들어오기 시작한 시간
        /// </summary>
        internal TimeSpan ReadStartTimeTick{ get; set; }
        internal TimeSpan ReadEndTimeTick { get; set; }
        internal byte[] RemainBytes { get; set; }
        /// <summary>
        /// 에러여부 확인용 코드
        /// <para>ErrorCode List</para>
        /// <para>0 : 정상, 1 : 데이터가 너무 오래 들어오는 Timeout, 2 : 데이터가 안 들어오는 Timeout</para>
        /// </summary>
        internal short IsError { get; set; }

        internal TimeSpan SendTimeTick { get; set; }
        internal byte MaxSendTryCount{ get; set; }
        internal byte CurTryCount{ get; set; }
    }

    internal class ProgramPort
    {
        /// <summary>Port Log 전송 Delegate</summary>
        /// <param name="Msg"></param>
        internal delegate void PortLogDelegate(string Msg);
        /// <summary>Port Log 작성용 이벤트</summary>
        internal PortLogDelegate PortLogHandler { get; set; }



        /// <summary>
        /// 프로그램 Serial Port
        /// </summary>
        internal ProgramPort(string COMName, string baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            this.PCPort = new PortSerial(COMName, baudRate, dataBits, parity, stopBits);

            SetDefaultValue();
        }
        /// <summary>
        /// 프로그램 Ethernet Port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="portNo"></param>
        internal ProgramPort(string ip, int portNo)
        {
            this.PCPort = new PortEthernet(ip, portNo);

            SetDefaultValue();
        }


        
        /// <summary>
        /// User가 Open했는지에대한 확인
        /// </summary>
        internal bool IsUserOpen { get; private set; }
        /// <summary>
        /// Port 연결 상태
        /// </summary>
        internal bool IsOpen { get { return this.PCPort.IsOpen; } }
        /// <summary>
        /// Program Port명
        /// </summary>
        internal string PortName { get { return this.PCPort.PortName; } }
        /// <summary>
        /// 통신 방법
        /// </summary>
        internal uProtocolType ProtocolType { get; set; }

        /// <summary>
        /// 프로그램 - Client 사이의 Port
        /// </summary>
        internal PCPortBase PCPort { get; set; }
        /// <summary>
        /// Data 송,수신 처리할 Background Thread
        /// </summary>
        internal BackgroundWorker BgWorker { get; private set; }
        /// <summary>
        /// 통신방법 Class
        /// </summary>
        internal ProtocolBase Protocol { get; set; }
        /// <summary>
        /// 연결 Device 목록
        /// </summary>
        internal Dictionary<int, Unit> Units { get; private set; }

        /// <summary>
        /// 전송할 Data 보관함
        /// </summary>
        internal Queue<CommFrame> SendingQueue { get; private set; }
        /// <summary>
        /// 받은 Data 보관함
        /// </summary>
        internal Queue<CommFrame> ReceiveQueue { get; private set; }
        /// <summary>
        /// 데이터가 오지 않는 Timeout 시간
        /// </summary>
        internal int NoneReceiveTimeout {  get; set; }
        /// <summary>
        /// 최대 읽는 시간 Timeout 값
        /// </summary>
        internal int ReadingTimeout {  get; set; }
        /// <summary>
        /// 읽어들인 Data Buffer
        /// </summary>
        private byte[] Buffer { get; set; }
        /// <summary>
        /// 최근 받은 Data
        /// </summary>
        internal CommFrame RecentSendFrame { get; set; }



        private void SetDefaultValue()
        {
            this.IsUserOpen = false;

            this.BgWorker = new BackgroundWorker();
            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += this.BgWorker_DoWork;
            this.Units = new Dictionary<int, Unit>();
            this.SendingQueue = new Queue<CommFrame>();
            this.ReceiveQueue = new Queue<CommFrame>();

            this.NoneReceiveTimeout = 3000;
            this.ReadingTimeout = 3000;
        }

        /// <summary>
        /// 통신 열기
        /// </summary>
        internal void Open()
        {
            if (this.PCPort.IsOpen == true) return;

            if(this.PCPort.Open() == true)
            {
                this.IsUserOpen = true;
                this.SendingQueue.Clear();
                this.RecentSendFrame = new CommFrame();

                this.BgWorker.RunWorkerAsync();

                this.PortLogHandler?.Invoke(string.Format("({0}) Port Connected", this.PortName));
            }
            else
                this.PortLogHandler?.Invoke(string.Format("[ERROR]({0}) Port Connected", this.PortName));

        }
        /// <summary>
        /// 통신 닫기
        /// </summary>
        internal void Close()
        {
            if (this.PCPort.IsOpen == false) return;

            if(this.PCPort.Close() == true)
            {
                this.IsUserOpen = false;
                this.BgWorker.CancelAsync();

                this.PortLogHandler?.Invoke(string.Format("({0}) Port Disconnected", this.PortName));
            }
            else
                this.PortLogHandler?.Invoke(string.Format("[ERROR]({0}) Port Disconnected", this.PortName));
        }
        /// <summary>
        /// 통신에 쓰기
        /// </summary>
        /// <param name="frame"></param>
        internal void Write(CommFrame frame)
        {
            if (frame.ReqDataBytes == null || frame.ReqDataBytes.Length == 0) return;

            //Frmae 
            frame.SendTimeTick = TimeSpan.FromTicks(DateTime.Now.Ticks);
            frame.IsError = 0;

            //Data 전송
            this.PCPort.Write(frame.ReqDataBytes);

            //최근 보낸 Frame 지정
            this.RecentSendFrame = frame;

            this.PortLogHandler?.Invoke(string.Format("({0}) Port Write\r\nWrite Data : {1}\r\n", this.PortName, this.ByteArrayToString(frame.ReqDataBytes)));
        }
        /// <summary>
        /// 통신에서 Data 읽기
        /// </summary>
        /// <param name="frame"></param>
        internal void Read(CommFrame frame)
        {
            if (this.PCPort.IsOpen == false) return;
            //Timeout 검사
            if (this.TimeoutCheck(frame) == true) return;

            if (this.Buffer == null)
                this.Buffer = new byte[0];

            this.Buffer = this.PCPort.Read(this.Buffer);

            //읽은 Buffer가 없으면 종료
            if (this.Buffer.Length == 0) return;

            if(frame.ReadStartTimeTick ==  TimeSpan.Zero)
                frame.ReadStartTimeTick = TimeSpan.FromTicks(DateTime.Now.Ticks);

            //if (this.Protocol == null) return;

            ////프로토콜에 따른 데이터 Receive 처리
            //this.Protocol.DataExtract(frame, this.Buffer);

            ////정상 Receive 확인
            //if (frame.RcvDataBytes == null) return;

            frame.RcvDataBytes = this.Buffer;   //테스트용

            string log = string.Format("({0}) Port Read\r\n" +
                "Receive Data : {1}\r\n",
                this.PortName, ByteArrayToString(this.Buffer));

            //남은 데이터를 Buffer에 적용
            this.Buffer = frame.RemainBytes;

            this.ReceiveQueue.Enqueue(frame);
            this.SendingQueue.Dequeue();

            //받은 Data, 남은 Data string 변환
            string rcvStr = ByteArrayToString(frame.RcvDataBytes),
                remainStr = ByteArrayToString(frame.RemainBytes);

            log += string.Format("Extract Data : {0}\r\n" +
                "Remain Data : {1}\r\n\r\n",
                rcvStr, remainStr);
            //Log 기록
            this.PortLogHandler?.Invoke(log);
        }
        /// <summary>
        /// Timeout 검사
        /// </summary>
        /// <param name="sendingFrame">보냈던 Frame 정보</param>
        /// <param name="readStartTime">Read 시작시간, Receive가 너무 오래하는 Timeout에 이용</param>
        /// <returns>true : Timeout, false : 아직 Timeout 안됨</returns>
        private bool TimeoutCheck(CommFrame frame)
        {
            TimeSpan curTimeTick = TimeSpan.FromTicks(DateTime.Now.Ticks);
            //데이터가 너무 오래들어오는 Timeout
            if (frame.ReadStartTimeTick != TimeSpan.Zero)
            {
                if ((curTimeTick - frame.ReadStartTimeTick).TotalMilliseconds > this.ReadingTimeout)
                {
                    if (frame.CurTryCount > frame.MaxSendTryCount)
                    {
                        this.SendingQueue.Dequeue();
                        frame.IsError = 1;

                        this.ReceiveQueue.Enqueue(frame);
                    }
                    else
                    {
                        //Send 재시도
                        frame.CurTryCount++;
                        this.RecentSendFrame = new CommFrame();
                    }

                    this.PortLogHandler?.Invoke(
                        string.Format("({0}) Port - Too long receive Timeout\r\n" +
                        "Receive Data : {1}\r\n",
                        this.PortName, this.ByteArrayToString(this.Buffer)));

                    this.Buffer = null;

                    return true;
                }
            }
            //데이터가 들어오지 않는 Timeout
            else if (frame.SendTimeTick != TimeSpan.Zero)
            {
                if ((curTimeTick - frame.SendTimeTick).TotalMilliseconds > this.NoneReceiveTimeout)
                {
                    if (frame.CurTryCount > frame.MaxSendTryCount)
                    {
                        this.SendingQueue.Dequeue();
                        frame.IsError = 2;

                        this.ReceiveQueue.Enqueue(frame);
                    }
                    else
                    {
                        //Send 재시도
                        frame.CurTryCount++;
                        this.RecentSendFrame = new CommFrame();
                    }

                    this.PortLogHandler?.Invoke(
                        string.Format("({0}) Port - None receive Timeout\r\n" +
                        "Receive Data : {1}\r\n",
                        this.PortName, this.ByteArrayToString(this.Buffer)));

                    this.Buffer = null;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Byte Array 각 값들 string으로 표기
        /// </summary>
        /// <param name="bytes">번역할 Byte Array</param>
        /// <returns>번역된 string값</returns>
        private string ByteArrayToString(byte[] bytes)
        {
            string str = string.Empty;
            if (bytes == null) return str;

            foreach (byte b in bytes)
            {
                //Dec(Hex)
                str += b.ToString() + "(" + b.ToString("X2") + ") ";
            }

            return str;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (this.BgWorker.CancellationPending == true)
                    break;
                else
                {
                    if (this.IsUserOpen == false) return;

                    if(this.IsOpen == false)
                    {
                        //Queue 초기화
                        this.SendingQueue.Clear();

                        //Port 재연결
                        this.PCPort.Open();

                        continue;
                    }

                    if(this.SendingQueue.Count > 0)
                    {
                        if (this.IsOpen == false) continue;

                        CommFrame sendingframe = this.SendingQueue.Peek();
                        CommFrame frame = this.RecentSendFrame;

                        //마지막에 보낸 Data가 최근 보낸 Data와 같은 경우
                        if(sendingframe.ReqDataBytes.Equals(frame.ReqDataBytes) == true)
                        {
                            //데이터 읽기
                            if(frame.ReadStartTimeTick == TimeSpan.Zero)
                                frame.ReadStartTimeTick = TimeSpan.FromTicks(DateTime.Now.Ticks);

                            this.Read(frame);
                        }
                        else
                        {
                            //데이터 보내기
                            this.Write(sendingframe);
                        }
                    }

                    Thread.Sleep(50);
                }
            }
        }
    }
}
