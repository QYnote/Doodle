using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.ClientPorts
{
    /// <summary>Port 종류</summary>
    public enum PortType
    {
        Serial,
        Ethernet,
    }

    public delegate void PCPortLogHandler(string msg);

    public abstract class PCPortBase
    {
        /// <summary>
        /// PCPort Log Event
        /// </summary>
        public event PCPortLogHandler Log;
        /// <summary>Port 종류</summary>
        public PortType PortType { get; }

        /// <summary>
        /// PC Port 기본형태
        /// </summary>
        /// <param name="type">Port 종류</param>
        public PCPortBase(PortType type)
        {
            this.PortType = type;
        }

        /// <summary>
        /// Port명
        /// </summary>
        public abstract string PortName { get; set; }
        /// <summary>
        /// Port Open 상태
        /// </summary>
        public abstract bool IsOpen { get; }
        /// <summary>
        /// Port 열기
        /// </summary>
        /// <returns>true: 열기성공 / false: 열기실패</returns>
        public abstract bool Open();
        /// <summary>
        /// Port 닫기
        /// </summary>
        /// <returns>true: 닫기성공 / false: 닫기실패</returns>
        public abstract bool Close();
        /// <summary>
        /// Data 읽기
        /// </summary>
        /// <returns>PC Port에 담겨있던 Buffer</returns>
        public abstract byte[] Read();
        /// <summary>
        /// Data 쓰기
        /// </summary>
        /// <param name="bytes">전송할 Byte Array</param>
        public abstract void Write(byte[] bytes);
        public abstract void InitPort();
        /// <summary>
        /// Log Event 실행
        /// </summary>
        /// <param name="msg">진행할 Log Message</param>
        /// <remarks>
        /// 상속 Class에서 Log 실행용
        /// </remarks>
        protected void LogRun(string msg)
        {
            this.Log?.Invoke(msg);
        }
    }
}
