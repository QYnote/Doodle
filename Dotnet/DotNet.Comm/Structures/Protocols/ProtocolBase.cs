using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Structures.Protocols
{
    public class CommData
    {
        public byte[] ReqData { get; set; }
        public byte[] RcvData { get; set; }
        public DateTime SendingTime { get; set; }
    }

    public abstract class ProtocolFrame
    {
        public int ErrCodeLength { get; set; } = 2;

        protected bool IsClient { get; }
        protected ProtocolFrame(bool isClient)
        {
            this.IsClient = isClient;
        }

        /// <summary>
        /// Receive된 Data 추출
        /// </summary>
        /// <param name="reqBytes">담아갈 Byte</param>
        /// <param name="buffer">해석할 Buffer</param>
        /// <remarks>
        /// Data 적합성 여부 상관 없이 Data 길이나 Tail위치만 찾아 Receive Frame을 추출
        /// </remarks>
        public abstract byte[] DataExtract_Receive(byte[] reqBytes, byte[] buffer);
        /// <summary>
        /// Request된 Data 추출
        /// </summary>
        /// <param name="addr">Request 받은 Address</param>
        /// <param name="buffer">해석할 Buffer</param>
        /// <remarks>
        /// Data 적합성 여부 상관 없이 Data 길이나 Tail위치만 찾아 Request Frame을 추출
        /// </remarks>
        public abstract byte[] DataExtract_Request(byte addr, byte[] buffer);
        /// <summary>
        /// Receive Data 결과 추출
        /// </summary>
        /// <param name="rcvBytes">검사할 Receive Data</param>
        /// <returns>true: 정상 / false: NG</returns>
        public abstract bool ReceiveConfirm(byte[] rcvBytes);
        /// <summary>
        /// Receive Data에서 Data 입력처리
        /// </summary>
        /// <remarks>
        /// Receive된 Frame에서 각 Address의 Data를 추출하여 Dictionary에 입력
        /// </remarks>
        /// <param name="dic">입력할 Dictionary</param>
        /// <param name="reqBytes">Request Data</param>
        /// <param name="rcvBytes">Receive Data</param>
        public abstract void GetData(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes);
        public abstract byte[] CreateResponse(Dictionary<int, object> dic, byte[] reqBytes);
    }
}
