using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class HYModbus : Modbus
    {
        private bool _isTCP = false;
        public bool IsAscii { get; set; } = false;
        public bool IsEXP { get; set; } = false;
        public bool IsTCP
        {
            get => this._isTCP;
            set
            {
                this._isTCP = value;
                if (this._isTCP)
                    base.ErrCodeLength = 0;
                else
                    base.ErrCodeLength = 2;
            }
        }

        public HYModbus(bool isClient) : base(isClient)
        {
            base.ErrCodeLength = 2;
        }

        #region Response

        public override byte[] Response_ExtractFrame(byte[] buffer, params object[] subData)
        {
            if (subData[0] == null) return null;
            byte[] reqBytes = subData[0] as byte[],
                headerBytes;
            byte cmd;
            int idxHandle = 0,
                startIdx = -1,
                headerLen = 2,
                frameLen = -1;

            //Header
            if (this.IsAscii && this.IsTCP == false)
            {
                //Ascii: ':'[1] + Addr[2] + Cmd[2]
                if (buffer.Length < 5) return null;

                headerBytes = new byte[] { reqBytes[0], reqBytes[1], reqBytes[2], reqBytes[3], reqBytes[4], };
                headerLen = headerBytes.Length;
            }
            else if (this.IsAscii == false && this.IsTCP)
            {
                //TCP: Transaction ID[2] + Protocol ID[2] + DataLength[2] + Addr[1] + Cmd[1]
                if (buffer.Length < 8) return null;

                headerBytes = new byte[] { reqBytes[6], reqBytes[7] };
                idxHandle += 6;
            }
            else
            {
                //기본: Addr[1] + Cmd[1]
                if (reqBytes.Length < 2) return null;

                headerBytes = new byte[] { reqBytes[0], reqBytes[1] };
            }

            //Frame 검색
            while (idxHandle < buffer.Length - 1)
            {
                startIdx = Utils.Controls.Utils.QYUtils.Find(buffer, headerBytes, idxHandle);
                if (startIdx < 0)
                {
                    //Error Cmd가 날라온건지 검사
                    if (this.IsAscii)
                    {
                        cmd = (byte)(Convert.ToByte(Encoding.ASCII.GetString(new byte[] { headerBytes[3], headerBytes[4] }), 16) + 0x80);
                        byte[] errCmd = Encoding.ASCII.GetBytes(cmd.ToString("X2"));

                        startIdx = Utils.Controls.Utils.QYUtils.Find(buffer, new byte[] { headerBytes[0], headerBytes[1], headerBytes[2], errCmd[0], errCmd[1], });
                    }
                    else
                        startIdx = Utils.Controls.Utils.QYUtils.Find(buffer, new byte[] { headerBytes[0], (byte)(headerBytes[1] + 0x80) });
                }

                idxHandle++;
                if (startIdx < 0) continue;

                //FuncCode
                if (this.IsAscii)
                    //Ascii: ':'[1] + Addr[2] + Cmd[2]
                    cmd = Convert.ToByte(Encoding.ASCII.GetString(new byte[] { buffer[startIdx + 3], buffer[startIdx + 4] }), 16);
                else
                    //기본: Addr[1] + Cmd[1]
                    cmd = buffer[startIdx + 1];

                //기능코드별 Frame 길이
                frameLen = -1;
                switch (cmd)
                {
                    case 0x01:
                    case 0x02:
                    case 0x03:
                    case 0x04:
                        {
                            //기본: Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
                            //EXP: ByteCount[2]
                            int byteCountLen = this.IsEXP ? 2 : 1,
                                dataLen = 0;
                            byteCountLen = (this.IsAscii ? byteCountLen * 2 : byteCountLen);

                            if (startIdx + headerLen + byteCountLen < buffer.Length)

                                if (this.IsAscii)
                                {
                                    //Ascii → Rtu Bytes
                                    byte[] dataLenBytes = new byte[byteCountLen / 2];

                                    for (int i = 0; i < dataLenBytes.Length; i++)
                                    {
                                        dataLenBytes[i] = Convert.ToByte(
                                            Encoding.ASCII.GetString(
                                                new byte[] {
                                                buffer[startIdx + headerLen + (i * 2)],
                                                buffer[startIdx + headerLen + (i * 2) + 1]
                                                }
                                            ),
                                            16
                                        );
                                    }

                                    //Data 길이 추출
                                    for (int i = 0; i < dataLenBytes.Length; i++)
                                        dataLen += dataLenBytes[i] << (8 * ((dataLenBytes.Length - 1) - i));

                                    //RTU → Ascii Length변환
                                    dataLen *= 2;
                                }
                                else
                                {
                                    //기본
                                    for (int i = 0; i < byteCountLen; i++)
                                        dataLen += (buffer[startIdx + headerLen + i]) << (8 * (byteCountLen - 1 - i));
                                }

                            //':' + Protocol Len
                            frameLen = headerLen + byteCountLen + dataLen;
                        }
                        break;
                    case 0x06:
                    case 0x10:
                        {
                            //0x06 - 기본: (Addr[1] + Cmd[1]) + StartReg[2] + RegValue[2]
                            //0x10 - 기본: (Addr[1] + Cmd[1]) + StartReg[2] + ReadRegCount[2]
                            int bodyLen = 2 + 2;

                            if (this.IsAscii)
                                bodyLen *= 2;

                            frameLen = headerLen + bodyLen;
                        }
                        break;
                    case 0x14:
                        // Address(1) + Cmd(1 : 0x14) + 전문 Length(1) + 응답Seg count(1) + refType (0x06 고정) + Data(~) 이다.
                        frameLen = 7;

                        if (this.IsAscii) frameLen *= 2;
                        break;
                    case 0x15:
                        // Address(1) + Cmd(1 : 0x15) + 전문 Length(1) + refType (0x06 고정) + SegAddress(2) + SegCount(2) + Data(~) 이다.
                        frameLen = 8;

                        if (this.IsAscii) frameLen *= 2;
                        break;
                    case 0x2B:
                        //TD300, 500 - (0x2B)Read Device Indetification	
                        frameLen = 68;
                        break;
                    case 0x45:
                    case 0x6A:
                        //NX Initialize

                        //TD300, 500 - Holding Register Random Write
                        //Address(1) + Cmd(1) + Data(2) + CRC(2)
                        frameLen = 6;
                        break;
                    case 0x67:
                        //TD300, 500 - Holding Register Random Read
                        //일부 통신 시 ByteCount가 3byte로 작성되서 오기 때문에
                        //nCmd3, 4로 통합하지않고 따로 End Index 구해서 추출
                        frameLen = -1;
                        break;
                    default:
                        if (cmd >= 0x80)
                        {
                            //Address(1) + Cmd(1) + Code(1)
                            int bodyLen = 1;
                            if (this.IsAscii) bodyLen *= 2;

                            frameLen = headerLen + bodyLen;
                            if (buffer.Length < frameLen) continue;

                            System.Diagnostics.Debug.WriteLine(
                                string.Format("({0}) Protocol Error - Func: {1:X2} / ErrCode: {2:X2}",
                                    this.GetType().Name,
                                    cmd - 0x80,
                                    this.IsAscii ?
                                        Convert.ToByte(Encoding.ASCII.GetString(new byte[] { buffer[startIdx + 3], buffer[startIdx + 4] }), 16):
                                        buffer[startIdx + 2]
                                )
                            );
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(
                                string.Format("({0}) Protocol Error - UnSupport Command: {1:X2}", this.GetType().Name, cmd)
                            );
                        }
                        break;
                }

                //Error Code
                frameLen += base.ErrCodeLength;

                //[CR][LF]
                if (this.IsAscii) frameLen += 2;

                if (buffer.Length < startIdx + frameLen) continue;

                if (frameLen > 0)
                {
                    //TCP: Transaction ID[2] + Protocol ID[2] + DataLength[2]
                    //Data 추출
                    byte[] frameBytes = new byte[this.IsTCP ? frameLen + 6 : frameLen];
                    Buffer.BlockCopy(buffer,
                        this.IsTCP ? startIdx - 6 : startIdx,
                        frameBytes,
                        0,
                        frameBytes.Length);

                    return frameBytes;
                }
            }

            return null;
        }
        public override List<object> Response_ExtractData(byte[] frame, params object[] subData)
        {
            if (subData[0] == null) return null;
            byte[] reqBytes = subData[0] as byte[],
                   rcvBytes = null;

            //기본 Modbus 형태로 변환
            if (this.IsAscii)
            {
                //':' + Frame + [ErrorCode] + [CRLF]
                //1. Frame 추출
                //수신 Frame
                byte[] rcvTemp = new byte[frame.Length - 5];
                Buffer.BlockCopy(frame, 1, rcvTemp, 0, rcvTemp.Length);
                //송신 Frame
                byte[] reqTemp = new byte[reqBytes.Length - 5];
                Buffer.BlockCopy(reqBytes, 1, reqTemp, 0, reqTemp.Length);

                //2. 기본 Modbus 형태로 변환
                //수신 Frame
                string rcvStr = Encoding.ASCII.GetString(rcvTemp);
                rcvBytes = new byte[rcvTemp.Length / 2];
                for (int i = 0; i < rcvTemp.Length; i += 2)
                    rcvBytes[i / 2] = Convert.ToByte(rcvStr.Substring(i, 2), 16);
                //송신 Frame
                string reqStr = Encoding.ASCII.GetString(reqTemp);
                reqBytes = new byte[reqTemp.Length / 2];
                for (int i = 0; i < reqTemp.Length; i += 2)
                    reqBytes[i / 2] = Convert.ToByte(reqStr.Substring(i, 2), 16);
                subData[0] = reqBytes;
            }
            else if (this.IsTCP)
            {
                //Transaction ID[2] + Protocol ID[2] + DataLength[2] + Frame
                rcvBytes = new byte[frame.Length - 6];
                Buffer.BlockCopy(frame, 6, rcvBytes, 0, rcvBytes.Length);

                byte[] temp = new byte[reqBytes.Length - 6];
                Buffer.BlockCopy(reqBytes, 6, temp, 0, temp.Length);
                subData[0] = temp;
            }
            else
                rcvBytes = frame;

            if (rcvBytes == null)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("({0}) Protocol Error - Ascii → RTU Convert Fail", this.GetType().Name));
                return null;
            }

            if (this.IsEXP)
            {
                byte cmd = rcvBytes[1];

                switch (cmd)
                {
                    case 0x01:
                    case 0x02: return this.Response_GetReadCoils(cmd, rcvBytes, reqBytes);
                    case 0x03:
                    case 0x04: return this.Response_GetReadHoldingRegister(cmd, rcvBytes, reqBytes);
                    case 0x05: return base.Response_GetWriteSingleCoils(rcvBytes);
                    case 0x06: return base.Response_GetWriteSingleRegister(rcvBytes);
                    case 0x0F: return base.Response_GetWriteMultipleCoils(rcvBytes);
                    case 0x10: return base.Response_GetWriteMultipleRegister(rcvBytes);
                    default:
                        if (cmd > 0x80)
                            return new List<object>()
                            {
                                new DataFrame_Modbus()
                                {
                                    DeviceAddr = rcvBytes[0],
                                    FuncCode = cmd,
                                    Value = rcvBytes[2]
                                }
                            };
                        else
                            System.Diagnostics.Debug.WriteLine(string.Format("({0}) Protocol Error - UnSupport Command: {1:X2}", this.GetType().Name, cmd)); break;
                }

                return null;
            }
            else
                return base.Response_ExtractData(rcvBytes, subData);
        }

        #region Response Command Process

        /// <summary>
        /// 01(0x01), 02(0x02) ReadCoils Frame 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected override List<object> Response_GetReadCoils(byte cmd, byte[] frame, byte[] reqBytes)
        {
            if (this.IsEXP == false)
                return base.Response_GetReadCoils(cmd, frame, reqBytes);
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Res : Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount]
            //ByteCount 1→ 2증가

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                //Value = (bool)((담당Byte >> Bit위치) & 1 == 1)
                list.Add(new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = cmd,
                    DataAddr = startAddr + i,
                    Value = ((frame[4 + (i / 8)] >> (i % 8)) & 1) == 1
                });
            }

            return list;
        }
        /// <summary>
        /// 03(0x03), 04(0x04) ReadHoldingRegister Frame 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected override List<object> Response_GetReadHoldingRegister(byte cmd, byte[] frame, byte[] reqBytes)
        {
            if (this.IsEXP == false)
                return base.Response_GetReadHoldingRegister(cmd, frame, reqBytes);
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount] Hi/Lo
            //ByteCount 1→ 2증가

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (frame[2] << 8) + frame[3];
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i += 2)
            {
                list.Add(new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = cmd,
                    DataAddr = startAddr + (i / 2),
                    Value = (Int16)((frame[4 + i] << 8) + frame[4 + i + 1])
                });
            }

            return list;
        }

        #endregion Response Command Process End

        #endregion Response End
        #region ErrorCode


        public override bool ConfirmErrCode(byte[] frame)
        {
            if (this.IsTCP) return true;

            if (this.IsAscii)
                return this.ConfirmErrCode_Ascii(frame);

            return this.ConfirmErrCode_RTU(frame);
        }

        #region ErrorCode 검사

        private bool ConfirmErrCode_RTU(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;
            int errStartIdx = bytes.Length - 2;


            for (int Index = 0; Index < errStartIdx; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool ConfirmErrCode_Ascii(byte[] bytes)
        {
            int sum = 0;
            int errStartIdx = bytes.Length - 4;
            byte bytemp;

            //Header[1] ~ (ErrCd[2] + Tail[2]))
            for (int Index = 1; Index < errStartIdx; Index += 2)
            {
                bytemp = 0x00;
                bytemp += (byte)(AsciiToHex(bytes[Index]) * 0x10);
                bytemp += (byte)AsciiToHex(bytes[Index + 1]);
                sum = (byte)((sum + bytemp) & 0xFF);
            }
            sum = (byte)(((sum ^ 0xFF) + 1) & 0xFF);

            bytemp = 0x00;
            bytemp += (byte)(AsciiToHex(bytes[errStartIdx]) * 0x10);
            bytemp += (byte)AsciiToHex(bytes[errStartIdx + 1]);
            if (bytemp == sum)
                return true;

            return false;
        }

        public static byte AsciiToHex(byte b)
        {
            switch (b)
            {
                case 0x30: return 0x00;
                case 0x31: return 0x01;
                case 0x32: return 0x02;
                case 0x33: return 0x03;
                case 0x34: return 0x04;
                case 0x35: return 0x05;
                case 0x36: return 0x06;
                case 0x37: return 0x07;
                case 0x38: return 0x08;
                case 0x39: return 0x09;
                case 0x40: return 0x10;
                case 0x41: return 0x0a;
                case 0x42: return 0x0b;
                case 0x43: return 0x0c;
                case 0x44: return 0x0d;
                case 0x45: return 0x0e;
                case 0x46: return 0x0f;
            }

            return 0;
        }

        #endregion ErrorCode 검사 End

        public override byte[] CreateErrCode(byte[] frame)
        {
            if (this.IsTCP) return null;

            if (this.IsAscii) return this.CreateErrCode_Ascii(frame);

            return this.CreateErrCode_RTU(frame);
        }

        #region ErrorCode 생성

        private byte[] CreateErrCode_RTU(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;


            for (int Index = 0; Index < bytes.Length; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            return chkCd;
        }

        private byte[] CreateErrCode_Ascii(byte[] bytes)
        {
            int sum = 0;

            //Header[1] ~ len
            //Ascii → RTU
            for (int Index = 0; Index < bytes.Length; Index += 2)
            {
                byte bytemp = 0x00;
                bytemp += (byte)(AsciiToHex(bytes[Index]) * 0x10);
                bytemp += (byte)AsciiToHex(bytes[Index + 1]);
                sum = (byte)((sum + bytemp) & 0xFF);
            }
            sum = (byte)(((sum ^ 0xFF) + 1) & 0xFF);

            byte[] sumBytes = BitConverter.GetBytes(sum);
            byte[] chkCd = Encoding.ASCII.GetBytes(Convert.ToString(sumBytes[0], 16).ToUpper());

            if (chkCd.Length == 1)
            {
                byte chkSumValue = chkCd[0];
                chkCd = new byte[2];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            return chkCd;
        }

        #endregion ErrorCode 생성 End

        #endregion ErrorCode End

        public List<byte[]> CreateRequest_ReadCoils(int deviceAddr, List<int> readList, int maxFrameCount = 63)
        {
            List<int[]> addrList = base.SortContinuouseAddress(readList, maxFrameCount);
            List<byte[]> dataFrame = base.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x01;

            return CreateRequest_ApplyCustom(dataFrame);
        }
        public List<byte[]> CreateRequest_ReadDiscreteInputs(int deviceAddr, List<int> readList, int maxFrameCount = 63)
        {
            List<int[]> addrList = base.SortContinuouseAddress(readList, maxFrameCount);
            List<byte[]> dataFrame = base.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x02;

            return CreateRequest_ApplyCustom(dataFrame);
        }
        public List<byte[]> CreateRequest_ReadHoldingRegister(int deviceAddr, List<int> readList, int maxFrameCount = 63)
        {
            List<int[]> addrList = base.SortContinuouseAddress(readList, maxFrameCount);
            List<byte[]> dataFrame = base.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x03;

            return CreateRequest_ApplyCustom(dataFrame);
        }
        public List<byte[]> CreateRequest_ReadInputRegister(int deviceAddr, List<int> readList, int maxFrameCount = 63)
        {
            List<int[]> addrList = base.SortContinuouseAddress(readList, maxFrameCount);
            List<byte[]> dataFrame = base.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x04;

            return CreateRequest_ApplyCustom(dataFrame);
        }

        /// <summary>
        /// Modbus 기본처리 Frame → HYModbus 처리 Process 진행
        /// </summary>
        /// <param name="frameList">Modbus Frame</param>
        /// <returns></returns>
        private List<byte[]> CreateRequest_ApplyCustom(List<byte[]> frameList)
        {
            //RTU → Ascii
            if (this.IsAscii)
                frameList = this.ConvertFrame_RtuToAscii(frameList);

            //ErrorCode Append
            frameList = Append_ErrorCode(frameList);

            //TCP
            if (this.IsTCP)
                frameList = Append_TCP(frameList);

            if (frameList.Count == 0) return null;

            return frameList;
        }
        /// <summary>
        /// Modbus → Ascii 변환처리
        /// </summary>
        /// <param name="frameList">Modbus Frmae</param>
        /// <returns>Ascii Frame</returns>
        private List<byte[]> ConvertFrame_RtuToAscii(List<byte[]> frameList)
        {
            List<byte[]> asciiFrameList = new List<byte[]>();

            foreach (var rtuFrame in frameList)
            {
                byte[] asciiFrame = new byte[rtuFrame.Length * 2];

                for (int i = 0; i < rtuFrame.Length; i++)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(rtuFrame[i].ToString("X2"));
                    asciiFrame[(i * 2)] = bytes[0];
                    asciiFrame[(i * 2) + 1] = bytes[1];
                }

                asciiFrameList.Add(asciiFrame);
            }

            return asciiFrameList;
        }
        /// <summary>
        /// ErrorCode 붙이기
        /// </summary>
        /// <param name="frameList">계산할 Frame</param>
        /// <returns>ErrorCode가 붙은 Frame</returns>
        private List<byte[]> Append_ErrorCode(List<byte[]> frameList)
        {
            List<byte[]> errList = new List<byte[]>();
            foreach (var frame in frameList)
            {
                byte[] errCode = this.CreateErrCode(frame);
                byte[] result = frame;

                if (errCode != null)
                {
                    result = Utils.Controls.Utils.QYUtils.Comm.BytesAppend(frame, errCode);

                    if (this.IsAscii)
                    {
                        byte[] ascii = new byte[result.Length + 3];
                        ascii[0] = 0x3A; //':'
                        ascii[ascii.Length - 2] = 0x0D;//CR
                        ascii[ascii.Length - 1] = 0x0A;//LF

                        Buffer.BlockCopy(result, 0, ascii, 1, result.Length);

                        result = ascii;
                    }
                }

                errList.Add(result);
            }

            return errList;
        }
        /// <summary>
        /// TCP Frame 붙이기
        /// </summary>
        /// <param name="frameList">붙일 Frame</param>
        /// <returns>TCP Frame이 붙은 Frame</returns>
        private List<byte[]> Append_TCP(List<byte[]> frameList)
        {
            List<byte[]> tcpList = new List<byte[]>();
            foreach (var frame in frameList)
            {
                //Transaction ID[2] + Protocol ID[2] + DataLength[2] + ModbusFrame
                byte[] tcpFrame = new byte[6];
                tcpFrame[4] = (byte)((frame.Length >> 8) & 0xFF);
                tcpFrame[5] = (byte)(frame.Length & 0xFF);

                byte[] temp = Utils.Controls.Utils.QYUtils.Comm.BytesAppend(tcpFrame, frame);

                tcpList.Add(temp);
            }

            if (tcpList.Count == 0)
                return frameList;

            return tcpList;
        }

    }
}
