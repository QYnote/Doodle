using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol.Modbus
{
    internal class ModbusItem : ProtocolItem
    {
        internal UInt16 RegAddr { get; }
        internal string RegName { get; }

        public ModbusItem(UInt16 addr, object value) : base(value)
        {
            this.RegAddr = addr;
        }
        public ModbusItem(string name, object value) : base(value)
        {
            this.RegName = name;
        }
    }

    internal class ModbusExtractor
    {
        /// <summary>
        /// Protocol DataUnit 해석
        /// </summary>
        /// <param name="reqPDU">Request PDU</param>
        /// <param name="resPDU">Response PDU</param>
        /// <returns>해석 결과물</returns>
        /// <remarks>
        /// 
        /// </remarks>
        internal virtual ModbusItem[] Parse(byte[] reqPDU, byte[] resPDU)
        {
            byte cmd = resPDU[0];

            switch (cmd)
            {
                case 0x01:
                case 0x02: return this.ReadCoils(reqPDU, resPDU);
                case 0x03:
                case 0x04: return this.ReadHoldingRegister(reqPDU, resPDU);
                case 0x05: return this.WriteSingleCoils(resPDU);
                case 0x06: return this.WriteSingleRegister(resPDU);
                case 0x0F: return this.WriteMultipleCoils(reqPDU, resPDU);
                case 0x10: return this.WriteMultipleRegister(reqPDU, resPDU);
                case 0x17: return this.ReadWriteMultipleRegister(reqPDU, resPDU);
            }

            return null;
        }

        /// <summary>
        /// 01(0x01), 02(0x02) ReadCoils Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual ModbusItem[] ReadCoils(byte[] request, byte[] response)
        {
            //Request  : Cmd[1] + StartAddr H/L[2] + ReadCount H/L[2]
            //Response : Cmd[1] + ByteCount[1] + Data[ByteCount]

            List<ModbusItem> list = new List<ModbusItem>();
            int startAddr = (request[1] << 8) | request[2];
            int readCount = (request[3] << 8) | request[4];

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                bool value = ((response[2 + (i / 8)] >> (i % 8)) & 0b1) == 0b1;

                list.Add(new ModbusItem(addr, value));
            }

            return list.ToArray();
        }
        /// <summary>
        /// 03(0x03), 04(0x04) ReadHoldingRegister Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual ModbusItem[] ReadHoldingRegister(byte[] request, byte[] response)
        {
            //Request  : Cmd[1] + StartAddr H/L[2] + ReadCount H/L[2]
            //Response : Cmd[1] + ByteCount[1] + (Data H/L[2] * ReadCount)

            List<ModbusItem> list = new List<ModbusItem>();
            int startAddr = (request[1] << 8) | request[2];
            int readCount = (request[3] << 8) | request[4];

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                byte[] binary = new byte[2];
                Buffer.BlockCopy(response, 2 + (i * 2), binary, 0, binary.Length);

                list.Add(new ModbusItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// 05(0x05) WriteSingleCoils Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual ModbusItem[] WriteSingleCoils(byte[] response)
        {
            //Request & Response : Cmd[1] + WriteAddr H/L[1] + WriteData H/L[2]
            //WriteData: true = 0xFF00 / false = 0x0000

            List<ModbusItem> list = new List<ModbusItem>();
            UInt16 addr = (UInt16)((response[1] << 8) | response[2]);
            bool value = response[3] == 0xFF && response[4] == 0x00;

            list.Add(new ModbusItem(addr, value));

            return list.ToArray();
        }
        /// <summary>
        /// 06(0x06) WriteSingleRegister Frame 해석
        /// </summary>
        /// <param name="response">Response Data</param>
        protected virtual ModbusItem[] WriteSingleRegister(byte[] response)
        {
            //Request & Response : Cmd[1] + WriteAddr H/L[1] + WriteData H/L[2]

            List<ModbusItem> list = new List<ModbusItem>();
            UInt16 addr = (UInt16)((response[1] << 8) | response[2]);
            byte[] binary = new byte[2];
            Buffer.BlockCopy(response, 2, binary, 0, binary.Length);

            list.Add(new ModbusItem(addr, binary));

            return list.ToArray();
        }
        /// <summary>
        /// 15(0x0F) WriteMultipleCoils Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual ModbusItem[] WriteMultipleCoils(byte[] request, byte[] response)
        {
            //Request  : Cmd[1] + StartAddr H/L[2] + WriteCount H/L[2] + ByteCount[1] + (Data H/L[2] * WriteCount)
            //Response : Cmd[1] + StartAddr H/L[2] + WriteCount H/L[2]

            List<ModbusItem> list = new List<ModbusItem>();
            int startAddr = (request[1] << 8) | request[2];
            int writeCount = (request[3] << 8) | request[4];

            for (int i = 0; i < writeCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                bool value = ((response[6 + (i / 8)] >> (i % 8)) & 0b1) == 0b1;

                list.Add(new ModbusItem(addr, value));
            }

            return list.ToArray();
        }
        /// <summary>
        /// 16(0x10) WriteMultipleRegister Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual ModbusItem[] WriteMultipleRegister(byte[] request, byte[] response)
        {
            //Request  : Cmd[1] + StartAddr H/L[2] + WriteCount H/L[2] + ByteCount[1] + (Data H/L[2] * WriteCount)
            //Response : Cmd[1] + StartAddr H/L[2] + WriteCount H/L[2]

            List<ModbusItem> list = new List<ModbusItem>();
            int startAddr = (request[1] << 8) | request[2];
            int writeCount = (request[3] << 8) | request[4];

            for (int i = 0; i < writeCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                byte[] binary = new byte[2];
                Buffer.BlockCopy(response, 6 + (i * 2), binary, 0, binary.Length);

                list.Add(new ModbusItem(addr, binary));
            }

            return list.ToArray();
        }
        /// <summary>
        /// 23(0x17) ReadWriteMultipleRegister Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected virtual ModbusItem[] ReadWriteMultipleRegister(byte[] request, byte[] response)
        {
            //Request  : Cmd[1] + Read StartAddr H/L[2] + ReadCount H/L[2] +
            //                    Write StartAddr H/L[2] + WriteCount H/L[2] +
            //                    WriteByteCount[1] + (WriteData H/L[2] * WriteCount)
            //Response : Cmd[1] + ByteCount[1] + (ReadData H/L[2] * ReadCount)
            //Write 먼저 실행

            List<ModbusItem> list = new List<ModbusItem>();
            int wStartAddr = (request[5] << 8) | request[6];
            int wCount = (request[7] << 8) | request[8];
            int rStartAddr = (request[1] << 8) | request[2];
            int rCount = (request[3] << 8) | request[4];
            Dictionary<UInt16, object> dic = new Dictionary<UInt16, object>();

            //쓰기 결과
            for (int i = 0; i < wCount; i++)
            {
                UInt16 addr = (UInt16)(wStartAddr + i);
                byte[] binary = new byte[2];
                Buffer.BlockCopy(request, 10 + (i * 2), binary, 0, binary.Length);

                dic[addr] = binary;
            }

            //읽기 결과
            for (int i = 0; i < rCount; i++)
            {
                UInt16 addr = (UInt16)(rStartAddr + i);
                byte[] binary = new byte[2];
                Buffer.BlockCopy(request, 10 + (i * 2), binary, 0, binary.Length);

                dic[addr] = binary;
            }

            //중복 Address 처리 결과
            foreach (var pair in dic)
                list.Add(new ModbusItem(pair.Key, pair.Value));

            return list.ToArray();
        }
    }
}
