using DotNet.Comm.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Protocol
{
    public enum ProtocolType
    {
        None,
        ModbusRTU,
        ModbusAscii,
        HY_ModbusRTU_EXP,
        HY_ModbusAscii_EXP,
        HY_ModbusTCP,
        HY_PCLinkSTD,
        HY_PCLinkSTD_TH300500,
        HY_PCLinkSUM,
        HY_PCLinkSUM_TD300500,
        HY_PCLinkSUM_TH300500,
    }

    internal interface IProtocol
    {
        /// <summary>수신된 Buffer에서 Protocol Frame규격 추출</summary>
        /// <param name="buffer">수신된 Bufer</param>
        /// <returns>Protocol Frame</returns>
        byte[] Parse(byte[] buffer, byte[] req);
        /// <summary>
        /// Protocol 규격에 의한 내부 결과값 추출
        /// </summary>
        /// <param name="frame">Protocol Frame</param>
        /// <returns>Protocol 결과</returns>
        IProtocolResult Extraction(byte[] frame, byte[] req);
        /// <summary>
        /// CheckSum Error 검사
        /// </summary>
        /// <param name="frame">검사할 Frame</param>
        /// <returns>true: 에러 / false: 정상</returns>
        bool CheckError(byte[] frame);
        /// <summary>
        /// CheckSum 생성
        /// </summary>
        /// <param name="frame">생성할 Frame</param>
        /// <returns>생성된 CheckSum</returns>
        byte[] CreateCheckSum(byte[] frame);
    }
}
