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
        public bool ChEnable;
        public Dictionary<string, ChannelValue> ChValues;

        public UnitChannel()
        {
            ChEnable = true;
            ChValues = new Dictionary<string, ChannelValue>();
        }
    }
}
