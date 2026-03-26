using DotNet.Comm.Transport;
using DotNetFrame.CommTester.Model.Protocol.Custom.HYNux;
using DotNetFrame.CommTester.Model.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetFrame.CommTester.Model.Port.Protocol.Custom.HYNux;
using DotNetFrame.CommTester.Model.Protocol.Modbus;
using DotNet.CommTester.Model.Protocol;

namespace DotNetFrame.CommTester.Model
{
    internal class CommPort : INotifyPropertyChanged
    {
        internal event Action<byte[]> NoneProtocolReceive;
        internal event Action<byte[]> OnSendBinary;
        internal event Action<IProtocolResult> OnResult;

        public event PropertyChangedEventHandler PropertyChanged;

        private PortType _port_type = PortType.Socket;
        private ITransport _port;
        private ProtocolType _protocol_type = ProtocolType.None;
        private IProtocol _protocol;

        private bool _is_user_send = false;
        private byte[] _write_binary = null;
        private bool _is_write = false;
        private int _count_max = 3;
        private int _count_now = 0;

        private bool _create_errorcode = false;

        private BackgroundWorker _bgworker = new BackgroundWorker();
        private DotNet.Comm.Protocols.TimeoutChecker _timeout = new DotNet.Comm.Protocols.TimeoutChecker();

        internal PortType PortType
        {
            get => this._port_type;
            set
            {
                this._port_type = value;

                switch (this._port_type)
                {
                    case PortType.Serial: this._port = new QYSerialPort(); break;
                    case PortType.Socket: this._port = new QYSocketPort(); break;
                    default: this._port = null; break;
                }
            }
        }
        internal ITransport Transport => this._port;
        internal ProtocolType ProtocolType
        {
            get => this._protocol_type;
            set
            {
                this._protocol_type = value;

                if(this._protocol is IDisposable dispose)
                    dispose.Dispose();

                switch (this._protocol_type)
                {
                    case ProtocolType.ModbusRTU: this._protocol = new ModbusRTU(this); break;
                    case ProtocolType.ModbusAscii: this._protocol = new ModbusAscii(this); break;
                    case ProtocolType.HY_ModbusRTU_EXP: this._protocol = new ModbusRTU_HYExpand(this); break;
                    case ProtocolType.HY_ModbusAscii_EXP: this._protocol = new ModbusAscii_HYExpand(this); break;
                    case ProtocolType.HY_ModbusTCP: this._protocol = new ModbusTCP(this); break;
                    case ProtocolType.HY_PCLinkSTD: this._protocol = new PCLinkSTD(this); break;
                    case ProtocolType.HY_PCLinkSTD_TH300500: this._protocol = new PCLinkSTD_TH300500(this); break;
                    case ProtocolType.HY_PCLinkSUM: this._protocol = new PCLinkSUM(this); break;
                    case ProtocolType.HY_PCLinkSUM_TD300500: this._protocol = new PCLinkSUM_TD300500(this); break;
                    case ProtocolType.HY_PCLinkSUM_TH300500: this._protocol = new PCLinkSUM_TH300500(this); break;
                    default: this._protocol = null; break;
                }
            }
        }
        internal int MaxCount { get => this._count_max; set => this._count_max = value; }
        internal bool CreateErrorCode { get => this._create_errorcode; set => this._create_errorcode = value; }
        internal bool IsSending => this._is_user_send;
        
        internal CommPort()
        {
            this._bgworker.WorkerSupportsCancellation = true;
            this._bgworker.DoWork += _bgworker_DoWork;
        }

        internal void Send(string text)
        {
            //Text → Binary 변환
            byte[] bin = this.TextToBinary(text);
            if (bin == null) return;

            //Checksum 추가
            if (this.CreateErrorCode)
            {
                byte[] checksum = this._protocol.CreateCheckSum(bin);

                if (checksum != null)
                {
                    byte[] temp = new byte[bin.Length + checksum.Length];
                    Buffer.BlockCopy(bin, 0, temp, 0, bin.Length);
                    Buffer.BlockCopy(checksum, 0, temp, bin.Length, checksum.Length);

                    bin = temp;
                }
            }

            //전송 등록
            this._write_binary = bin;

            //통신 초기 설정
            this._is_write = false;
            this._count_now = 0;
            this._is_user_send = true;


            //통신 시작
            if (this._bgworker.IsBusy == false)
                this._bgworker.RunWorkerAsync();
        }

        internal void Stop()
        {
            if(this._bgworker.IsBusy)
                this._bgworker.CancelAsync();
            this._is_user_send = false;
        }

        private byte[] TextToBinary(string text)
        {
            int handle = 0;
            List<byte> bytes = new List<byte>();

            while (handle < text.Length)
            {
                char c = text[handle];
                int len;

                //1. Dec, Hex 구분문자 탐색
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

        private void _bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;

            while (true)
            {
                if (bgWorker.CancellationPending) break;
                if (this._port == null) break;

                if (this._port.IsOpen == false)
                {
                    this._port.Initialize();
                    this._port.Open();

                    this._protocol?.Initialize();

                    this._is_write = false;

                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                if (this._is_write)
                {
                    //수신 Process
                    DotNet.Comm.Protocols.TimeoutType timeout_type = this._timeout.CheckTimeout();
                    if (timeout_type != DotNet.Comm.Protocols.TimeoutType.NONE)
                    {
                        this._is_write = false;

                        continue;
                    }

                    byte[] read = this._port.Read();

                    if(this._protocol == null)
                    {
                        //Protocol 미설정 시 읽은 Binary만 전송
                        if(read != null)
                            this.NoneProtocolReceive?.Invoke(read);
                    }
                    else
                    {
                        //Binary → Protocol Frame 추출
                        byte[] frame = this._protocol.Parse(read);

                        if(frame != null)
                        {
                            IProtocolResult result = this._protocol.Extraction(frame);
                            this.OnResult?.Invoke(result);

                            this._is_write = false;
                        }
                    }
                }
                else
                {
                    //송신 Process
                    if(this._count_max <= this._count_now) break;
                    else
                    {
                        this._is_write = true;
                        this._count_now++;
                        this._timeout.OnSend();
                        this._protocol?.Initialize();

                        this._port.Write(this._write_binary);
                        this.OnSendBinary?.Invoke(this._write_binary);
                    }
                }
            }//End While

            this._write_binary = null;
            this._is_write = false;
        }
    }
}
