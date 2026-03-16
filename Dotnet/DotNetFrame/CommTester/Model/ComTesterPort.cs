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

namespace DotNet.CommTester.Model
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

    internal enum ResultType
    {
        Request,
        Response,
        Error_Timeout_None,
        Error_Timeout_Long,
        Error_Timeout_Stop,
        Error_Protocol_ErrorCode,
        Error_Protocol_Frame,
        Error_Etc,
    }

    internal class ComTesterPort : AppPort
    {
        internal event Action<PortResult> GetResult;
        private void OnResult(ResultType type, byte[] request, byte[] response)
        {
            if (this.GetResult != null)
            {
                byte[] req = null, res = null;

                if (request != null)
                {
                    req = new byte[request.Length];

                    Buffer.BlockCopy(request, 0, req, 0, req.Length);
                }

                if (response != null)
                {
                    res = new byte[response.Length];

                    Buffer.BlockCopy(response, 0, res, 0, res.Length);
                }

                this.GetResult?.Invoke(new PortResult(type, request, res));
            }
        }

        BackgroundWorker _bgWorker = new BackgroundWorker();
        object _buffer_lock = new object();

        bool _write_cancel = false;
        byte[] _write_bytes = null;
        int _write_count_current = 0;
        int _write_count_max = 3;
        DateTime _write_time = DateTime.MinValue;

        byte[] _read_buffer = null;
        bool _read_reading = false;
        int _read_buffer_last_length = 0;
        DateTime _read_buffer_last_time = DateTime.MinValue;

        int _timeout_none_miliseconds = 3000;
        int _timeout_long_miliseconds = 10000;
        int _timeout_stop_miliseconds = 5000;

        ProtocolBase _protocol = null;
        ProtocolType _protocol_type;

        //2. Property
        /// <summary>Protocol 종류</summary>
        public ProtocolType ProtocolType
        {
            get => this._protocol_type;
            set
            {
                if (this.ProtocolType != value)
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

                    this._protocol_type = value;
                }
            }
        }
        public ProtocolBase Protocol => this._protocol;
        public int MaxRepeat { get => this._write_count_max; set => this._write_count_max = value; }
        public int Timeout_None_Miliseconds => this._timeout_none_miliseconds;
        public int Timeout_Long_Miliseconds => this._timeout_long_miliseconds;
        public int Timeout_Stop_Miliseconds => this._timeout_stop_miliseconds;

        public bool IsSending => this._read_reading;
        public byte[] PortBuffer
        {
            get
            {
                lock (this._buffer_lock)
                {
                    if (this._read_buffer == null) return null;

                    byte[] copy = new byte[this._read_buffer.Length];
                    Buffer.BlockCopy(this._read_buffer, 0, copy, 0, copy.Length);

                    return copy;
                }
            }
        }

        internal ComTesterPort()
        {
            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += _bgWorker_DoWork;
        }

        public override bool Connect()
        {
            if (base.IsAppOpen || this._bgWorker.IsBusy) return false;

            base.OSPort.Open();
            base.IsAppOpen = true;

            if(this._bgWorker.IsBusy == false) this._bgWorker.RunWorkerAsync();

            return true;
        }
        public override bool Disconnect()
        {
            base.IsAppOpen = false;
            if(this._bgWorker.IsBusy) this._bgWorker.CancelAsync();

            base.OSPort.Close();

            return true;
        }
        public override void Initialize()
        {
            this._write_bytes = null;       //Write Data
            this._write_cancel = false;     //중단요청
            this._write_count_current = 0;  //전송 Try 횟수
            this._write_count_max = 3;

            this._read_buffer = null;
            this._read_reading = false;     //Read 진행중

            base.IsAppOpen = false;
            if (this._bgWorker.IsBusy) this._bgWorker.CancelAsync();

            base.OSPort.InitPort();
        }
        public override byte[] Read()
        {
            byte[] read = base.OSPort.Read();

            if(read != null)
            {
                lock (this._buffer_lock)
                {
                    if (this._read_buffer == null)
                        this._read_buffer = read;
                    else
                        this._read_buffer = QYUtils.BytesAppend(this._read_buffer, read);
                }

                this._read_buffer_last_time = DateTime.Now;
            }

            return this._read_buffer;
        }
        public override void Write(byte[] bytes)
        {
            this._write_count_current = 0;
            this._read_reading = false;     //Read 진행중

            this._write_bytes = bytes;
        }

        public void DoStop() => this._write_cancel = true;

        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (true)
            {
                try
                {
                    if (worker.CancellationPending ||
                        base.IsAppOpen == false)
                        break;

                    if(base.OSPort.IsOpen == false)
                    {
                        base.OSPort.InitPort();
                        base.OSPort.Open();

                        System.Threading.Thread.Sleep(3000);
                        continue;
                    }

                    if (this._write_cancel)
                    {
                        //초기화
                        this._write_bytes = null;       //Write Data
                        this._write_cancel = false;     //중단요청
                        this._write_count_current = 0;  //전송 Try 횟수

                        this._read_reading = false;     //Read 진행중

                        continue;
                    }

                    if(this._write_bytes != null &&
                        this._write_count_current < this._write_count_max &&
                        this._read_reading == false)
                    {
                        //Write 조건
                        //1. Write Data 있음
                        //2. TryCount 안넘음
                        //3. Request에대한 Reading중이 아님

                        this._write_count_current++;
                        this._write_time = DateTime.Now;
                        this.OSPort.Write(this._write_bytes);

                        this._read_buffer = null;
                        this._read_buffer_last_length = 0;
                        this._read_buffer_last_time = DateTime.MinValue;
                        this._read_reading = true;

                        this.OnResult(ResultType.Request, this._write_bytes, null);
                    }
                    else if(this._write_bytes != null &&
                        this._read_reading)
                    {
                        //Read 조건
                        //1. Write Data 있음
                        //2. Request에대한 Reading 중
                        //3. Write Data Timeout 안됨
                        if (this.IsTimeout())
                        {
                            if (this._write_count_current >= this._write_count_max)
                            {
                                this._write_bytes = null;
                                this._write_count_current = 0;
                            }

                            this._read_reading = false;
                        }

                        this.Read();

                        if(this._protocol != null)
                        {
                            //Protocol 처리
                            byte[] frameBytes = this._protocol.Response_ExtractFrame(this._read_buffer, this._write_bytes);

                            //Protocol Frame 추출됨
                            if(frameBytes != null)
                            {
                                int error_type = 0;

                                if(this._protocol.ConfirmErrCode(frameBytes) == false)
                                {
                                    //에러코드 에러 발생
                                    error_type = 1;
                                    this.OnResult(ResultType.Error_Protocol_ErrorCode, this._write_bytes, frameBytes);
                                }
                                else
                                {
                                    //Protocol 결과 Item 목록
                                    List<object> readItems = this._protocol.Response_ExtractData(frameBytes, this._write_bytes);

                                    if(readItems != null && readItems.Count > 0)
                                    {
                                        if (this._protocol is HYModbus modbus)
                                        {
                                            foreach (DataFrame_Modbus frame in readItems)
                                            {
                                                if (frame.FuncCode > 0x80)
                                                {
                                                    //Protocol Error 처리
                                                    error_type = 2;
                                                    this.OnResult(ResultType.Error_Protocol_Frame, this._write_bytes, frameBytes);
                                                    break;
                                                }
                                            }
                                        }
                                        else if (this._protocol is PCLink pcLink)
                                        {

                                        }
                                    }
                                }

                                //Protocol 완료처리
                                if(error_type == 0)
                                {
                                    //수신 완료
                                    this.OnResult(ResultType.Response, this._write_bytes, frameBytes);
                                }

                                //Read 종료
                                if (this._write_count_current >= this._write_count_max)
                                {
                                    //Read 최종 종료
                                    this._write_bytes = null;
                                    this._write_count_current = 0;
                                }
                                this._read_reading = false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._write_bytes = null;
                    this._write_count_current = 0;
                    this._read_reading = false;

                    Console.WriteLine($"Communication Error {ex.Message}\r\nTrace: {ex.StackTrace}");
                    this.OnResult(ResultType.Error_Etc, null, null);
                }
            }
        }
        /// <summary>
        /// Timeout 여부
        /// </summary>
        /// <returns>Timeout 결과</returns>
        private bool IsTimeout()
        {
            if (this._write_bytes == null) return false;

            TimeSpan ts;
            if (this._read_buffer_last_length <= 0)
            {
                //None Receive Timeout
                ts = DateTime.Now - this._write_time;

                if (ts.TotalMilliseconds > this.Timeout_None_Miliseconds)
                {
                    //Receive 없음
                    this.OnResult(ResultType.Error_Timeout_None, this._write_bytes, this._read_buffer);
                    return true;
                }
            }
            else
            {
                ts = DateTime.Now - this._write_time;
                //Sending시간 > 10초전 && 계속 StackBuffer가 증가중일 경우
                if (this._read_buffer == null ||
                    (ts.TotalMilliseconds > this.Timeout_Long_Miliseconds && (this._read_buffer != null && (this._read_buffer_last_length != this._read_buffer.Length)))
                    )
                {
                    //Receie가 너무 김
                    this.OnResult(ResultType.Error_Timeout_Long, this._write_bytes, this._read_buffer);
                    return true;
                }

                ts = DateTime.Now - this._read_buffer_last_time;
                //최근 Receive 시간 > 5초전
                if (ts.TotalMilliseconds > this.Timeout_Stop_Miliseconds)
                {
                    //Receive 중단됨
                    this.OnResult(ResultType.Error_Timeout_Stop, this._write_bytes, this._read_buffer);
                    return true;
                }
            }

            this._read_buffer_last_length = this._read_buffer == null ? 0 : this._read_buffer.Length;

            return false;
        }
    }

    internal class PortResult
    {
        private ResultType _type;
        private DateTime _time;
        private byte[] _req;
        private byte[] _res;

        public ResultType Type => this._type;
        public DateTime Time => this._time;
        public byte[] Request => this._req;
        public byte[] Response => this._res;

        public PortResult(ResultType type, byte[] req, byte[] res)
        {
            this._type = type;
            this._time = DateTime.Now;
            this._req = req;
            this._res = res;
        }
    }
}
