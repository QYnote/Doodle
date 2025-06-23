using Dnf.Comm.Controls.Protocols;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls.Protocols
{
    /// <summary>
    /// Modbus Protocol
    /// </summary>
    public class Modbus : ProtocolFrame
    {
        public override void DataExtract(CommData frame, byte[] buffer)
        {
            //Header 시작 위치 확인
            byte[] headerBytes;

            //Header
            //기본: Addr[1] + Cmd[1]
            headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1] };
            int startIdx = QYUtils.Find(buffer, headerBytes);
            if (startIdx < 0) return;

            //FuncCode
            byte cmd = buffer[startIdx + 1];

            //기능코드별 Frame 추출
            int frameLen = 0;

            if(cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04)
            {
                //기본: Addr[1] + Cmd[1] + ByteLen[1] + Data[Len]
                //EXP: Len[2]
                int byteLen = buffer[startIdx + headerBytes.Length];

                frameLen = headerBytes.Length + 1 + byteLen + base.ErrCodeLength;
            }
            else if(cmd == 0x05 || cmd == 0x06 || cmd == 0x10)
            {
                //0x05 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + Value[2]
                //0x06 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + RegValue[2]
                //0x10 - 기본: Addr[1] + Cmd[1] + StartReg[2] + ReadRegCount[2]
                frameLen = headerBytes.Length + 2 + 2 + base.ErrCodeLength;
            }
            else if(cmd >= 0x80)
            {
                //가장 < bit값이 1일 경우 Error값으로 확인
                //Addr[1] + Cmd[1] + ErrCode[1]
                frameLen = headerBytes.Length + 1 + base.ErrCodeLength;
            }

            if (buffer.Length < startIdx + frameLen) return;

            //Data 추출
            byte[] frameBytes = new byte[frameLen];
            Buffer.BlockCopy(buffer, startIdx, frameBytes, 0, frameBytes.Length);
            frame.RcvData = frameBytes;
        }
    }
}
