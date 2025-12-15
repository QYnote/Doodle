using DotNet.Comm;
using DotNet.Comm.ClientPorts.AppPort;
using DotNet.Comm.ClientPorts.OSPort;
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

    internal class M_CommTester
    {
        /// <summary>
        /// Request 전송 후 이벤트
        /// </summary>
        /// <remarks>
        /// Param[0] = RequestByte
        /// </remarks>
        public event Update_WithParam AfterSendRequest;
        /// <summary>
        /// 데이터 읽은 후 Port에 누적된 Buffer
        /// </summary>
        /// <remarks>
        /// Param[0]: Port의 현재 누적 Buffer
        /// </remarks>
        public event Update_WithParam PortCurrentBuffer;
        /// <summary>
        /// Request 에러코드 미일치
        /// </summary>
        /// <remarks>
        /// Param[0]: Response Data
        /// </remarks>
        public event Update_WithParam Error_ErrorCode;
        /// <summary>
        /// Request Protocol 에러
        /// </summary>
        /// <remarks>
        /// Param[0]: Response Data
        /// </remarks>
        public event Update_WithParam Error_Protocol;
        /// <summary>
        /// Request 정상종료
        /// </summary>
        /// <remarks>
        /// Param[0]: Request Data<br/>
        /// Param[1]: Response Data
        /// </remarks>
        public event Update_WithParam RequestComplete;
        /// <summary>
        /// Request Timeout
        /// </summary>
        /// <remarks>
        /// Param[0]:<br/>
        /// None Response: Request 응답없음<br/>
        /// Long Response: Receive Data가 끊임없이 들어올 경우<br/>
        /// Stop Response: Receive 중단됨<br/>
        /// <br/>
        /// Param[1]: Port의 누적된 Buffer
        /// </remarks>
        public event Update_WithParam RequestTimeout;
        /// <summary>
        /// OS Port Log
        /// </summary>
        /// <remarks>
        /// Param[0]: Log Text
        /// </remarks>
        public event Update_WithParam OSPortLog;

        /// <summary>Application ↔ OS Port</summary>
        private AppPort _appPort = new AppPort();
        /// <summary>Protocol 종류</summary>
        private ProtocolType _protocolType = ProtocolType.None;
        /// <summary>Protocol</summary>
        private ProtocolBase _protocol = null;
        /// <summary>Port 동작기</summary>
        private BackgroundWorker _bgWorker = new BackgroundWorker();

        /// <summary>에러코드 추가 여부</summary>
        private bool _errCode_add = false;
        /// <summary>반복전송 여부</summary>
        private bool _do_repeat = false;
        /// <summary>반복전송 수</summary>
        private int _do_repeat_count = 3;
        /// <summary>반복전송 - 무한전송 여부</summary>
        private bool _do_repeat_infinity = false;

        /// <summary>전송 Queue</summary>
        private Queue<CommFrame> _write_queue = new Queue<CommFrame>();
        /// <summary>현재 전송중인 Frame</summary>
        private CommFrame _write_frame_current = null;
        /// <summary>현재 Buffer</summary>
        private byte[] _read_buffer = null;
        /// <summary>최근 Buffer 길이</summary>
        /// <remarks>Timeout에서 검사한 최근 Buffer 길이</remarks>
        private int _read_buffer_last_length = 0;
        /// <summary>최근 Buffer 읽은 시간</summary>
        private DateTime _read_buffer_last_time = DateTime.MinValue;

        /// <summary>OS Port 종류</summary>
        internal CommType PortType { get => this._appPort.CommType; set => this._appPort.CommType = value; }
        /// <summary>OS Port</summary>
        internal OSPortBase OSPort { get => this._appPort.OSPort; }
        /// <summary>Protocol 종류</summary>
        internal ProtocolType ProtocolType
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
                }
            }
        }
        internal bool IsOpen { get => this._appPort.IsUserOpen; }
        /// <summary>에러코드 추가 여부</summary>
        internal bool ErrCode_add { get => this._errCode_add; set => this._errCode_add = value; }
        /// <summary>반복전송 여부</summary>
        internal bool Do_repeat { get => this._do_repeat; set => this._do_repeat = value; }
        /// <summary>반복전송 수</summary>
        internal int Do_repeat_count
        {
            get => this._do_repeat_count;
            set
            {
                if (this._do_repeat)
                    this._do_repeat_count = value;
            }
        }
        /// <summary>반복전송 - 무한전송 여부</summary>
        internal bool Do_repeat_infinity
        {
            get => this._do_repeat_infinity;
            set
            {
                if (this._do_repeat)
                    this._do_repeat_infinity = value;
            }
        }
        /// <summary>전송등록 여부</summary>
        internal bool IsWriting { get => this._write_queue.Count > 0; }

        internal M_CommTester()
        {
            this._appPort.ComPortLog += (obj) => { this.OSPortLog?.Invoke(obj); };
        }

        /// <summary>
        /// Port 연결
        /// </summary>
        /// <returns>연결 여부</returns>
        internal bool Connect()
        {
            if (this._appPort.IsUserOpen) return false;

            this._appPort.Connect();
            this._bgWorker.RunWorkerAsync();

            return true;
        }
        /// <summary>
        /// Port 연결 해제
        /// </summary>
        internal bool Disconnect()
        {
            if (this._bgWorker.IsBusy)
                this._bgWorker.CancelAsync();

            this._appPort.Disconnect();

            return true;
        }
        /// <summary>Port 동작 Process</summary>
        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this._bgWorker.CancellationPending || this._appPort.IsUserOpen == false) return;

                    if (this._appPort.OSPort.IsOpen == false)
                    {
                        this._appPort.Connect();
                        System.Threading.Thread.Sleep(3000);
                        continue;
                    }

                    if (this._write_frame_current == null)
                    {
                        //비정기 전송
                        if (this._write_queue.Count > 0)
                            this._write_frame_current = this._write_queue.Peek();

                        if (this._write_frame_current != null)
                        {
                            this.Write_Data(this._write_frame_current);
                        }
                    }
                    else
                    {
                        //수신
                        //1. Timeout
                        if (this.IsTimeout())
                        {
                            if (this._write_frame_current.TryCount >= this._write_frame_current.MaxTryCount)
                            {
                                //최대 시도횟수 초과
                                if (this._write_queue.Count != 0 && this._write_frame_current == this._write_queue.Peek())
                                    this._write_queue.Dequeue();

                                this._write_frame_current = null;
                                this._read_buffer = null;

                                this._appPort.OSPort.InitPort();
                                continue;
                            }
                            else
                                this.Write_Data(this._write_frame_current);
                        }

                        //2. 수신처리
                        byte[] readBytes = this._appPort.Read();

                        if (readBytes != null)
                        {
                            if (this._read_buffer != null)
                            {
                                byte[] temp = new byte[this._read_buffer.Length + readBytes.Length];
                                Buffer.BlockCopy(this._read_buffer, 0, temp, 0, this._read_buffer.Length);
                                Buffer.BlockCopy(readBytes, 0, temp, this._read_buffer.Length, readBytes.Length);
                                this._read_buffer = temp;
                            }
                            else
                                this._read_buffer = readBytes;

                            this._read_buffer_last_time = DateTime.Now;
                            this.PortCurrentBuffer?.Invoke(this._read_buffer);

                            //3. Protocol 처리
                            if (this._protocol != null)
                            {
                                byte[] frameBytes = this._protocol.Response_ExtractFrame(this._read_buffer, this._write_frame_current.ReqBytes);

                                if (frameBytes != null)
                                {
                                    this._write_frame_current.ResBytes = frameBytes;

                                    //ErrorCode 확인
                                    bool isErr = false;
                                    if (this._protocol.ConfirmErrCode(this._write_frame_current.ResBytes) == false)
                                    {
                                        isErr = true;
                                        this.Error_ErrorCode?.Invoke(this._write_frame_current.ResBytes);
                                    }
                                    else
                                    {
                                        List<object> readItems = this._protocol.Response_ExtractData(this._write_frame_current.ResBytes, this._write_frame_current.ReqBytes);

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
                                    if (isErr)
                                        this.Error_Protocol?.Invoke(this._write_frame_current.ResBytes);
                                    else
                                        this.RequestComplete?.Invoke(this._write_frame_current.ReqBytes, this._write_frame_current.ResBytes);

                                    if ((this._do_repeat && this._write_frame_current.TryCount >= this._write_frame_current.MaxTryCount)
                                        || this._do_repeat == false)
                                    {
                                        if (this._write_queue.Count != 0 && this._write_frame_current == this._write_queue.Peek())
                                            this._write_queue.Dequeue();

                                    }
                                    this._write_frame_current = null;
                                    this._read_buffer = null;
                                }
                            }//Protocol 처리 End
                        }//read 처리 End
                    }//수신 End
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format(
                        "HYCommTestport.cs - bgWorkerDoWork()\r\n" +
                        "{0}\r\n\r\n" +
                        "{1}",
                        ex.Message, ex.StackTrace));

                    if (this._write_frame_current != null)
                    {
                        if (this._write_queue.Count != 0 && this._write_frame_current == this._write_queue.Peek())
                            this._write_queue.Dequeue();

                        this._write_frame_current = null;
                    }
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
        /// 전송 등록
        /// </summary>
        /// <param name="text">전송할 Text</param>
        /// <returns>등록 결과</returns>
        internal bool Register_Data(string text)
        {
            if (this._write_queue.Count > 0)
            {
                this._write_queue.Clear();

                return true;
            }

            byte[] bytes = this.ConvertTextToByte(text);
            if (bytes == null) return false;

            if (this._errCode_add)
            {
                if (this._protocol == null) return false;

                byte[] errCode = this._protocol.CreateErrCode(bytes);
                bytes = QYUtils.Comm.BytesAppend(bytes, errCode);
            }

            CommFrame frame = new CommFrame(bytes);
            frame.MaxTryCount = this._do_repeat ? (this._do_repeat_infinity ? int.MaxValue : this._do_repeat_count) : 1;

            this._write_queue.Enqueue(frame);

            return true;
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
        /// <summary>
        /// Data 전송
        /// </summary>
        /// <param name="frame">전송할 Data Frame</param>
        private void Write_Data(CommFrame frame)
        {
            frame.TryCount++;
            frame.SendingTime = DateTime.Now;

            this._read_buffer = null;
            this._read_buffer_last_length = 0;

            this._appPort.Write(frame.ReqBytes);
            this.AfterSendRequest?.Invoke(frame);
        }
        /// <summary>
        /// Timeout 여부
        /// </summary>
        /// <returns>Timeout 결과</returns>
        private bool IsTimeout()
        {
            if (this._write_frame_current == null) return false;

            TimeSpan ts;
            if (this._read_buffer_last_length <= 0)
            {
                //None Receive Timeout
                ts = DateTime.Now - this._write_frame_current.SendingTime;

                if (ts.TotalMilliseconds > 3000)
                {
                    //Receive 없음
                    this.RequestTimeout?.Invoke("None Response");
                    return true;
                }
            }
            else
            {
                ts = DateTime.Now - this._write_frame_current.SendingTime;
                //Sending시간 > 10초전 && 계속 StackBuffer가 증가중일 경우
                if (this._read_buffer == null ||
                    (ts.TotalMilliseconds > 10000 && (this._read_buffer != null && (this._read_buffer_last_length != this._read_buffer.Length)))
                    )
                {
                    //Receie가 너무 김
                    this.RequestTimeout?.Invoke("Long Response", this._read_buffer);

                    return true;
                }

                ts = DateTime.Now - this._read_buffer_last_time;
                //최근 Receive 시간 > 5초전
                if (ts.TotalMilliseconds > 5000)
                {
                    //Receive 중단됨
                    this.RequestTimeout?.Invoke("Stop Response", this._read_buffer);

                    return true;
                }
            }

            this._read_buffer_last_length = this._read_buffer == null ? 0 : this._read_buffer.Length;

            return false;
        }
    }
    public class CommFrame
    {
        /// <summary>
        /// 요청(Request) Data
        /// </summary>
        public byte[] ReqBytes { get; }
        /// <summary>
        /// 응답(Response) Data
        /// </summary>
        public byte[] ResBytes { get; set; }
        public int MaxTryCount { get; set; } = 1;
        /// <summary>
        /// 요청 시도 수
        /// </summary>
        public int TryCount { get; set; } = 0;
        /// <summary>
        /// 최근 요청 시간
        /// </summary>
        public DateTime SendingTime { get; set; }

        public CommFrame(byte[] reqFrame)
        {
            this.ReqBytes = reqFrame;
        }
    }
}
