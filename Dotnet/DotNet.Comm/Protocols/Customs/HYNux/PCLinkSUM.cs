using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class PCLinkSUM : PCLinkSTD
    {
        public virtual bool CheckSum(byte[] bytes)
        {
            //STX[1] ~ (ErrCd[2] + CRLF[2])
            int sum = 0;
            int errStartIdx = bytes.Length - 4;
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
                chkCd = new byte[2];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    return true;
                }
            }

            return false;
        }

        public virtual byte[] CreateErrCode(byte[] bytes)
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
                chkCd = new byte[2];
                chkCd[0] = 48;
                chkCd[1] = chkSumValue;
            }

            return chkCd;
        }

        protected override byte[] Build(string body)
        {
            byte[] stx = new byte[] { 0x02 };
            byte[] frame = Encoding.ASCII.GetBytes(body);
            byte[] checksum = this.CreateErrCode(frame);
            byte[] build = new byte[stx.Length + frame.Length + checksum.Length + base.Tail.Length];

            //STX
            Buffer.BlockCopy(stx, 0, build, 0, stx.Length);
            //Body
            Buffer.BlockCopy(frame, 0, build, stx.Length, frame.Length);
            //CheckSum
            Buffer.BlockCopy(checksum, 0, build, stx.Length + frame.Length, checksum.Length);
            //Tail
            Buffer.BlockCopy(base.Tail, 0, build, stx.Length + frame.Length + checksum.Length, base.Tail.Length);

            return build;
        }
    }
}
