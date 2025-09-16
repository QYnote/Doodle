using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Server.Servers
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
        /// Request에따른 Response 이벤트
        /// </summary>
        public event ResponseHandler CreateResponseEvent;
        /// <summary>
        /// Client에 주기적으로 Data를 전송하기위한 EventHandler
        /// </summary>
        /// <returns>Client에게 전송할 Data</returns>
        public delegate byte[] PeriodicHandler();
        /// <summary>
        /// 주기적으로 Client에 Data를 전송하는 Event
        /// </summary>
        public event PeriodicHandler PeriodicSendEvent;
        public delegate void LogEventHandler(string msg);
        public event LogEventHandler Log;

        /// <summary>서버 열림 여부, true : 열림 / false : 닫힘</summary>
        public bool IsOpen { get; set; } = false;

        /// <summary>서버 열기</summary>
        public abstract void Open();

        /// <summary>서버 닫기</summary>
        public abstract void Close();

        protected void RunLog(string msg) => this.Log?.Invoke(msg);
        protected byte[] RunCreateResponse(byte[] bytes) => this.CreateResponseEvent?.Invoke(bytes);
        protected byte[] RunPeriodicSend() => this.PeriodicSendEvent?.Invoke();
    }
}
