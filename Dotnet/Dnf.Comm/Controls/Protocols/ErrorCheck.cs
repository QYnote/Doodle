using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls.Protocols
{
    public abstract class ErrorCheck
    {
        public byte CheckLen { get; set; } = 2;

        /// <summary>
        /// Data 검사
        /// </summary>
        /// <param name="bytes">검사할 Data</param>
        /// <returns>true: 미일치 / false: 일치</returns>
        public abstract bool FrameConfirm(byte[] bytes);
        /// <summary>
        /// Error Check Byte 생성
        /// </summary>
        /// <param name="bytes">검사할 Data</param>
        /// <returns>생성된 Error Check Bytes</returns>
        public abstract byte[] CreateCheckBytes(byte[] bytes);
    }
}
