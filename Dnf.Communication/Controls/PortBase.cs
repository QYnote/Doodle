using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    internal abstract class PortBase
    {
        /// <summary>
        /// Port 연결 상태
        /// </summary>
        internal abstract bool IsOpen { get; }
        /// <summary>
        /// Port 열기
        /// </summary>
        /// <returns>true : 정상 열림 / false : 열기 실패</returns>
        internal abstract bool Open();
        /// <summary>
        /// Port 닫기
        /// </summary>
        /// <returns>true : 정상 닫기 / false : 닫기 실패</returns>
        internal abstract bool Close();
        /// <summary>
        /// Port Data 읽어서 PortClass의 ReadingData에 쌓기
        /// </summary>
        /// <param name="buffer">담아갈 byte Array</param>
        internal abstract void Read(ref byte[] buffer);
        /// <summary>
        /// Port Data 전송
        /// </summary>
        /// <param name="bytes">전송할 Data byte Array</param>
        internal abstract void Write(byte[] bytes);
    }
}
