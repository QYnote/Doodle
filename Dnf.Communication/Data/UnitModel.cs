using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Data
{
    internal class UnitModel
    {
        internal string ModelName;
        internal Dictionary<uProtocolType, bool> SupportProtocol = new Dictionary<uProtocolType, bool>();

        internal UnitModel()
        {
            //지원 Protocol 기본틀
            foreach (uProtocolType protocol in UtilCustom.EnumToItems<uProtocolType>())
            {
                SupportProtocol.Add(protocol, false);
            }
        }
    }
}
