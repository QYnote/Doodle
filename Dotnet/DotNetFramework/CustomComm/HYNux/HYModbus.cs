using DotNet.Comm.Structures.Protocols;
using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CustomComm.HYNux
{
    public class HYModbus : Modbus
    {
        public bool IsAscii { get; set; } = false;
        public bool IsEXP { get; set; } = false;
        public bool IsEthernet { get; set; } = false;

        public HYModbus(bool isClient) : base(isClient) { }

        public override byte[] DataExtract_Receive(byte[] rcvBytes, byte[] buffer)
        {
            //CustomCode 미사용일 경우
            if (this.IsAscii == false
                && this.IsEXP == false
                && this.IsEthernet == false)
            {
                //순수한 Modbus Protocol
                return base.DataExtract_Receive(rcvBytes, buffer);
            }
            //Header 시작 위치 확인
            byte[] headerBytes;
            int startIdx,
                idxHandle = 0,
                headerLen,
                frameLen;
            byte cmd;

            //Header
            if (this.IsAscii)
            {
                int addIdx = this.IsEthernet ? 6 : 0;
                //Ascii: ':'[1] + Addr[2] + Cmd[2]
                headerBytes = new byte[] { rcvBytes[0 + addIdx], rcvBytes[1 + addIdx], rcvBytes[2 + addIdx] };
                headerLen = headerBytes.Length + 2;
            }
            else
            {
                //기본: Addr[1] + Cmd[1]
                headerBytes = new byte[] { rcvBytes[this.IsEthernet ? 6 : 0] };
                headerLen = headerBytes.Length + 1;
            }

            while (idxHandle < buffer.Length - 1)
            {
                //Header 시작위치 확인
                //Cmd는 Error Code로 날라올 수 있기 때문에 Addr만 먼저 찾기
                startIdx = QYUtils.Find(buffer, headerBytes, idxHandle++);
                if (startIdx < 0) continue;

                //FuncCode
                if (buffer.Length < startIdx + headerLen) continue;

                if (this.IsAscii == false)
                    //기본: Addr[1] + Cmd[1]
                    cmd = buffer[startIdx + 1];
                else
                    //Ascii: ':'[1] + Addr[2] + Cmd[2]
                    cmd = Convert.ToByte(Encoding.ASCII.GetString(new byte[] { buffer[startIdx + 3], buffer[startIdx + 4] }), 16);

                //기능코드별 Frame 추출
                frameLen = 0;

                if (cmd == 0x03)
                {
                    //기본: Addr[1] + Cmd[1] + Len[1] + Data[Len]
                    //EXP: Len[2]
                    int byteCountLen = 1, dataLen = 0;

                    //ByteCount 길이
                    if (this.IsEXP) byteCountLen = 2;
                    if (this.IsAscii) byteCountLen *= 2;

                    //Data 길이
                    if (this.IsAscii == false)
                    {
                        //기본
                        for (int i = 0; i < byteCountLen; i++)
                            dataLen += buffer[startIdx + headerLen + i] << (8 * (byteCountLen - 1 - i));
                    }
                    else
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
                                )
                            );
                        }

                        //Data 길이 추출
                        for (int i = 0; i < dataLenBytes.Length; i++)
                            dataLen += dataLenBytes[i] << (8 * (dataLenBytes.Length - 1 - i));

                        //RTU → Ascii Length변환
                        dataLen *= 2;
                    }

                    frameLen = headerLen + byteCountLen + dataLen + base.ErrCodeLength;
                }
                else if (cmd == 0x06 || cmd == 0x10)
                {
                    //0x06 - 기본: Addr[1] + Cmd[1] + StartReg[2] + RegValue[2]
                    //0x10 - 기본: Addr[1] + Cmd[1] + StartReg[2] + ReadRegCount[2]
                    int bodyLen = 2 + 2;

                    if (this.IsAscii)
                        bodyLen *= 2;

                    frameLen = headerLen + bodyLen + base.ErrCodeLength;
                }
                else if (cmd >= 0x80)
                {
                    //가장 < bit값이 1일 경우 Error값으로 확인
                    //Addr[1] + Cmd[1] + ErrCode[1]
                    int bodyLen = 1;

                    if (this.IsAscii)
                        bodyLen *= 2;

                    frameLen = headerLen + bodyLen + base.ErrCodeLength;
                }

                //CRLF[2]
                if (this.IsAscii) frameLen += 2;

                if (buffer.Length < startIdx + frameLen) continue;

                //Data 추출
                byte[] frameBytes = new byte[frameLen];
                Buffer.BlockCopy(buffer, startIdx, frameBytes, 0, frameBytes.Length);

                return frameBytes;
            }//End While

            return null;
        }
        public override byte[] DataExtract_Request(byte addr, byte[] buffer)
        {
            //CustomCode 미사용일 경우
            if(this.IsAscii == false
                && this.IsEXP == false
                && this.IsEthernet == false)
            {
                //순수한 Modbus Protocol
                return base.DataExtract_Request(addr, buffer);
            }

            //Header 시작 위치 확인
            byte[] headerBytes;
            int startIdx,
                idxHandle = 0,
                headerLen,
                frameLen;
            byte cmd;

            //Header
            if(this.IsAscii)
            {
                int addIdx = this.IsEthernet ? 6 : 0;
                //Ascii: ':'[1] + Addr[2] + Cmd[2]
                byte[] bAddr = Encoding.ASCII.GetBytes(addr.ToString("X2"));
                headerBytes = new byte[3];
                headerBytes[0 + addIdx] = (byte)':';
                Buffer.BlockCopy(bAddr, 0, headerBytes, 1 + addIdx, bAddr.Length);
                headerLen = headerBytes.Length + 2;
            }
            else
            {
                //기본: Addr[1] + Cmd[1]
                headerBytes = new byte[] { addr };
                headerLen = headerBytes.Length + 1;
            }

            while (idxHandle < buffer.Length - 1)
            {
                //Header 시작위치 확인
                //Cmd는 Error Code로 날라올 수 있기 때문에 Addr만 먼저 찾기
                startIdx = QYUtils.Find(buffer, headerBytes, idxHandle++);
                if (startIdx < 0) continue;

                //FuncCode
                if (buffer.Length < startIdx + headerLen) continue;

                if (this.IsAscii == false)
                    //기본: Addr[1] + Cmd[1]
                    cmd = buffer[startIdx + 1];
                else
                    //Ascii: ':'[1] + Addr[2] + Cmd[2]
                    cmd = Convert.ToByte(Encoding.ASCII.GetString(new byte[] { buffer[startIdx + 3], buffer[startIdx + 4] }), 16);

                //기능코드별 Frame 추출
                frameLen = 0;

                if (cmd == 0x03 || cmd == 0x06)
                {
                    //기본: Addr[1] + Cmd[1] + StartAddr[2] + ReadCount[2]
                    int dataLen = 4;

                    frameLen = headerLen + (dataLen * (this.IsAscii ? 2 : 1)) + base.ErrCodeLength;
                }
                else if (cmd == 0x10)
                {
                    //0x10 - 기본: Addr[1] + Cmd[1] + startAddr[2] + ReadRegCount[2] + ByteCount[1] + Value[ByteCount]
                    int byteLen = buffer[startIdx + headerLen + 4],
                        dataLen = 5 + byteLen;

                    if (this.IsAscii)
                        dataLen *= 2;

                    frameLen = headerLen + dataLen + base.ErrCodeLength;
                }

                //CRLF[2]
                if (this.IsAscii) frameLen += 2;

                if (buffer.Length < startIdx + frameLen) continue;

                //Data 추출
                byte[] frameBytes = new byte[frameLen];
                Buffer.BlockCopy(buffer, startIdx, frameBytes, 0, frameBytes.Length);

                return frameBytes;
            }//End While

            return null;
        }
        public override bool ReceiveConfirm(byte[] rcvBytes)
        {
            if (this.IsAscii == false)
                return base.ReceiveConfirm(rcvBytes);
            else
            {
                byte cmd = Convert.ToByte(Encoding.ASCII.GetString(new byte[] { rcvBytes[3], rcvBytes[4] }), 16);

                if (cmd >= 0x80)
                {
                    Debug.WriteLine(string.Format("[Error]Request Error - Error Code:{0:X2}",
                        Convert.ToByte(Encoding.ASCII.GetString(new byte[] { rcvBytes[5], rcvBytes[6] }), 16)
                        ));
                    return false;
                }
            }

            return true;
        }
        public override void GetData(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes)
        {
            if (dic == null
                || reqBytes == null || rcvBytes == null)
                throw new Exception("Method Parameter Null Error");

            if (this.IsAscii)
            {
                byte[] tempReq = new byte[reqBytes.Length - 3];
                Buffer.BlockCopy(reqBytes, 1, tempReq, 0, tempReq.Length);
                reqBytes = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(tempReq));

                byte[] tempRes = new byte[rcvBytes.Length - 3];
                Buffer.BlockCopy(tempRes, 1, rcvBytes, 0, rcvBytes.Length);
                rcvBytes = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(tempRes));
            }

            if (this.IsEXP)
            {
                byte cmd = rcvBytes[1];

                switch (cmd)
                {
                    case 0x01:
                    case 0x02: this.Get_ReadCoils(dic, reqBytes, rcvBytes); break;
                    case 0x03:
                    case 0x04: this.Get_ReadHoldingRegister(dic, reqBytes, rcvBytes); break;
                    case 0x05: base.Get_WriteSingleCoils(dic, reqBytes); break;
                    case 0x06: base.Get_WriteSingleRegister(dic, reqBytes); break;
                    case 0x0F: base.Get_WriteMultipleCoils(dic, reqBytes); break;
                    case 0x10: base.Get_WriteMultipleRegister(dic, reqBytes); break;
                }
            }
            else
                base.GetData(dic, reqBytes, rcvBytes);
        }
        #region Get Command Process
        
        protected override void Get_ReadCoils(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes)
        {
            if (this.IsEXP == false)
                base.Get_ReadCoils(dic, reqBytes, rcvBytes);

            //Expend Process 처리
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Res : Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount]
            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];

            for (int i = 0; i < readCount; i++)
                //Data = (bool)((담당Byte >> Bit위치) & 1 == 1)
                dic[startAddr + i + 1] = ((rcvBytes[4 + (i / 8)] >> (i % 8)) & 1) == 1;
        }
        protected override void Get_ReadHoldingRegister(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes)
        {
            if (this.IsEXP == false)
                base.Get_ReadHoldingRegister(dic, reqBytes, rcvBytes);

            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount] Hi/Lo

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (rcvBytes[2] << 8) + rcvBytes[3];

            for (int i = 0; i < readCount; i += 2)
                dic[startAddr + 1 + (i / 2)] = (Int16)((rcvBytes[4 + i] << 8) + rcvBytes[4 + i + 1]);
        }

        #endregion Get Command Process
        public override byte[] CreateResponse(Dictionary<int, object> dic, byte[] reqBytes)
        {
            if (dic == null
                || reqBytes == null)
                throw new Exception("Method Parameter Null Error");

            if (this.IsAscii)
            {
                //':'삭제처리
                byte[] tempReq = new byte[reqBytes.Length - 3];
                Buffer.BlockCopy(reqBytes, 1, tempReq, 0, tempReq.Length);
                reqBytes = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(tempReq));
            }

            byte cmd = reqBytes[1];
            byte[] cmdBytes = null;

            if (this.IsEXP)
            {
                switch (cmd)
                {
                    case 0x01:
                    case 0x02: cmdBytes = this.CreResponse_ReadCoils(dic, reqBytes); break;
                    case 0x03:
                    case 0x04: cmdBytes = this.CreResponse_ReadHoldingRegister(dic, reqBytes); break;
                    case 0x05:
                    case 0x06: cmdBytes = base.CreResponse_WriteSingle(dic, reqBytes); break;
                    case 0x0F:
                    case 0x10: cmdBytes = base.CreResponse_WriteMultiple(dic, reqBytes); break;
                }
            }
            else
                cmdBytes = base.CreateResponse(dic, reqBytes);

            if (this.IsAscii)
            {
                string strAscii = string.Empty;

                foreach (byte b in cmdBytes)
                {
                    strAscii += b.ToString("X2");
                }

                cmdBytes = Encoding.ASCII.GetBytes(strAscii);
                cmdBytes = QYUtils.BytesAppend(new byte[] { 0x3A }, cmdBytes);  //':'추가 처리
            }

            return cmdBytes;
        }
        #region Create Response
        
        protected override byte[] CreResponse_ReadCoils(Dictionary<int, object> dic, byte[] reqBytes)
        {
            byte[] cmdBytes = base.CreResponse_ReadCoils(dic, reqBytes);
            byte[] expBytes = new byte[cmdBytes.Length + 1];
            int readCount = (reqBytes[4] << 8) + reqBytes[5],
                byteCount = readCount * 2;

            Buffer.BlockCopy(cmdBytes, 0, expBytes, 0, 2);
            Buffer.BlockCopy(new byte[] { (byte)((byteCount >> 8) & 0xFF), (byte)(byteCount & 0xFF) }, 0, expBytes, 2, 2);
            Buffer.BlockCopy(cmdBytes, 3, expBytes, 4, cmdBytes.Length - 3);

            return expBytes;
        }
        protected override byte[] CreResponse_ReadHoldingRegister(Dictionary<int, object> dic, byte[] reqBytes)
        {
            byte[] cmdBytes = base.CreResponse_ReadHoldingRegister(dic, reqBytes);
            byte[] expBytes = new byte[cmdBytes.Length + 1];
            int readCount = (reqBytes[4] << 8) + reqBytes[5],
                byteCount = readCount * 2;

            Buffer.BlockCopy(cmdBytes, 0, expBytes, 0, 2);
            Buffer.BlockCopy(new byte[] { (byte)((byteCount >> 8) & 0xFF), (byte)(byteCount & 0xFF) }, 0, expBytes, 2, 2);
            Buffer.BlockCopy(cmdBytes, 3, expBytes, 4, cmdBytes.Length - 3);

            return expBytes;
        }

        #endregion Create Response
    }
}
