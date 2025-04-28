using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetFrameWork.Communication
{
    public class CommFrame
    {
        /// <summary>
        /// Request(송신) Data
        /// </summary>
        public byte[] ReqData { get; set; }
        /// <summary>
        /// Request(송신) 시간
        /// </summary>
        public DateTime ReqTime { get; set; }
        /// <summary>
        /// 우선전송 Data 인지 확인
        /// </summary>
        public bool IsFrontReq { get; set; }
        /// <summary>
        /// 최대 Request 시도 횟수
        /// </summary>
        public int MaxReqTryCount { get; }
        /// <summary>
        /// 최대 Request 시도 횟수 기본값
        /// </summary>
        private readonly int Default_MaxReqTryCount = 3;
        /// <summary>
        /// Request 시도 횟수
        /// </summary>
        public int ReqTryCount { get; set; }
        /// <summary>
        /// Reponse(수신) Data
        /// </summary>
        public byte[] ResData { get; set; }
        /// <summary>
        /// Response(수신) 시간
        /// </summary>
        public DateTime ResTime { get; set; }

        /// <summary>
        /// 송/수신 틀
        /// </summary>
        public CommFrame()
        {
            this.MaxReqTryCount = this.Default_MaxReqTryCount;
            this.ReqTryCount = 0;
            this.IsFrontReq = false;
        }
    }
}
