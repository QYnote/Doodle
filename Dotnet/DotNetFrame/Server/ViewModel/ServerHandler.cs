using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Server.ViewModel
{
    internal enum ServerType
    {
        TeraHz,
        Modbus,
    }

    internal class ServerHandler : QYViewModelHandler
    {
        private List<QYViewUtils.EnumItem<ServerType>> _server_List;
        private ServerType _server_current;

        public List<QYViewUtils.EnumItem<ServerType>> ServerList { get => _server_List; }
        public ServerType Server_Current
        {
            get => _server_current;
            set
            {
                if (_server_current != value)
                {
                    _server_current = value;

                    base.OnPopertyChanged(nameof(this.Server_Current));
                }
            }
        }

        internal ServerHandler()
        {
            Initialize();
        }

        private void Initialize()
        {
            this._server_List = QYViewUtils.GetEnumItems<ServerType>().ToList();
            this.Server_Current = ServerType.TeraHz;
        }
    }
}
