using DotNet.Comm.Transport;
using DotNet.Utils.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel.Port
{
    internal class SocketVM :TransportVM
    {
        private List<QYItem> _list_protocol = new List<QYItem>();

        private QYSocketPort Socket { get; }
        public List<QYItem> ProtocolList => this._list_protocol;

        public string IP
        {
            get => this.Socket.IP;
            set => this.Socket.IP = value;
        }
        public int PortNo
        {
            get => this.Socket.PortNo;
            set => this.Socket.PortNo = value;
        }
        public System.Net.Sockets.ProtocolType Protocol
        {
            get => this.Socket.SocketProtocol;
            set => this.Socket.SocketProtocol = value;
        }
        public int MaxBufferSize
        {
            get => this.Socket.MaxBufferSize;
            set => this.Socket.MaxBufferSize = value;
        }

        internal SocketVM(QYSocketPort port)
        {
            this.Socket = port;
            this._list_protocol = QYViewUtils.EnumToItem<System.Net.Sockets.ProtocolType>().ToList();
        }
    }
}
