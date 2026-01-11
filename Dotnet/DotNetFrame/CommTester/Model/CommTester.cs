using DotNet.Comm.ClientPorts.AppPort;
using DotNet.Comm.Protocols;
using DotNet.Comm.Protocols.Customs.HYNux;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model
{
    /// <summary>
    /// Application Protocol
    /// </summary>
    /// <remarks>
    /// OSI 7계층 App Layer에서 확인하는 Protocol
    /// </remarks>
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

    public enum FrameStatus
    {
        Ready,
        Requesting,
        Reading,
        Result_Comm_OK,
        Result_Comm_None,
        Result_Comm_Stop,
        Result_Comm_Long,
        Result_Protocol_ErrorCode,
        Result_Protocol_NG,
    }

    internal class CommTester : AppPort
    {
        public event EventHandler<string> OSPortLog;
        public event EventHandler<TestDataFrame> TestFrameUpdated;

        /// <summary>Protocol 종류</summary>
        private ProtocolType _protocolType = ProtocolType.None;
        /// <summary>Protocol</summary>
        private ProtocolBase _protocol = null;
        /// <summary>Port 동작기</summary>
        private BackgroundWorker _bgWorker = new BackgroundWorker();

        /// <summary>에러코드 추가 여부</summary>
        private bool _reg_errCode = false;
        /// <summary>반복전송 여부</summary>
        private bool _reg_repeat_enable = false;
        /// <summary>반복전송 수</summary>
        private int _reg_repeat_count = 3;
        /// <summary>반복전송 - 무한전송 여부</summary>
        private bool _reg_repeat_infinity = false;

        /// <summary>현재 전송중인 Frame</summary>
        private TestDataFrame _write_frame_current_test = null;
        /// <summary>현재 Buffer</summary>
        private byte[] _read_buffer = null;
        /// <summary>최근 Buffer 길이</summary>
        /// <remarks>Timeout에서 검사한 최근 Buffer 길이</remarks>
        private int _read_buffer_last_length = 0;
        /// <summary>최근 Buffer 읽은 시간</summary>
        private DateTime _read_buffer_last_time = DateTime.MinValue;

        /// <summary>Protocol 종류</summary>
        public ProtocolType ProtocolType
        {
            get => this._protocolType;
            set
            {
                if (this._protocolType != value)
                {
                    switch (value)
                    {
                        case ProtocolType.Modbus: this._protocol = new Modbus(true); break;
                        case ProtocolType.HY_ModbusRTU: this._protocol = new HYModbus(true); break;
                        case ProtocolType.HY_ModbusRTU_EXP: this._protocol = new HYModbus(true) { IsEXP = true }; break;
                        case ProtocolType.HY_ModbusAscii: this._protocol = new HYModbus(true) { IsAscii = true }; break;
                        case ProtocolType.HY_ModbusAscii_EXP: this._protocol = new HYModbus(true) { IsAscii = true, IsEXP = true }; break;
                        case ProtocolType.HY_ModbusTCP: this._protocol = new HYModbus(true) { IsTCP = true }; break;
                        case ProtocolType.HY_PCLink_STD: this._protocol = new PCLink(true); break;
                        case ProtocolType.HY_PCLink_STD_TH300500: this._protocol = new PCLink(true) { IsTH3500 = true }; break;
                        case ProtocolType.HY_PCLink_SUM: this._protocol = new PCLink(true) { IsSUM = true }; break;
                        case ProtocolType.HY_PCLink_SUM_TH300500: this._protocol = new PCLink(true) { IsSUM = true, IsTH3500 = true }; break;
                        case ProtocolType.HY_PCLink_SUM_TD300500: this._protocol = new PCLink(true) { IsSUM = true, IsTD3500 = true }; break;

                        default: this._protocol = null; break;
                    }

                    this._protocolType = value;

                    base.OnPropertyChanged(nameof(this.ProtocolType));
                }
            }
        }
        /// <summary>에러코드 추가 여부</summary>
        public bool Reg_AddErrCode
        {
            get => _reg_errCode;
            set
            {
                if (this.ProtocolType != ProtocolType.None
                    && this.Reg_AddErrCode != value)
                {
                    this._reg_errCode = value;

                    base.OnPropertyChanged(nameof(this.Reg_AddErrCode));
                }
            }
        }
        /// <summary>반복전송 여부</summary>
        public bool Reg_Repeat_Enable
        {
            get => _reg_repeat_enable;
            set
            {
                if (this.Reg_Repeat_Enable != value)
                {
                    this._reg_repeat_enable = value;

                    base.OnPropertyChanged(nameof(this.Reg_Repeat_Enable));
                }
            }
        }
        /// <summary>반복전송 수</summary>
        public int Reg_Repeat_Count
        {
            get => _reg_repeat_count;
            set
            {
                if (this.Reg_Repeat_Enable
                    && this.Reg_Repeat_Infinity == false
                    && this.Reg_Repeat_Count != value)
                {
                    this._reg_repeat_count = value;
                }
            }
        }
        /// <summary>반복전송 - 무한전송 여부</summary>
        public bool Reg_Repeat_Infinity
        {
            get => _reg_repeat_infinity;
            set
            {
                if (this.Reg_Repeat_Enable
                    && this.Reg_Repeat_Infinity != value)
                {
                    this._reg_repeat_infinity = value;

                    base.OnPropertyChanged(nameof(this.Reg_Repeat_Infinity));
                }
            }
        }
        public bool IsWriting => this._write_frame_current_test != null;

        internal CommTester()
        {
            base.ComPortLog += (obj) => { this.OSPortLog?.Invoke(this, (string)obj[0]); };
            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += this._bgWorker_DoWork;
        }

        /// <summary>
        /// Port 연결
        /// </summary>
        /// <returns>연결 여부</returns>
        public override bool Connect()
        {
            if (base.IsAppOpen) return false;

            base.OSPort.Open();
            this._bgWorker.RunWorkerAsync();

            base.IsAppOpen = true;

            return true;
        }
        /// <summary>
        /// Port 연결 해제
        /// </summary>
        public override bool Disconnect()
        {
            if (this._bgWorker.IsBusy)
                this._bgWorker.CancelAsync();

            base.OSPort.Close();

            base.IsAppOpen = false;

            return true;
        }
        public override void Initialize()
        {
            if(this._bgWorker.IsBusy) this._bgWorker.CancelAsync();
            base.IsAppOpen = false;
            base.OSPort.InitPort();

            this._read_buffer = null;
            this._read_buffer_last_length = 0;
            this._read_buffer_last_time = DateTime.MinValue;

            this.Reg_AddErrCode = false;
            this.Reg_Repeat_Enable = false;
            this.Reg_Repeat_Count = 3;
            this.Reg_Repeat_Infinity = false;
        }
        public override byte[] Read()
        {
            byte[] readBytes = this.OSPort.Read();

            if (readBytes != null)
            {
                if (this._read_buffer == null)
                    this._read_buffer = readBytes;
                else
                {
                    byte[] temp = new byte[this._read_buffer.Length + readBytes.Length];
                    Buffer.BlockCopy(this._read_buffer, 0, temp, 0, this._read_buffer.Length);
                    Buffer.BlockCopy(readBytes, 0, temp, this._read_buffer.Length, readBytes.Length);
                    this._read_buffer = temp;
                }

                this._write_frame_current_test.Comm.Buffer = this._read_buffer;
                this._read_buffer_last_time = DateTime.Now;

                this.TestFrameUpdated?.Invoke(this, this._write_frame_current_test);
            }

            return this._read_buffer;
        }
        public override void Write(byte[] bytes)
        {
            if (this._write_frame_current_test == null) return;

            this._write_frame_current_test.Status = FrameStatus.Requesting;
            this._read_buffer = null;
            this._read_buffer_last_length = 0;
            this._read_buffer_last_time = DateTime.MinValue;

            this._write_frame_current_test.Comm.TryCount_Cur++;
            this._write_frame_current_test.Comm.SendingTime = DateTime.Now;

            base.OSPort.Write(this._write_frame_current_test.Comm.ReqBytes);
            this.TestFrameUpdated?.Invoke(this, this._write_frame_current_test);

            this._write_frame_current_test.Status = FrameStatus.Reading;
        }

        /// <summary>Port 동작 Process</summary>
        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this._bgWorker.CancellationPending || base.IsAppOpen == false) return;

                    //포트 상태값 전송
                    base.OnPropertyChanged(nameof(this.OSPort.IsOpen));
                    base.OnPropertyChanged(nameof(this.IsWriting));

                    if (base.OSPort.IsOpen == false)
                    {
                        base.OSPort.Open();
                        System.Threading.Thread.Sleep(3000);
                        continue;
                    }

                    if (this._write_frame_current_test == null) continue;

                    if (this._write_frame_current_test.Status == FrameStatus.Reading)
                    {
                        //Timeout
                        if (this.IsTimeout())
                        {
                            if (this._write_frame_current_test.Comm.TryCount_Cur >= this._write_frame_current_test.Comm.TryCount_Max)
                            {
                                //최대 시도횟수 초과
                                this._write_frame_current_test = null;
                                this._read_buffer = null;
                                this._read_buffer_last_length = 0;

                                base.OSPort.InitPort();
                            }
                            else
                            {
                                //재시도 처리
                                this._write_frame_current_test.Status = FrameStatus.Ready;
                                this.TestFrameUpdated?.Invoke(this, this._write_frame_current_test);
                            }

                            continue;
                        }

                        //Port 읽기
                        this.Read();

                        //3. Protocol 처리
                        if (this._protocol != null)
                        {
                            byte[] frameBytes = this._protocol.Response_ExtractFrame(this._read_buffer, this._write_frame_current_test.Comm.ReqBytes);

                            if (frameBytes != null)
                            {
                                this._write_frame_current_test.Comm.RcvBytes = frameBytes;

                                //ErrorCode 확인
                                bool isErr = false;
                                if (this._protocol.ConfirmErrCode(this._write_frame_current_test.Comm.RcvBytes) == false)
                                {
                                    isErr = true;
                                    this._write_frame_current_test.Status = FrameStatus.Result_Protocol_ErrorCode;
                                    this._write_frame_current_test.Result.Protocol.ErrorCode++;
                                }
                                else
                                {
                                    List<object> readItems
                                        = this._protocol.Response_ExtractData(
                                            this._write_frame_current_test.Comm.RcvBytes,
                                            this._write_frame_current_test.Comm.ReqBytes
                                            );

                                    if (readItems != null && readItems.Count > 0)
                                    {
                                        if (this._protocol is HYModbus modbus)
                                        {
                                            foreach (DataFrame_Modbus frame in readItems)
                                            {
                                                if (frame.FuncCode > 0x80)
                                                {
                                                    //Protocol Error 처리
                                                    this._write_frame_current_test.Status = FrameStatus.Result_Protocol_NG;
                                                    isErr = true;
                                                    break;
                                                }
                                            }
                                        }
                                        else if (this._protocol is PCLink pcLink)
                                        {

                                        }
                                    }
                                }

                                //수신 완료처리
                                if (isErr == false)
                                    this._write_frame_current_test.Status = FrameStatus.Result_Comm_OK;

                                this.TestFrameUpdated?.Invoke(this, this._write_frame_current_test);

                                if (this._write_frame_current_test.Comm.TryCount_Cur >= this._write_frame_current_test.Comm.TryCount_Max)
                                    this._write_frame_current_test.Status = FrameStatus.Ready;
                                else
                                    this._write_frame_current_test = null;

                                this._read_buffer = null;
                            }
                        }//Protocol 처리 End
                    }
                    else
                    {
                        //데이터 전송
                        this.Write(this._write_frame_current_test.Comm.ReqBytes);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format(
                        "HYCommTestport.cs - bgWorkerDoWork()\r\n" +
                        "{0}\r\n\r\n" +
                        "{1}",
                        ex.Message, ex.StackTrace));

                    this._write_frame_current_test = null;
                    this._read_buffer = null;
                    System.Threading.Thread.Sleep(1000);
                }
                finally
                {
                    System.Threading.Thread.Sleep(20);
                }
            }
        }
        /// <summary>
        /// Timeout 여부
        /// </summary>
        /// <returns>Timeout 결과</returns>
        private bool IsTimeout()
        {
            if (this._write_frame_current_test == null) return false;

            TimeSpan ts;
            if (this._read_buffer_last_length <= 0)
            {
                //None Receive Timeout
                ts = DateTime.Now - this._write_frame_current_test.Comm.SendingTime;

                if (ts.TotalMilliseconds > 3000)
                {
                    //Receive 없음
                    this._write_frame_current_test.Status = FrameStatus.Result_Comm_None;
                    this._write_frame_current_test.Result.Comm.None++;
                    this.TestFrameUpdated?.Invoke(this, this._write_frame_current_test);
                    return true;
                }
            }
            else
            {
                ts = DateTime.Now - this._write_frame_current_test.Comm.SendingTime;
                //Sending시간 > 10초전 && 계속 StackBuffer가 증가중일 경우
                if (this._read_buffer == null ||
                    (ts.TotalMilliseconds > 10000 && (this._read_buffer != null && (this._read_buffer_last_length != this._read_buffer.Length)))
                    )
                {
                    //Receie가 너무 김
                    this._write_frame_current_test.Status = FrameStatus.Result_Comm_Long;
                    this._write_frame_current_test.Result.Comm.Long++;
                    this.TestFrameUpdated?.Invoke(this, this._write_frame_current_test);

                    return true;
                }

                ts = DateTime.Now - this._read_buffer_last_time;
                //최근 Receive 시간 > 5초전
                if (ts.TotalMilliseconds > 5000)
                {
                    //Receive 중단됨
                    this._write_frame_current_test.Status = FrameStatus.Result_Comm_Stop;
                    this._write_frame_current_test.Result.Comm.Stop++;
                    this.TestFrameUpdated?.Invoke(this, this._write_frame_current_test);

                    return true;
                }
            }

            this._read_buffer_last_length = this._read_buffer?.Length ?? 0;

            return false;
        }

        /// <summary>
        /// 전송 등록
        /// </summary>
        /// <param name="text">전송할 Text</param>
        /// <returns>등록 결과</returns>
        internal bool Register_Data(byte[] bytes)
        {
            if (bytes == null) return false;

            //Protocol Error Code 추가
            if (this.ProtocolType != ProtocolType.None && this.Reg_AddErrCode)
            {
                byte[] errCode = this._protocol.CreateErrCode(bytes);

                bytes = QYUtils.Comm.BytesAppend(bytes, errCode);
            }

            //TryCount 설정
            int tryCount = 1;
            if (this.Reg_Repeat_Enable)
                tryCount = this.Reg_Repeat_Infinity ? int.MaxValue : this._reg_repeat_count;

            //전송 프레임 생성
            TestDataFrame testFrame = new TestDataFrame(bytes);
            testFrame.Comm.TryCount_Max = tryCount;

            this._write_frame_current_test = testFrame;

            return true;
        }
    }

    public class TestDataFrame
    {
        private FrameInfo _comm = new FrameInfo();
        private FrameResult _result = new FrameResult();
        private FrameStatus _status = FrameStatus.Ready;

        public FrameInfo Comm { get => _comm; }
        public FrameResult Result { get => _result; }
        public FrameStatus Status { get => _status; set => _status = value; }

        public TestDataFrame(byte[] reqFrame)
        {
            this._comm.ReqBytes = reqFrame;
        }

        public TestDataFrame()
        {
            this._comm.ReqBytes = null;
        }

        public class FrameInfo
        {
            private byte[] _comm_buffer = null;
            private byte[] _comm_reqBytes = null;
            private byte[] _comm_rcvBytes = null;
            private int _comm_trycount_max = 1;
            private int _comm_trycount_cur = 0;
            private DateTime _comm_time_sending_last;

            public byte[] Buffer { get => _comm_buffer; set => _comm_buffer = value; }
            public byte[] ReqBytes { get => _comm_reqBytes; set => _comm_reqBytes = value; }
            public byte[] RcvBytes { get => _comm_rcvBytes; set => _comm_rcvBytes = value; }
            public int TryCount_Max { get => _comm_trycount_max; set => _comm_trycount_max = value; }
            public int TryCount_Cur { get => _comm_trycount_cur; set => _comm_trycount_cur = value; }
            public DateTime SendingTime { get => _comm_time_sending_last; set => _comm_time_sending_last = value; }
        }

        public class FrameResult
        {
            private CommResult _comm = new CommResult();
            private ProtocolResult _protocol = new ProtocolResult();

            public CommResult Comm { get => _comm;}
            public ProtocolResult Protocol { get => _protocol;}

            public class CommResult
            {
                private int _ok = 0;
                private int _none = 0;
                private int _stop = 0;
                private int _long = 0;

                public int OK { get => _ok; set => _ok = value; }
                public int None { get => _none; set => _none = value; }
                public int Stop { get => _stop; set => _stop = value; }
                public int Long { get => _long; set => _long = value; }
            }

            public class ProtocolResult
            {
                private int _errorcode = 0;
                private int _ng = 0;

                public int ErrorCode { get => _errorcode; set => _errorcode = value; }
                public int NG { get => _ng; set => _ng = value; }
            }
        }
    }
}
