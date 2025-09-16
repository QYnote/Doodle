using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Server.Servers
{
    public enum ServerSendType
    {
        /// <summary>읽기에따른 Write</summary>
        ReadWrite,
        /// <summary>쓰기반복 중 가끔 Read</summary>
        WriteRead,
        /// <summary>읽기만</summary>
        ReadOnly
    }

    public delegate void LogEventHandler(string msg);

    public abstract class ServerBase
    {
        public event LogEventHandler Log;

        /// <summary>서버 열림 여부, true : 열림 / false : 닫힘</summary>
        public bool IsOpen { get; set; }
        public ServerSendType SendType { get; set; }

        internal ServerBase(ServerSendType type)
        {
            this.IsOpen = false;

            this.SendType = type;
        }

        /// <summary>서버 열기</summary>
        public abstract void Open();

        /// <summary>서버 닫기</summary>
        public abstract void Close();

        protected void RunLog(string msg)
        {
            this.Log?.Invoke(msg);
        }
    }
}
