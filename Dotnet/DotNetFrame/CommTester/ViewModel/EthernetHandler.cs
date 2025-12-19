using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using DotNetFrame.CommTester.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel
{
    internal class EthernetHandler : QYViewModelHandler
    {
        private OSPortBase _base;

        private QYEthernet Ethernet => this._base as QYEthernet;

        public bool IsOpen => this.Ethernet?.IsOpen ?? false;

        public string IP
        {
            get => this.Ethernet?.IP ?? "127.0.0.1";
            set
            {
                if (this.Ethernet == null) return;

                if (this.Ethernet.IP != value
                    && this.Ethernet?.IsOpen == false)
                {
                    this.Ethernet.IP = value;
                }
            }
        }
        public int PortNo
        {
            get => this.Ethernet?.PortNo ?? 5000;
            set
            {
                if (this.Ethernet == null) return;

                if (this.Ethernet.PortNo != value
                    && this.Ethernet.IsOpen == false)
                {
                    this.Ethernet.PortNo = value;
                }
            }
        }

        internal EthernetHandler(OSPortBase osport)
        {
            this._base = osport;
        }
    }
}
