using DotNet.CommTester.Model.Protocol;
using DotNet.CommTester.Model.Protocol.Custom.HYNux;
using DotNet.CommTester.Model.Protocol.Custom.HYNux.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Port.Protocol.Custom.HYNux
{
    internal class PCLinkSTD : PCLinkBase
    {
        DotNet.Comm.Protocols.Customs.HYNux.PCLinkSTD origin = new DotNet.Comm.Protocols.Customs.HYNux.PCLinkSTD();
        PCLinkExtractor unpacker = new PCLinkExtractor();

        internal PCLinkSTD(CommPort handler) : base(handler) { }

        public override byte[] Parse(byte[] buffer) => this.origin.Parse(buffer, base.Request);
        public override byte[] CreateCheckSum(byte[] frame) => null;
        public override IProtocolResult Extraction(byte[] frame)
        {
            ProtocolResult<PCLInkItem> result = new ProtocolResult<PCLInkItem>(base.Request);
            result.Response = frame;

            //1. Header, Tail 제외 추출
            byte[] reqBody = new byte[frame.Length - 3],
                   resBody = new byte[base.Request.Length - 3];
            Buffer.BlockCopy(base.Request, 1, reqBody, 0, reqBody.Length);
            Buffer.BlockCopy(frame, 1, resBody, 0, resBody.Length);

            //2. String 변환
            string reqStr = Encoding.ASCII.GetString(reqBody),
                   resStr = Encoding.ASCII.GetString(resBody);

            //3. NG 판단
            if(resStr.Substring(6, 2) == "NG")
            {
                result.Type = ResultType.Protocol_Exception;
                result.ErrorMessage = "Protocol Error";
            }
            else
            {
                //4. Item 추출
                PCLInkItem[] items = this.unpacker.Parse(reqBody, resBody);
                if (items == null)
                {
                    result.Type = ResultType.Protocol_Exception;
                    result.ErrorMessage = "Undeveloped Command or Extract Error";
                }
                else
                {
                    result.Type = ResultType.Success;

                    foreach (var item in items)
                        result.Items.Add(item);
                }
            }

            return result;
        }

        public override void Initialize()
        {
            this.Request = null;
            this.origin.Initialize();
        }
    }
}
