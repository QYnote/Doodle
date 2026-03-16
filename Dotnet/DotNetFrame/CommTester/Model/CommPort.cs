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

namespace DotNetFrame.CommTester.Model
{
    internal class CommPort
    {
        internal event Action<CommResult> Log;
        private void OnLog(CommResult rst) => this.Log?.Invoke(rst);

        private PortType _port_type = PortType.Socket;
        private ITransport _port;
        private ProtocolType _protocol_type = ProtocolType.None;
        private IProtocol _protocol;

        private byte[] _write_binary = null;
        private bool _is_write = false;
        private int _count_max = 3;
        private int _count_now = 0;
        private byte[] _buffer = null;
        private DateTime _send_time = DateTime.MinValue;
        private bool _is_read = false;
        private DateTime _read_time = DateTime.MinValue;

        private bool _create_errorcode = false;

        private BackgroundWorker _bgworker = new BackgroundWorker();

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
                switch (this._protocol_type)
                {
                    case ProtocolType.ModbusRTU: this._protocol = new ModbusRTU(); break;
                    case ProtocolType.ModbusAscii: this._protocol = new ModbusAscii(); break;
                    case ProtocolType.HY_ModbusRTU_EXP: this._protocol = new ModbusRTU_HYExpand(); break;
                    case ProtocolType.HY_ModbusAscii_EXP: this._protocol = new ModbusAscii_HYExpand(); break;
                    case ProtocolType.HY_ModbusTCP: this._protocol = new ModbusTCP_HY(); break;
                    case ProtocolType.HY_PCLinkSTD: this._protocol = new PCLinkSTD(); break;
                    case ProtocolType.HY_PCLinkSTD_TH300500: this._protocol = new PCLinkSTD_TH300500(); break;
                    case ProtocolType.HY_PCLinkSUM: this._protocol = new PCLinkSUM(); break;
                    case ProtocolType.HY_PCLinkSUM_TD300500: this._protocol = new PCLinkSUM_TD300500(); break;
                    case ProtocolType.HY_PCLinkSUM_TH300500: this._protocol = new PCLinkSUM_TH300500(); break;
                    default: this._protocol = null; break;
                }
            }
        }
        internal int MaxCount { get => this._count_max; set => this._count_max = value; }
        internal bool CreateErrorCode { get => this._create_errorcode; set => this._create_errorcode = value; }
        internal bool IsSending => this._is_write || this._is_read;
        
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


            //통신 시작
            if (this._bgworker.IsBusy == false)
                this._bgworker.RunWorkerAsync();
        }

        internal void Stop()
        {
            if(this._bgworker.IsBusy)
                this._bgworker.CancelAsync();
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

                    this._is_write = false;

                    System.Threading.Thread.Sleep(1000);
                    continue;
                }

                if (this._is_write)
                {
                    //수신 Process
                    if (this.IsTimeout())
                    {
                        this._is_write = false;
                        this._port.Initialize();

                        continue;
                    }

                    byte[] read = this._port.Read();
                    if(read != null)
                    {
                        //Read Log 전송
                        this.OnLog(new CommResult(ResultType.Read, this._write_binary, read));

                        //Read 누적 처리
                        if (this._buffer == null)
                        {
                            this._buffer = new byte[read.Length];
                            Buffer.BlockCopy(read, 0, this._buffer, 0, read.Length);
                        }
                        else
                        {
                            byte[] temp = new byte[this._buffer.Length + read.Length];
                            Buffer.BlockCopy(this._buffer, 0, temp, 0 , this._buffer.Length);
                            Buffer.BlockCopy(read, 0, temp, this._buffer.Length, read.Length);

                            this._buffer = temp;
                        }

                        this._is_read = true;
                        this._read_time = DateTime.Now;
                    }

                    if(this._protocol != null)
                    {
                        //Binary → Protocol Type 변환

                        //Protocol Frame 추출
                        byte[] frame = this._protocol.Parse(this._buffer, this._write_binary);

                        if(frame != null)
                        {
                            if (this._protocol.CheckError(frame))
                            {
                                //CheckSum Error Log
                                this.OnLog(new CommResult(ResultType.Protocol_Error_CheckSum, this._write_binary, frame));
                            }
                            else
                            {
                                DotNet.Comm.Protocols.IProtocolResult result = this._protocol.Extraction(frame, this._write_binary);

                                if(result.Type == DotNet.Comm.Protocols.ResultType.Success)
                                {
                                    //일반 성공 Log
                                    this.OnLog(new CommResult(ResultType.Protocol_Success, this._write_binary, frame));
                                }
                                else if (result.Type == DotNet.Comm.Protocols.ResultType.Protocol_Exception)
                                {
                                    //Protocol Error Log
                                    this.OnLog(new CommResult(ResultType.Protocol_Error_Exception, this._write_binary, frame));
                                }
                            }

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
                        this._is_read = false;
                        this._buffer = null;
                        this._count_now++;
                        this._send_time = DateTime.Now;
                        this._read_time = DateTime.Now;

                        this._port.Write(this._write_binary);
                        //Write Log 전송
                        this.OnLog(new CommResult(ResultType.Protocol_Error_Exception, this._write_binary, null));
                    }
                }
            }//End While

            this._write_binary = null;
            this._buffer = null;
            this._is_read = false;
            this._is_write = false;
        }

        private bool IsTimeout()
        {
            TimeSpan ts;
            if (this._is_read)
            {
                //Response Stop
                ts = DateTime.Now - this._read_time;
                if(ts.TotalMilliseconds > 5000)
                {
                    //Response Stop Timeout 처리
                    this.OnLog(new CommResult(ResultType.Timeout_Stop, this._write_binary, this._buffer));

                    return true;
                }

                ts = DateTime.Now - this._send_time;
                if(ts.TotalMilliseconds > 10000)
                {
                    //Response Long Timeout
                    this.OnLog(new CommResult(ResultType.Timeout_Long, this._write_binary, this._buffer));

                    return true;
                }
            }
            else
            {
                //None Receive Timeout
                ts = DateTime.Now - this._send_time;
                if(ts.TotalMilliseconds > 3000)
                {
                    //None Receive Timeout 처리
                    this.OnLog(new CommResult(ResultType.Timeout_None, this._write_binary, this._buffer));

                    return true;
                }
            }

            return false;
        }
    }

    internal enum ResultType
    {
        Write,
        Read,
        Timeout_Stop,
        Timeout_Long,
        Timeout_None,
        Protocol_Success,
        Protocol_Error_CheckSum,
        Protocol_Error_Exception,
    }

    internal class CommResult
    {
        internal ResultType Type { get; }
        internal byte[] Req { get; }
        internal byte[] Rcv { get; }
        internal DateTime Time { get; }

        internal CommResult(ResultType type, byte[] req, byte[] rcv)
        {
            this.Type = type;
            this.Req = req;
            this.Rcv = rcv;
            this.Time = DateTime.Now;
        }
    }
}
