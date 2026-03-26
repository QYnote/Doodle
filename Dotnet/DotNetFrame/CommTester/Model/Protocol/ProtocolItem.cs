using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol
{
    internal abstract class ProtocolItem
    {
        internal object Value { get; }

        internal ProtocolItem(object value)
        {
            this.Value = value;
        }
    }
}
