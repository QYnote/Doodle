using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    /// <summary>
    /// HanYuoung Exp
    /// </summary>
    /// <remarks>
    /// 0x01 ~ 04의 ByteCount[1] → [2]
    /// </remarks>
    public class ModbusRTU_HYExpand : ModbusRTU
    {
        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        public override byte[] Parse(byte[] buffer, byte[] req)
        {
            int idxHandle = 0,
                startIdx = -1,
                frameLen = -1;
            byte cmd;

            //Header: Addr[1] + Cmd[1]
            if (buffer.Length < 2) return null;

            //Frame 검색
            while (idxHandle < buffer.Length - 1)
            {
                startIdx = QYUtils.Find(buffer, new byte[] { req[0], req[1] }, idxHandle);
                if (startIdx < 0)
                {
                    //Error Cmd가 날라온건지 검사
                    startIdx = QYUtils.Find(buffer, new byte[] { req[0], (byte)(req[1] + 0x80) }, idxHandle);
                }

                idxHandle++;
                if (startIdx < 0) continue;
                cmd = buffer[startIdx + 1];

                //기능코드별 Frame 길이
                frameLen = -1;
                if (cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04)
                {
                    //Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount]
                    if (buffer.Length < startIdx + 3) continue; //ByteCount receive 검사
                    int byteCount = buffer[startIdx + 2];

                    frameLen = 1 + 1 + 2 + byteCount;
                }
                else if (cmd == 0x05 || cmd == 0x06 || cmd == 0x10)
                {
                    //0x05 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + Value[2]
                    //0x06 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + RegValue[2]
                    //0x10 - 기본: Addr[1] + Cmd[1] + StartReg[2] + ReadRegCount[2]
                    frameLen = 1 + 1 + 2 + 2;
                }
                else if (cmd >= 0x80)
                {
                    //Addr[1] + Cmd[1] + ErrCode[1]
                    frameLen = 1 + 1 + 1;
                }

                if (buffer.Length < startIdx + frameLen) continue;

                //Frame 추출
                byte[] frameByte = new byte[frameLen];
                Buffer.BlockCopy(buffer, startIdx, frameByte, 0, frameLen);

                return frameByte;
            }

            return null;
        }

        protected override List<ModbusBlock> Response_GetReadCoils(byte cmd, byte[] frame, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Res : Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount]

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];
            List<ModbusBlock> list = new List<ModbusBlock>();

            for (int i = 0; i < readCount; i++)
            {
                //Value = (bool)((담당Byte >> Bit위치) & 1 == 1)
                byte[] block = new byte[] { frame[4 + (i / 8)] };
                list.Add(new ModbusBlock(frame[0], cmd, startAddr + i, block, i % 8));
            }

            return list;
        }
        protected override List<ModbusBlock> Response_GetReadHoldingRegister(byte cmd, byte[] frame, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount] Hi/Lo

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (frame[2] << 8) + frame[3];
            List<ModbusBlock> list = new List<ModbusBlock>();

            for (int i = 0; i < readCount; i += 2)
            {
                byte[] block = new byte[2];
                Buffer.BlockCopy(frame, 4 + i, block, 0, block.Length);

                list.Add(new ModbusBlock(frame[0], cmd, startAddr + (i / 2), block));
            }

            return list;
        }
    }
}
