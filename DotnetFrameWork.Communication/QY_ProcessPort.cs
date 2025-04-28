using DotnetFrameWork.Communication.Ports;
using DotnetFrameWork.Communication.Protocols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DotnetFrameWork.Communication
{
    public class QY_ProcessPort
    {
        /// <summary>
        /// PC Port
        /// </summary>
        /// (Program Port - PC Port)중 PC Port
        private PCPort Port { get; set; }
        /// <summary>
        /// 들어온 데이터 처리할 Protocol
        /// </summary>
        private ProtocolBase Protocol { get; set; }
        /// <summary>
        /// 동기 읽기
        /// <para>true: 동기 읽기, false: 비동기 읽기</para>
        /// </summary>
        /// 동기 읽기 시 무한정으로 계속 읽기처리, 비동기 읽기 시 Request Data가 있으면 읽기
        private bool CommSync { get; set; }
        /// <summary>
        /// 사용자가 Port 열기 여부
        /// <para>true: 열림 / false: 닫힘</para>
        /// </summary>
        private bool UserOpen { get; set; }

        private BackgroundWorker BgWorker { get; set; }
        /// <summary>
        /// PC Port에서 읽은 누적 Data
        /// </summary>
        private byte[] StackBuffer { get; set; }

        /// <summary>
        /// 전송 Data
        /// </summary>
        private Queue<CommFrame> SendingQueue { get; set; }
        /// <summary>
        /// 우선전송 Data
        /// </summary>
        private Queue<CommFrame> FrontSendingQueue { get; set; }
        /// <summary>
        /// 수신 Data
        /// </summary>
        private Queue<byte[]> ReceiveQueue { get; set; }
        /// <summary>
        /// 마지막 전송한 Data
        /// </summary>
        private CommFrame LastSendingData { get; set; }

        private int LastBufferLength { get; set; }
        /// <summary>
        /// Timeout - Data가 들어오다 만 시간
        /// </summary>
        public int Timeout_ReceiveStop { get; set; }
        /// <summary>
        /// Timeout - 너무 오래들어오는 Data 시간
        /// </summary>
        public int Timeout_TooLong { get; set; }

        public QY_ProcessPort()
        {
            this.CommSync = false;
            this.Timeout_ReceiveStop = 3000;
            this.Timeout_TooLong = 10000;
            this.SendingQueue = new Queue<CommFrame>();
            this.FrontSendingQueue = new Queue<CommFrame>();
            this.ReceiveQueue = new Queue<byte[]>();

            this.BgWorker = new BackgroundWorker();
            this.BgWorker.WorkerSupportsCancellation = true;
            this.BgWorker.DoWork += BgWorker_DoWork;

            Initialize();
        }

        private void Initialize()
        {
            this.UserOpen = false;
            this.LastBufferLength = 0;

            if (this.Port != null)
                this.Port.Initialize();
        }

        /// <summary>
        /// Port Open
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            this.UserOpen = true;

            if (this.Port == null)
            {
                this.Port = new QY_SerialPort("Test");
            }

            if (this.Port.Open())
            {
                //정상 Open
                this.Protocol = new ProtocolBase();

                if (this.BgWorker.IsBusy == false)
                    this.BgWorker.RunWorkerAsync();
                return true;
            }

            //Open 실패
            return false;
        }

        public bool Close()
        {
            this.UserOpen = false;

            if (this.Port == null) return true; 

            if (this.Port.Close())
            {
                this.Port = null;
                this.Protocol = null;

                if (this.BgWorker.IsBusy)
                    this.BgWorker.CancelAsync();

                return true;
            }

            return false;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    //BgWorker 종료
                    if (this.BgWorker.CancellationPending == true) break;
                    else
                    {
                        if (this.UserOpen == false) return;
                        else
                        {
                            //UserOpen인데 Port가 Open이 아닐시 재 연결 시도
                            if(this.Port.IsOpen == false || this.Protocol == null)
                            {
                                this.Initialize();
                                this.Open();

                                continue;
                            }
                        }

                        if (this.CommSync)
                        {
                            //동기 연결 통신
                            if(this.LastSendingData != null && this.SendingQueue.Count > 0)
                            {
                                //송신처리
                                this.Communication_Write(this.SendingQueue);
                            }
                            else
                            {
                                //수신처리
                                this.Communication_Read();
                            }
                        }
                        else
                        {
                            //비동기 연결 통신
                            if(this.LastSendingData != null)
                            {
                                if (this.FrontSendingQueue.Count > 0)
                                {
                                    //우선 송신처리
                                    this.Communication_Write(this.FrontSendingQueue);
                                }
                                else if (this.SendingQueue.Count > 0)
                                {
                                    //일반 송신처리
                                    this.Communication_Write(this.SendingQueue);
                                }
                            }
                            else
                            {
                                //수신처리
                                this.Communication_Read();
                            }
                        }
                    }
                }
                catch(Exception ex) 
                {
                    Debug.WriteLine(string.Format("[Error]({0})Port BgWorker Error\nMessage: {1}\n\nTrack: {2}",
                        this.Port.Name, ex.Message, ex.StackTrace));
                }
            }
        }

        /// <summary>
        /// 통신 읽기
        /// </summary>
        /// <returns>true: Timeout, false: 정상진행</returns>
        /// <exception cref="Exception">통신 변환 에러</exception>
        private void Communication_Read()
        {
            this.StackBuffer = this.Port.Read(this.StackBuffer);

            //Timeout 처리
            if (this.TimeoutCheck())
            {
                if (this.LastSendingData.ReqTryCount < this.LastSendingData.MaxReqTryCount)
                {
                    //Request 재시도
                    this.Port.Write(this.LastSendingData.ReqData);

                    this.LastSendingData.ReqTime = DateTime.Now;
                    this.LastSendingData.ReqTryCount++;
                }
                else
                {
                    //최종 실패 처리
                    //Port Buffer Clear, Connection 처리 어떻게 할지 추가 구상 필요

                    if (this.LastSendingData.IsFrontReq)
                        this.FrontSendingQueue.Dequeue();
                    else
                        this.SendingQueue.Dequeue();

                    this.LastSendingData = null;
                }

                return;
            }

            //Protocol에따른 Data 추출
            int parseCode;
            Queue<byte[]> parseData = this.Protocol.ParseData(this.StackBuffer, out parseCode);
            if (parseData == null) parseCode = -1;

            switch (parseCode)
            {
                case 0: //아직 안들어옴
                case 1: //아직 덜들어옴
                    break;
                case 2:
                    //정상처리
                    if (this.CommSync)
                    {
                        while (parseData.Count > 0)
                        {
                            this.ReceiveQueue.Enqueue(parseData.Dequeue());

                            this.SendingQueue.Dequeue();
                        }
                    }
                    else
                    {
                        this.LastSendingData.ResData = parseData.Dequeue();
                        this.LastSendingData.ResTime = DateTime.Now;

                        if (this.LastSendingData.IsFrontReq)
                            this.FrontSendingQueue.Dequeue();
                        else
                            this.SendingQueue.Dequeue();

                        this.ReceiveQueue.Enqueue(this.LastSendingData.ResData);

                        this.LastSendingData = null;
                    }
                    break;
                default:
                    //변환 에러
                    throw new Exception(string.Format("({0}) Protocol Parse Data Error", this.Protocol.GetType().Name));
            }

            return;
        }

        /// <summary>
        /// Timeout 검사
        /// </summary>
        /// <returns>true: Timeout, false: 문제없음</returns>
        private bool TimeoutCheck()
        {
            bool receiveStop = true;

            if (this.LastBufferLength == 0
                || (this.LastBufferLength != 0 && this.LastBufferLength != this.StackBuffer.Length))
            {
                this.LastBufferLength = this.StackBuffer.Length;
                receiveStop = false;
            }

            //Timeout검사
            if (this.LastBufferLength == 0)
            {
                //Receive Data 없음
                if (this.Timeout_ReceiveStop < (DateTime.Now - this.LastSendingData.ReqTime).TotalMilliseconds)
                {
                    Debug.WriteLine(string.Format("[Error]({0}) Port Timeout - None Receive", this.Port.Name));
                    return true;
                }
            }
            else
            {
                if (receiveStop)
                {
                    //Receive 오다 멈춤
                    if (this.Timeout_ReceiveStop < (DateTime.Now - this.LastSendingData.ReqTime).TotalMilliseconds)
                    {
                        Debug.WriteLine(string.Format("[Error]({0}) Port Timeout - Receive Stop", this.Port.Name));
                        return true;
                    }
                }
                else
                {
                    //Receive 너무 오래걸림
                    if (this.Timeout_TooLong < (DateTime.Now - this.LastSendingData.ReqTime).TotalMilliseconds)
                    {
                        Debug.WriteLine(string.Format("[Error]({0}) Port Timeout - Receive Too Long", this.Port.Name));
                        return true;
                    }
                }
            }

            return false;   
        }

        /// <summary>
        /// 통신 쓰기
        /// </summary>
        /// <param name="queue">담당 Queue</param>
        private void Communication_Write(Queue<CommFrame> queue)
        {
            CommFrame commFrame = queue.Peek();

            this.Port.Write(commFrame.ReqData);

            commFrame.ReqTime = DateTime.Now;
            commFrame.ReqTryCount++;

            if (queue == this.FrontSendingQueue)
                commFrame.IsFrontReq = true;

            this.LastSendingData = commFrame;
        }
    }
}
