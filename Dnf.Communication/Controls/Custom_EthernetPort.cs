using Dnf.Communication.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    public class Custom_EthernetPort : Port
    {
        /// <summary>
        /// Port IP번호
        /// </summary>
        public IPAddress IPAddr;
        /// <summary>
        /// Port번호
        /// </summary>
        public ushort PortNo;

        public Custom_EthernetPort(uProtocolType type, IPAddress addr, ushort portNo)
        {
            IPAddr = addr;
            PortNo = portNo;

            this.PortName = addr.ToString() + ":" + portNo;
            base.Units = new Dictionary<int, Unit>();
            base.ProtocolType = type;
            base.State = ConnectionState.Closed;
        }

        public override bool Open()
        {
            return true;
        }

        public override bool Close()
        {
            return true;
        }

        public override bool Send()
        {
            return true;
        }
    }
}
