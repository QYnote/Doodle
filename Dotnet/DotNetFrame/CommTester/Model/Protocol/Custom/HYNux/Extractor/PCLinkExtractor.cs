using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol.Custom.HYNux.Extractor
{
    internal class PCLInkItem : ProtocolItem
    {
        internal UInt16 RegAddr { get; }
        internal string RegName { get; }

        public PCLInkItem(UInt16 regAddr, object value) : base(value)
        {
            this.RegAddr = regAddr;
        }

        public PCLInkItem(string name, object value) : base(value)
        {
            this.RegName = name;
        }
    }

    internal class PCLinkExtractor
    {
        internal virtual PCLInkItem[] Parse(byte[] reqPayload, byte[] resPayload)
        {
            string req = Encoding.ASCII.GetString(reqPayload),
                   res = Encoding.ASCII.GetString(resPayload);
            string cmd = res.Substring(2, 3);

            switch (cmd)
            {
                case "WHO": return this.WHO(res);
                case "DRS": return this.DRS(req, res);
                case "DRR": return this.DRR(req, res);
                case "IRS": return this.IRS(req, res);
                case "IRR": return this.IRR(req, res);
            }

            return null;
        }


        /// <summary>
        /// WHO Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] WHO(string response)
        {
            List<PCLInkItem> list = new List<PCLInkItem>();
            list.Add(new PCLInkItem("WHO", response));

            return list.ToArray();
        }

        /// <summary>
        /// DRS Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] DRS(string request, string response)
        {
            //Request : Addr[2] + Command[3] + ,[1] + ReadCount[2] + ,[1] + Start Reg Dec[4]
            //Resonse : Addr[2] + Command[3] + ,[1] + OK[2] +  { ( ,[1] + Value Hex[4] ) * ReadCOunt }
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(6, 2));
            int startAddr = Convert.ToInt32(response.Substring(9, 4));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                UInt16 preValue = Convert.ToUInt16(response.Substring(9 + (i * 5), 4), 16);
                byte[] binary = BitConverter.GetBytes(preValue);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// DRR Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] DRR(string request, string response)
        {
            //Request : Addr[2] + Command[3] + ,[1] + ReadCount[2] + { ( ,[1] + RegAddr Dec[4] ) * ReadCOunt }
            //Resonse : Addr[2] + Command[3] + ,[1] + OK[2] +  { ( ,[1] + Value Hex[4] ) * ReadCOunt }
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(6, 2));
            int startAddr = Convert.ToInt32(request.Substring(9, 4));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = Convert.ToUInt16(request.Substring(9 + (i * 5), 4));
                UInt16 preValue = Convert.ToUInt16(response.Substring(9 + (i * 5), 4), 16);
                byte[] binary = BitConverter.GetBytes(preValue);

                list.Add(new PCLInkItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// IRS Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] IRS(string request, string response)
        {
            //Request : Addr[2] + Command[3] + ,[1] + ReadCount[2] + ,[1] + Start Reg Dec[4]
            //Resonse : Addr[2] + Command[3] + ,[1] + OK[2] +  { ( ,[1] + Value[1] ) * ReadCOunt }
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(6, 2));
            int startAddr = Convert.ToInt32(response.Substring(9, 4));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                bool value = response.Substring(9 + (i * 2), 1) == "1" ;

                list.Add(new PCLInkItem(addr, value));
            }

            return list.ToArray();
        }
        /// <summary>
        /// IRR Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual PCLInkItem[] IRR(string request, string response)
        {
            //Request : Addr[2] + Command[3] + ,[1] + ReadCount[2] + { ( ,[1] + RegAddr Dec[4] ) * ReadCOunt }
            //Resonse : Addr[2] + Command[3] + ,[1] + OK[2] +  { ( ,[1] + Value Hex[1] ) * ReadCOunt }
            List<PCLInkItem> list = new List<PCLInkItem>();
            int readCount = Convert.ToInt32(request.Substring(6, 2));
            int startAddr = Convert.ToInt32(request.Substring(9, 4));

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = Convert.ToUInt16(request.Substring(9 + (i * 5), 4));
                bool value = response.Substring(9 + (i * 2), 1) == "1";

                list.Add(new PCLInkItem(addr, value));
            }

            return list.ToArray();
        }
    }
}
