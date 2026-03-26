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
        // =============================
        // 
        // Response 정보
        // 
        // =============================
        protected override byte[] ParseFrame()
        {
            //1. 최소 Header 길이 탐색
            //Header: Addr[1] + Cmd[1]
            if (this._buffer.Length < 2)
            {
                this._buffer = null;
                return null;
            }

            //2. 기능코드별 Frame 길이 탐색
            byte cmd = this._buffer[1];
            int frameLen = -1;
            if (cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04)
            {
                //Addr[1] + Cmd[1] + ByteCount[2] + Data[ByteCount]
                if (this._buffer.Length < 4)
                {
                    this._buffer = null;
                    return null;
                }
                int byteCount = (this._buffer[2] << 8) | this._buffer[3];

                frameLen = 1 + 1 + 2 + byteCount;
            }
            else if (cmd == 0x05 || cmd == 0x06 || cmd == 0x0F || cmd == 0x10)
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

            if (this._buffer.Length < frameLen)
            {
                this._buffer = null;
                return null;
            }

            //3. Frame 추출
            byte[] frameByte = new byte[frameLen];
            Buffer.BlockCopy(this._buffer, 0, frameByte, 0, frameLen);

            return frameByte;
        }
    }
}
