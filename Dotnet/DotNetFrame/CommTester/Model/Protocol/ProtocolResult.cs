using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol
{
    internal enum ResultType
    {
        /// <summary>정상</summary>
        Success,
        /// <summary>수신시간 초과</summary>
        Timeout,
        /// <summary>CheckSum 미일치 에러</summary>
        CheckSum_Error,
        /// <summary>Protocol 전용 에러</summary>
        /// <remarks>
        /// ErrorMessage 안내 필요
        /// </remarks>
        Protocol_Exception
    }

    internal interface IProtocolResult
    {
        ResultType Type { get; }
        string ErrorMessage { get; }
        IEnumerable<ProtocolItem> Items { get; }
        byte[] Request { get; }
        byte[] Response { get; }
    }

    internal class ProtocolResult<T> : IProtocolResult
        where T : ProtocolItem
    {
        public ResultType Type { get; set; }
        public string ErrorMessage { get; set; }
        public byte[] Request { get; }
        public byte[] Response { get; set; }

        public List<T> Items { get; } = new List<T>();
        IEnumerable<ProtocolItem> IProtocolResult.Items => Items;

        internal ProtocolResult(byte[] request)
        {
            this.Request = request;
        }
    }
}
