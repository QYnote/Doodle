using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Comm.Controls.Protocols
{
    internal class ModbusRTU : ProtocolBase
    {
        private byte[] FrameBytes { get; set; }

        #region Read
        
        /// <summary>
        /// Data 읽기 처리할 byte Array 추출
        /// </summary>
        /// <param name="frame">담아갈 Frame struct</param>
        /// <param name="buffer">읽은 Buffer</param>
        internal override void DataExtract(CommFrame frame, byte[] buffer)
        {
            switch (frame.ReqDataBytes[1])
            {
                case 0x02:
                case 0x03:
                case 0x04:
                    Extract_Read(frame, buffer);
                    break;
                case 0x06:
                case 0x10:  //16
                    Extract_Write(frame, buffer);
                    break;
            }
        }

        /// <summary>
        /// 읽기 Array 추출
        /// <para>Function Code : 02,03,04</para>
        /// </summary>
        /// <param name="frame">담아갈 Frame struct</param>
        /// <param name="buffer">읽은 Buffer</param>
        private void Extract_Read(CommFrame frame, byte[] buffer)
        {
            //Header 시작위치 확인
            int startIdx = Array.IndexOf(buffer, new byte[] { frame.ReqDataBytes[0], frame.ReqDataBytes[1] });

            if (startIdx < 0) return;

            //Addr[1] + Func[1] + ByteCount[1] + Byte[ByteCount]
            int byteCount = buffer[startIdx + 2];
            int lastIdx = startIdx + 2 + byteCount;

            int dataFullCount = lastIdx - startIdx + 1; //옮길 Data 총 수

            //Data 추출
            this.FrameBytes = new byte[dataFullCount];
            Buffer.BlockCopy(buffer, startIdx, this.FrameBytes, 0, this.FrameBytes.Length);

            frame.ReadEndTimeTick = TimeSpan.FromTicks(DateTime.Now.Ticks);
            frame.RcvDataBytes = this.FrameBytes;

            //남은 Data 땡기기
            int remainLength = buffer.Length - lastIdx - 1;
            if (remainLength > 0)
            {
                byte[] remainBytes = new byte[remainLength];
                Buffer.BlockCopy(buffer, lastIdx + 1, remainBytes, 0, remainBytes.Length);

                frame.RemainBytes = remainBytes;
            }

            this.FrameBytes = null;
        }

        /// <summary>
        /// 쓰기 Array 추출
        /// <para>Function Code : 02,03,04</para>
        /// </summary>
        /// <param name="frame">담아갈 Frame struct</param>
        /// <param name="buffer">읽은 Buffer</param>
        private void Extract_Write(CommFrame frame, byte[] buffer)
        {
            //Header 시작위치 확인
            int startIdx = Array.IndexOf(buffer, new byte[] { frame.ReqDataBytes[0], frame.ReqDataBytes[1] });

            if (startIdx < 0) return;

            //Addr[1] + Func[1] + Start Address[2] + WriteCount[2]
            int lastIdx = startIdx + 5;

            //Data 추출
            this.FrameBytes = new byte[6];
            Buffer.BlockCopy(buffer, startIdx, this.FrameBytes, 0, this.FrameBytes.Length);

            frame.ReadEndTimeTick = TimeSpan.FromTicks(DateTime.Now.Ticks);
            frame.RcvDataBytes = this.FrameBytes;

            //남은 Data 땡기기
            int remainLength = buffer.Length - lastIdx - 1;
            if (remainLength > 0)
            {
                byte[] remainBytes = new byte[remainLength];
                Buffer.BlockCopy(buffer, lastIdx + 1, remainBytes, 0, remainBytes.Length);

                frame.RemainBytes = remainBytes;
            }

            this.FrameBytes = null;
        }

        #endregion Read
    }
}
