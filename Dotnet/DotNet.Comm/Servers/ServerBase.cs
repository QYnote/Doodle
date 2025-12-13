using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Servers
{
    public abstract class ServerBase
    {
        /// <summary>
        /// Client에서 Server에 요청한 Data를 처리하기위한 EventHandler
        /// </summary>
        /// <param name="request">요청된 Request Data</param>
        /// <returns>Client에게 전송할 Data</returns>
        public delegate byte[] ResponseHandler(byte[] request);
        /// <summary>
        /// Client에 주기적으로 Data를 전송하기위한 EventHandler
        /// </summary>
        /// <returns>Client에게 전송할 Data</returns>
        public delegate byte[] PeriodicHandler();
        /// <summary>
        /// Request에따른 Response 이벤트
        /// </summary>
        public event ResponseHandler CreateResponseEvent;
        /// <summary>
        /// 주기적으로 Client에 Data를 전송하는 Event
        /// </summary>
        public event PeriodicHandler PeriodicSendEvent;
        /// <summary>
        /// 서버 Log Event Handler
        /// </summary>
        /// <param name="msg">전송할 Message</param>
        public delegate void LogEventHandler(string msg);
        /// <summary>
        /// 서버에서 발생한 Log받아오는 Event
        /// </summary>
        public event LogEventHandler Log;

        /// <summary>서버 열림 여부, true : 열림 / false : 닫힘</summary>
        public bool IsOpen { get; set; } = false;

        /// <summary>서버 열기</summary>
        public abstract void Open();

        /// <summary>서버 닫기</summary>
        public abstract void Close();

        /// <summary>
        /// Log Event 실행
        /// </summary>
        /// <param name="msg">전송할 Log Text</param>
        protected void RunLog(string msg) => this.Log?.Invoke(msg);
        /// <summary>
        /// Reponse 생성 Event 실행
        /// </summary>
        /// <param name="reqFrame">수신받은 Request byte Frame</param>
        /// <returns>Event를 통해 생성된 Reponse Frame</returns>
        protected byte[] RunCreateResponse(byte[] reqFrame) => this.CreateResponseEvent?.Invoke(reqFrame);
        /// <summary>
        /// 주기적인 전송 Data 생성 Event 실행
        /// </summary>
        /// <returns>전송할 Buffer</returns>
        protected byte[] RunPeriodicSend() => this.PeriodicSendEvent?.Invoke();
    }
}
