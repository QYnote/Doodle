using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Structures.Protocols
{
    /// <summary>
    /// Modbus Protocol
    /// </summary>
    public class Modbus : ProtocolFrame
    {
        public Modbus(bool isClient) : base(isClient) { }

        public override byte[] DataExtract_Receive(byte[] reqBytes, byte[] buffer)
        {
            if (reqBytes == null) return null;
            int startIdx,
                idxHandle = 0,
                headerLen = 2,
                frameLen = 0;
            byte cmd;

            //Header : Addr[1] + Cmd[1]
            if (buffer.Length < 2) return null;

            while (idxHandle < buffer.Length - 1)
            {
                //Header 시작위치 확인
                //Cmd는 Error Code로 날라올 수 있기 때문에 Addr만 먼저 찾기
                startIdx = Array.IndexOf(buffer, reqBytes[0], idxHandle++);
                if (startIdx < 0) continue;

                //FuncCode
                if (buffer.Length < startIdx + headerLen) continue;
                cmd = buffer[startIdx + headerLen - 1];

                if (cmd >= 0x80 && (cmd - 0x80) != reqBytes[1])
                    continue;

                //기능코드별 Frame 추출
                if (cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04)
                {
                    //기본: Addr[1] + Cmd[1] + ByteLen[1] + Data[Len]
                    int byteLen = buffer[startIdx + headerLen];

                    frameLen = headerLen + 1 + byteLen + base.ErrCodeLength;
                }
                else if (cmd == 0x05 || cmd == 0x06 || cmd == 0x10)
                {
                    //0x05 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + Value[2]
                    //0x06 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + RegValue[2]
                    //0x10 - 기본: Addr[1] + Cmd[1] + StartReg[2] + ReadRegCount[2]
                    frameLen = headerLen + 2 + 2 + base.ErrCodeLength;
                }
                else if (cmd >= 0x80)
                {
                    //가장 < bit값이 1일 경우 Error값으로 확인
                    //Addr[1] + Cmd[1] + ErrCode[1]
                    frameLen = headerLen + 1 + base.ErrCodeLength;
                }

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
            //Header : Addr[1] + Cmd[1]
            int startIdx,
                idxHandle = 0,
                headerLen = 2,
                frameLen = 0;
            byte cmd;
            while (idxHandle < buffer.Length - 1)
            {
                //Header 시작위치 확인
                //Cmd는 Error Code로 날라올 수 있기 때문에 Addr만 먼저 찾기
                startIdx = QYUtils.Find(buffer, new byte[] { addr }, idxHandle++);
                if (startIdx < 0) continue;

                //FuncCode
                if (buffer.Length < startIdx + headerLen) continue;
                cmd = buffer[startIdx + headerLen - 1];

                //기능코드별 Frame 추출
                if (cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04 || cmd == 0x05 || cmd == 0x06)
                {
                    //기본: Addr[1] + Cmd[1] + StartAddr[2] + ReadCount[2]
                    //0x05: Addr[1] + Cmd[1] + Addr[2]      + Value[2]
                    //0x06: Addr[1] + Cmd[1] + Addr[2]      + RegValue[2]
                    frameLen = headerLen + 2 + 2 + base.ErrCodeLength;
                }
                else if (cmd == 0x10)
                {
                    //0x10 - 기본: Addr[1] + Cmd[1] + startAddr[2] + ReadRegCount[2] + ByteCount[1] + Value[ByteCount]
                    int byteLen = buffer[startIdx + headerLen + 4];

                    frameLen = headerLen + 5 + byteLen + base.ErrCodeLength;
                }

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
            //가장 왼쪽 bit가 1일경우 Error 처리
            if (rcvBytes[1] >= 0x80)
            {
                Debug.WriteLine(string.Format("[Error]Request Error - Error Code:{0:X2}",
                    rcvBytes[2]
                    ));

                return false;
            }

            return true;
        }
        public override void GetData(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes)
        {
            if (dic == null
                || reqBytes == null || rcvBytes == null)
                throw new Exception("Method Parameter Null Error");
            byte cmd = rcvBytes[1];

            switch (cmd)
            {
                case 0x01:
                case 0x02: this.Get_ReadCoils(dic, reqBytes, rcvBytes); break;
                case 0x03:
                case 0x04: this.Get_ReadHoldingRegister(dic, reqBytes, rcvBytes); break;
                case 0x05: this.Get_WriteSingleCoils(dic, reqBytes); break;
                case 0x06: this.Get_WriteSingleRegister(dic, reqBytes); break;
                case 0x0F: this.Get_WriteMultipleCoils(dic, reqBytes); break;
                case 0x10: this.Get_WriteMultipleRegister(dic, reqBytes); break;
            }
        }
        #region Command Get Process
        
        /// <summary>
        /// 01(0x01), 02(0x02) ReadCoils Frame 읽기
        /// </summary>
        /// <param name="dic">입력될 Dictionary</param>
        /// <param name="reqBytes">Request Data</param>
        /// <param name="rcvBytes">Response Data</param>
        protected virtual void Get_ReadCoils(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Res : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];

            for (int i = 0; i < readCount; i++)
                //Data = (bool)((담당Byte >> Bit위치) & 1 == 1)
                dic[startAddr + i] = ((rcvBytes[3 + (i / 8)] >> (i % 8)) & 1) == 1;
        }
        /// <summary>
        /// 05(0x05) WriteSingleCoils Frame 읽기
        /// </summary>
        /// <param name="dic">입력될 Dictionary</param>
        /// <param name="reqBytes">Response Data</param>
        protected virtual void Get_WriteSingleCoils(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + Addr[2] + WriteData[2]
            int addr = (reqBytes[2] << 8) + reqBytes[3];

            if (reqBytes[4] == 0xFF && reqBytes[5] == 0x00)
                dic[addr] = true;
            else if (reqBytes[4] == 0x00 && reqBytes[5] == 0x00)
                dic[addr] = false;
        }
        /// <summary>
        /// 15(0x0F) WriteMultipleCoils Frame 읽기
        /// </summary>
        /// <param name="dic">입력될 Dictionary</param>
        /// <param name="reqBytes">Response Data</param>
        protected virtual void Get_WriteMultipleCoils(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2] + ByteCount[1] + Value[ByteCount]
            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];

            for (int i = 0; i < readCount; i++)
                //Data = (bool)((담당Byte >> Bit위치) & 1 == 1)
                dic[startAddr + i] = ((reqBytes[7 + (i / 8)] >> (i % 8)) & 1) == 1;
        }
        /// <summary>
        /// 03(0x03), 04(0x04) ReadHoldingRegister Frame 읽기
        /// </summary>
        /// <param name="dic">입력될 Dictionary</param>
        /// <param name="reqBytes">Request Data</param>
        /// <param name="rcvBytes">Response Data</param>
        protected virtual void Get_ReadHoldingRegister(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount] Hi/Lo

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = rcvBytes[2];

            for (int i = 0; i < readCount; i += 2)
                dic[startAddr + (i / 2)] = (Int16)((rcvBytes[3 + i] << 8) + rcvBytes[3 + i + 1]);
        }
        /// <summary>
        /// 06(0x06) WriteSingleRegister Frame 읽기
        /// </summary>
        /// <param name="dic">입력될 Dictionary</param>
        /// <param name="reqBytes">Response Data</param>
        protected virtual void Get_WriteSingleRegister(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + Data[2]

            int addr = (reqBytes[2] << 8) + reqBytes[3];

            dic[addr] = (Int16)((reqBytes[4] << 8) + reqBytes[5]);
        }
        /// <summary>
        /// 16(0x10) WriteMultipleRegister Frame 읽기
        /// </summary>
        /// <param name="dic">입력될 Dictionary</param>
        /// <param name="reqBytes">Response Data</param>
        protected virtual void Get_WriteMultipleRegister(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2] + ByteCount[1] + Value[ByteCount]
            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];

            for (int i = 0; i < readCount; i += 2)
                dic[startAddr + (i / 2)] = (Int16)((reqBytes[7 + i] << 8) + reqBytes[7 + i + 1]);
        }

        #endregion Command Get Process
        public override byte[] CreateResponse(Dictionary<int, object> dic, byte[] reqBytes)
        {
            if (dic == null
                || reqBytes == null)
                throw new Exception("Method Parameter Null Error");
            byte cmd = reqBytes[1];
            byte[] cmdBytes = null;

            switch (cmd)
            {
                case 0x01:
                case 0x02: cmdBytes = CreResponse_ReadCoils(dic, reqBytes); break;
                case 0x03:
                case 0x04: cmdBytes = CreResponse_ReadHoldingRegister(dic, reqBytes); break;
                case 0x05: 
                case 0x06: cmdBytes = CreResponse_WriteSingle(dic, reqBytes); break;
                case 0x0F: 
                case 0x10: cmdBytes = CreResponse_WriteMultiple(dic, reqBytes); break;
            }

            return cmdBytes;
        }
        #region Create Response
        
        protected virtual byte[] CreResponse_ReadCoils(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Res : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5],
                byteCount = (readCount / 8) + 1;
            byte[] cmdBytes = new byte[3 + byteCount];

            for (int i = 0; i < readCount; i++)
            {
                //Data = (bool)((담당Byte >> Bit위치) & 1 == 1)
                int addr = startAddr + i + 1,
                    idx = 3 + (i / 8);
                byte value;
                if (dic.ContainsKey(addr))
                    value = (byte)((bool)dic[addr] ? 1 : 0);
                else
                    value = 0;

                if (i % 8 == 0)
                    cmdBytes[idx] = value;
                else
                    cmdBytes[idx] += (byte)(value << (i % 8));
            }

            cmdBytes[0] = reqBytes[0];  //Client Addr
            cmdBytes[1] = reqBytes[1];  //Cmd
            cmdBytes[2] = (byte)byteCount;

            return cmdBytes;
        }
        protected virtual byte[] CreResponse_ReadHoldingRegister(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount] Hi/Lo

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5],
                byteCount = readCount * 2;
            byte[] cmdBytes = new byte[3 + byteCount];

            for (int i = 0; i < readCount; i ++)
            {
                Int16 value;
                if (dic.ContainsKey(startAddr + 1 + i))
                    value = (byte)((bool)dic[startAddr + 1 + i] ? 1 : 0);
                else
                    value = 0;

                cmdBytes[3 + (i * 2)]     = (byte)((value >> 8) & 0xFF);    //H
                cmdBytes[3 + (i * 2) + 1] = (byte)( value       & 0xFF);    //L
            }

            cmdBytes[0] = reqBytes[0];  //Client Addr
            cmdBytes[1] = reqBytes[1];  //Cmd
            cmdBytes[2] = (byte)byteCount;

            return cmdBytes;
        }
        protected virtual byte[] CreResponse_WriteSingle(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + Addr[2] + WriteData[2]
            byte[] cmdBytes = new byte[reqBytes.Length];

            Buffer.BlockCopy(reqBytes, 0, cmdBytes, 0, reqBytes.Length);

            return cmdBytes;
        }
        protected virtual byte[] CreResponse_WriteMultiple(Dictionary<int, object> dic, byte[] reqBytes)
        {
            //0x0F, 0x10 - Req : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2] + ByteCount[1] + Value[ByteCount]
            byte[] cmdBytes = new byte[6];

            Buffer.BlockCopy(reqBytes, 0, cmdBytes, 0, cmdBytes.Length);

            return cmdBytes;
        }

        #endregion Create Response
    }
}
