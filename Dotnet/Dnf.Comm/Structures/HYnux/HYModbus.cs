using Dnf.Comm.Structures.Protocols;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Comm.Structures.HYNux
{
    internal class HYModbus : Modbus
    {
        private int _customCode = 0;

        private bool IsAscii
        {
            get
            {
                switch (this._customCode)
                {
                    case 1:
                    case 3:
                        return true;
                    default:
                        return false;
                }
            }
        }
        private bool IsEXP
        {
            get
            {
                switch (this._customCode)
                {
                    case 2:
                    case 3:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Modbus Protocol
        /// </summary>
        /// <param name="customCode">
        /// Custom 번호
        /// <para>
        /// 0: 기본<br/>
        /// 1: Modbus Ascii<br/>
        /// 2: 기본 + EXP<br/>
        /// 3: Modbus Ascii + EXP<br/>
        /// </para>
        /// </param>
        public HYModbus(int customCode = 0)
        {
            this._customCode = customCode;
        }

        public override void DataExtract(CommData frame, byte[] buffer)
        {
            //CustomCode 미사용일 경우
            if(this._customCode == 0)
            {
                //순수한 Modbus Protocol
                base.DataExtract(frame, buffer);
                return;
            }

            //Header 시작 위치 확인
            byte[] headerBytes;
            int startIdx,
                idxHandle = 0,
                headerLen,
                frameLen;
            byte cmd;

            //Header
            if (this.IsAscii == false)
            {
                //기본: Addr[1] + Cmd[1]
                headerBytes = new byte[] { frame.ReqData[0] };
                headerLen = headerBytes.Length + 1;
            }
            else
            {
                //Ascii: ':'[1] + Addr[2] + Cmd[2]
                headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1], frame.ReqData[2] };
                headerLen = headerBytes.Length + 2;
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
                frame.RcvData = frameBytes;

                break;
            }//End While
        }

        public override bool FrameConfirm(CommData frame)
        {
            if (this._customCode == 0)
                return base.FrameConfirm(frame);

            byte cmd;
            if (this.IsAscii == false)
                cmd = frame.RcvData[1];
            else
                cmd = Convert.ToByte(Encoding.ASCII.GetString(new byte[] { frame.RcvData[3], frame.RcvData[4] }), 16);


            if (cmd >= 0x80)
                return false;

            return true;
        }
    }
}
