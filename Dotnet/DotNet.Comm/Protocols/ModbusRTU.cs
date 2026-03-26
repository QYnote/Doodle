using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public class ModbusRTU
    {
        // =============================
        // 
        // CheckSum 정보
        // 
        // =============================
        /// <summary>
        /// 통신 수신 검사
        /// </summary>
        /// <param name="bytes">검사 할 Frame</param>
        /// <returns>true: 에러발생 / false : 정상</returns>
        /// <remarks>
        /// ErrorCode가 포함 된 Frame으로 진행
        /// </remarks>
        public bool CheckCRC(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;
            int errStartIdx = bytes.Length - 2;


            for (int Index = 0; Index < errStartIdx; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// ErrorCode 생성
        /// </summary>
        /// <param name="bytes">생성 할 Frame</param>
        /// <returns>생성된 Error Code</returns>
        public byte[] CreateCRC(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;


            for (int Index = 0; Index < bytes.Length; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            return chkCd;
        }

        // =============================
        // 
        // Response 정보
        // 
        // =============================
        private bool _isResStart = false;
        private DateTime _timeResStart = DateTime.MinValue;
        protected byte[] _buffer = null;
        /// <summary>
        /// Char 3.5시간
        /// </summary>
        /// <remarks>
        /// char(11 bit) / baudrate(bit per second) * milliseconds * 3.5
        /// </remarks>
        private double _char35 = 11d / 9600 * 1000 * 3.5;

        public void SetBaudrate(int baudrate)
        {
            this._char35 = 11d / baudrate * 1000 * 3.5;
        }

        public byte[] Parse(byte[] buffer)
        {
            if(this._isResStart == false)
            {
                if(buffer != null)
                {
                    this._isResStart = true;
                    this._timeResStart = DateTime.Now;
                    this.StackBuffer(buffer);
                }
            }
            else
            {
                if (buffer == null)
                {
                    if ((DateTime.Now - this._timeResStart).TotalMilliseconds > this._char35)
                    {
                        //최근 수신으로부터 3.5 Char 시간동안 수신되지 않으면
                        //수신이 다되었다고 판단
                        try
                        {
                            byte[] frame = this.ParseFrame();

                            if (frame != null)
                            {
                                //4. 추출 후처리
                                byte[] remain = new byte[this._buffer.Length - frame.Length];
                                Buffer.BlockCopy(this._buffer, frame.Length, remain, 0, remain.Length);

                                this._buffer = remain;
                            }

                            return frame;
                        }
                        finally
                        {
                            this._isResStart = false;
                        }
                    }
                }
                else
                {
                    this.StackBuffer(buffer);
                    this._timeResStart = DateTime.Now;
                }
            }

            return null;
        }

        protected void StackBuffer(byte[] buffer)
        {
            if (buffer == null) return;

            if (this._buffer == null)
            {
                byte[] temp = new byte[buffer.Length];
                Buffer.BlockCopy(buffer, 0, temp, 0, buffer.Length);

                this._buffer = temp;
            }
            else
            {
                byte[] temp = new byte[this._buffer.Length + buffer.Length];
                Buffer.BlockCopy(this._buffer, 0, temp, 0, this._buffer.Length);
                Buffer.BlockCopy(buffer, 0, temp, this._buffer.Length, buffer.Length);

                this._buffer = temp;
            }
        }

        protected virtual byte[] ParseFrame()
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
                //Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
                if (this._buffer.Length < 3)
                {
                    this._buffer = null;
                    return null;
                }
                int byteCount = this._buffer[2];

                frameLen = 1 + 1 + 1 + byteCount;
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

        public void Initialize()
        {
            this._buffer = null;
            this._isResStart = false;
        }

        // =============================
        // 
        // Request 정보
        // 
        // =============================

        /// <summary>
        /// Packet 생성 마무리
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="PDU"></param>
        /// <returns></returns>
        public virtual byte[] Build(byte clientID, byte[] PDU)
        {
            byte[] pre = new byte[1 + PDU.Length];
            pre[0] = clientID;
            Buffer.BlockCopy(PDU, 0, pre, 1, PDU.Length);

            byte[] crc = this.CreateCRC(pre);

            byte[] packet = new byte[pre.Length + 2];
            Buffer.BlockCopy(pre, 0, packet, 0, pre.Length);
            Buffer.BlockCopy(crc, 0, packet, pre.Length, crc.Length);

            return packet;
        }
        /// <summary>
        /// 01(0x01) ReadCoils Request Binary 생성
        /// </summary>
        /// <param name="startAddr">시작 Registry 주소</param>
        /// <param name="count">읽으려는 개수</param>
        /// <returns>Requeset Binary</returns>
        public virtual byte[] CreateRequest_ReadCoils(UInt16 startAddr, UInt16 count)
        {
            return new byte[] { 
                0x01,
                (byte)((startAddr >> 8) & 0xFF),
                (byte)(startAddr & 0xFF),
                (byte)((count >> 8) & 0xFF),
                (byte)(count & 0xFF),
            };
        }
        /// <summary>
        /// 02(0x02) ReadDiscreteInputs Request Binary 생성
        /// </summary>
        /// <param name="startAddr">시작 Registry 주소</param>
        /// <param name="count">읽으려는 개수</param>
        /// <returns>Requeset Binary</returns>
        public virtual byte[] CreateRequest_ReadDiscreteInputs(UInt16 startAddr, UInt16 count)
        {
            return new byte[] {
                0x02,
                (byte)((startAddr >> 8) & 0xFF),
                (byte)(startAddr & 0xFF),
                (byte)((count >> 8) & 0xFF),
                (byte)(count & 0xFF),
            };
        }
        /// <summary>
        /// 03(0x03) ReadHoldingRegister Request Binary 생성
        /// </summary>
        /// <param name="startAddr">시작 Registry 주소</param>
        /// <param name="count">읽으려는 개수</param>
        /// <returns>Requeset Binary</returns>
        public virtual byte[] CreateRequest_ReadHoldingRegister(UInt16 startAddr, UInt16 count)
        {
            return new byte[] {
                0x03,
                (byte)((startAddr >> 8) & 0xFF),
                (byte)(startAddr & 0xFF),
                (byte)((count >> 8) & 0xFF),
                (byte)(count & 0xFF),
            };
        }
        /// <summary>
        /// 04(0x04) ReadInputRegister Request Binary 생성
        /// </summary>
        /// <param name="startAddr">시작 Registry 주소</param>
        /// <param name="count">읽으려는 개수</param>
        /// <returns>Requeset Binary</returns>
        public virtual byte[] CreateRequest_ReadInputRegister(UInt16 startAddr, UInt16 count)
        {
            return new byte[] {
                0x04,
                (byte)((startAddr >> 8) & 0xFF),
                (byte)(startAddr & 0xFF),
                (byte)((count >> 8) & 0xFF),
                (byte)(count & 0xFF),
            };
        }

        /// <summary>
        /// 연속적인 Address목록별 추출
        /// </summary>
        /// <param name="list">전송 할 Address 목록</param>
        /// <param name="maxFrameCount">1회 연속으로 보낼 수 있는 Frame 수</param>
        /// <returns>연속적인 Address 목록</returns>
        protected List<int[]> SortAddress(List<int> list, int maxFrameCount = 63)
        {
            list.Sort();
            List<int> continuousAddr = new List<int>();
            List<int[]> frameList = new List<int[]>();

            for (int i = 0; i < list.Count; i++)
            {
                int curAddr = list[i];

                if (list.Count == 1)
                {
                    //List가 1개만 있는경우
                    continuousAddr.Add(curAddr);
                    frameList.Add(continuousAddr.ToArray());
                }
                else if (i == 0)
                {
                    //첫번째 Address일 경우
                    continuousAddr.Add(curAddr);
                }
                else if (i == list.Count - 1)
                {
                    //여러 Address 중 마지막 Address일 경우
                    if (curAddr - 1 == continuousAddr.Last())
                    {
                        //연속되는 Address일 경우
                        continuousAddr.Add(curAddr);
                        frameList.Add(continuousAddr.ToArray());
                    }
                    else
                    {
                        //비연속 Address일 경우
                        frameList.Add(continuousAddr.ToArray());

                        continuousAddr.Clear();
                        continuousAddr.Add(curAddr);
                        frameList.Add(continuousAddr.ToArray());
                    }
                }
                else
                {
                    if (continuousAddr.Count == 0)
                    {
                        //연속진행 중 개수초과로 초기화 되었을 경우
                        continuousAddr.Add(curAddr);
                    }
                    else if (curAddr - 1 == continuousAddr.Last())
                    {
                        //연속 Address일경우
                        continuousAddr.Add(curAddr);

                        if (continuousAddr.Count >= maxFrameCount)
                        {
                            frameList.Add(continuousAddr.ToArray());
                            continuousAddr.Clear();
                        }
                    }
                    else
                    {
                        //연속 Address가 아닐경우
                        frameList.Add(continuousAddr.ToArray());
                        continuousAddr.Clear();

                        continuousAddr.Add(curAddr);
                    }
                }
            }

            return frameList;
        }
    }
}
