using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol.Custom.HYNux.Extractor
{
    internal class PCLinkExtractor_TD300500
    {
        internal virtual PCLInkItem[] Parse(byte[] reqPayload, byte[] resPayload)
        {
            string req = Encoding.ASCII.GetString(reqPayload),
                   res = Encoding.ASCII.GetString(resPayload);
            string cmd = res.Substring(4, 3);

            switch (cmd)
            {
                case "WHO": return this.WHO(resPayload);
                case "RDR": return this.RDR(req, res);
                case "RRD": return this.RRD(req, res);
                case "WDR": return this.WDR(req);
                case "WRD": return this.WRD(req);
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
        /// RDR / Word Register Read, 연속읽기
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] RDR(string request, string response)
        {
            //Request : Addr[3] + ,[1] + Command[3] + ,[1] + ReadCount[2] + ,[1] + Start.Reg Dec[4] + ,[1]
            //Resonse : Addr[3] + ,[1] + Command[3] + ,[1] + OK[2] + { ( ,[1] + Value Hex[4] ) * ReadCount } + ,[1]
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(8, 2)),
                startAddr = Convert.ToInt32(request.Substring(11, 4));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                UInt16 preValue = Convert.ToUInt16(response.Substring(11 + (i * 5), 4), 16);
                byte[] binary = BitConverter.GetBytes(preValue);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// RRD / Word Register Read, 비연속읽기
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] RRD(string request, string response)
        {
            //Request : Addr[3] + ,[1] + Command[3] + ,[1] + ReadCount[2] + { ( ,[1] + Start.Reg Dec[4] ) * ReadCount } + ,[1]
            //Resonse : Addr[3] + ,[1] + Command[3] + ,[1] + OK[2] + { ( ,[1] + Value Hex[4] ) * ReadCount } + ,[1]
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(8, 2));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = Convert.ToUInt16(request.Substring(11 + (i * 5), 4), 16);
                UInt16 preValue = Convert.ToUInt16(response.Substring(11 + (i * 5), 4), 16);
                byte[] binary = BitConverter.GetBytes(preValue);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// WDR / Word Register Write, 연속쓰기
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] WDR(string request)
        {
            //Request : Addr[3] + ,[1] + Command[3] + ,[1] + WriteCount[2] + ,[1] + Start.Reg Dec[4] + ,[1] + { ( Value Hex[4] + ,[1] ) * WriteCount }
            //Resonse : Addr[3] + ,[1] + Command[3] + ,[1] + OK[2] + ,[1]
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(8, 2)),
                startAddr = Convert.ToInt32(request.Substring(11, 4));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                UInt16 preValue = Convert.ToUInt16(request.Substring(16 + (i * 5), 4), 16);
                byte[] binary = BitConverter.GetBytes(preValue);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// WRD / Word Register Write, 비연속쓰기
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] WRD(string request)
        {
            //Request : Addr[3] + ,[1] + Command[3] + ,[1] + WriteCount[2] + ,[1] + { ( Reg Addr Dec[4] + ,[1] + Value Hex[4] + ,[1] ) * WriteCount }
            //Resonse : Addr[3] + ,[1] + Command[3] + ,[1] + OK[2] + ,[1]
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(8, 2));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = Convert.ToUInt16(request.Substring(11 + (i * 5), 4), 16);
                UInt16 preValue = Convert.ToUInt16(request.Substring(16 + (i * 5), 4), 16);
                byte[] binary = BitConverter.GetBytes(preValue);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
    }
}
