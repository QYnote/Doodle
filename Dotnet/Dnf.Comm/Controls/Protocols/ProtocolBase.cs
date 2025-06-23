using Dnf.Communication.Controls.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Comm.Controls.Protocols
{
    internal abstract class ProtocolBase
    {
        internal abstract void DataExtract(CommFrame frame, byte[] buffer);
    }


    public class CommData
    {
        public byte[] ReqData { get; set; }
        public byte[] RcvData { get; set; }
        public DateTime SendingTime { get; set; }
    }

    public abstract class ProtocolFrame
    {
        public int ErrCodeLength { get; set; } = 2;

        /// <summary>
        /// Data 추출
        /// </summary>
        /// <param name="frame">담아갈 Struct</param>
        /// <param name="buffer">해석할 Buffer</param>
        /// <remarks>
        /// Data 적합성 여부 상관 없이 Data 길이나 Tail위치만 찾아 Response Frame을 추출
        /// </remarks>
        public abstract void DataExtract(CommData frame, byte[] buffer);
        /// <summary>
        /// 추출된 Data Protocol검사
        /// </summary>
        /// <param name="frame">검사할 통신 Data</param>
        /// <returns>true: 정상 / false: NG</returns>
        public abstract bool FrameConfirm(CommData frame);
    }
}
