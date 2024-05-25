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
    internal class Custom_EthernetPort : Port
    {
        /// <summary>
        /// Port IP번호
        /// </summary>
        internal IPAddress IPAddr;
        /// <summary>
        /// Port번호
        /// </summary>
        internal ushort PortNo;

        internal Custom_EthernetPort(uProtocolType type, IPAddress addr, ushort portNo)
        {
            IPAddr = addr;
            PortNo = portNo;

            this.PortName = addr.ToString() + ":" + portNo;
            base.ProtocolType = type;
            base.State = PortConnectionState.Close;
        }

        internal override bool Open()
        {
            return true;
        }

        internal override bool Close()
        {
            return true;
        }

        internal override bool Write(byte[] bytes)
        {
            return true;
        }
    }
}
