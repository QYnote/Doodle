using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public class ModbusResult : IProtocolResult<ModbusBlock>
    {
        public ResultType Type { get; }
        public string ErrorMessage { get; }
        public List<ModbusBlock> Blocks { get; }
        IEnumerable<IProtocolBlock> IProtocolResult.Blocks => this.Blocks;

        public ModbusResult(ResultType type, string errorMessage, List<ModbusBlock> blocks)
        {
            this.Blocks = blocks;
            this.Type = type;
            this.ErrorMessage = errorMessage;
        }
    }
    public class ModbusBlock : IProtocolBlock
    {
        /// <summary>통신 Device Addres</summary>
        public int UnitAddress { get; }
        /// <summary>명령어</summary>
        public byte Command { get; }
        /// <summary>수신 Registry 대상</summary>
        public object Target { get; }
        /// <summary>Bit Data Block Index 순번</summary>
        public int BitIndex { get; }
        public byte[] Block { get; }

        public ModbusBlock(int unitAddr, byte cmd, object target, byte[] block, int bitIndex = -1)
        {
            this.UnitAddress = unitAddr;
            this.Command = cmd;
            this.Target = target;
            this.Block = block;
            this.BitIndex = bitIndex;
        }
    }

    public class ModbusRTU
    {
        /// <summary>
        /// 통신 수신 검사
        /// </summary>
        /// <param name="bytes">검사 할 Frame</param>
        /// <returns>true: 에러발생 / false : 정상</returns>
        /// <remarks>
        /// ErrorCode가 포함 된 Frame으로 진행
        /// </remarks>
        public virtual bool CheckSum(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;
            int errStartIdx = bytes.Length - 2;


            for (int Index = 0; Index < errStartIdx; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ErrorCode 생성
        /// </summary>
        /// <param name="bytes">생성 할 Frame</param>
        /// <returns>생성된 Error Code</returns>
        public virtual byte[] CreateErrCode(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;


            for (int Index = 0; Index < bytes.Length; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            return chkCd;
        }


        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        public virtual byte[] Parse(byte[] buffer, byte[] req)
        {
            int idxHandle = 0,
                startIdx = -1,
                frameLen = -1;
            byte cmd;

            //Header: Addr[1] + Cmd[1]
            if (buffer.Length < 2) return null;

            //Frame 검색
            while (idxHandle < buffer.Length - 1)
            {
                startIdx = QYUtils.Find(buffer, new byte[] { req[0], req[1] }, idxHandle);
                if (startIdx < 0)
                {
                    //Error Cmd가 날라온건지 검사
                    startIdx = QYUtils.Find(buffer, new byte[] { req[0], (byte)(req[1] + 0x80) }, idxHandle);
                }

                idxHandle++;
                if (startIdx < 0) continue;
                cmd = buffer[startIdx + 1];

                //기능코드별 Frame 길이
                frameLen = -1;
                if (cmd == 0x01 || cmd == 0x02 || cmd == 0x03 || cmd == 0x04)
                {
                    //Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]
                    if (buffer.Length < startIdx + 3) continue; //ByteCount receive 검사
                    int byteCount = buffer[startIdx + 2];

                    frameLen = 1 + 1 + 1 + byteCount;
                }
                else if (cmd == 0x05 || cmd == 0x06 || cmd == 0x10)
                {
                    //0x05 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + Value[2]
                    //0x06 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + RegValue[2]
                    //0x10 - 기본: Addr[1] + Cmd[1] + StartReg[2] + ReadRegCount[2]
                    frameLen = 1 + 1 + 2 + 2;
                }
                else if (cmd >= 0x80)
                {
                    //Addr[1] + Cmd[1] + ErrCode[1]
                    frameLen = 1 + 1 + 1;
                }

                if (buffer.Length < startIdx + frameLen) continue;

                //Frame 추출
                byte[] frameByte = new byte[frameLen];
                Buffer.BlockCopy(buffer, startIdx, frameByte, 0, frameLen);

                return frameByte;
            }

            return null;
        }

        public virtual ModbusResult Extraction(byte[] frame, byte[] req)
        {
            byte cmd = frame[1];
            ResultType type = ResultType.Success;
            string errorMessage = string.Empty;
            List<ModbusBlock> blocks = null;

            switch (cmd)
            {
                case 0x01:
                case 0x02: blocks = this.Response_GetReadCoils(cmd, frame, req); break;
                case 0x03:
                case 0x04: blocks = this.Response_GetReadHoldingRegister(cmd, frame, req); break;
                case 0x05: blocks = this.Response_GetWriteSingleCoils(frame); break;
                case 0x06: blocks = this.Response_GetWriteSingleRegister(frame); break;
                case 0x0F: blocks = this.Response_GetWriteMultipleCoils(frame); break;
                case 0x10: blocks = this.Response_GetWriteMultipleRegister(frame); break;
                default:
                    if (cmd > 0x80)
                    {
                        type = ResultType.Protocol_Exception;
                        switch (frame[2])
                        {
                            case 0x01: errorMessage = "Invalid code"; break;
                            case 0x02: errorMessage = "Invalid register"; break;
                            case 0x03: errorMessage = "Wrong Data Count"; break;
                            case 0x04: errorMessage = "Data error"; break;
                            case 0x21: errorMessage = "Full Buffer"; break;
                            default: errorMessage = "Unknown Error"; break;
                        }
                        errorMessage = "Protocol Error";
                    }
                    else
                    {
                        type = ResultType.Protocol_Exception;
                        errorMessage = "Undevelop Command";
                    }
                    break;
            }

            if (blocks != null && blocks.Count == 0) blocks = null;

            return new ModbusResult(type, errorMessage, blocks);
        }

        #region FuncCode Get Process

        /// <summary>
        /// 01(0x01), 02(0x02) ReadCoils Frame 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<ModbusBlock> Response_GetReadCoils(byte cmd, byte[] frame, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Res : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];
            List<ModbusBlock> list = new List<ModbusBlock>();

            for (int i = 0; i < readCount; i++)
            {
                //Value = (bool)((담당Byte >> Bit위치) & 1 == 1)
                byte[] block = new byte[] { frame[3 + (i / 8)] };
                list.Add(new ModbusBlock(frame[0], cmd, startAddr + i, block, i % 8));
            }

            return list;
        }
        /// <summary>
        /// 03(0x03), 04(0x04) ReadHoldingRegister Frame 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<ModbusBlock> Response_GetReadHoldingRegister(byte cmd, byte[] frame, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount] Hi/Lo

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = frame[2];
            List<ModbusBlock> list = new List<ModbusBlock>();

            for (int i = 0; i < readCount; i += 2)
            {
                byte[] block = new byte[2];
                Buffer.BlockCopy(frame, 3 + i, block, 0, block.Length);

                list.Add(new ModbusBlock(frame[0], cmd, startAddr + (i / 2), block));
            }

            return list;
        }
        /// <summary>
        /// 05(0x05) WriteSingleCoils Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<ModbusBlock> Response_GetWriteSingleCoils(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + Addr[2] + WriteData[2]
            int addr = (frame[2] << 8) + frame[3];
            List<ModbusBlock> list = new List<ModbusBlock>();
            byte[] block = new byte[2];

            Buffer.BlockCopy(frame, 4, block, 0, block.Length);
            list.Add(new ModbusBlock(frame[0], 0x05, addr, block));

            return list;
        }
        /// <summary>
        /// 06(0x06) WriteSingleRegister Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<ModbusBlock> Response_GetWriteSingleRegister(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + Data[2]
            int addr = (frame[2] << 8) + frame[3];
            List<ModbusBlock> list = new List<ModbusBlock>();
            byte[] block = new byte[2];

            Buffer.BlockCopy(frame, 4, block, 0, block.Length);
            list.Add(new ModbusBlock(frame[0], 0x06, addr, block));

            return list;
        }
        /// <summary>
        /// 15(0x0F) WriteMultipleCoils Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<ModbusBlock> Response_GetWriteMultipleCoils(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2] + ByteCount[1] + Value[ByteCount]
            int startAddr = (frame[2] << 8) + frame[3],
                readCount = (frame[4] << 8) + frame[5];
            List<ModbusBlock> list = new List<ModbusBlock>();

            for (int i = 0; i < readCount; i++)
            {
                byte[] block = new byte[] { frame[7 + (i / 8)] };

                list.Add(new ModbusBlock(frame[0], 0x0F, startAddr + i, block, i % 8));
            }

            return list;
        }
        /// <summary>
        /// 16(0x10) WriteMultipleRegister Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<ModbusBlock> Response_GetWriteMultipleRegister(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2] + ByteCount[1] + Value[ByteCount]
            int startAddr = (frame[2] << 8) + frame[3],
                readCount = (frame[4] << 8) + frame[5];
            List<ModbusBlock> list = new List<ModbusBlock>();

            for (int i = 0; i < readCount; i += 2)
            {
                byte[] block = new byte[2];

                Buffer.BlockCopy(frame, 7 + i, block, 0, block.Length);
                list.Add(new ModbusBlock(frame[0], 0x10, startAddr + (i / 2), block));
            }

            return list;
        }

        #endregion FuncCode Get Process End

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
        /// <summary>
        /// 01~04(0x01~0x04) Request 기본구조 byte Array List 생성
        /// </summary>
        /// <returns>Cmd값이 0x00으로 지정되어있는 Frame List</returns>
        protected List<byte[]> CreateRequest_Read(int deviceAddr, List<int[]> readList)
        {
            byte addr = (byte)(deviceAddr & 0xFF);
            List<byte[]> dataFrame = new List<byte[]>();

            foreach (var addrFrame in readList)
            {
                byte[] frame = new byte[6];
                frame[0] = addr;
                frame[1] = 0x00;
                frame[2] = (byte)((addrFrame[0] >> 8) & 0xFF);
                frame[3] = (byte)(addrFrame[0] & 0xFF);
                frame[4] = (byte)((addrFrame.Length >> 8) & 0xFF);
                frame[5] = (byte)(addrFrame.Length & 0xFF);

                dataFrame.Add(frame);
            }

            if (dataFrame.Count == 0) return null;

            return dataFrame;
        }

        /// <summary>
        /// 01(0x01) ReadCoils Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록, </param>
        /// <param name="maxFrameCount">1개 전송시 최대 연속 Address수</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_ReadCoils(int deviceAddr, List<int> readList)
        {
            List<int[]> addrList = this.SortAddress(readList, 0xFFFF);
            List<byte[]> dataFrame = this.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x01;

            if (dataFrame.Count == 0) return null;

            return dataFrame;
        }
        /// <summary>
        /// 02(0x02) ReadDiscreteInputs Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <param name="maxFrameCount">1개 전송시 최대 연속 Address수</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_ReadDiscreteInputs(int deviceAddr, List<int> readList)
        {
            List<int[]> addrList = this.SortAddress(readList, 0xFFFF);
            List<byte[]> dataFrame = this.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x02;

            if (dataFrame.Count == 0) return null;

            return dataFrame;
        }
        /// <summary>
        /// 03(0x03) ReadHoldingRegister Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <param name="maxFrameCount">1개 전송시 최대 연속 Address수</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_ReadHoldingRegister(int deviceAddr, List<int> readList)
        {
            List<int[]> addrList = this.SortAddress(readList, 0xFFFF);
            List<byte[]> dataFrame = this.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x03;

            if (dataFrame.Count == 0) return null;

            return dataFrame;
        }
        /// <summary>
        /// 04(0x04) ReadInputRegister Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <param name="maxFrameCount">1개 전송시 최대 연속 Address수</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_ReadInputRegister(int deviceAddr, List<int> readList)
        {
            List<int[]> addrList = this.SortAddress(readList, 0xFFFF);
            List<byte[]> dataFrame = this.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x04;

            if (dataFrame.Count == 0) return null;

            return dataFrame;
        }
    }
}
