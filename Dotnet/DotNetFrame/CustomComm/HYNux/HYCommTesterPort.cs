using DotNet.Comm;
using DotNet.Comm.ClientPorts;
using DotNet.Comm.Protocols;
using DotNet.Comm.Protocols.Customs.HYNux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CustomComm.HYNux
{
    internal enum ProtocolType
    {
        None,
        Modbus,
        HY_ModbusRTU,
        HY_ModbusRTU_EXP,
        HY_ModbusAscii,
        HY_ModbusAscii_EXP,
        HY_ModbusTCP,
        HY_PCLink_STD,
        HY_PCLink_STD_TH300500,
        HY_PCLink_SUM,
        HY_PCLink_SUM_TD300500,
        HY_PCLink_SUM_TH300500,
    }
    /// <summary>
    /// HY Port
    /// </summary>
    /// <remarks>
    /// 1Port - (1 Request ↔ 1Response) 처리방식
    /// </remarks>
    public class HYCommTesterPort
    {
        /// <summary>
        /// HYPort Log Event Handler
        /// </summary>
        /// <param name="logName">log 이름</param>
        /// <param name="data">추가적으로 가질 Data</param>
        public delegate void HYPortLogHandler(string logName, params object[] data);
        /// <summary>
        /// HYCommTester Port Log Event
        /// </summary>
        public event HYPortLogHandler Log;

        //Setting Fields
        /// <summary>
        /// Port 별칭
        /// </summary>
        private string _tag = string.Empty;
        /// <summary>
        /// Port 통신 종류
        /// </summary>
        private CommType _commType = CommType.Serial;
        /// <summary>
        /// 사용자의 Port Open여부
        /// </summary>
        private bool _isUserOpen = false;
        /// <summary>
        /// Port Protocol 종류
        /// </summary>
        private ProtocolType _protocolType = ProtocolType.HY_ModbusRTU;

        //Process Fields
        /// <summary>
        /// 통신 Port
        /// </summary>
        private CommPortFrame _commPort = new QYSerialPort();
        /// <summary>
        /// 통신 Protocol
        /// </summary>
        private ProtocolFrame _protocol = null;
        /// <summary>
        /// Port에서 최근 전송한 byte Array
        /// </summary>
        private byte[] _sendingBytes = null;
        /// <summary>
        /// Port에서 수신한 누적 Buffer
        /// </summary>
        private byte[] _stackBuffer = null;
        /// <summary>
        /// 최근 Port에서 전송한 시간
        /// </summary>
        /// <remarks>
        /// Receive Timeout - None Recieve, Receive Long에 사용
        /// </remarks>
        private DateTime _sendingTime = DateTime.MinValue;
        /// <summary>
        /// 최근 Data 수신 시간
        /// </summary>
        /// <remarks>
        /// Receive Timeout - Receive Stop에 사용
        /// </remarks>
        private DateTime _recentReadTime = DateTime.MinValue;
        /// <summary>
        /// 현재 StackBuffer의 길이
        /// </summary>
        private int _curBufferLen = 0;
        /// <summary>
        /// 최근 Timeout 검사한 Buffern길이
        /// </summary>
        /// <remarks>
        /// Timeout 검사에서 데이터 수신이 완전히 안왔거나, 중단됨 검사에 사용됨
        /// </remarks>
        private int _recentBufferLen = 0;

        //Setting Properties
        /// <summary>
        /// Port 별칭
        /// </summary>
        /// <remarks>
        /// 사용자가 정한 AppPort명
        /// </remarks>
        public string Tag { get => this._tag; set => this._tag = value; }
        /// <summary>
        /// 통신 Port 종류
        /// </summary>
        /// <remarks>
        /// 기존 종류와 다를경우 통신Port 신규 생성
        /// </remarks>
        public CommType CommType
        {
            get { return this._commType; }
            set
            {
                if (this._commType != value)
                {
                    if (value == CommType.Serial)
                        this._commPort = new QYSerialPort();
                    else if (value == CommType.Ethernet)
                        this._commPort = new QYEthernet(false);

                    if(this.ComPort != null)
                        this.ComPort.Log += (msg) => { this.Log?.Invoke("ComPortLog", new object[] { msg }); };
                }

                this._commType = value;
            }
        }
        /// <summary>
        /// 사용자의 Port Open여부
        /// </summary>
        public bool IsUserOpen { get => this._isUserOpen; }
        /// <summary>
        /// Port Protocol 종류
        /// </summary>
        /// <remarks>
        /// 기존 protocol Type과 다를경우 Protocol Class 신규생성
        /// </remarks>
        internal ProtocolType ProtocolType
        {
            get { return this._protocolType; }
            set
            {
                this.RegularQueue.Clear();
                this.WriteQueue.Clear();
                this._sendingBytes = null;

                //변경될 value가 기존 Type과 다를경우 Protocol 재지정
                if (this._protocolType != value)
                {
                    switch (value)
                    {
                        case ProtocolType.Modbus:                   this._protocol = new Modbus(true); break;
                        case ProtocolType.HY_ModbusRTU:             this._protocol = new HYModbus(true); break;
                        case ProtocolType.HY_ModbusRTU_EXP:         this._protocol = new HYModbus(true) { IsEXP = true }; break;
                        case ProtocolType.HY_ModbusAscii:           this._protocol = new HYModbus(true) { IsAscii = true }; break;
                        case ProtocolType.HY_ModbusAscii_EXP:       this._protocol = new HYModbus(true) { IsAscii = true, IsEXP = true }; break;
                        case ProtocolType.HY_ModbusTCP:             this._protocol = new HYModbus(true) { IsTCP = true }; break;
                        case ProtocolType.HY_PCLink_STD:            this._protocol = new PCLink(true); break;
                        case ProtocolType.HY_PCLink_STD_TH300500:   this._protocol = new PCLink(true) { IsTH3500 = true }; break;
                        case ProtocolType.HY_PCLink_SUM:            this._protocol = new PCLink(true) { IsSUM = true }; break;
                        case ProtocolType.HY_PCLink_SUM_TH300500:   this._protocol = new PCLink(true) { IsSUM = true, IsTH3500 = true }; break;
                        case ProtocolType.HY_PCLink_SUM_TD300500:   this._protocol = new PCLink(true) { IsSUM = true, IsTD3500 = true }; break;
                        default:                                    this._protocol = null; break;
                    }
                }

                this._protocolType = value;
            }
        }


        //Process Properties
        /// <summary>
        /// 통신 Port
        /// </summary>
        public CommPortFrame ComPort { get => this._commPort; }
        /// <summary>
        /// 통신 Protocol
        /// </summary>
        public ProtocolFrame Protocol { get => this._protocol; }
        /// <summary>
        /// 주기적인 Request Queue
        /// </summary>
        internal Queue<byte[]> RegularQueue { get; } = new Queue<byte[]>();
        /// <summary>
        /// 우선순위가 높은 Write Queue
        /// </summary>
        internal Queue<byte[]> WriteQueue { get; } = new Queue<byte[]>();
        /// <summary>
        /// App Port 동작 Thread
        /// </summary>

        private BackgroundWorker _bgWorker = new BackgroundWorker();



        internal HYCommTesterPort()
        {
            this.CommType = CommType.Serial;
            this.ComPort.Log += (msg) => { this.Log?.Invoke("ComPortLog", new object[] { msg }); };
            this.ProtocolType = ProtocolType.None;
            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += _bgWorker_DoWork;
        }

        internal void Connect()
        {
            try
            {
                if (this.ComPort.IsOpen) return;

                this.ComPort.Open();

                this._bgWorker.RunWorkerAsync();
                this._isUserOpen = true;
            }
            catch
            {
                this.Disconnect();
                throw;
            }
        }

        internal void Disconnect()
        {
            if (this._bgWorker.IsBusy)
                this._bgWorker.CancelAsync();

            this.ComPort.Close();
            this.ComPort.InitPort();
            this._isUserOpen = false;
        }

        private void _bgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this._bgWorker.CancellationPending || this.IsUserOpen == false) return;

                    if (this.ComPort.IsOpen == false)
                    {
                        this.ComPort.Open();
                        System.Threading.Thread.Sleep(3000);
                        continue;
                    }

                    if (this._sendingBytes == null)
                    {
                        //비정기 전송
                        if (this.WriteQueue.Count > 0)
                        {
                            this._sendingBytes = this.WriteQueue.Peek();
                        }
                        //정기 전송
                        else if (this.RegularQueue.Count > 0)
                        {
                            this._sendingBytes = this.RegularQueue.Peek();
                        }

                        if (this._sendingBytes != null)
                        {
                            this.SendData(this._sendingBytes);
                            this.Log?.Invoke("Request", new object[] { this._sendingBytes });
                        }
                    }
                    else
                    {
                        //수신
                        //1. Timeout
                        if (this.IsTimeout())
                        {
                            if (this.WriteQueue.Count != 0 && this._sendingBytes == this.WriteQueue.Peek())
                                this.WriteQueue.Dequeue();
                            else if (this.RegularQueue.Count != 0 && this._sendingBytes == this.RegularQueue.Peek())
                                this.RegularQueue.Dequeue();

                            this._sendingBytes = null;
                            this._stackBuffer = null;
                            this.ComPort.InitPort();
                            continue;
                        }

                        //2. 수신처리
                        byte[] readBytes = this.ComPort.Read();

                        if (readBytes != null)
                        {
                            if (this._stackBuffer != null)
                            {
                                byte[] temp = new byte[this._stackBuffer.Length + readBytes.Length];
                                Buffer.BlockCopy(this._stackBuffer, 0, temp, 0, this._stackBuffer.Length);
                                Buffer.BlockCopy(readBytes, 0, temp, this._stackBuffer.Length, readBytes.Length);
                                this._stackBuffer = temp;
                            }
                            else
                                this._stackBuffer = readBytes;


                            this._recentReadTime = DateTime.Now;
                            this._curBufferLen = this._stackBuffer.Length;
                            this.Log?.Invoke("StackBuffer", new object[] { this._stackBuffer });

                            //3. Protocol 처리
                            if (this.Protocol == null)
                            {

                            }
                            else
                            {
                                byte[] frameBytes = this.Protocol.Response_ExtractFrame(this._stackBuffer, this._sendingBytes);

                                if (frameBytes != null)
                                {
                                    //ErrorCode 확인
                                    bool isErr = false;
                                    if (this.Protocol.ConfirmErrCode(frameBytes) == false)
                                    {
                                        isErr = true;
                                        this.Log?.Invoke("Error-ErrorCode", new object[] { frameBytes });
                                    }
                                    else
                                    {
                                        List<object> readItems = this.Protocol.Response_ExtractData(frameBytes, this._sendingBytes);

                                        if (readItems != null && readItems.Count > 0)
                                        {
                                            if (this.Protocol is HYModbus modbus)
                                            {
                                                foreach (DataFrame_Modbus frame in readItems)
                                                {
                                                    if (frame.FuncCode > 0x80)
                                                    {
                                                        //Protocol Error 처리
                                                        isErr = true;
                                                        this.Log?.Invoke("Error-Protocol", new object[] { frameBytes });
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (this.Protocol is PCLink pcLink)
                                            {

                                            }
                                        }
                                    }
                                    //수신 최종 완료처리
                                    if(isErr == false)
                                        this.Log?.Invoke("Receive End", new object[] { this._sendingBytes, frameBytes });
                                    if (this.WriteQueue.Count != 0 && this._sendingBytes == this.WriteQueue.Peek())
                                        this.WriteQueue.Dequeue();
                                    else if (this.RegularQueue.Count != 0 && this._sendingBytes == this.RegularQueue.Peek())
                                        this.RegularQueue.Dequeue();
                                    this._sendingBytes = null;
                                    this._stackBuffer = null;
                                }
                            }//Protocol 처리 End
                        }//read 처리 End
                    }//수신 End
                }
                catch
                {
                    this._sendingBytes = null;
                    System.Threading.Thread.Sleep(3000);
                }
                finally
                {
                    System.Threading.Thread.Sleep(20);
                }
            }
        }

        private bool IsTimeout()
        {
            TimeSpan ts;
            if (this._recentBufferLen <= 0)
            {
                //None Receive Timeout
                ts = DateTime.Now - this._sendingTime;

                if (ts.TotalMilliseconds > 3000)
                {
                    //Receive 없음
                    this.Log?.Invoke("None Response");
                    return true;
                }
            }
            else
            {
                ts = DateTime.Now - this._sendingTime;
                //Sending시간 > 10초전 && 계속 StackBuffer가 증가중일 경우
                if(ts.TotalMilliseconds > 10000 && (this._recentBufferLen != this._curBufferLen))
                {
                    //Receie가 너무 김
                    this.Log?.Invoke("Long Response", new object[] { this._stackBuffer });

                    return true;
                }

                ts = DateTime.Now - this._recentReadTime;
                //최근 Receive 시간 > 5초전
                if(ts.TotalMilliseconds > 5000)
                {
                    //Receive 중단됨
                    this.Log?.Invoke("Stop Response", new object[] { this._stackBuffer });

                    return true;
                }
            }

            this._recentBufferLen = this._curBufferLen;

            return false;
        }

        private void SendData(byte[] bytes)
        {
            this._sendingTime = DateTime.Now;
            this._curBufferLen = 0;
            this._recentBufferLen = 0;
            this._stackBuffer = null;

            this.ComPort.Write(bytes);
        }
    }
}
