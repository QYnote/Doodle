using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public class ModbusAscii : ModbusRTU
    {
        public override bool CheckSum(byte[] bytes)
        {
            int sum = 0;
            int errStartIdx = bytes.Length - 4;
            byte bytemp;

            //Header[1] ~ (ErrCd[2] + Tail[2]))
            for (int Index = 1; Index < errStartIdx; Index += 2)
            {
                bytemp = 0x00;
                bytemp += (byte)(AsciiToHex(bytes[Index]) * 0x10);
                bytemp += (byte)AsciiToHex(bytes[Index + 1]);
                sum = (byte)((sum + bytemp) & 0xFF);
            }
            sum = (byte)(((sum ^ 0xFF) + 1) & 0xFF);

            bytemp = 0x00;
            bytemp += (byte)(AsciiToHex(bytes[errStartIdx]) * 0x10);
            bytemp += (byte)AsciiToHex(bytes[errStartIdx + 1]);
            if (bytemp == sum)
                return true;

            return false;
        }

        public override byte[] CreateErrCode(byte[] bytes)
        {
            int sum = 0;

            //Header[1] ~ len
            //Ascii → RTU
            for (int Index = 0; Index < bytes.Length; Index += 2)
            {
                byte bytemp = 0x00;
                bytemp += (byte)(AsciiToHex(bytes[Index]) * 0x10);
                bytemp += (byte)AsciiToHex(bytes[Index + 1]);
                sum = (byte)((sum + bytemp) & 0xFF);
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

        protected byte AsciiToHex(byte b)
        {
            switch (b)
            {
                case 0x30: return 0x00;
                case 0x31: return 0x01;
                case 0x32: return 0x02;
                case 0x33: return 0x03;
                case 0x34: return 0x04;
                case 0x35: return 0x05;
                case 0x36: return 0x06;
                case 0x37: return 0x07;
                case 0x38: return 0x08;
                case 0x39: return 0x09;
                case 0x40: return 0x10;
                case 0x41: return 0x0a;
                case 0x42: return 0x0b;
                case 0x43: return 0x0c;
                case 0x44: return 0x0d;
                case 0x45: return 0x0e;
                case 0x46: return 0x0f;
            }

            return 0;
        }


        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        public override byte[] Parse(byte[] buffer, byte[] req)
        {
            byte header = 0x3A;
            byte[] tail = new byte[] { 0x0D, 0x0A };

            int startIndex = Array.IndexOf(buffer, header);
            if (startIndex < 0) return null;

            int endIndex = buffer.Find(tail, startIndex);
            if(endIndex < 0) return null;

            byte[] preFrame = new byte[endIndex - startIndex];
            Buffer.BlockCopy(buffer, startIndex, preFrame, 0, preFrame.Length);

            return preFrame;
        }

        /// <summary>
        /// Modbus Ascii 결과 추출
        /// </summary>
        /// <param name="frame">Frame</param>
        /// <param name="req">Request Frame</param>
        /// <returns>Protocol 결과</returns>
        /// <remarks>
        /// Frame, Request 모두 ModbusAscii 규칙에 따른 string Text일것,
        /// 또한 Frame의 경우 LRC는 없어야함
        /// </remarks>
        public override ModbusResult Extraction(byte[] frame, byte[] req)
        {
            //Ascii Binary → Ascii String
            string rcvStr = Encoding.ASCII.GetString(frame),
                   reqStr = Encoding.ASCII.GetString(req);

            //Ascii String → RTU Binary
            byte[] rcvRTU = new byte[frame.Length / 2],
                   reqRTU = new byte[req.Length / 2];
            for (int i = 0; i < rcvRTU.Length; i++)
                rcvRTU[i] = Convert.ToByte(rcvStr.Substring(i * 2, 2));
            for (int i = 0; i < reqRTU.Length; i++)
                reqRTU[i] = Convert.ToByte(reqStr.Substring(i * 2, 2));

            return base.Extraction(rcvRTU, reqRTU);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Request 정보        //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        private List<byte[]> RTUToAscii(List<byte[]> rtu)
        {
            List<byte[]> binaryAscii = new List<byte[]>();

            foreach (byte[] binary in rtu)
            {
                //RTU Binary → Ascii String
                string str = string.Empty;
                foreach (byte b in binary)
                    str += b.ToString("X2");

                //Ascii String → Ascii Binary
                binaryAscii.Add(Encoding.ASCII.GetBytes(str));
            }

            if (binaryAscii.Count == 0) return null;
            return binaryAscii;
        }

        public override List<byte[]> CreateRequest_ReadCoils(int deviceAddr, List<int> readList)
        {
            return this.RTUToAscii(base.CreateRequest_ReadCoils(deviceAddr, readList));
        }

        public override List<byte[]> CreateRequest_ReadDiscreteInputs(int deviceAddr, List<int> readList)
        {
            return this.RTUToAscii(base.CreateRequest_ReadDiscreteInputs(deviceAddr, readList));
        }

        public override List<byte[]> CreateRequest_ReadHoldingRegister(int deviceAddr, List<int> readList)
        {
            return this.RTUToAscii(base.CreateRequest_ReadHoldingRegister(deviceAddr, readList));
        }

        public override List<byte[]> CreateRequest_ReadInputRegister(int deviceAddr, List<int> readList)
        {
            return this.RTUToAscii(base.CreateRequest_ReadInputRegister(deviceAddr, readList));
        }

    }
}
