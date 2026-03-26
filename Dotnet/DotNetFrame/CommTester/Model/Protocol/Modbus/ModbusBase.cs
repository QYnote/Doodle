using DotNetFrame.CommTester.Model;
using DotNetFrame.CommTester.Model.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol.Modbus
{
    internal abstract class ModbusBase : IProtocol
    {
        protected CommPort Handler { get; }
        protected byte[] Request { get; set; }

        internal ModbusBase(CommPort handler)
        {
            this.Handler = handler;
        }

        public abstract byte[] Parse(byte[] buffer);

        public abstract IProtocolResult Extraction(byte[] frame);

        public abstract byte[] CreateCheckSum(byte[] frame);

        public abstract void Initialize();
    }
}
