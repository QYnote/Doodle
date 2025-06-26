using DotNet.Comm.Structures.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Structures.CustomStruct.HYNux
{
    internal class ModbusRTUErrorCheck : ErrorCheck
    {
        internal ModbusRTUErrorCheck()
        {
            base.CheckLen = 2;
        }

        public override bool FrameConfirm(byte[] bytes)
        {
            byte[] chkCd = new byte[base.CheckLen];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;
            int errStartIdx = bytes.Length - base.CheckLen;


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


            bool isError = false;
            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    isError = true;
                    break;
                }
            }

            return isError;
        }

        public override byte[] CreateCheckBytes(byte[] bytes)
        {
            byte[] chkCd = new byte[base.CheckLen];

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
    }

    internal class ModbusAsciiErrorCheck : ErrorCheck
    {
        //ErrCd[2] + CR[1] + LF[1]

        internal ModbusAsciiErrorCheck()
        {
            base.CheckLen = 2;
        }

        public override bool FrameConfirm(byte[] bytes)
        {
            int sum = 0;
            int errStartIdx = bytes.Length - (base.CheckLen + 2);
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
                return false;

            return true;
        }

        public override byte[] CreateCheckBytes(byte[] bytes)
        {
            int sum = 0;

            //Header[1] ~ len
            for (int Index = 1; Index < bytes.Length; Index += 2)
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

        private byte AsciiToHex(byte b)
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
    }

    internal class PCLinkErrorCheck : ErrorCheck
    {
        internal PCLinkErrorCheck()
        {
            base.CheckLen = 2;
        }

        public override bool FrameConfirm(byte[] bytes)
        {
            //STX[1] ~ (ErrCd[2] + CRLF[2])
            int sum = 0;
            int errStartIdx = bytes.Length - (base.CheckLen + 2);
            byte[] chkCd;
            for (int i = 1; i < errStartIdx; i++)
            {
                sum += bytes[i];
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd = Encoding.ASCII.GetBytes(Convert.ToString(sumBytes[0], 16).ToUpper());

            if (chkCd.Length == 1)
            {
                byte chkSumValue = chkCd[0];
                chkCd = new byte[base.CheckLen];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            bool isError = false;
            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    isError = true;
                    break;
                }
            }

            return isError;
        }

        public override byte[] CreateCheckBytes(byte[] bytes)
        {
            byte[] chkCd;
            int sum = 0;

            for (int i = 1; i < bytes.Length; i++)
            {
                sum += bytes[i];
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd = Encoding.ASCII.GetBytes(Convert.ToString(sumBytes[0], 16).ToUpper());

            if (chkCd.Length == 1)
            {
                byte chkSumValue = chkCd[0];
                chkCd = new byte[base.CheckLen];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            return chkCd;
        }
    }

    internal class PCLinkTHErrorCheck : ErrorCheck
    {
        //ETX + CR + LF + CRC[2]
        public override bool FrameConfirm(byte[] bytes)
        {
            byte[] chkCd = new byte[2];
            int sum = 0;

            for (int i = 0; i < bytes.Length - base.CheckLen; i++)
            {
                sum = (sum + bytes[i]) % 65536;
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd[0] = sumBytes[1];
            chkCd[1] = sumBytes[0];

            bool isError = false;
            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[bytes.Length - base.CheckLen + i] != chkCd[i])
                {
                    isError = true;
                    break;
                }
            }

            return isError;
        }

        public override byte[] CreateCheckBytes(byte[] bytes)
        {
            byte[] chkCd = new byte[2];
            int sum = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                sum = (sum + bytes[i]) % 65536;
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd[0] = sumBytes[1];
            chkCd[1] = sumBytes[0];

            return chkCd;
        }
    }
}
