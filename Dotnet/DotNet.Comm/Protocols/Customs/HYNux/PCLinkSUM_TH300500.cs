using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class PCLinkSUM_TH300500 : PCLinkSTD_TH300500
    {
        public bool CheckSum(byte[] bytes)
        {
            byte[] chkCd = new byte[2];
            int sum = 0;

            for (int i = 0; i < bytes.Length - 2; i++)
            {
                sum = (sum + bytes[i]) % 65536;
            }

            byte[] sumBytes = BitConverter.GetBytes(sum);
            chkCd[0] = sumBytes[1];
            chkCd[1] = sumBytes[0];

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[bytes.Length - 2 + i] != chkCd[i])
                {
                    return true;
                }
            }

            return false;
        }

        public byte[] CreateErrCode(byte[] bytes)
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

        public override byte[] Parse(byte[] buffer, byte[] req)
        {
            if (buffer == null) return null;

            int startIdx,
                idxHandle = 0,
                endStartIdx = -1,
                endLastIdx = -1;

            while (idxHandle < buffer.Length - 1)
            {
                //1. Header(0x02, STX) Receive 검사
                startIdx = Array.IndexOf(buffer, 0x02, idxHandle++);
                if (startIdx < 0) continue;

                //2. Tail Receive 검사
                endStartIdx = Utils.Controls.Utils.QYUtils.Find(buffer, this.Tail, startIdx + 1);
                if (endStartIdx < 0) continue;

                endLastIdx = endStartIdx + this.Tail.Length - 1;
                //Error Code 길이 추가
                endLastIdx += 2;
                if (buffer.Length < endLastIdx + 1) continue;

                //3. Frame 추출
                byte[] frameByte = new byte[endLastIdx - startIdx + 1];
                Buffer.BlockCopy(buffer, startIdx, frameByte, 0, frameByte.Length);

                return frameByte;
            }

            return null;
        }

        protected override byte[] Build(string body)
        {
            byte[] stx = new byte[] { 0x02 };
            byte[] frame = Encoding.ASCII.GetBytes(body);
            byte[] preBuild = new byte[stx.Length + frame.Length + this.Tail.Length];

            //STX
            Buffer.BlockCopy(stx, 0, preBuild, 0, stx.Length);
            //Body
            Buffer.BlockCopy(frame, 0, preBuild, stx.Length, frame.Length);
            //Tail
            Buffer.BlockCopy(this.Tail, 0, preBuild, stx.Length + frame.Length, this.Tail.Length);
            //CheckSum
            byte[] checksum = this.CreateErrCode(preBuild);
            byte[] build = new byte[preBuild.Length + checksum.Length];
            Buffer.BlockCopy(preBuild, 0, build, 0, preBuild.Length);
            Buffer.BlockCopy(checksum, 0, build, preBuild.Length, checksum.Length);

            return build;
        }
    }
}
