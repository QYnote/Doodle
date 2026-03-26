using DotNetFrame.CommTester.Model;
using DotNetFrame.CommTester.Model.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol.Custom.HYNux
{
    internal abstract class PCLinkBase : IProtocol
    {
        protected CommPort Handler { get; }
        protected byte[] Request { get; set; }

        internal PCLinkBase(CommPort handler)
        {
            this.Handler = handler;
        }

        public abstract byte[] Parse(byte[] buffer);

        public abstract IProtocolResult Extraction(byte[] frame);

        public abstract byte[] CreateCheckSum(byte[] frame);

        public abstract void Initialize();
    }
}
