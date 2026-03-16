using DotNet.Comm;
using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Comm.Protocols;
using DotNet.CommTester.Model;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.ViewModel
{
    internal class CommTesterVM : QYViewModel
    {
        internal event Action<TesterResult> GetResult;
        private void OnResult(TesterResult rst) => this.GetResult?.Invoke(rst);

        //1. Fields
        private List<QYItem> _portype_list = new List<QYItem>();
        private List<QYItem> _protocol_list = new List<QYItem>();

        private ComTesterPort _port = new ComTesterPort();
        private object _os_viewmodel = null;

        //1. Fields - Tester Proeprty
        private bool _errorcode_add_enable = false;
        private bool _repeat_enable = false;
        private int _repeat_count = 3;
        private bool _repeat_infinity_enable = false;

        //1. Fields - Status
        /// <summary>Frame 현재 전송 수</summary>
        private int _tester_result_success = 0;
        private int _write_count = 0;
        private int _error_timeout_none_count = 0;
        private int _error_timeout_long_count = 0;
        private int _error_timeout_stop_count = 0;
        private int _error_protocol_errorcode_count = 0;
        private int _error_protocol_frame_count = 0;

        private BindingList<TesterResult> _tester_result_list = new BindingList<TesterResult>();
        private string _log_text = string.Empty;

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
                            this._os_viewmodel = new SerialVM(this._port.OSPort as QYSerialPort,  this);
                            break;
                        case PortType.Ethernet:
                            this._os_viewmodel = new EthernetVM(this._port.OSPort as QYEthernet);
                            break;
                    }

                    base.OnPropertyChanged(nameof(this.PortType));
                }
            }
        }
        public ProtocolType ProtocolType
        {
            get => this._port.ProtocolType;
            set
            {
                if(this.ProtocolType != value)
                {
                    this._port.ProtocolType = value;

                    base.OnPropertyChanged(nameof(this.ProtocolType));
                }
            }
        }
        public ProtocolBase Protocol => this._port.Protocol;
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
                    this.SetRepeatCount();

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
                    this.SetRepeatCount();

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
                    this.SetRepeatCount();

                    base.OnPropertyChanged(nameof(this.RepeatInfinity));
                    base.OnPropertyChanged(nameof(this.RepeatEnable));
                }
            }
        }
        private void SetRepeatCount()
        {
            if (this.RepeatEnable)
            {
                if (this.RepeatInfinity)
                    this._port.MaxRepeat = int.MaxValue;
                else
                    this._port.MaxRepeat = this.RepeatCount;
            }
            else
                this._port.MaxRepeat = 1;
        }

        //2. Property - Status
        public bool IsAppOpen => this._port.IsAppOpen;
        public bool IsOSPortOpen => this._port.OSPort.IsOpen;
        public bool IsSending => this._port.IsSending;
        public byte[] PortBuffer => this._port.PortBuffer;
        public BindingList<TesterResult> Results => this._tester_result_list;
        public int SucessCount { get => this._tester_result_success; private set => this._tester_result_success = value; }
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

            this._port.GetResult += _port_GetResult;
        }

        //4. Method
        public void Connection()
        {
            if (this.IsAppOpen) this.Disconnect();
            else this.Connect();

            base.OnPropertyChanged(nameof(CommTesterVM.Connection));
            base.OnPropertyChanged(nameof(this.IsAppOpen));
        }
        public void Connect()
        {
            this._port.Connect();
        }
        public void Disconnect()
        {
            this._port.Disconnect();
        }

        public void RunStop(string text)
        {
            if (this.IsSending) this.Stop();
            else this.Send(text);
        }
        private void Send(string text)
        {
            this.Initialize();

            if (text.Trim() == string.Empty) return;

            byte[] bytes = this.ConvertTextToByte(text);
            if (bytes == null) return;

            if (this.ErrCodeEnable && this.Protocol != null)
            {
                byte[] errcode = this.Protocol.CreateErrCode(bytes);

                bytes = QYUtils.BytesAppend(bytes, errcode);
            }

            this._port.Write(bytes);

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
        private void Stop() => this._port.DoStop();

        private void Initialize()
        {
            this._write_count = 0;
            this._tester_result_success = 0;
            this._tester_result_list.Clear();
            this._log_text = string.Empty;

            this.SucessCount = 0;
            this.Error_Timeout_None_Count = 0;
            this.Error_Timeout_Long_Count = 0;
            this.Error_Timeout_Stop_Count = 0;
            this.Error_Protocol_ErrorCode_Count = 0;
            this.Error_Protocol_Frame_Count = 0;
        }

        private void _port_GetResult(PortResult rst)
        {
            base.UIThread.Post(_ =>
            {
                if (rst.Type == ResultType.Request)
                {
                    this._write_count++;
                    this._tester_result_list.Add(new TesterResult(rst.Type, rst.Time, rst.Request));
                    //this._log_text
                }
                else if (rst.Type == ResultType.Response)
                {
                    this._tester_result_success++;

                    this._tester_result_list.Add(new TesterResult(rst.Type, rst.Time, rst.Response));
                }
                else if (rst.Type == ResultType.Error_Protocol_ErrorCode)
                {
                    this._error_protocol_errorcode_count++;

                    this._tester_result_list.Add(new TesterResult(rst.Type, rst.Time, rst.Response));
                }
                else if (rst.Type == ResultType.Error_Protocol_Frame)
                {
                    this._error_protocol_frame_count++;

                    this._tester_result_list.Add(new TesterResult(rst.Type, rst.Time, rst.Response));
                }
                else if (rst.Type == ResultType.Error_Timeout_None)
                {
                    this._error_timeout_none_count++;

                    this._tester_result_list.Add(new TesterResult(rst.Type, rst.Time, rst.Response));
                }
                else if (rst.Type == ResultType.Error_Timeout_Long)
                {
                    this._error_timeout_long_count++;

                    this._tester_result_list.Add(new TesterResult(rst.Type, rst.Time, rst.Response));
                }
                else if (rst.Type == ResultType.Error_Timeout_Stop)
                {
                    this._error_timeout_stop_count++;

                    this._tester_result_list.Add(new TesterResult(rst.Type, rst.Time, rst.Response));
                }
            }, null);
        }

        private string ByteToString(byte[] bytes)
        {
            string str = string.Empty;
            if (bytes == null) return str;

            for (int i = 0; i < bytes.Length; i++)
                str += $"{bytes[i]:X2} ";

            return str;
        }
    }

    internal class TesterResult
    {
        private ResultType _type;
        private string _item;
        private DateTime _time;
        private byte[] _data;

        public ResultType Type => this._type;
        public string ItemType => this._item;
        public DateTime Time => this._time;
        public string this[int index]
        {
            get
            {
                if (this._data == null || index >= this._data.Length) return "--";
                return this._data[index].ToString("X2");
            }
        }

        public TesterResult(ResultType type, DateTime time, byte[] data)
        {
            this._type = type;
            this._item = type == ResultType.Request ? "Req" : "Rcv";
            this._time = time;
            this._data = data;
        }
    }
}
