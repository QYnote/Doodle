using DotNet.Comm.Protocols;
using DotNet.Comm.Servers;
using DotNet.Utils.Controls.Utils;
using DotNetFrame.Model.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.ViewModel.Server
{
    internal class VM_Server_Modbus
    {
        public event Update_WithParam ServerLog;

        private M_Server_Modbus _server = new M_Server_Modbus();

        public string IP { get => this._server.IP; set => this._server.IP = value; }
        public int PortNo { get => this._server.PortNo; set => this._server.PortNo = value; }

        internal VM_Server_Modbus()
        {
            this._server.ServerLog += (msg) => { this.ServerLog?.Invoke(msg); };
        }

        internal void Open() => this._server.Open();
        internal void Close() => this._server.Close();
    }
}
