using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Comm.Controls.Protocols
{
    internal abstract class ProtocolBase
    {
        internal abstract void DataExtract(CommFrame frame, byte[] buffer);
    }
}
