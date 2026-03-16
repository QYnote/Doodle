using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class ModbusAscii_HYExpand : ModbusAscii
    {
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
