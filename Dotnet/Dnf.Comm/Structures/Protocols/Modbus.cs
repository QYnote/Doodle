using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Comm.Structures.Protocols
{
    /// <summary>
    /// Modbus Protocol
    /// </summary>
    public class Modbus : ProtocolFrame
    {
        public override void DataExtract(CommData frame, byte[] buffer)
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
                startIdx = QYUtils.Find(buffer, new byte[] { frame.ReqData[0] }, idxHandle++);
                if (startIdx < 0) continue;

                //FuncCode
                if (buffer.Length < startIdx + headerLen) continue;
                cmd = buffer[startIdx + headerLen - 1];

                //기능코드별 Frame 추출
                if (cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04)
                {
                    //기본: Addr[1] + Cmd[1] + ByteLen[1] + Data[Len]
                    //EXP: Len[2]
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
                frame.RcvData = frameBytes;

                break;
            }//End While
        }

        public override bool FrameConfirm(CommData frame)
        {
            //가장 왼쪽 bit가 1일경우 Error 처리
            if (frame.RcvData[1] >= 0x80)
                return false;

            return true;
        }

        private Dictionary<int, object> ReadCmd(CommData frame)
        {
            Dictionary<int, object> dic = null;

            return dic;
        }

        /// <summary>
        /// 01(0x01) ReadCoils Frame 읽기
        /// </summary>
        /// <param name="frame">읽은 Data Frame</param>
        /// <returns>읽은 Data[Dec Address, Boolean]</returns>
        private Dictionary<int, object> ReadCoils(CommData frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
            Dictionary<int, object> dic = new Dictionary<int, object>();

            int startAddr = (frame.ReqData[2] << 8) + frame.ReqData[3],
                readCount = (frame.ReqData[4] << 8) + frame.ReqData[5];

            for (int i = 0; i < readCount; i++)
                //Data = (bool)((담당Byte >> Bit위치) & 1 == 1)
                dic[startAddr + i + 1] = ((frame.RcvData[3 + (i / 8)] >> (i % 8)) & 1) == 1;

            return dic;
        }

        private Dictionary<int, object> ReadHoldingRegister(CommData frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount] Hi/Lo
            Dictionary<int, object> dic = new Dictionary<int, object>();

            int startAddr = (frame.ReqData[2] << 8) + frame.ReqData[3],
                readCount = frame.RcvData[2];

            for (int i = 0; i < readCount; i += 2)
                dic[startAddr + 1 + (i / 2)] = (Int16)((frame.RcvData[3 + i] << 8) + frame.RcvData[3 + i + 1]);

            return dic;
        }
    }
}
