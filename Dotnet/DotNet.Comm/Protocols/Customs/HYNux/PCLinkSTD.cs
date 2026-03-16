using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class PCLinkResult : IProtocolResult<PCLinkBlock>
    {
        public ResultType Type { get; }
        public string ErrorMessage { get; }
        public List<PCLinkBlock> Blocks { get; }
        IEnumerable<IProtocolBlock> IProtocolResult.Blocks => this.Blocks;

        public PCLinkResult(ResultType type, string errorMessage, List<PCLinkBlock> blocks)
        {
            this.Type = type;
            this.ErrorMessage = errorMessage;
            this.Blocks = blocks;
        }
    }
    public class PCLinkBlock : IProtocolBlock
    {
        /// <summary>통신 Device Addres</summary>
        public int UnitAddress { get; }
        /// <summary>명령어</summary>
        public string Command { get; }
        /// <summary>수신 Registry 대상</summary>
        public object Target { get; }
        public byte[] Block { get; }

        public PCLinkBlock(int unitAddress, string command, object target, byte[] block)
        {
            this.UnitAddress = unitAddress;
            this.Command = command;
            this.Target = target;
            this.Block = block;
        }
    }

    public class PCLinkSTD
    {
        protected virtual byte[] Tail => new byte[] { 0x0D, 0x0A };
        public virtual string[] Commands => new string[]
        {
            "WHO",
            "DWS", "DWR", "IWS", "IWR", "DMS", "DMC", "IMS", "IMC",
            "DRS", "DRR", "IRS", "IRR",
        };

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        public virtual byte[] Parse(byte[] buffer, byte[] req)
        {
            if (buffer == null) return null;

            int startIdx,
                idxHandle = 0,
                endStartIdx = -1,
                endLastIdx = -1;

            while (idxHandle < buffer.Length - 1)
            {
                //1. Header(0x02, STX) Receive 검사
                startIdx = Array.IndexOf(buffer, 0x02, idxHandle++);
                if (startIdx < 0) continue;

                //2. Tail Receive 검사
                endStartIdx = Utils.Controls.Utils.QYUtils.Find(buffer, this.Tail, startIdx + 1);
                if (endStartIdx < 0) continue;

                endLastIdx = endStartIdx + this.Tail.Length - 1;
                if (buffer.Length < endLastIdx + 1) continue;

                //3. Frame 추출
                byte[] frameByte = new byte[endLastIdx - startIdx + 1];
                Buffer.BlockCopy(buffer, startIdx, frameByte, 0, frameByte.Length);

                return frameByte;
            }

            return null;
        }

        public virtual PCLinkResult Extraction(byte[] frame, byte[] req)
        {
            string reqStr = Encoding.ASCII.GetString(req),
                   rcvStr = Encoding.ASCII.GetString(frame),
                   cmd = rcvStr.Substring(3, 3);

            ResultType type = ResultType.Success;
            string errorMessage = string.Empty;
            List<PCLinkBlock> blocks = null;

            switch (cmd)
            {
                case "WHO": blocks = this.Get_WHO(cmd, rcvStr); break;
                case "DRS": blocks = this.Get_DRS(cmd, rcvStr, reqStr); break;
                case "DRR": blocks = this.Get_DRR(cmd, rcvStr, reqStr); break;
                case "IRS": blocks = this.Get_IRS(cmd, rcvStr, reqStr); break;
                case "IRR": blocks = this.Get_IRR(cmd, rcvStr, reqStr); break;
                case "DWS": blocks = this.Get_DWS(cmd, reqStr); break;
                case "DWR": blocks = this.Get_DWR(cmd, reqStr); break;
                case "IWS": blocks = this.Get_IWS(cmd, reqStr); break;
                case "IWR": blocks = this.Get_IWR(cmd, reqStr); break;
                case "DMS": 
                case "DMC": 
                case "IMS": 
                case "IMC": 
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

        /// <summary>
        /// WHO / 자기정보 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        protected virtual List<PCLinkBlock> Get_WHO(string cmd, string frame)
        {
            List<PCLinkBlock> list = new List<PCLinkBlock>();
            int unitAddr = Convert.ToInt32(frame.Substring(1, 3));

            list.Add(new PCLinkBlock(unitAddr, cmd, frame, null));

            return list;
        }

        #region 기본 Command

        /// <summary>
        /// DRS / Word Register Read, 연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_DRS(string cmd, string rcv, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + ,[1] + Start.Reg[4]
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2] + { ,[1] + HexString[4] } * Count
            int unitAddr = Convert.ToInt32(rcv.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2)),
                startAddr = Convert.ToInt32(req.Substring(10, 4));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                byte[] block = this.HexToByte(rcv.Substring(10 + (i * 5), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, startAddr + i, block));
            }

            return list;
        }
        /// <summary>
        /// DRR / Word Register Read, 비연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_DRR(string cmd, string rcv, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + { ,[1] + RegAddr[4] } * Count
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2] + { ,[1] + HexString[4] } * Count
            int unitAddr = Convert.ToInt32(rcv.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                int reg = Convert.ToInt32(req.Substring(10 + (i * 5), 4));
                byte[] block = this.HexToByte(rcv.Substring(10 + (i * 5), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, reg, block));
            }

            return list;
        }
        /// <summary>
        /// IRS / Bit Register Read, 연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_IRS(string cmd, string rcv, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + ,[1] + Start.Reg[4]
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2] + { ,[1] + BitString[1] } * Count
            int unitAddr = Convert.ToInt32(rcv.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2)),
                startAddr = Convert.ToInt32(req.Substring(10, 4));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                byte[] block = this.HexToByte(rcv.Substring(10 + (i * 2), 1));

                list.Add(new PCLinkBlock(unitAddr, cmd, startAddr + i, block));
            }

            return list;
        }
        /// <summary>
        /// IRS / Bit Register Read, 비연속읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_IRR(string cmd, string rcv, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + { ,[1] + RegAddr[4] } * Count
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2] + { ,[1] + BitString[1] } * Count
            int unitAddr = Convert.ToInt32(rcv.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                int reg = Convert.ToInt32(req.Substring(10 + (i * 5), 4));
                byte[] block = this.HexToByte(rcv.Substring(10 + (i * 2), 1));

                list.Add(new PCLinkBlock(unitAddr, cmd, reg, block));
            }

            return list;
        }
        /// <summary>
        /// DWS / Word Register Write, 연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_DWS(string cmd, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + ,[1] + Start.Reg[4] + { ,[1] + HexString[4] } * Count
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2]
            int unitAddr = Convert.ToInt32(req.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2)),
                startAddr = Convert.ToInt32(req.Substring(10, 4));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                byte[] block = this.HexToByte(req.Substring(15 + (i * 5), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, startAddr + i, block));
            }

            return list;
        }
        /// <summary>
        /// DWR / Word Register Write, 비연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_DWR(string cmd, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + { ,[1] + Start.Reg[4] + ,[1] + HexString[4] } * Count
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2]
            int unitAddr = Convert.ToInt32(req.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                int reg = Convert.ToInt32(req.Substring(10 + (i * 10), 4));
                byte[] block = this.HexToByte(req.Substring(15 + (i * 10), 4));

                list.Add(new PCLinkBlock(unitAddr, cmd, reg, block));
            }

            return list;
        }
        /// <summary>
        /// IWS / Bit Register Write, 연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_IWS(string cmd, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + ,[1] + Start.Reg[4] + { ,[1] + BitString[1] } * Count
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2]
            int unitAddr = Convert.ToInt32(req.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2)),
                startAddr = Convert.ToInt32(req.Substring(10, 4));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                byte[] block = this.HexToByte(req.Substring(15 + (i * 2), 1));

                list.Add(new PCLinkBlock(unitAddr, cmd, startAddr + i, block));
            }

            return list;
        }
        /// <summary>
        /// IWR / Bit Register Write, 비연속쓰기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<PCLinkBlock> Get_IWR(string cmd, string req)
        {
            //Req: STX[1] + Addr[2] + Command[3] + ,[1] + Count[2] + { ,[1] + Start.Reg[4] + ,[1] + HexString[1] } * Count
            //Rcv: STX[1] + Addr[2] + Command[3] + ,[1] + OK[2]
            int unitAddr = Convert.ToInt32(req.Substring(1, 2)),
                readCount = Convert.ToInt32(req.Substring(7, 2));
            List<PCLinkBlock> list = new List<PCLinkBlock>();

            for (int i = 0; i < readCount; i++)
            {
                int reg = Convert.ToInt32(req.Substring(10 + (i * 7), 4));
                byte[] block = this.HexToByte(req.Substring(15 + (i * 7), 1));

                list.Add(new PCLinkBlock(unitAddr, cmd, reg, block));
            }

            return list;
        }

        #endregion 기본 Command End

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Request 정보        //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 연속적인 Address목록별 추출
        /// </summary>
        /// <param name="list">전송 할 Address 목록</param>
        /// <param name="maxFrameCount">1회 연속으로 보낼 수 있는 Frame 수</param>
        /// <returns>연속적인 Address 목록</returns>
        protected List<int[]> SortAddress(List<int> list, int maxFrameCount = 63)
        {
            list.Sort();
            List<int> continuousAddr = new List<int>();
            List<int[]> frameList = new List<int[]>();

            for (int i = 0; i < list.Count; i++)
            {
                int curAddr = list[i];

                if (list.Count == 1)
                {
                    //List가 1개만 있는경우
                    continuousAddr.Add(curAddr);
                    frameList.Add(continuousAddr.ToArray());
                }
                else if (i == 0)
                {
                    //첫번째 Address일 경우
                    continuousAddr.Add(curAddr);
                }
                else if (i == list.Count - 1)
                {
                    //여러 Address 중 마지막 Address일 경우
                    if (curAddr - 1 == continuousAddr.Last())
                    {
                        //연속되는 Address일 경우
                        continuousAddr.Add(curAddr);
                        frameList.Add(continuousAddr.ToArray());
                    }
                    else
                    {
                        //비연속 Address일 경우
                        frameList.Add(continuousAddr.ToArray());

                        continuousAddr.Clear();
                        continuousAddr.Add(curAddr);
                        frameList.Add(continuousAddr.ToArray());
                    }
                }
                else
                {
                    if (continuousAddr.Count == 0)
                    {
                        //연속진행 중 개수초과로 초기화 되었을 경우
                        continuousAddr.Add(curAddr);
                    }
                    else if (curAddr - 1 == continuousAddr.Last())
                    {
                        //연속 Address일경우
                        continuousAddr.Add(curAddr);

                        if (continuousAddr.Count >= maxFrameCount)
                        {
                            frameList.Add(continuousAddr.ToArray());
                            continuousAddr.Clear();
                        }
                    }
                    else
                    {
                        //연속 Address가 아닐경우
                        frameList.Add(continuousAddr.ToArray());
                        continuousAddr.Clear();

                        continuousAddr.Add(curAddr);
                    }
                }
            }

            return frameList;
        }

        protected virtual byte[] Build(string body)
        {
            byte[] stx = new byte[] { 0x02 };
            byte[] frame = Encoding.ASCII.GetBytes(body);
            byte[] build = new byte[stx.Length + frame.Length + this.Tail.Length];

            //STX
            Buffer.BlockCopy(stx, 0, build, 0, stx.Length);
            //Frame
            Buffer.BlockCopy(frame, 0, build, stx.Length, frame.Length);
            //Tail
            Buffer.BlockCopy(this.Tail, 0, build, stx.Length + frame.Length, this.Tail.Length);

            return build;
        }

        #region 일반 PCLink Request 생성

        /// <summary>
        /// IRR 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_IRR(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D2}IRR{addrAry.Length:D2}";
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0) return frameList;
            return null;
        }

        /// <summary>
        /// DRS 연속 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_DRS(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D2}DRS{addrAry.Length:D2}";

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        /// <summary>
        /// DRR 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_DRR(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D2}DRR{addrAry.Length:D2}";
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion 일반 PCLink Request 생성 End

        protected byte[] HexToByte(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2));

            return bytes;
        }
    }
}
