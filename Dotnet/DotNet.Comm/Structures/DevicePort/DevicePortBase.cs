using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls.DevicePort
{
    /// <summary>
    /// PC - Device 통신 역할 Port
    /// </summary>
    public abstract class DevicePortBase
    {
        /// <summary>
        /// Port명
        /// </summary>
        public abstract string PortName { get; set; }
        /// <summary>
        /// Port 연결 상태
        /// </summary>
        public abstract bool IsOpen { get; }
        /// <summary>
        /// 포트 열기
        /// </summary>
        /// <returns>true: 정상 열림 / false: 열기 실패</returns>
        public abstract bool Open();
        /// <summary>
        /// 포트 닫기
        /// </summary>
        /// <returns>true: 정상 닫기 / 닫기 실패</returns>
        public abstract bool Close();
        /// <summary>
        /// Data 읽기
        /// </summary>
        /// <returns>Port에서 읽은 byte Array</returns>
        public abstract byte[] Read();
        /// <summary>
        /// Data 쓰기
        /// </summary>
        /// <param name="data">전송할 Byte Array</param>
        /// <returns>전송 성공 여부</returns>
        public abstract bool Write(byte[] data);
    }
}
