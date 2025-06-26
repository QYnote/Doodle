using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotnet.Comm.Structures.PCPorts
{
    public enum PortType
    {
        Serial,
        Ethernet,
    }

    public abstract class PCPortBase
    {
        public PortType PortType { get; }

        public PCPortBase(PortType type)
        {
            this.PortType = type;
        }

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
    }
}
