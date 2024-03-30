using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    public class UnitChannel
    {
        public Unit ParentUnit;
        public int ChNumber;
        public bool ChEnable;
        public Dictionary<string, ChannelValue> ChValues;

        public UnitChannel(Unit unit)
        {
            ParentUnit = unit;
            ChEnable = true;
            ChValues = new Dictionary<string, ChannelValue>();
        }
    }
}
