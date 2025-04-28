using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetFrameWork.Communication.Protocols
{
    public abstract class ProtocolBase
    {
        protected enum ReturnCode
        {
            None = -1,
            Enter_None = 0,
            Enter_Yet = 1,
            Success = 2,
            Error = 3,
        }

        public abstract Queue<byte[]> ParseData(byte[] readBuffer, out int parseCode);
    }
}
