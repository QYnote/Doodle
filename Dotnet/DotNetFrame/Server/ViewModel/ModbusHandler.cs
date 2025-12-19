using DotNet.Utils.Controls.Utils;
using DotNetFrame.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Server.ViewModel
{
    internal class ModbusHandler : QYViewModelHandler
    {
        internal event EventHandler<string> ServerLog;

        private Server_Modbus _server = new Server_Modbus();

        public string Server_IPAddress
        {
            get => this._server.IP;
            set
            {
                if (this._server.IP != value
                    && this._server.IsOpen == false)
                {
                    this._server.IP = value;
                }
            }
        }
        public int Server_PortNo
        {
            get => this._server.PortNo;
            set
            {
                if (this._server.PortNo != value
                    && this._server.IsOpen == false)
                {
                    this._server.PortNo = value;
                }
            }
        }

        internal ModbusHandler()
        {
            this._server.ServerLog += this.ServerLog;
            this.Initialize();
        }

        private void Initialize()
        {
            this.Server_IPAddress = "127.0.0.1";
            this.Server_PortNo = 5000;
        }

        internal void Open() => this._server.Open();
        internal void Close() => this._server.Close();
    }
}
