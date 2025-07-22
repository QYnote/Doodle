using DotNet.Comm.Structures.Protocols;
using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CustomComm.HYNux
{
    /// <summary>
    /// PCLink Protocol
    /// </summary>
    public class PCLink : ProtocolFrame
    {
        public bool _isSUM { get; set; } = false;
        public bool _isTH3500 { get; set; } = false;
        public bool _isTD3500 { get; set; } = false;

        private readonly string WhoCmd = "#02#30#31#57#48#4F#0D#0A";
        public byte[] TailBytes
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
        public byte[] HeadBytes
        {
            get
            {
                //STX
                return new byte[] { 0x02 };
            }
        }
        private List<string> CmdList
        {
            get
            {
                List<string> cmdList = new List<string>() { "WHO" };
                if (this._isTD3500)
                {
                    cmdList.AddRange(new string[]
                    {
                        "WHO",
                        "RCS", "RPI",
                        "RLG", "RDR", "RRD", "RPD", "RSD",
                        "WLG", "WDR", "WRD", "WPD", "WSD",
                    });
                }
                else if (this._isTH3500)
                {
                    cmdList.AddRange(new string[]
                    {
                        "WHO",
                        "RCS", "RCV",
                        "RSP", "RRP", "RUP", "RPD", "RSD", "RTD", "RLG",
                        "WSP", "WRP", "WUP", "WPD", "WSD", "WTD", "WLG",
                    });
                }
                else
                {
                    cmdList.AddRange(new string[]
                    {
                        "WHO",
                        "DWS", "DWR", "IWS", "IWR",
                        "DRS", "DRR", "IRS", "IRR",
                        "DMS", "DMC", "IMS", "IMC",
                    });
                }

                return cmdList;
            }
        }

        /// <summary>
        /// PCLink Protocol
        /// </summary>
        /// <param name="isClient">
        /// Request 요청자 여부
        /// </param>
        public PCLink(bool isClient) : base(isClient) { }

        public override byte[] DataExtract_Receive(byte[] reqBytes, byte[] buffer)
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
                headerBytes = new byte[] { reqBytes[0], reqBytes[1], reqBytes[2], reqBytes[3], reqBytes[4] };
                headerLen = headerBytes.Length + 4;
            }
            else if (this._isTH3500)
            {
                //STX[1] + Addr[3] + Cmd[3]
                headerBytes = new byte[] { reqBytes[0], reqBytes[1], reqBytes[2], reqBytes[3] };
                headerLen = headerBytes.Length + 3;
            }
            else
            {
                //기본값: STX[1] + Addr[2] + Cmd[3]
                headerBytes = new byte[] { reqBytes[0], reqBytes[1], reqBytes[2] };
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

                return frameBytes;
            }

            return null;
        }
        public override byte[] DataExtract_Request(byte addr, byte[] buffer)
        {
            //Header 시작 위치 확인
            byte[] headerBytes;
            int startIdx,
                idxHandle = 0,
                headerLen,
                endStartIdx,
                lastIdx;

            //Header
            byte[] bytes;

            if (this._isTD3500)
            {
                bytes = Encoding.ASCII.GetBytes(addr.ToString("D3"));
                //STX[1] + Addr[3] + ','[1] + Cmd[3]
                headerBytes = new byte[] { 0x02, bytes[0], bytes[1], bytes[2] };
                headerLen = headerBytes.Length + 4;
            }
            else if(this._isTH3500)
            {
                bytes = Encoding.ASCII.GetBytes(addr.ToString("D3"));
                //STX[1] + Addr[3] + Cmd[3]
                headerBytes = new byte[] { 0x02, bytes[0], bytes[1], bytes[2] };
                headerLen = headerBytes.Length + 3;
            }
            else
            {
                bytes = Encoding.ASCII.GetBytes(addr.ToString("D2"));
                //기본값: STX[1] + Addr[2] + Cmd[3]
                headerBytes = new byte[] { 0x02, bytes[0], bytes[1] };
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
                return frameBytes;
            }

            return null;
        }
        public override bool ReceiveConfirm(byte[] rcvBytes)
        {
            int cmdStartIdx;
            byte[] bCmd = new byte[3];
            string cmd;
            if (this._isTD3500)
                cmdStartIdx = 5;
            else if(this._isTH3500)
                cmdStartIdx = 4;
            else
                cmdStartIdx = 3;
            //Cmd 추출
            Buffer.BlockCopy(rcvBytes, cmdStartIdx, bCmd, 0, bCmd.Length);
            cmd = Encoding.ASCII.GetString(bCmd);

            if (cmd.StartsWith("NG"))
            {
                Debug.WriteLine(string.Format("[Error]Request Error - Error Code:{0}",
                    Encoding.ASCII.GetString(new byte[] { rcvBytes[cmdStartIdx + 2], rcvBytes[cmdStartIdx + 3] })
                    ));
                return false;
            }
            else
            {
                return true;
            }
        }
        public override void GetData(Dictionary<int, object> dic, byte[] reqBytes, byte[] rcvBytes)
        {
            if (dic == null
                || reqBytes == null || rcvBytes == null)
                throw new Exception("Method Parameter Null Error");

            byte[] cmdByte = new byte[3];
            if (this._isTD3500)
                //STX[1] + Addr[3] + ','[1] + Cmd[3]
                Buffer.BlockCopy(reqBytes, 5, cmdByte, 0, 3);
            else if (this._isTH3500)
                //STX[1] + Addr[3] + Cmd[3]
                Buffer.BlockCopy(reqBytes, 4, cmdByte, 0, 3);
            else
                //기본값: STX[1] + Addr[2] + Cmd[3]
                Buffer.BlockCopy(reqBytes, 3, cmdByte, 0, 3);

            string cmd = Encoding.ASCII.GetString(cmdByte);

            if (this._isTH3500)
            {
                switch (cmd)
                {
                    case "WHO": break;
                    case "RCS": break;
                    case "RCV": break;
                    case "RSP": break;
                    case "RRP": break;
                    case "RUP": break;
                    case "RPD": break;
                    case "RSD": break;
                    case "RTD": break;
                    case "RLG": break;
                    case "WSP": break;
                    case "WRP": break;
                    case "WUP": break;
                    case "WPD": break;
                    case "WSD": break;
                    case "WTD": break;
                    case "WLG": break;
                }
            }
            else if (this._isTD3500)
            {
                switch (cmd)
                {
                    case "WHO": break;
                    case "RCS": break;
                    case "RPI": break;
                    case "RLG": break;
                    case "RDR": break;
                    case "RRD": break;
                    case "RPD": break;
                    case "RSD": break;
                    case "WLG": break;
                    case "WDR": break;
                    case "WRD": break;
                    case "WPD": break;
                    case "WSD": break;
                }
            }
            else
            {
                switch (cmd)
                {
                    case "WHO": break;
                    case "DRS": break;
                    case "DRR": break;
                    case "DWS": break;
                    case "DWR": break;
                    case "DMS": break;
                    case "DMC": break;
                    case "IRS": break;
                    case "IRR": break;
                    case "IWS": break;
                    case "IWR": break;
                    case "IMS": break;
                    case "IMC": break;
                }
            }
        }
        #region Command Get Process

        #endregion Command Get Process

        public override byte[] CreateResponse(Dictionary<int, object> dic, byte[] reqBytes)
        {
            if (dic == null
                || reqBytes == null)
                throw new Exception("Method Parameter Null Error");

            byte[] cmdByte = new byte[3];
            if (this._isTD3500)
                //STX[1] + Addr[3] + ','[1] + Cmd[3]
                Buffer.BlockCopy(reqBytes, 5, cmdByte, 0, 3);
            else if (this._isTH3500)
                //STX[1] + Addr[3] + Cmd[3]
                Buffer.BlockCopy(reqBytes, 4, cmdByte, 0, 3);
            else
                //기본값: STX[1] + Addr[2] + Cmd[3]
                Buffer.BlockCopy(reqBytes, 3, cmdByte, 0, 3);

            string cmd = Encoding.ASCII.GetString(cmdByte);
            byte[] cmdBytes = null;

            if (this._isTH3500)
            {
                switch (cmd)
                {
                    case "WHO": cmdBytes = null; break;
                    case "RCS": cmdBytes = null; break;
                    case "RCV": cmdBytes = null; break;
                    case "RSP": cmdBytes = null; break;
                    case "RRP": cmdBytes = null; break;
                    case "RUP": cmdBytes = null; break;
                    case "RPD": cmdBytes = null; break;
                    case "RSD": cmdBytes = null; break;
                    case "RTD": cmdBytes = null; break;
                    case "RLG": cmdBytes = null; break;
                    case "WSP": cmdBytes = null; break;
                    case "WRP": cmdBytes = null; break;
                    case "WUP": cmdBytes = null; break;
                    case "WPD": cmdBytes = null; break;
                    case "WSD": cmdBytes = null; break;
                    case "WTD": cmdBytes = null; break;
                    case "WLG": cmdBytes = null; break;
                }
            }
            else if (this._isTD3500)
            {
                switch (cmd)
                {
                    case "WHO": cmdBytes = null; break;
                    case "RCS": cmdBytes = null; break;
                    case "RPI": cmdBytes = null; break;
                    case "RLG": cmdBytes = null; break;
                    case "RDR": cmdBytes = null; break;
                    case "RRD": cmdBytes = null; break;
                    case "RPD": cmdBytes = null; break;
                    case "RSD": cmdBytes = null; break;
                    case "WLG": cmdBytes = null; break;
                    case "WDR": cmdBytes = null; break;
                    case "WRD": cmdBytes = null; break;
                    case "WPD": cmdBytes = null; break;
                    case "WSD": cmdBytes = null; break;
                }
            }
            else
            {
                switch (cmd)
                {
                    case "WHO": cmdBytes = null; break;
                    case "DRS": cmdBytes = null; break;
                    case "DRR": cmdBytes = null; break;
                    case "DWS": cmdBytes = null; break;
                    case "DWR": cmdBytes = null; break;
                    case "DMS": cmdBytes = null; break;
                    case "DMC": cmdBytes = null; break;
                    case "IRS": cmdBytes = null; break;
                    case "IRR": cmdBytes = null; break;
                    case "IWS": cmdBytes = null; break;
                    case "IWR": cmdBytes = null; break;
                    case "IMS": cmdBytes = null; break;
                    case "IMC": cmdBytes = null; break;
                }
            }

            return cmdBytes;
        }
        #region Create Response

        #endregion Create Response
    }
}
