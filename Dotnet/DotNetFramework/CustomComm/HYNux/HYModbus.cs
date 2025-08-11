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
        public bool IsTCP { get; set; } = false;

        public HYModbus(bool isClient) : base(isClient) { }

        public override byte[] DataExtract_Receive(byte[] reqBytes, byte[] buffer)
        {
            //CustomCode 미사용일 경우
            if (this.IsAscii == false
                && this.IsEXP == false
                && this.IsTCP == false)
            {
                //순수한 Modbus Protocol
                return base.DataExtract_Receive(reqBytes, buffer);
            }
            //Header 시작 위치 확인
            byte[] headerBytes;
            int startIdx,
                idxHandle = 0,
                headerLen = 2,
                frameLen;
            byte cmd;


            //Header
            if (this.IsAscii && this.IsTCP == false)
            {
                //Ascii: ':'[1] + Addr[2] + Cmd[2]
                if (reqBytes.Length < 3) return null;

                headerBytes = new byte[] { reqBytes[0], reqBytes[1], reqBytes[2], reqBytes[3], reqBytes[4] };
                headerLen = headerBytes.Length;
            }
            else if(this.IsTCP && this.IsAscii == false)
            {
                //TCP: Transaction ID[2] + Protocol ID[2] + DataLength[2] + Addr[1] + Cmd[1]
                if (reqBytes.Length < 8) return null;

                headerBytes = new byte[] { reqBytes[6], reqBytes[7] };
                idxHandle = 6;
            }
            else
            {
                //기본: Addr[1] + Cmd[1]
                if (reqBytes.Length < 2) return null;

                headerBytes = new byte[] { reqBytes[0], reqBytes[1] };
            }

            while (idxHandle < buffer.Length - 1)
            {
                //Header 시작위치 확인
                //Cmd는 Error Code로 날라올 수 있기 때문에 Addr만 먼저 찾기
                startIdx = QYUtils.Find(buffer, headerBytes, idxHandle++);
                if (startIdx < 0)
                {
                    //Error Cmd가 날라온건지 확인
                    if(this.IsAscii == false)
                    {
                        startIdx = buffer.Find(new byte[] { headerBytes[0], (byte)(headerBytes[1] + 0x80) }, idxHandle);
                    }
                    else
                    {
                        cmd = (byte)(Convert.ToByte(Encoding.ASCII.GetString(new byte[] { headerBytes[3], headerBytes[4] }), 16) + 0x80);
                        byte[] errCmd = Encoding.ASCII.GetBytes(cmd.ToString("X2"));
                        headerBytes[3] = errCmd[0];
                        headerBytes[4] = errCmd[1];

                        startIdx = QYUtils.Find(buffer, headerBytes, idxHandle++);
                    }
                }
                idxHandle++;
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
                                    buffer[startIdx + headerLen + 2 + (i * 2)],
                                    buffer[startIdx + headerLen + 2 + (i * 2) + 1]
                                    }
                                ),
                                16
                            );
                        }

                        //Data 길이 추출
                        for (int i = 0; i < dataLenBytes.Length; i++)
                            dataLen += dataLenBytes[i] << (8 * ((dataLenBytes.Length - 1) - i));

                        //Error Code
                        dataLen += base.ErrCodeLength;

                        //RTU → Ascii Length변환
                        dataLen *= 2;
                    }

                    //':' + Protocol Len
                    frameLen = headerLen + byteCountLen + dataLen;
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

                //TCP: Transaction ID[2] + Protocol ID[2] + DataLength[2]
                if (this.IsTCP)
                {
                    startIdx -= 6;
                    frameLen += 6;
                }

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
                && this.IsTCP == false)
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
                int addIdx = this.IsTCP ? 6 : 0;
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
            if (this.IsAscii == false && this.IsTCP == false)
                return base.ReceiveConfirm(rcvBytes);
            else if (this.IsAscii == false && this.IsTCP == true)
                return true;
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
                byte[] tempReq = new byte[reqBytes.Length - 5];
                Buffer.BlockCopy(reqBytes, 1, tempReq, 0, tempReq.Length);

                string reqStr = Encoding.ASCII.GetString(tempReq);
                byte[] convReq = new byte[tempReq.Length / 2];
                for (int i = 0; i < tempReq.Length; i += 2)
                {
                    convReq[(i / 2)] = Convert.ToByte(reqStr.Substring(i, 2), 16);
                }
                reqBytes = convReq;

                byte[] tempRes = new byte[rcvBytes.Length - 5];
                Buffer.BlockCopy(rcvBytes, 1, tempRes, 0, tempRes.Length);

                string resStr = Encoding.ASCII.GetString(tempRes);
                byte[] convRes = new byte[tempRes.Length / 2];
                for (int i = 0; i < tempRes.Length; i += 2)
                {
                    convRes[(i / 2)] = Convert.ToByte(resStr.Substring(i, 2), 16);
                }
                rcvBytes = convRes;
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
            if(this.IsEXP == false)
            {
                base.Get_ReadHoldingRegister(dic, reqBytes, rcvBytes);
                return;
            }
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
