using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    public class UnitChannel
    {
        public int ChNumber;
        public Dictionary<string, ChannelValue> ChValues;

        public UnitChannel()
        {
            ChValues = new Dictionary<string, ChannelValue>();
        }
    }
}
