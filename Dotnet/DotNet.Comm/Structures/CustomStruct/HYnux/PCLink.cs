using DotNet.Comm.Structures.Protocols;
using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Structures.CustomStruct.HYNux
{
    /// <summary>
    /// PCLink Protocol
    /// </summary>
    internal class PCLink : ProtocolFrame
    {
        private bool _isSUM = false;
        private bool _isTH3500 = false;
        private bool _isTD3500 = false;

        private readonly string WhoCmd = "#02#30#31#57#48#4F#0D#0A";
        private byte[] TailBytes
        {
            get
            {
                if (this._isTH3500)
                    //ETX + CR + LF
                    return new byte[] { 0x03, 0x0D, 0x0A };
                else
                    //CR + LF
                    return new byte[] { 0x0D, 0x0A };
            }
        }

        /// <summary>
        /// PCLink Protocol
        /// </summary>
        /// <param name="customCode">
        /// Custom 번호
        /// <para>
        /// 0: PCLinkSTD<br/>
        /// 1: PCLinkSUM<br/>
        /// 2: PCLinkSTD_TH300500<br/>
        /// 3: PCLinkSUM_TD300500<br/>
        /// 4: PCLinkSUM_TH300500<br/>
        /// </para>
        /// </param>
        internal PCLink(int customCode = 0)
        {
            switch (customCode)
            {
                case 0:
                    break;
                case 1:
                    this._isSUM = true;
                    break;
                case 2:
                    this._isTH3500 = true;
                    break;
                case 3:
                    this._isSUM = true;
                    this._isTD3500 = true;
                    break;
                case 4:
                    this._isSUM = true;
                    this._isTH3500 = true;
                    break;
            }
        }

        public override void DataExtract(CommData frame, byte[] buffer)
        {
            //Header 시작 위치 확인
            byte[] headerBytes;
            int startIdx,
                idxHandle = 0,
                headerLen,
                endStartIdx,
                lastIdx;

            //Header
            if (this._isTD3500)
            {
                //STX[1] + Addr[3] + ','[1] + Cmd[3]
                headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1], frame.ReqData[2], frame.ReqData[3], frame.ReqData[4] };
                headerLen = headerBytes.Length + 4;
            }
            else if(this._isTH3500)
            {
                //STX[1] + Addr[3] + Cmd[3]
                headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1], frame.ReqData[2], frame.ReqData[3] };
                headerLen = headerBytes.Length + 3;
            }
            else
            {
                //기본값: STX[1] + Addr[2] + Cmd[3]
                headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1], frame.ReqData[2] };
                headerLen = headerBytes.Length + 3;
            }

            while (idxHandle < buffer.Length - 1)
            {
                startIdx = QYUtils.Find(buffer, headerBytes, idxHandle++);
                if (startIdx < 0) continue;

                //FuncCode
                if (buffer.Length < startIdx + headerLen) continue;

                //Byte Last Index 추출
                endStartIdx = buffer.Find(this.TailBytes, startIdx + 1);
                if (endStartIdx < 0) continue;

                lastIdx = endStartIdx + this.TailBytes.Length - 1;
                if (this._isTH3500 && this._isSUM)
                {
                    //ETX[1] + CRLF[2] + ErrCode[2]
                    lastIdx += base.ErrCodeLength;
                }

                if (buffer.Length < lastIdx + 1) continue;

                //Data 추출
                byte[] frameBytes = new byte[lastIdx - startIdx + 1];
                Buffer.BlockCopy(buffer, startIdx, frameBytes, 0, frameBytes.Length);
                frame.RcvData = frameBytes;

                break;
            }
        }

        public override bool FrameConfirm(CommData frame)
        {
            int cmdStartIdx = 3;
            if (this._isTD3500) cmdStartIdx = 5;
            else if(this._isTH3500) cmdStartIdx = 4;

            if ((frame.ReqData[cmdStartIdx] == frame.RcvData[cmdStartIdx])
                && (frame.ReqData[cmdStartIdx + 1] == frame.RcvData[cmdStartIdx + 1])
                && (frame.ReqData[cmdStartIdx + 2] == frame.RcvData[cmdStartIdx + 2]))
            {
                return true;
            }

            return false;
        }
    }
}
