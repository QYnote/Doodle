using DotNet.Comm.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Protocol
{
    internal class ModbusAscii : Model.Protocol.IProtocol
    {
        DotNet.Comm.Protocols.ModbusAscii origin = new DotNet.Comm.Protocols.ModbusAscii();

        public byte[] Parse(byte[] buffer, byte[] req) => this.origin.Parse(buffer, req);
        public IProtocolResult Extraction(byte[] frame, byte[] req) => this.origin.Extraction(frame, req);
        public bool CheckError(byte[] frame) => this.origin.CheckSum(frame);
        public byte[] CreateCheckSum(byte[] frame) => this.origin.CreateErrCode(frame);
    }
}
