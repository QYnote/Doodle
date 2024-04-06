using Dnf.Communication.Data;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    internal abstract class Port
    {
        /// <summary>
        /// 포트 연결 상태
        /// </summary>
        internal ConnectionState State;
        /// <summary>
        /// 통신 방법
        /// </summary>
        internal uProtocolType ProtocolType;
        /// <summary>
        /// Port명, RunTimeData의 Port Dictionary 이름 구분용으로 사용
        /// </summary>
        internal string PortName;
        /// <summary>
        /// Port에 연결된 하위 Unit들(ex. PLC, 센서 등), <slaveAddr, Unit>
        /// </summary>
        internal Dictionary<int, Unit> Units;

        internal string DebugStr = "";

        /// <summary>
        /// 연결된 Port 열기
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        internal abstract bool Open();
        /// <summary>
        /// 연결된 Port 닫기
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        internal abstract bool Close();
        /// <summary>
        /// Port Data 전송
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        internal abstract bool Send();
    }
}