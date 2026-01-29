using DotNet.Comm;
using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Comm.Protocols;
using DotNet.Comm.Protocols.Customs.HYNux;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.CommTester.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel
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

    internal class CommTesterVM : QYViewModel
    {
        private enum CommStatus
        {
            None,
            Write,
            Read,
            Protocol,
        }

        internal event Action<CommResult> GetResult;

        //1. Fields
        private List<QYItem> _portype_list = new List<QYItem>();
        private List<QYItem> _protocol_list = new List<QYItem>();

        private object _buffer_lock = new object();
        private Model.ComTesterPort _port = new Model.ComTesterPort();
        private object _os_viewmodel = null;

        //1. Fields - 통신
        /// <summary>Protocol 종류</summary>
        private ProtocolType _protocol_type;
        /// <summary>Protocol</summary>
        private ProtocolBase _protocol = null;

        //1. Fields - Tester Proeprty
        private bool _errorcode_add_enable = false;
        private bool _repeat_enable = false;
        private int _repeat_count = 3;
        private bool _repeat_infinity_enable = false;

        //1. Fields - 통신 Item
        private BackgroundWorker _bgWorker = new BackgroundWorker();
        /// <summary>현재 전송중인 Frame</summary>
        private byte[] _write_frame = null;
        private DateTime _write_time = DateTime.MinValue;
        /// <summary>현재 Buffer</summary>
        private byte[] _read_buffer = null;
        /// <summary>최근 Buffer 길이</summary>
        /// <remarks>Timeout에서 검사한 최근 Buffer 길이</remarks>
        private int _read_buffer_last_length = 0;
        /// <summary>최근 Buffer 읽은 시간</summary>
        private DateTime _read_buffer_last_time = DateTime.MinValue;

        //1. Fields - Status
        private CommStatus _tester_status = CommStatus.None;
        /// <summary>Frame 현재 전송 수</summary>
        private int _tester_result_success = 0;
        private int _write_count = 0;
        private int _write_count_complete = 0;
        private int _error_timeout_none_count = 0;
        private int _error_timeout_long_count = 0;
        private int _error_timeout_stop_count = 0;
        private int _error_protocol_errorcode_count = 0;
        private int _error_protocol_frame_count = 0;

        //2. Property
        public object OSPort_VM => this._os_viewmodel;
        public List<QYItem> PortTypeList => this._portype_list;
        public List<QYItem> ProtocolList => this._protocol_list;

        //2. Property - 통신설정
        public PortType PortType
        {
            get => this._port.PortType;
            set
            {
                if (this.PortType != value)
                {
                    this._port.PortType = value;

                    this._os_viewmodel = null;
                    switch (value)
                    {
                        case PortType.Serial:
                            this._os_viewmodel = new SerialVM(this._port.OSPort as QYSerialPort);
                            break;
                        case PortType.Ethernet:
                            this._os_viewmodel = new EthernetVM(this._port.OSPort as QYEthernet);
                            break;
                    }

                    base.OnPropertyChanged(nameof(this.PortType));
                }
            }
        }
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

                    base.OnPropertyChanged(nameof(this.ProtocolType));
                }
            }
        }
        public bool ErrCodeEnable
        {
            get => this._errorcode_add_enable;
            set
            {
                if (this.ErrCodeEnable != value)
                {
                    this._errorcode_add_enable = value;

                    base.OnPropertyChanged(nameof(this.ErrCodeEnable));
                }
            }
        }

        //2. Property - Tester 설정
        public bool RepeatEnable
        {
            get => this._repeat_enable;
            set
            {
                if (this.RepeatEnable != value)
                {
                    this._repeat_enable = value;

                    base.OnPropertyChanged(nameof(this.RepeatEnable));
                    base.OnPropertyChanged(nameof(this.RepeatEnable));
                }
            }
        }
        public int RepeatCount
        {
            get => this._repeat_count;
            set
            {
                if (this.RepeatCount != value
                    && this.RepeatEnable
                    && this.RepeatInfinity == false)
                {
                    this._repeat_count = value;

                    base.OnPropertyChanged(nameof(this.RepeatCount));
                    base.OnPropertyChanged(nameof(this.RepeatEnable));
                }
            }
        }
        public bool RepeatInfinity
        {
            get => this._repeat_infinity_enable;
            set
            {
                if (this.RepeatInfinity != value
                    && this.RepeatEnable)
                {
                    this._repeat_infinity_enable = value;

                    base.OnPropertyChanged(nameof(this.RepeatInfinity));
                    base.OnPropertyChanged(nameof(this.RepeatEnable));
                }
            }
        }

        //2. Property - Status
        public bool IsAppOpen => this._port.IsAppOpen;
        public bool IsOSPortOpen => this._port.OSPort.IsOpen;
        public bool IsSending => this._tester_status != CommStatus.None;
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
        public int SucessCount { get => this._tester_result_success; private set => this._tester_result_success = value; }
        public int MaxSendCount
        {
            get
            {
                int count = 1;
                if (this.RepeatEnable)
                {
                    if (this.RepeatInfinity) count = int.MaxValue;
                    else count = this.RepeatCount;
                }

                return count;
            }
        }
        public int SendingCount { get => this._write_count; private set => this._write_count = value; }
        public int Error_Timeout_None_Count { get => this._error_timeout_none_count; private set => this._error_timeout_none_count = value; }
        public int Error_Timeout_Long_Count { get => this._error_timeout_long_count; private set => this._error_timeout_long_count = value; }
        public int Error_Timeout_Stop_Count { get => this._error_timeout_stop_count; private set => this._error_timeout_stop_count = value; }
        public int Error_Protocol_ErrorCode_Count { get => this._error_protocol_errorcode_count; private set => this._error_protocol_errorcode_count = value; }
        public int Error_Protocol_Frame_Count { get => this._error_protocol_frame_count; private set => this._error_protocol_frame_count = value; }

        //3. 생성자
        internal CommTesterVM()
        {
            this._portype_list = QYViewUtils.EnumToItem<PortType>().ToList();
            this._protocol_list = QYViewUtils.EnumToItem<ProtocolType>().ToList();

            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += this._bgWorker_DoWork;
        }

        //4. Method
        public void Connection()
        {
            if (this.IsAppOpen)
                this.Disconnect();
            else
                this.Connect();
        }
        public void Connect()
        {
            this._port.Connect();

            base.OnPropertyChanged(nameof(this.IsAppOpen));
        }

        public void Disconnect()
        {
            this._port.Disconnect();

            base.OnPropertyChanged(nameof(this.IsAppOpen));
        }

        public void RunStop(string text)
        {
            if (this.IsSending)
                this.Stop();
            else
                this.Send(text);
        }

        private void Send(string text)
        {
            this.Initialize();

            if (text.Trim() == string.Empty) return;

            byte[] bytes = this.ConvertTextToByte(text);
            if (bytes == null) return;

            if (this.ErrCodeEnable && this._protocol != null)
            {
                byte[] errcode = this._protocol.CreateErrCode(bytes);

                bytes = QYUtils.Comm.BytesAppend(bytes, errcode);
            }

            this._write_frame = bytes;
            this._tester_status = CommStatus.Write;
            this._bgWorker.RunWorkerAsync();

            base.OnPropertyChanged(nameof(this.IsSending));
        }

        /// <summary>
        /// Text → Byte 변환
        /// </summary>
        /// <param name="text">변환할 Text</param>
        /// <returns>변환 된 Byte Array</returns>
        private byte[] ConvertTextToByte(string text)
        {
            int handle = 0;
            List<byte> bytes = new List<byte>();

            while (handle < text.Length)
            {
                char c = text[handle];
                int len;

                //범위 지정
                if (c == '@') len = 3;
                else if (c == '#') len = 2;
                else
                {
                    if (++handle > text.Length) break;
                    else continue;
                }

                if (++handle + len > text.Length) break;

                //변환 시도
                bool tryResult = false;
                string byteStr = text.Substring(handle, len);
                byte b = 0;
                if (c == '@')
                    tryResult = byte.TryParse(byteStr, out b);
                else if (c == '#')
                    tryResult = byte.TryParse(byteStr,
                        System.Globalization.NumberStyles.HexNumber,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out b);


                if (tryResult)
                {
                    bytes.Add(b);
                    handle += len;
                }
            }

            if (bytes.Count == 0)
                return null;
            else
                return bytes.ToArray();
        }

        private void Stop()
        {
            if (this._bgWorker.IsBusy)
                this._bgWorker.CancelAsync();

            this._tester_status = CommStatus.None;

            base.OnPropertyChanged(nameof(this.IsSending));
        }

        private void Initialize()
        {
            this._write_frame = null;
            this._write_count_complete = 0;
            this._write_count = 0;

            this._read_buffer = null;
            this._read_buffer_last_length = 0;
            this._read_buffer_last_time = DateTime.MinValue;

            this.SucessCount = 0;
            this.Error_Timeout_None_Count = 0;
            this.Error_Timeout_Long_Count = 0;
            this.Error_Timeout_Stop_Count = 0;
            this.Error_Protocol_ErrorCode_Count = 0;
            this.Error_Protocol_Frame_Count = 0;
        }

        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                lock (this._buffer_lock)
                {

                    try
                    {
                        if (this._bgWorker.CancellationPending
                            || this.IsAppOpen == false
                            || this.IsSending == false
                            ) return;

                        if (this.IsOSPortOpen == false)
                        {
                            this._port.OSPort.InitPort();
                            this._port.OSPort.Open();

                            System.Threading.Thread.Sleep(3000);
                        }

                        if (this._write_count_complete >= this.MaxSendCount)
                        {
                            //전송횟수 종료처리
                            this._write_frame = null;
                            this._tester_status = CommStatus.None;
                            break;
                        }

                        if (this._tester_status == CommStatus.Write
                        && this._write_frame != null)
                        {
                            this._read_buffer = null;
                            this._read_buffer_last_length = 0;
                            this._read_buffer_last_time = DateTime.MinValue;

                            this._write_count++;
                            this._write_time = DateTime.Now;
                            this._port.Write(this._write_frame);

                            byte[] temp = new byte[this._write_frame.Length];
                            Buffer.BlockCopy(this._write_frame, 0, temp, 0, temp.Length);

                            this.GetResult.Invoke(new CommResult("Write", temp));
                            this._tester_status = CommStatus.Read;
                        }
                        else
                        {
                            //1. Timeout
                            if (this.IsTimeout())
                            {
                                //재시도
                                this._write_count_complete++;
                                this._tester_status = CommStatus.Write;

                                continue;
                            }

                            //2. Data 읽기
                            byte[] readBytes = this._port.Read();
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

                                this._read_buffer_last_time = DateTime.Now;
                            }

                            //3. Protocol 처리
                            if (this._protocol != null)
                            {
                                byte[] frameBytes = this._protocol.Response_ExtractFrame(this._read_buffer, this._write_frame);

                                if (frameBytes != null)
                                {
                                    this._tester_status = CommStatus.Protocol;
                                    bool isErr = false;

                                    //3-1. ErrorCode 확인
                                    if (this._protocol.ConfirmErrCode(frameBytes) == false)
                                    {
                                        isErr = true;
                                        this.Error_Protocol_ErrorCode_Count++;

                                        this.GetResult.Invoke(new CommResult("Error-Protocol-ErrorCode", frameBytes));
                                    }
                                    else
                                    {
                                        List<object> readItems = this._protocol.Response_ExtractData(frameBytes, this._write_frame);

                                        if (readItems != null && readItems.Count > 0)
                                        {
                                            if (this._protocol is HYModbus modbus)
                                            {
                                                foreach (DataFrame_Modbus frame in readItems)
                                                {
                                                    if (frame.FuncCode > 0x80)
                                                    {
                                                        //Protocol Error 처리
                                                        isErr = true;
                                                        this.Error_Protocol_Frame_Count++;

                                                        this.GetResult.Invoke(new CommResult("Error-Protocol-Frame", frameBytes));
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (this._protocol is PCLink pcLink)
                                            {

                                            }
                                        }
                                    }
                                    if (isErr == false)
                                    {
                                        this.SucessCount++;

                                        this.GetResult.Invoke(new CommResult("Read", frameBytes));
                                    }

                                    //3-2. Protocol 완료처리
                                    this._write_count_complete++;
                                    this._read_buffer = null;
                                    this._tester_status = CommStatus.Write;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this._read_buffer = null;
                        this._read_buffer_last_length = 0;
                        this._read_buffer_last_time = DateTime.MinValue;

                        this.GetResult.Invoke(new CommResult($"Send Error: {ex.Message}\r\nTrace: {ex.StackTrace}", this._read_buffer));
                        this._tester_status = CommStatus.None;
                    }
                }
            }
        }
        /// <summary>
        /// Timeout 여부
        /// </summary>
        /// <returns>Timeout 결과</returns>
        private bool IsTimeout()
        {
            if (this._write_frame == null) return false;

            TimeSpan ts;
            if (this._read_buffer_last_length <= 0)
            {
                //None Receive Timeout
                ts = DateTime.Now - this._write_time;

                if (ts.TotalMilliseconds > 3000)
                {
                    //Receive 없음
                    this.Error_Timeout_None_Count++;

                    this.GetResult.Invoke(new CommResult("Error-Timeout-None", this._read_buffer));
                    return true;
                }
            }
            else
            {
                ts = DateTime.Now - this._write_time;
                //Sending시간 > 10초전 && 계속 StackBuffer가 증가중일 경우
                if (this._read_buffer == null ||
                    (ts.TotalMilliseconds > 10000 && (this._read_buffer != null && (this._read_buffer_last_length != this._read_buffer.Length)))
                    )
                {
                    //Receie가 너무 김
                    this.Error_Timeout_Long_Count++;

                    this.GetResult.Invoke(new CommResult("Error-Timeout-Long", this._read_buffer));

                    return true;
                }

                ts = DateTime.Now - this._read_buffer_last_time;
                //최근 Receive 시간 > 5초전
                if (ts.TotalMilliseconds > 5000)
                {
                    //Receive 중단됨
                    this.Error_Timeout_Stop_Count++;

                    this.GetResult.Invoke(new CommResult("Error-Timeout-Stop", this._read_buffer));

                    return true;
                }
            }

            this._read_buffer_last_length = this._read_buffer == null ? 0 : this._read_buffer.Length;

            return false;
        }
    }

    public class CommResult
    {
        private string _type;
        private DateTime _time;
        private byte[] _data;

        public string Type => this._type;
        public DateTime Time => this._time;
        public byte[] Data => this._data;
        public string this[int index]
        {
            get
            {
                if (this.Data == null || index > this._data.Length) return "--";
                return this.Data[index].ToString("X2");
            }
        }

        public CommResult(string type, byte[] data)
        {
            this._type = type;
            this._time = DateTime.Now;
            this._data = data;
        }
    }
}
