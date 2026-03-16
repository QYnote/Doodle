using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    /// <summary>
    /// PCLink TD300, TD500 Custom
    /// </summary>
    /// <remarks>
    /// Address[2] → [3], Unit Address 뒤에 ',(쉼표)' 추가, 전용 Command
    /// </remarks>
    public class PCLinkSUM_TD300500 :PCLinkSUM
    {
        public override string[] Commands => new string[]
        {
            "WHO",
            "RCS", "RPI",
            "RLG", "RDR", "RRD", "RPD", "RSD",
            "WLG", "WDR", "WRD", "WPD", "WSD",
        };

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        public override PCLinkResult Extraction(byte[] frame, byte[] req)
        {
            string reqStr = Encoding.ASCII.GetString(req),
                   rcvStr = Encoding.ASCII.GetString(frame),
                   cmd = rcvStr.Substring(5, 3);

            ResultType type = ResultType.Success;
            string errorMessage = string.Empty;
            List<PCLinkBlock> blocks = null;

            switch (cmd)
            {
                case "WHO": blocks = base.Get_WHO(cmd, rcvStr); break;
                case "RDR": blocks = this.Get_RDR(cmd, rcvStr, reqStr); break;
                case "RRD": blocks = this.Get_RRD(cmd, rcvStr, reqStr); break;
                case "WDR": blocks = this.Get_WDR(cmd, reqStr); break;
                case "WRD": blocks = this.Get_WRD(cmd, reqStr); break;
                case "RCS":
                case "RPI":
                case "RLG":
                case "RPD":
                case "RSD":
                case "WLG":
                case "WPD":
                case "WSD":
                default:
                    if (cmd.StartsWith("NG"))
                    {
                        type = ResultType.Protocol_Exception;
                        int ngCode = this.HexToByte(rcvStr.Substring(5, 2))[0];

                        switch (ngCode)
                        {
                            case 0x01: errorMessage = "Unsupport command"; break;
                            case 0x02: errorMessage = "Empty register"; break;
                            case 0x03: errorMessage = "Register range out"; break;
                            case 0x04: errorMessage = "Data format error"; break;
                            case 0x08: errorMessage = "Command format error"; break;
                            case 0x16: errorMessage = "Checksum error"; break;
                            default: errorMessage = "Unknown error"; break;
                        }
                    }
                    else
                    {
                        type = ResultType.Protocol_Exception;
                        errorMessage = "Undevelop Command";
                    }
                    break;
            }

            if (blocks != null && blocks.Count == 0) blocks = null;
            return new PCLinkResult(type, errorMessage, blocks);
        }

        #region TD3,500 Command

        /// <summary>
        /// RDR / Word Register Read, 연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<PCLinkBlock> Get_RDR(string cmd, string rcv, string req)
        {
            //Req: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + Count[2] + ,[1] + Start.Reg[4]
            //Rcv: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + OK[2] + { ,[1] + HexString[4] } * Count
            int unitAddr = Convert.ToInt32(rcv.Substring(1, 3)),
                readCount = Convert.ToInt32(req.Substring(9, 2)),
                startAddr = Convert.ToInt32(req.Substring(12, 4));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                byte[] block = this.HexToByte(rcv.Substring(12 + (i * 5), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, startAddr + i, block));
            }

            return list;
        }
        /// <summary>
        /// RRD / Word Register Read, 비연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        private List<PCLinkBlock> Get_RRD(string cmd, string rcv, string req)
        {
            //Req: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + Count[2] + { ,[1] + RegAddr[4] } * Count
            //Rcv: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + OK[2] + { ,[1] + HexString[4] } * Count
            int unitAddr = Convert.ToInt32(rcv.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                int reg = Convert.ToInt32(req.Substring(12 + (i * 5), 4));
                byte[] block = this.HexToByte(rcv.Substring(12 + (i * 5), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, reg, block));
            }

            return list;
        }
        /// <summary>
        /// WDR / Word Register Write, 연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<PCLinkBlock> Get_WDR(string cmd, string req)
        {
            //Req: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + Count[2] + ,[1] + Start.Reg[4] + { ,[1] + HexString[4] } * Count
            //Rcv: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + OK[2]
            int unitAddr = Convert.ToInt32(req.Substring(1, 3)),
                readCount = Convert.ToInt32(req.Substring(9, 2)),
                startAddr = Convert.ToInt32(req.Substring(12, 4));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                byte[] block = this.HexToByte(req.Substring(17 + (i * 5), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, startAddr + i, block));
            }

            return list;
        }
        /// <summary>
        /// WRD / Word Register Write, 비연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        private List<PCLinkBlock> Get_WRD(string cmd, string req)
        {
            //Req: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + Count[2] + { ,[1] + RegAddr[4] + ,[1] + HexString[4] } * Count
            //Rcv: STX[1] + Addr[3] + ,[1] + Command[3] + ,[1] + OK[2]
            int unitAddr = Convert.ToInt32(req.Substring(1, 3)),
                readCount = Convert.ToInt32(req.Substring(9, 2));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                int reg = Convert.ToInt32(req.Substring(12 + (i * 5), 4));
                byte[] block = this.HexToByte(req.Substring(17 + (i * 5), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, reg, block));
            }

            return list;
        }

        #endregion TD3,500 Command End

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Request 정보        //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        #region TD3,500 PCLink Request 생성

        /// <summary>
        /// RDR 연속 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_RDR(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 64);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3},RDR,{addrAry.Length},{addrAry[0]}";

                //3. Frame 생성
                byte[] frame = base.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        /// <summary>
        /// RRD 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_RRD(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 64);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3},RRD,{addrAry.Length}";
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);
                bodyStr += ",";

                //3. Frame 생성
                byte[] frame = base.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion TD3,500 PCLink Request 생성
    }
}
