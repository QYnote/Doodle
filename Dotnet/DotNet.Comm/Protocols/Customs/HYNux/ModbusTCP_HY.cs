using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class ModbusTCP_HY : ModbusRTU
    {
        public override byte[] Parse(byte[] buffer, byte[] req)
        {
            int idxHandle = 0,
                startIdx = -1,
                frameLen = -1;
            byte cmd;

            //Header: TCPHeader[6] + Addr[1] + Cmd[1]
            if (buffer.Length < 8) return null;

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
                    //Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
                    if (buffer.Length < startIdx + 3) continue; //ByteCount receive 검사
                    int byteCount = buffer[startIdx + 2];

                    frameLen = 1 + 1 + 1 + byteCount;
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

                //TCP Header
                frameLen += 6;

                if (buffer.Length < startIdx + frameLen) continue;

                //Frame 추출
                byte[] frameByte = new byte[frameLen];
                Buffer.BlockCopy(buffer, startIdx - 6, frameByte, 0, frameLen);

                return frameByte;
            }

            return null;
        }
    }
}
