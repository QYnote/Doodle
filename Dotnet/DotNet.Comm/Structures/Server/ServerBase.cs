using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Server.Server
{
    internal enum ServerSendType
    {
        /// <summary>읽기에따른 Write</summary>
        ReadWrite,
        /// <summary>쓰기반복 중 가끔 Read</summary>
        WriteRead,
        /// <summary>읽기만</summary>
        ReadOnly
    }

    internal abstract class ServerBase
    {
        /// <summary>서버 열림 여부, true : 열림 / false : 닫힘</summary>
        internal bool IsOpen { get; set; }
        internal ServerSendType SendType { get; set; }

        internal ServerBase(ServerSendType type)
        {
            this.IsOpen = false;

            this.SendType = type;
        }

        /// <summary>서버 열기</summary>
        internal abstract void Open();

        /// <summary>서버 닫기</summary>
        internal abstract void Close();

        internal delegate void MsgDelegate(string msg);
        internal MsgDelegate SendMsg;
    }
}
