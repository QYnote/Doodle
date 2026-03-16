using DotNet.Comm.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Port.Protocol.Custom.HYNux
{
    internal class PCLinkSTD_TH300500 : Model.Protocol.IProtocol
    {
        DotNet.Comm.Protocols.Customs.HYNux.PCLinkSTD_TH300500 origin = new DotNet.Comm.Protocols.Customs.HYNux.PCLinkSTD_TH300500();

        public byte[] Parse(byte[] buffer, byte[] req) => this.origin.Parse(buffer, req);
        public IProtocolResult Extraction(byte[] frame, byte[] req) => this.origin.Extraction(frame, req);
        public bool CheckError(byte[] frame) => true;
        public byte[] CreateCheckSum(byte[] frame) => null;
    }
}
