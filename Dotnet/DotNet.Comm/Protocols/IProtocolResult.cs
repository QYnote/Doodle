using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public enum ResultType
    {
        /// <summary>정상</summary>
        Success,
        /// <summary>CheckSum 미일치 에러</summary>
        CheckSum_Error,
        /// <summary>Protocol 전용 에러</summary>
        /// <remarks>
        /// ErrorMessage 안내 필요
        /// </remarks>
        Protocol_Exception
    }
    /// <summary>
    /// Protocol 결과 정보
    /// </summary>
    public interface IProtocolResult
    {
        /// <summary>결과 종류</summary>
        ResultType Type { get; }
        /// <summary>장애 시 메시지</summary>
        string ErrorMessage { get; }
        /// <summary>하위 데이터 Block 목록</summary>
        IEnumerable<IProtocolBlock> Blocks { get; }
    }

    /// <summary>
    /// Protocol별로 Protocol Block class까지 자동으로 정의하기 위한 상속 interface
    /// </summary>
    /// <typeparam name="TB">하위Block 형태</typeparam>
    internal interface IProtocolResult<TB> : IProtocolResult
        where TB : IProtocolBlock
    {
        /// <summary>상속 형태의 Blocks</summary>
        /// <remarks>
        /// 상위 Block를 대체 할 상속받은 TB 형태의 Block 목록
        /// </remarks>
        new List<TB> Blocks { get; }
    }

    public interface IProtocolBlock
    {
        /// <summary>수신 Raw Data</summary>
        byte[] Block { get; }
    }
}
