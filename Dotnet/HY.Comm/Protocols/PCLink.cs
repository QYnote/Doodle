using Dnf.Comm.Controls.Protocols;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HY.Comm.Protocols
{
    /// <summary>
    /// PCLink Protocol
    /// </summary>
    internal class PCLink : ProtocolFrame
    {
        private bool _isSUM = false;
        private bool _isTH3500 = false;
        private bool _isTD3500 = false;

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

            //Header
            if (this._isTD3500)
                //STX[1] + Addr[3] + ','[1] + Cmd[3]
                headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1], frame.ReqData[2], frame.ReqData[3], frame.ReqData[4], frame.ReqData[5], frame.ReqData[6], frame.ReqData[7] };
            else if(this._isTH3500)
                //STX[1] + Addr[3] + Cmd[3]
                headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1], frame.ReqData[2], frame.ReqData[3], frame.ReqData[4], frame.ReqData[5], frame.ReqData[6] };
            else
                //기본값: STX[1] + Addr[2] + Cmd[3]
                headerBytes = new byte[] { frame.ReqData[0], frame.ReqData[1], frame.ReqData[2], frame.ReqData[3], frame.ReqData[4], frame.ReqData[5] };

            int startIdx = buffer.Find(headerBytes);
            if (startIdx < 0) return;


            //Byte Last Index 추출
            byte[] endBytes;
            if (this._isTH3500)
                //ETX + CR + LF
                endBytes = new byte[] { 0x03, 0x0D, 0x0A };
            else
                //CR + LF
                endBytes = new byte[] { 0x0D, 0x0A };

            int endStartIdx = buffer.Find(endBytes);
            if (endStartIdx < 0) return;

            int lastIdx = endStartIdx + endBytes.Length - 1;
            if (this._isTH3500 && this._isSUM)
            {
                //ETX[1] + CRLF[2] + ErrCode[2]
                lastIdx = lastIdx + 2;
            }

            if (buffer.Length < lastIdx + 1) return;


            //Data 추출
            byte[] frameBytes = new byte[lastIdx - startIdx + 1];
            Buffer.BlockCopy(buffer, startIdx, frameBytes, 0, frameBytes.Length);
            frame.RcvData = frameBytes;
        }
    }
}
