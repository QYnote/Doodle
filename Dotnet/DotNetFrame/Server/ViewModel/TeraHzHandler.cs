using DotNet.Utils.Controls.Utils;
using DotNetFrame.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Server.ViewModel
{
    internal class TeraHzHandler : QYViewModelHandler
    {
        internal event EventHandler<string> ServerLog;

        private Server_HY_TeraHz _server = new Server_HY_TeraHz();

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
                if(this._server.PortNo != value
                    && this._server.IsOpen == false)
                {
                    this._server.PortNo = value;
                }
            }
        }
        public int Sensor_Count
        {
            get => (int)Math.Log(2, this._server.SensorCount);
            set
            {
                int conv = (int)Math.Pow(2, value);

                if(conv != this._server.SensorCount)
                {
                    this._server.SensorCount = conv;
                }
            }
        }

        public bool Data_Span_Run
        {
            get => this._server.ApplyMax;
            set
            {
                if(this._server.ApplyMax != value)
                {
                    this._server.ApplyMax = value;
                    base.OnPopertyChanged(nameof(this.Data_Span_Run));
                }
            }
        }
        public short Data_Span_Offset
        {
            get => this._server.OffsetMax;
            set 
            {
                if(this._server.OffsetMax != value)
                {
                    this._server.OffsetMax = value;
                }
            }
        }
        public bool Data_Object_Run
        {
            get => this._server.ApplyObject;
            set
            {
                if (this._server.ApplyObject != value)
                {
                    this._server.ApplyObject = value;
                    base.OnPopertyChanged(nameof(this.Data_Object_Run));
                }
            }
        }
        public short Data_Object_Offset
        {
            get => this._server.OffsetObject;
            set
            {
                if (this._server.OffsetObject != value)
                {
                    this._server.OffsetObject = value;
                }
            }
        }
        public bool Data_RandomValue_Run
        {
            get => this._server.ApplyRandom;
            set
            {
                if (this._server.ApplyRandom != value)
                {
                    this._server.ApplyRandom = value;
                    base.OnPopertyChanged(nameof(this.Data_RandomValue_Run));
                }
            }
        }
        public short Data_RandomValue_Offset
        {
            get => this._server.OffsetBoundScale;
            set
            {
                if (this._server.OffsetBoundScale != value)
                {
                    this._server.OffsetBoundScale = value;
                }
            }
        }

        internal TeraHzHandler()
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
