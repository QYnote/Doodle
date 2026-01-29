using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel
{
    internal class EthernetVM : QYViewModel
    {
        private QYEthernet _ethernet = null;

        public string IP
        {
            get => this._ethernet.IP;
            set
            {
                if(this.IP != value
                    && System.Net.IPAddress.TryParse(value, out _))
                {
                    this._ethernet.IP = value;

                    base.OnPropertyChanged(nameof(this.IP));
                }
            }
        }
        public int PortNo
        {
            get => this._ethernet.PortNo;
            set
            {
                if (this.PortNo != value)
                {
                    this._ethernet.PortNo = value;

                    base.OnPropertyChanged(nameof(this.PortNo));
                }
            }
        }

        internal EthernetVM(QYEthernet ethernet)
        {
            this._ethernet = ethernet;
        }
    }
}
