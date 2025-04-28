using DotnetFrameWork.Communication.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetFrameWork.Communication.Ports
{
    public class ModbusProtocol:ProtocolBase
    {
        public override Queue<byte[]> ParseData(byte[] readBuffer, out int parseCode)
        {
            parseCode = (int)ReturnCode.None;

            return null;
        }
    }

}
