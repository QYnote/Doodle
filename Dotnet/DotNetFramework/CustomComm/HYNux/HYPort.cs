using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.Comm.Structures.ClientPorts;
using DotNet.Comm.Structures.Protocols;
using DotNet.Utils.Controls;

namespace DotNetFrame.CustomComm.HYNux
{
    internal enum ProtocolType
    {
        None,
        HY_ModbusRTU,
        HY_ModbusAscii,
        HY_ModbusRTU_EXP,
        HY_ModbusAscii_EXP,
        HY_ModbusTCP,
        PCLink_STD,
        PCLink_SUM,
        PCLink_STD_TH300500,
        PCLink_SUM_TD300500,
        PCLink_SUM_TH300500,
    }
    /// <summary>
    /// HY Port
    /// </summary>
    /// <remarks>
    /// 1Port - (1 Request ↔ 1Response) 처리방식
    /// </remarks>
    public class HYPort
    {
        public delegate void ReadWriteHandler(string title, byte[] data);
        public event ReadWriteHandler CommLog;
        public event ReadWriteHandler StackBuff;


        private PCPortBase _port;
        /// <summary>
        /// User가 Port Open 여부
        /// </summary>
        private bool _isUserOpen = false;
        private BackgroundWorker _bgWorker = new BackgroundWorker();
        /// <summary>
        /// 최근 통신 Data Set
        /// </summary>
        private CommData _commData = new CommData();
        /// <summary>
        /// 현재 Buffer 길이
        /// </summary>
        private int _curBufferLen = 0;
        /// <summary>
        /// 최근 검사 Buffer 길이
        /// </summary>
        private int _recentBufferLen = 0;
        /// <summary>
        /// 마지막 Read 시간
        /// </summary>
        private DateTime _recentReadTime;
        /// <summary>
        /// Port에 누적된 Buffer
        /// </summary>
        private byte[] _stackBuffer;
        /// <summary>
        /// Port 종류
        /// </summary>
        private PortType _type;
        /// <summary>
        /// Protocol 종류
        /// </summary>
        private ProtocolType _protocolType = ProtocolType.None;

        internal bool IsOpen
        {
            get
            {
                if (this._port == null)
                    return false;
                else
                    return this._port.IsOpen;
            }
        }
        /// <summary>
        /// Port 종류
        /// </summary>
        internal PortType Type
        {
            get { return this._type; }
            set
            {
                this._type = value;
                if (value == PortType.Serial)
                {
                    this._port = new QYSerialPort();
                }
                else if (value == PortType.Ethernet)
                {
                    this._port = new QYEthernet(false);
                }
            }
        }
        /// <summary>
        /// 실 통신 Port
        /// </summary>
        internal PCPortBase PCPort { get { return this._port; } }
        /// <summary>
        /// 데이터가 들어오지 않는 Timeout 설정 시간
        /// </summary>
        internal int NoneReceiveTimeoutTime { get; set; } = 3000;
        /// <summary>
        /// 데이터가 들어왔는 온 Timeout 설정 시간
        /// </summary>
        internal int ReceiveTimeoutTime { get; set; } = 5000;
        /// <summary>
        /// Protocol 종류
        /// </summary>
        internal ProtocolType ProtocolType
        {
            get { return this._protocolType; }
            set
            {
                this._protocolType = value;
                switch (value)
                {
                    case ProtocolType.None:
                        this.Protocol = null;
                        this.ErrorCheck = null;
                        break;
                    case ProtocolType.HY_ModbusRTU:
                        this.Protocol = new HYModbus(true);
                        this.ErrorCheck = new ModbusRTUErrorCheck();
                        break;
                    case ProtocolType.HY_ModbusAscii:
                        this.Protocol = new HYModbus(true) { IsAscii = true };
                        this.ErrorCheck = new ModbusAsciiErrorCheck();
                        break;
                    case ProtocolType.HY_ModbusRTU_EXP:
                        this.Protocol = new HYModbus(true) { IsEXP = true };
                        this.ErrorCheck = new ModbusRTUErrorCheck();
                        break;
                    case ProtocolType.HY_ModbusAscii_EXP:
                        this.Protocol = new HYModbus(true) { IsAscii = true, IsEXP = true };
                        this.ErrorCheck = new ModbusAsciiErrorCheck();
                        break;
                    case ProtocolType.HY_ModbusTCP:
                        this.Protocol = new HYModbus(true) { IsTCP = true };
                        this.ErrorCheck = new ModbusRTUErrorCheck();
                        break;
                    case ProtocolType.PCLink_STD:
                        this.Protocol = new PCLink(true);
                        this.ErrorCheck = null;
                        break;
                    case ProtocolType.PCLink_SUM:
                        this.Protocol = new PCLink(true) { _isSUM = true };
                        this.ErrorCheck = new PCLinkErrorCheck();
                        break;
                    case ProtocolType.PCLink_STD_TH300500:
                        this.Protocol = new PCLink(true) { _isTH3500 = true };
                        this.ErrorCheck = null;
                        break;
                    case ProtocolType.PCLink_SUM_TD300500:
                        this.Protocol = new PCLink(true) { _isSUM = true, _isTD3500 = true };
                        this.ErrorCheck = new PCLinkErrorCheck();
                        break;
                    case ProtocolType.PCLink_SUM_TH300500:
                        this.Protocol = new PCLink(true) { _isSUM = true, _isTH3500 = true };
                        this.ErrorCheck = new PCLinkTHErrorCheck();
                        break;
                    default:
                        this.Protocol = null;
                        this.ErrorCheck = null;
                        break;
                }
            }
        }
        /// <summary>
        /// Protocol 처리 Class
        /// </summary>
        private ProtocolFrame Protocol { get; set; }
        /// <summary>
        /// Error Code 처리 Class
        /// </summary>
        internal ErrorCheck ErrorCheck { get; private set; }
        /// <summary>
        /// Request 진행중 여부
        /// </summary>
        /// <remarks>
        /// Request 후 Receive 중인지에대한 여부
        /// </remarks>
        internal bool IsRequesting { get; set; } = false;
        /// <summary>
        /// Error Code 생성 여부
        /// </summary>
        internal bool CreErrCheck { get; set; } = false;

        public HYPort(PortType type)
        {
            this.Type = type;

            this._bgWorker.WorkerSupportsCancellation = true;
        }

        public bool Open()
        {
            if (this._isUserOpen == false)
            {
                if (this._port != null
                    && this._port.IsOpen == false
                    && this._port.Open())
                {
                    this._isUserOpen = true;
                    return true;
                }
            }

            return false;
        }
        public bool Close()
        {
            if (this._isUserOpen == true)
            {
                if(this._port != null
                    && this._port.IsOpen == true
                    && this._port.Close()
                    )
                {
                    this._isUserOpen = false;
                    return true;
                }
            }

            return false;
        }
        public void Read()
        {
            if (this._isUserOpen == false
                || this._port == null
                || this._port.IsOpen == false
                ) return;

            //this.BeforeRead?.Invoke("BeforeRead", this._commData);

            if (IsTimeout())
            {
                this._recentBufferLen = 0;
                this._curBufferLen = 0;
                this._stackBuffer = null;

                this._commData = null;
                this.IsRequesting = false;

                return;
            }

            byte[] readBytes = this._port.Read();

            if (readBytes != null)
            {
                if (this._stackBuffer != null)
                    this._stackBuffer.BytesAppend(readBytes);
                else
                    this._stackBuffer = readBytes;

                this._recentReadTime = DateTime.Now;
                this._curBufferLen = this._stackBuffer.Length;

                if (this.Protocol == null)
                {
                    this.CommLog?.Invoke("Receive Success", this._stackBuffer);

                    this._recentBufferLen = 0;
                    this._curBufferLen = 0;
                    this.IsRequesting = false;
                    this._stackBuffer = null;
                }
                else
                {
                    //Protocol에 따라 추출된 Data Array
                    byte[] frameBytes = this.Protocol.DataExtract_Receive(this._commData.ReqData, this._stackBuffer);

                    if (frameBytes != null)
                    {
                        //추출된 Data 처리
                        this._commData.RcvData = frameBytes;
                        Dictionary<int, object> dataList = new Dictionary<int, object>();
                        if (this.Protocol != null)
                            this.Protocol.GetData(dataList, this._commData.ReqData, this._commData.RcvData);

                        //Protocol 검사
                        byte errorCode = GetErrorCode(this._commData.RcvData);

                        switch (errorCode)
                        {
                            case 0xFF: this.CommLog?.Invoke("Receive Success", this._commData.RcvData); break;
                            case 0x00: this.CommLog?.Invoke("ErrorCheck Dismatch", this._commData.RcvData); break;
                            case 0x10: this.CommLog?.Invoke("Protocol NG", this._commData.RcvData); break;
                        }

                        this._recentBufferLen = 0;
                        this._curBufferLen = 0;
                        this.IsRequesting = false;

                        //남은 Data Array 앞으로 당기기
                        int startIdx = this._stackBuffer.Find(this._commData.RcvData);
                        if (startIdx < 0) return;

                        int lastIdx = startIdx + this._commData.RcvData.Length - 1;

                        if (lastIdx < this._stackBuffer.Length - 1)
                        {
                            byte[] remainByte = new byte[this._stackBuffer.Length - lastIdx - 1];
                            Buffer.BlockCopy(this._stackBuffer, lastIdx + 1, remainByte, 0, remainByte.Length);
                            this._stackBuffer = new byte[remainByte.Length];
                            Buffer.BlockCopy(remainByte, 0, this._stackBuffer, 0, this._stackBuffer.Length);
                        }
                        else if (lastIdx == this._stackBuffer.Length - 1)
                        {
                            this._stackBuffer = null;
                        }
                    }
                }//Protocol 처리 End
            }

            this.StackBuff?.Invoke("Stack Buff", this._stackBuffer);

            return;
        }
        private bool IsTimeout()
        {
            if (this._recentBufferLen <= 0)
            {
                //None Receive Error
                TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - this._commData.SendingTime.Ticks);

                if (ts.TotalMilliseconds > this.NoneReceiveTimeoutTime)
                {
                    this._commData.RcvData = this._stackBuffer;

                    this.CommLog?.Invoke("None Receive", this._commData.RcvData);

                    return true;
                }
            }
            else
            {
                TimeSpan ts = DateTime.Now - this._recentReadTime;

                if (ts.TotalMilliseconds > this.ReceiveTimeoutTime)
                {
                    this._commData.RcvData = this._stackBuffer;

                    if (this._recentBufferLen == this._curBufferLen)
                    {
                        //Receive Stop Error
                        this.CommLog?.Invoke("Receive Stop", this._commData.RcvData);
                    }
                    else
                    {
                        //Receive Too Long Error
                        this.CommLog?.Invoke("Receive Too Long", this._commData.RcvData);
                    }

                    return true;
                }
            }

            this._recentBufferLen = this._curBufferLen;

            return false;
        }
        private byte GetErrorCode(byte[] rcvData)
        {
            byte errorCode = 0xFF;

            if (this.ErrorCheck != null)
            {
                if (this.ErrorCheck.FrameConfirm(rcvData))
                    errorCode = 0x00;
            }

            if (this.Protocol != null)
            {
                if(this.Protocol.ReceiveConfirm(rcvData) == false)
                {
                    //Protocol NG
                    errorCode = 0x10;
                }
            }

            return errorCode;
        }
        public void Write(byte[] data)
        {
            if (this.IsRequesting || data == null) return;

            if (this.CreErrCheck && this.ErrorCheck != null)
            {
                byte[] errCd = this.ErrorCheck.CreateCheckBytes(data);

                if (this.ProtocolType == ProtocolType.HY_ModbusRTU
                    || this.ProtocolType == ProtocolType.HY_ModbusRTU_EXP)
                    data = data.BytesAppend(errCd);
                else if (
                    this.ProtocolType == ProtocolType.HY_ModbusAscii
                    || this.ProtocolType == ProtocolType.HY_ModbusAscii_EXP
                    || this.ProtocolType == ProtocolType.PCLink_SUM
                    || this.ProtocolType == ProtocolType.PCLink_SUM_TD300500
                    )
                {
                    if (data[data.Length - 2] == 0x0D
                        && data[data.Length - 1] == 0x0A)
                    {
                        //CRLF를 작성한경우
                        byte[] bTemp = new byte[data.Length - 2];
                        Buffer.BlockCopy(data, 0, bTemp, 0, bTemp.Length);

                        bTemp = bTemp.BytesAppend(errCd);
                        bTemp = bTemp.BytesAppend(new byte[] { 0x0D, 0x0A });

                        data = bTemp;
                    }
                    else
                    {
                        //CRLF를 미작성한경우
                        data = data.BytesAppend(errCd);
                        data = data.BytesAppend(new byte[] { 0x0D, 0x0A });
                    }
                }
                else if (this.ProtocolType == ProtocolType.PCLink_SUM_TH300500)
                {
                    if (data[data.Length - 3] == 0x03
                        && data[data.Length - 2] == 0x0D
                        && data[data.Length - 1] == 0x0A)
                    {
                        //ETX + CRLF를 작성한경우
                        byte[] bTemp = new byte[data.Length - 3];
                        Buffer.BlockCopy(data, 0, bTemp, 0, bTemp.Length);

                        bTemp = bTemp.BytesAppend(errCd);
                        bTemp = bTemp.BytesAppend(new byte[] { 0x03, 0x0D, 0x0A });

                        data = bTemp;
                    }
                    else
                    {
                        //ETX + CRLF를 미작성한경우
                        data = data.BytesAppend(errCd);
                        data = data.BytesAppend(new byte[] { 0x03, 0x0D, 0x0A });
                    }
                }
            }
            if(this.ProtocolType == ProtocolType.HY_ModbusTCP)
            {
                byte[] bLength = new byte[2];
                bLength[0] = (byte)((data.Length >> 8) & 0xFF);
                bLength[1] = (byte)( data.Length       & 0xFF);
                bLength = QYUtils.BytesAppend(new byte[] { 0x00, 0x00, 0x00, 0x00 }, bLength);
                data = QYUtils.BytesAppend(bLength, data);
            }

            this._commData = new CommData();
            this._commData.ReqData = data;
            this._commData.SendingTime = DateTime.Now;

            this._port.Write(this._commData.ReqData);

            this._recentBufferLen = 0;
            this._curBufferLen = 0;
            this._stackBuffer = null;

            this.IsRequesting = true;
            this.CommLog?.Invoke("After Write", this._commData.ReqData);
        }
    }
}
