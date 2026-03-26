using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol.Custom.HYNux.Extractor
{
    internal class PCLinkExtractor_TH300500
    {
        internal virtual PCLInkItem[] Parse(byte[] reqPayload, byte[] resPayload)
        {
            string req = Encoding.ASCII.GetString(reqPayload),
                   res = Encoding.ASCII.GetString(resPayload);
            string cmd = res.Substring(2, 3);

            switch (cmd)
            {
                case "WHO": return this.WHO(resPayload);
                case "RCV": return this.RCV(resPayload);
                case "RRP": return this.RRP(resPayload);
                case "RUP": return this.RUP(resPayload);
                case "RTD": return this.RTD(resPayload);
            }

            return null;
        }


        /// <summary>
        /// WHO Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] WHO(byte[] response)
        {
            List<PCLInkItem> list = new List<PCLInkItem>();
            string res = Encoding.ASCII.GetString(response);

            list.Add(new PCLInkItem("WHO", res));

            return list.ToArray();
        }

        /// <summary>
        /// RRP Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] RRP(byte[] response)
        {
            //Request : Addr[3] + Command[3] + ,[1] + Reg No. Dec[4]
            //Resonse : Addr[3] + Command[3] + ,[1] + OK[2] + ,[1] + Reg.No[4] + ,[1] + 순수 Binary[2 or 34]
            List<PCLInkItem> list = new List<PCLInkItem>();
            int regNo = Convert.ToInt32(Encoding.ASCII.GetString(response).Substring(10, 4));

            if (regNo == 0)
            {
                for (int i = 0; i < 17; i++)
                {
                    UInt16 addr = (UInt16)(i + 1);
                    byte[] binary = new byte[2];
                    Buffer.BlockCopy(response, 15 + (i * 2), binary, 0, binary.Length);

                    list.Add(new PCLInkItem(addr, binary));
                }
            }
            else
            {
                UInt16 addr = (UInt16)regNo;
                byte[] binary = new byte[2];
                Buffer.BlockCopy(response, 15, binary, 0, binary.Length);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// RUP Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] RUP(byte[] response)
        {
            //Request : Addr[3] + Command[3] + ,[1] + Reg No. Dec[4]
            //Resonse : Addr[3] + Command[3] + ,[1] + OK[2] + ,[1] + Reg.No[4] + ,[1] + 순수 Binary[2 or 512]
            List<PCLInkItem> list = new List<PCLInkItem>();
            int regNo = Convert.ToInt32(Encoding.ASCII.GetString(response).Substring(10, 4));

            if (regNo == 0)
            {
                for (int i = 0; i < 512; i++)
                {
                    UInt16 addr = (UInt16)(i + 1);
                    byte[] binary = new byte[2];
                    Buffer.BlockCopy(response, 15 + (i * 2), binary, 0, binary.Length);

                    list.Add(new PCLInkItem(addr, binary));
                }
            }
            else
            {
                UInt16 addr = (UInt16)regNo;
                byte[] binary = new byte[2];
                Buffer.BlockCopy(response, 15, binary, 0, binary.Length);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// RTD Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] RTD(byte[] response)
        {
            //Request : Addr[3] + Command[3] + ,[1] + Reg No. Dec[4]
            //Resonse : Addr[3] + Command[3] + ,[1] + OK[2] + ,[1] + Reg.No[4] + ,[1] + 순수 Binary[29]
            List<PCLInkItem> list = new List<PCLInkItem>();
            UInt16 regNo = Convert.ToUInt16(Encoding.ASCII.GetString(response).Substring(10, 4));

            if (regNo < 12)
            {
                byte[] binary = new byte[24];
                Buffer.BlockCopy(response, 15, binary, 0, binary.Length);

                list.Add(new PCLInkItem(regNo, binary));
            }
            else if(12 <= regNo && regNo < 111)
            {
                //PatternName은 24byte만 사용
                byte[] binary = new byte[24];
                Buffer.BlockCopy(response, 15, binary, 0, binary.Length);

                list.Add(new PCLInkItem(regNo, binary));
            }

            if (list.Count == 0) return null;

            return list.ToArray();
        }
        /// <summary>
        /// RCV Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] RCV(byte[] response)
        {
            //Resonse : Addr[3] + Command[3] + ,[1] + OK[2] + ,[1] + Binary[29]
            List<PCLInkItem> list = new List<PCLInkItem>();
            int idxHandle = 10;
            byte[] binary;

            //TSV
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("TSV", binary));

            //TPV
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("TPV", binary));

            //TMV
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("TMV", binary));

            //HSV
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("HSV", binary));

            //HPV
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("HPV", binary));

            //HMV
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("HMV", binary));

            //T_I/S
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("T_I/S", binary));

            //T/S
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("T/S", binary));

            //A/S
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("A/S", binary));

            //RY
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("RY", binary));

            //O/C
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("O/C", binary));

            //D/I
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("D/I", binary));

            //RM
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("RM", binary));

            //RTH
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("RTH", binary));

            //RTS
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("RTS", binary));

            //SRTH
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("SRTH", binary));

            //SRTM
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("SRTM", binary));

            //SFTH
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("SFTH", binary));

            //SFTM
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("SFTM", binary));

            //RPTN
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("RPTN", binary));

            //RSEG
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("RSEG", binary));

            //RPRC
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("RPRC", binary));

            //RPRN
            binary = new byte[2];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 2;
            list.Add(new PCLInkItem("RPRN", binary));

            //RLC
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("RLC", binary));

            //RLN
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("RLN", binary));

            //UDSW
            binary = new byte[1];
            Buffer.BlockCopy(response, idxHandle, binary, 0, binary.Length);
            idxHandle += 1;
            list.Add(new PCLInkItem("UDSW", binary));

            return list.ToArray();
        }
    }
}
