using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Server.Server
{
    internal abstract class ServerBase
    {
        /// <summary>서버 열림 여부, true : 열림 / false : 닫힘</summary>
        internal bool IsOpen {  get; set; }

        /// <summary>서버 열기</summary>
        internal abstract void Open();

        /// <summary>서버 닫기</summary>
        internal abstract void Close();
    }
}
