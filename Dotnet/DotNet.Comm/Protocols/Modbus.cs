using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public class DataFrame_Modbus
    {
        public byte DeviceAddr { get; set; }
        public byte FuncCode { get; set; }
        public int DataAddr { get; set; }
        public object Value { get; set; }
    }

    public class Modbus : ProtocolFrame
    {
        public Modbus(bool isClient) : base(isClient) { }

        #region Response

        public override byte[] Response_ExtractFrame(byte[] buffer, params object[] subData)
        {
            if (subData[0] == null) return null;
            byte[] reqFrame = subData[0] as byte[];

            int idxHandle = 0,
                startIdx = -1,
                frameLen = -1;
            byte cmd;

            //Header: Addr[1] + Cmd[1]
            if (buffer.Length < 2) return null;

            //Frame 검색
            while (idxHandle < buffer.Length - 1)
            {
                startIdx = QYUtils.Find(buffer, new byte[] { reqFrame[0], reqFrame[1] }, idxHandle);
                if (startIdx < 0)
                {
                    //Error Cmd가 날라온건지 검사
                    startIdx = QYUtils.Find(buffer, new byte[] { reqFrame[0], (byte)(reqFrame[1] + 0x80) }, idxHandle);
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

                    frameLen = 1 + 1 + 1 + byteCount + this.ErrCodeLength;
                }
                else if (cmd == 0x05 || cmd == 0x06 || cmd == 0x10)
                {
                    //0x05 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + Value[2]
                    //0x06 - 기본: Addr[1] + Cmd[1] + StartAddr[2] + RegValue[2]
                    //0x10 - 기본: Addr[1] + Cmd[1] + StartReg[2] + ReadRegCount[2]
                    frameLen = 1 + 1 + 2 + 2 + this.ErrCodeLength;
                }
                else if (cmd >= 0x80)
                {
                    //Addr[1] + Cmd[1] + ErrCode[1]
                    frameLen = 1 + 1 + 1 + this.ErrCodeLength;
                }

                if (buffer.Length < startIdx + frameLen) continue;

                //Frame 추출
                byte[] frameByte = new byte[frameLen];
                Buffer.BlockCopy(buffer, startIdx, frameByte, 0, frameLen);

                return frameByte;
            }

            return null;
        }
        public override List<object> Response_ExtractData(byte[] frame, params object[] subData)
        {
            if (subData[0] == null) return null;
            byte[] reqBytes = subData[0] as byte[];
            byte cmd = frame[1];

            switch (cmd)
            {
                case 0x01:
                case 0x02: return this.Get_ReadCoils(cmd, frame, reqBytes);
                case 0x03:
                case 0x04: return this.Get_ReadHoldingRegister(cmd, frame, reqBytes);
                case 0x05: return this.Get_WriteSingleCoils(frame);
                case 0x06: return this.Get_WriteSingleRegister(frame);
                case 0x0F: return this.Get_WriteMultipleCoils(frame);
                case 0x10: return this.Get_WriteMultipleRegister(frame);
                default:
                    if (cmd > 0x80)
                        return new List<object>()
                            {
                                new DataFrame_Modbus()
                                {
                                    DeviceAddr = frame[0],
                                    FuncCode = cmd,
                                    Value = frame[2]
                                }
                            };
                    else
                        System.Diagnostics.Debug.WriteLine(string.Format("({0}) Protocol Error - UnSupport Command: {1:X2}", this.GetType().Name, cmd)); break;
            }

            return null;
        }

        #region FuncCode Get Process

        /// <summary>
        /// 01(0x01), 02(0x02) ReadCoils Frame 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<object> Get_ReadCoils(byte cmd, byte[] frame, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Res : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount]

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = (reqBytes[4] << 8) + reqBytes[5];
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                //Value = (bool)((담당Byte >> Bit위치) & 1 == 1)
                list.Add(new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = cmd,
                    DataAddr = startAddr + i,
                    Value = ((frame[3 + (i / 8)] >> (i % 8)) & 1) == 1
                });
            }

            return list;
        }
        /// <summary>
        /// 03(0x03), 04(0x04) ReadHoldingRegister Frame 읽기
        /// </summary>
        /// <param name="cmd">입력한 명령코드</param>
        /// <param name="frame">Response Data</param>
        /// <param name="reqBytes">Request Data</param>
        protected virtual List<object> Get_ReadHoldingRegister(byte cmd, byte[] frame, byte[] reqBytes)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + ReadAddrCount[2]
            //Rcv : Addr[1] + Cmd[1] + ByteCount[1] + Data[ByteCount] Hi/Lo

            int startAddr = (reqBytes[2] << 8) + reqBytes[3],
                readCount = frame[2];
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i += 2)
            {
                list.Add(new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = cmd,
                    DataAddr = startAddr + (i / 2),
                    Value = (Int16)((frame[3 + i] << 8) + frame[3 + i + 1])
                });
            }

            return list;
        }
        /// <summary>
        /// 05(0x05) WriteSingleCoils Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<object> Get_WriteSingleCoils(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + Addr[2] + WriteData[2]
            int addr = (frame[2] << 8) + frame[3];

            if (frame[4] == 0xFF && frame[5] == 0x00)
                return new List<object>() {new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = 0x05,
                    DataAddr = addr,
                    Value = true
                }};
            else if (frame[4] == 0x00 && frame[5] == 0x00)
                return new List<object>() {new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = 0x05,
                    DataAddr = addr,
                    Value = false
                }};

            return null;
        }
        /// <summary>
        /// 06(0x06) WriteSingleRegister Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<object> Get_WriteSingleRegister(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + Data[2]
            int addr = (frame[2] << 8) + frame[3];
            return new List<object>() {new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = 0x06,
                    DataAddr = addr,
                    Value = (Int16)((frame[4] << 8) + frame[5])
                }};
        }
        /// <summary>
        /// 15(0x0F) WriteMultipleCoils Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<object> Get_WriteMultipleCoils(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2] + ByteCount[1] + Value[ByteCount]
            int startAddr = (frame[2] << 8) + frame[3],
                readCount = (frame[4] << 8) + frame[5];
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i++)
            {
                list.Add(new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = 0x0F,
                    DataAddr = startAddr + i,
                    Value = ((frame[7 + (i / 8)] >> (i % 8)) & 1) == 1
                });
            }

            return list;
        }
        /// <summary>
        /// 16(0x10) WriteMultipleRegister Frame 읽기
        /// </summary>
        /// <param name="frame">Response Data</param>
        protected virtual List<object> Get_WriteMultipleRegister(byte[] frame)
        {
            //Req : Addr[1] + Cmd[1] + StartAddr[2] + WriteCount[2] + ByteCount[1] + Value[ByteCount]
            int startAddr = (frame[2] << 8) + frame[3],
                readCount = (frame[4] << 8) + frame[5];
            List<object> list = new List<object>();

            for (int i = 0; i < readCount; i += 2)
            {
                list.Add(new DataFrame_Modbus()
                {
                    DeviceAddr = frame[0],
                    FuncCode = 0x10,
                    DataAddr = startAddr + (i / 2),
                    Value = (Int16)((frame[7 + i] << 8) + frame[7 + i + 1])
                });
            }

            return list;
        }

        #endregion FuncCode Get Process End

        #endregion Response End
        #region Request

        public override byte[] Request_ExtractFrame(byte[] buffer, params object[] subData)
        {
            throw new NotImplementedException();
        }

        public override List<object> Request_ExtractData(byte[] frame, params object[] subData)
        {
            throw new NotImplementedException();
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
            List<int[]> addrList = base.SortContinuouseAddress(readList, 0xFFFF);
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
            List<int[]> addrList = base.SortContinuouseAddress(readList, 0xFFFF);
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
            List<int[]> addrList = base.SortContinuouseAddress(readList, 0xFFFF);
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
            List<int[]> addrList = base.SortContinuouseAddress(readList, 0xFFFF);
            List<byte[]> dataFrame = this.CreateRequest_Read(deviceAddr, addrList);

            for (int i = 0; i < dataFrame.Count; i++)
                dataFrame[i][1] = 0x04;

            if (dataFrame.Count == 0) return null;

            return dataFrame;
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

        #endregion Request End
        #region ErrorCode

        public override bool ConfirmErrCode(byte[] frame)
        {
            return true;
        }

        public override byte[] CreateErrCode(byte[] frame)
        {
            return null;
        }

        #endregion ErrorCode End
    }
}
