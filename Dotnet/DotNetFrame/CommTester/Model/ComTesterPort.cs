using DotNet.Comm;
using DotNet.Comm.ClientPorts.AppPort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model
{

    internal class ComTesterPort : AppPort
    {
        public override bool Connect()
        {
            if (base.IsAppOpen) return false;

            base.OSPort.Open();

            base.IsAppOpen = true;

            return true;
        }
        public override bool Disconnect()
        {
            base.OSPort.Close();

            base.IsAppOpen = false;

            return true;
        }
        public override void Initialize()
        {
            base.IsAppOpen = false;
            base.OSPort.InitPort();
        }
        public override byte[] Read() => base.OSPort.Read();
        public override void Write(byte[] bytes) => base.OSPort.Write(bytes);
    }
}
