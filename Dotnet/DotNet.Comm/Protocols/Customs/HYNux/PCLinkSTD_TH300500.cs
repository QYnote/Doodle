using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    /// <summary>
    /// PCLink TH300, TH500 Custom
    /// </summary>
    /// <remarks>
    /// Address[2] → [3], 전용 Command, 전용 CheckSum
    /// </remarks>
    public class PCLinkSTD_TH300500 : PCLinkSTD
    {
        protected override byte[] Tail => new byte[] { 0x03, 0x0D, 0x0A };
        public override string[] Commands => new string[] {
            "WHO",
            "RCS", "RCV",
            "RSP", "RRP", "RUP", "RPD", "RSD", "RTD", "RLG",
            "WSP", "WRP", "WUP", "WPD", "WSD", "WTD", "WLG",
        };

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        public override PCLinkResult Extraction(byte[] frame, byte[] req)
        {
            string reqStr = Encoding.ASCII.GetString(req),
                   cmd = reqStr.Substring(4, 3);

            ResultType type = ResultType.Success;
            string errorMessage = string.Empty;
            List<PCLinkBlock> blocks = null;

            switch (cmd)
            {
                case "WHO": blocks = base.Get_WHO(cmd, reqStr); break;
                case "RCV": blocks = this.Get_RCV(cmd, frame); break;
                case "RRP": blocks = this.Get_RRP(cmd, frame); break;
                case "RUP": blocks = this.Get_RUP(cmd, frame); break;
                case "RTD": blocks = this.Get_RTD(cmd, frame); break;
                case "RCS":
                case "RSP":
                case "RPD":
                case "RSD":
                case "RLG":
                case "WSP":
                case "WRP":
                case "WUP":
                case "WPD":
                case "WSD":
                case "WTD":
                case "WLG":
                default:
                    if (cmd.StartsWith("NG"))
                    {
                        type = ResultType.Protocol_Exception;
                        int ngCode = this.HexToByte(Encoding.ASCII.GetString(frame).Substring(5, 2))[0];

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

        #region TH3,500 Command

        /// <summary>
        /// RRP / Read Run Parameter
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<PCLinkBlock> Get_RRP(string cmd, byte[] rcv)
        {
            //Req: STX[1] + Addr[3] + Command[3] + ,[1] + Reg.No[4]
            //Rcv: STX[1] + Addr[3] + Command[3] + ,[1] + OK[2] + ,[1] + Reg.No[4] + ,[1] + 순수 Byte[2, 34]
            byte[] addrBuffer = new byte[3],
                   regNoBuffer = new byte[4];
            Buffer.BlockCopy(rcv, 1, addrBuffer, 0, addrBuffer.Length);
            Buffer.BlockCopy(rcv, 11, regNoBuffer, 0, regNoBuffer.Length);

            int unitAddr = Convert.ToInt32(Encoding.ASCII.GetString(addrBuffer)),
                regNo = Convert.ToInt32(Encoding.ASCII.GetString(regNoBuffer));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            if (regNo == 0)
            {
                for (int i = 0; i < 17; i++)
                {
                    byte[] block = new byte[2];
                    Buffer.BlockCopy(rcv, 16 + (i * 2), block, 0, block.Length);

                    list.Add(new PCLinkBlock(unitAddr, cmd, i + 1, block));
                }
            }
            else
            {
                byte[] block = new byte[2];
                Buffer.BlockCopy(rcv, 16, block, 0, block.Length);

                list.Add(new PCLinkBlock(unitAddr, cmd, regNo, block));
            }

            return list;
        }
        /// <summary>
        /// RUP / Read User Parameter
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<PCLinkBlock> Get_RUP(string cmd, byte[] rcv)
        {
            //Req: STX[1] + Addr[3] + Command[3] + ,[1] + Reg.No[4]
            //Rcv: STX[1] + Addr[3] + Command[3] + ,[1] + OK[2] + ,[1] + Reg.No[4] + ,[1] + 순수 Byte[2, 34]
            byte[] addrBuffer = new byte[3],
                   regNoBuffer = new byte[4];
            Buffer.BlockCopy(rcv, 1, addrBuffer, 0, addrBuffer.Length);
            Buffer.BlockCopy(rcv, 11, regNoBuffer, 0, regNoBuffer.Length);

            int unitAddr = Convert.ToInt32(Encoding.ASCII.GetString(addrBuffer)),
                regNo = Convert.ToInt32(Encoding.ASCII.GetString(regNoBuffer));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            if (regNo == 0)
            {
                for (int i = 0; i < 512; i++)
                {
                    byte[] block = new byte[2];
                    Buffer.BlockCopy(rcv, 16 + (i * 2), block, 0, block.Length);

                    list.Add(new PCLinkBlock(unitAddr, cmd, i + 1, block));
                }
            }
            else
            {
                byte[] block = new byte[2];
                Buffer.BlockCopy(rcv, 16, block, 0, block.Length);

                list.Add(new PCLinkBlock(unitAddr, cmd, regNo, block));
            }

            return list;
        }
        /// <summary>
        /// RTD / Read Text Data
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<PCLinkBlock> Get_RTD(string cmd, byte[] rcv)
        {
            //Req: STX[1] + Addr[3] + Command[3] + ,[1] + Reg.No[4]
            //Rcv: STX[1] + Addr[3] + Command[3] + ,[1] + OK[2] + ,[1] + Reg.No[4] + ,[1] + 순수 Byte[29]
            string rcvStr = Encoding.ASCII.GetString(rcv);
            int unitAddr = Convert.ToInt32(rcvStr.Substring(1, 3)),
                regNo = Convert.ToInt32(rcvStr.Substring(11, 4));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            byte[] txtTemp;

            if (12 <= regNo && regNo < 111)
            {
                //PatternName은 24byte만 사용
                txtTemp = new byte[24];
                Buffer.BlockCopy(rcv, 16, txtTemp, 0, txtTemp.Length);

                list.Add(new PCLinkBlock(unitAddr, cmd, regNo, txtTemp));
            }
            else
            {
                txtTemp = new byte[29];
                Buffer.BlockCopy(rcv, 16, txtTemp, 0, txtTemp.Length);

                list.Add(new PCLinkBlock(unitAddr, cmd, regNo, txtTemp));
            }

            return list;
        }
        /// <summary>
        /// RCV / Read Current Value
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        private List<PCLinkBlock> Get_RCV(string cmd, byte[] rcv)
        {
            List<PCLinkBlock> list = new List<PCLinkBlock>();
            int idxHandle = 11,
                unitAddr = Convert.ToInt32(Encoding.ASCII.GetString(new byte[] { rcv[1], rcv[2], rcv[3] }));
            byte[] block;

            //TSV
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "TSV", block));

            //TPV
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "TPV", block));

            //TMV
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "TMV", block));

            //HSV
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "HSV", block));

            //HPV
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "HPV", block));

            //HMV
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "HMV", block));

            //T_I/S
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "T_I/S", block));

            //T/S
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "T/S", block));

            //A/S
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "A/S", block));

            //RY
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RY", block));

            //O/C
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "O/C", block));

            //D/I
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "D/I", block));

            //RM
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RM", block));

            //RTH
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RTH", block));

            //RTS
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RTS", block));

            //SRTH
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "SRTH", block));

            //SRTM
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "SRTM", block));

            //SFTH
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "SFTH", block));

            //SFTM
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "SFTM", block));

            //RPTN
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RPTN", block));

            //RSEG
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RSEG", block));

            //RPRC
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RPRC", block));

            //RPRN
            block = new byte[2];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 2;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RPRN", block));

            //RLC
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RLC", block));

            //RLN
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "RLN", block));

            //UDSW
            block = new byte[1];
            Buffer.BlockCopy(rcv, idxHandle, block, 0, block.Length);
            idxHandle += 1;
            list.Add(new PCLinkBlock(unitAddr, cmd, "UDSW", block));

            return list;
        }

        #endregion TH3,500 Command End

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Request 정보        //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        #region TH3,500 PCLink Request 생성

        /// <summary>
        /// RSP / RRP / RUP / RTD, Data Address 읽기
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="cmd">명령어 Command</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_ReadAddress(int deviceAddr, string cmd, List<int> readList)
        {
            if (readList.Count == 0) return null;
            if (cmd != "RSP" && cmd != "RRP" && cmd != "RUP" && cmd != "RTD")
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadAddress : Protocol Command Error"));

            //1. 연속 Adress목록 추출
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addr in readList)
            {
                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3}{cmd},{addr:D4}";

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }
        /// <summary>
        /// RLG / RCS / RCV, Data Command 읽기
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="cmd">명령어 Command</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_Command(int deviceAddr, string cmd, List<int> readList)
        {
            if (readList.Count == 0) return null;
            if (cmd != "RLG" && cmd != "RCS" && cmd != "RCV")
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadCommand : Protocol Command Error"));


            //1. 연속 Adress목록 추출
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addr in readList)
            {
                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3}{cmd}";

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion TH3,500 PCLink Request 생성
    }
}
