using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Data
{
    internal enum FrmEditType
    {
        New,    //신규
        Edit    //수정
    }

    /// <summary>
    /// 통신방법 종류
    /// </summary>
    internal enum uProtocolType
    {
        ModBusRTU,
        ModBusAscii,
        ModBusTcpIp
    }

    internal enum PortConnectionState
    { 
        /// <summary>Port 닫음</summary>
        Close,
        /// <summary>Port 염</summary>
        Open
    }


    internal enum UnitConnectionState
    {
        /// <summary>Port닫힌 미연결</summary>
        Close_DisConnect,
        /// <summary>Port열린 미연결</summary>
        Open_DisConnect,
        /// <summary>연결</summary>
        Connect,
        /// <summary>연결중</summary>
        Initializing
    }

    internal enum BaudRate
    {
        _9600,
        _14400
    }

    /// <summary>
    /// Channel Value 종류
    /// </summary>
    internal enum ChValueType
    {
        CV, //현재값
        LV, //하한값
        HV  //상한값
    }
}
