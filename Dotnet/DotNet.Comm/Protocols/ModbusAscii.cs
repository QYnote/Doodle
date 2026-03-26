using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public class ModbusAscii
    {
        // =============================
        // 
        // CheckSum 정보
        // 
        // =============================
        /// <summary>
        /// LRC Check
        /// </summary>
        /// <param name="bytes">Ascii Full Binary</param>
        /// <returns></returns>
        public bool CheckLRC(byte[] bytes)
        {
            //Header[1] ~ (ErrCd[2] + Tail[2]))
            byte[] asciiFrame = new byte[bytes.Length - 5];
            Buffer.BlockCopy(bytes, 1, asciiFrame, 0, asciiFrame.Length);
            byte[] rtu = this.AsciiToRTU(asciiFrame);

            //수신된 LRC 계산
            int sum = 0;
            for (int Index = 0; Index < rtu.Length; Index ++)
                sum = (byte)((sum + rtu[Index]) & 0xFF);

            sum = (byte)(((sum ^ 0xFF) + 1) & 0xFF);

            //수신된 LRC 추출
            byte[] rcvLRC = new byte[2];
            Buffer.BlockCopy(bytes, bytes.Length - 4, rcvLRC, 0, rcvLRC.Length);
            byte checkLRC = Convert.ToByte(Encoding.ASCII.GetString(rcvLRC), 16);

            if (checkLRC == sum)
                return true;

            return false;
        }
        /// <summary>
        /// LRC 생성
        /// </summary>
        /// <param name="bytes">생성할 frame
        /// </param>
        /// <returns>LRC</returns>
        /// <remarks>Address ~ Data Body</remarks>
        public byte[] CreateLRC(byte[] rtuFrame)
        {
            int sum = 0;

            //Header[1] ~ len
            //Ascii → RTU
            for (int Index = 0; Index < rtuFrame.Length; Index ++)
            {
                sum = (byte)((sum + rtuFrame[Index]) & 0xFF);
            }
            sum = (byte)(((sum ^ 0xFF) + 1) & 0xFF);

            byte[] sumBytes = BitConverter.GetBytes(sum);
            byte[] chkCd = Encoding.ASCII.GetBytes(Convert.ToString(sumBytes[0], 16).ToUpper());

            if (chkCd.Length == 1)
            {
                byte chkSumValue = chkCd[0];
                chkCd = new byte[2];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            return chkCd;
        }
        public byte[] AsciiToRTU(byte[] ascii)
        {
            List<byte> binaryRTU = new List<byte>();

            string asciiStr = Encoding.ASCII.GetString(ascii);

            int asciiLen = asciiStr.Length;
            for (int i = 0; i < asciiLen; i += 2)
            {
                byte b = Convert.ToByte(asciiStr.Substring(i, 2), 16);
                binaryRTU.Add(b);
            }

            return binaryRTU.ToArray();
        }

        // =============================
        // 
        // Response 정보
        // 
        // =============================
        private byte[] _buffer = null;

        public virtual byte[] Parse(byte[] buffer)
        {
            this.StackBuffer(buffer);

            byte header = 0x3A;
            byte[] tail = new byte[] { 0x0D, 0x0A };

            int startIndex = Array.IndexOf(this._buffer, header);
            if (startIndex < 0) return null;

            int endIndex = this._buffer.Find(tail, startIndex);
            if(endIndex < 0) return null;

            byte[] preFrame = new byte[endIndex - startIndex];
            Buffer.BlockCopy(this._buffer, startIndex, preFrame, 0, preFrame.Length);

            return preFrame;
        }
        private void StackBuffer(byte[] buffer)
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

        public void Initialize()
        {
            this._buffer = null;
        }

        // =============================
        // 
        // Request 정보
        // 
        // =============================

        public byte[] Build(byte clientID, byte[] rtuPDU)
        {
            //RTU Packet 생성
            byte[] pre = new byte[1 + rtuPDU.Length];
            pre[0] = clientID;
            Buffer.BlockCopy(rtuPDU, 0, pre, 1, rtuPDU.Length);

            //RTU → Ascii Binary 변환
            byte[] asciiPre = this.RTUToAscii(pre);

            //LRC 생성
            byte[] lrc = this.CreateLRC(pre);
            //병합
            byte[] binary = new byte[asciiPre.Length + lrc.Length];
            Buffer.BlockCopy(asciiPre, 0, binary, 0, asciiPre.Length);
            Buffer.BlockCopy(lrc, 0, binary, asciiPre.Length, lrc.Length);

            return binary;
        }

        public byte[] RTUToAscii(byte[] rtu)
        {
            List<byte> binaryAscii = new List<byte>();

            //RTU Binary → Ascii String
            string str = string.Empty;
            foreach (byte b in rtu)
                str += b.ToString("X2");

            //Ascii String → Ascii Binary
            binaryAscii.AddRange(Encoding.ASCII.GetBytes(str));

            if (binaryAscii.Count == 0) return null;
            
            return binaryAscii.ToArray();
        }
    }
}
