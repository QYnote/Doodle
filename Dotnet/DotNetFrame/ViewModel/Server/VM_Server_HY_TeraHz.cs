using DotNet.Utils.Controls.Utils;
using DotNetFrame.Model.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.ViewModel.Server
{
    internal class VM_Server_HY_TeraHz
    {
        internal const int DEFAULT_SENSOR_COUNT = M_Server_HY_TeraHz.DEFAULT_SENSOR_COUNT;
        internal const int DEFAULT_SENSOR_OFFSET_OBJECT = M_Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_OBJECT;
        internal const int DEFAULT_SENSOR_OFFSET_MAX = M_Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_MAX;
        internal const int DEFAULT_SENSOR_OFFSET_BOUNDSCALE = M_Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_BOUNDSCALE;
        internal const int SENSOR_PER_CHIP = M_Server_HY_TeraHz.SENSOR_PER_CHIP;
        
        public event Update_WithParam ServerLog;

        private M_Server_HY_TeraHz _server = new M_Server_HY_TeraHz();

        internal string IP { get => this._server.IP; set => this._server.IP = value; }
        internal int PortNo { get => this._server.PortNo; set => this._server.PortNo = value; }
        internal int SensorCount { get => this._server.SensorCount; set => this._server.SensorCount = value; }

        public bool ApplyObject { get => this._server.ApplyObject; set => this._server.ApplyObject= value; }
        public short OffsetObject { get => this._server.OffsetObject; set => this._server.OffsetObject= value; }
        public bool ApplyMax { get => this._server.ApplyMax; set => this._server.ApplyMax= value; }
        public short OffsetMax { get => this._server.OffsetMax; set => this._server.OffsetMax= value; }
        public bool ApplyRandom { get => this._server.ApplyRandom; set => this._server.ApplyRandom= value; }
        public short OffsetBoundScale { get => this._server.OffsetBoundScale; set => this._server.OffsetBoundScale = value; }

        internal void Open() => this._server.Open();
        internal void Close() => this._server.Close();
    }
}
