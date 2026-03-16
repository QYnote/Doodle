using DotNet.Comm.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Protocol.Custom.HYNux
{
    internal class ModbusAscii_HYExpand : Model.Protocol.IProtocol
    {
        DotNet.Comm.Protocols.Customs.HYNux.ModbusAscii_HYExpand origin = new DotNet.Comm.Protocols.Customs.HYNux.ModbusAscii_HYExpand();

        public byte[] Parse(byte[] buffer, byte[] req) => this.origin.Parse(buffer, req);
        public IProtocolResult Extraction(byte[] frame, byte[] req) => this.origin.Extraction(frame, req);
        public bool CheckError(byte[] frame) => this.origin.CheckSum(frame);
        public byte[] CreateCheckSum(byte[] frame) => this.origin.CreateErrCode(frame);
    }
}
