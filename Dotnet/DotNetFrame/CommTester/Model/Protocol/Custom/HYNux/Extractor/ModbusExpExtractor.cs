using DotNet.CommTester.Model.Protocol.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.CommTester.Model.Protocol.Custom.HYNux.Extractor
{
    internal class ModbusExpExtractor : ModbusExtractor
    {

        /// <summary>
        /// 01(0x01), 02(0x02) ReadCoils Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected override ModbusItem[] ReadCoils(byte[] request, byte[] response)
        {
            //Request  : Cmd[1] + StartAddr H/L[2] + ReadCount H/L[2]
            //Response : Cmd[1] + ByteCount[2] + Data[ByteCount]

            List<ModbusItem> list = new List<ModbusItem>();
            int startAddr = (request[1] << 8) | request[2];
            int readCount = (request[3] << 8) | request[4];

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                bool value = ((response[3 + (i / 8)] >> (i % 8)) & 0b1) == 0b1;

                list.Add(new ModbusItem(addr, value));
            }

            return list.ToArray();
        }
        /// <summary>
        /// 03(0x03), 04(0x04) ReadHoldingRegister Frame 해석
        /// </summary>
        /// <param name="request">Request Data</param>
        /// <param name="response">Response Data</param>
        protected override ModbusItem[] ReadHoldingRegister(byte[] request, byte[] response)
        {
            //Request  : Cmd[1] + StartAddr H/L[2] + ReadCount H/L[2]
            //Response : Cmd[1] + ByteCount[2] + (Data H/L[2] * ReadCount)

            List<ModbusItem> list = new List<ModbusItem>();
            int startAddr = (request[1] << 8) | request[2];
            int readCount = (request[3] << 8) | request[4];

            for (int i = 0; i < readCount; i++)
            {
                UInt16 addr = (UInt16)(startAddr + i);
                byte[] binary = new byte[2];
                Buffer.BlockCopy(response, 3 + (i * 2), binary, 0, binary.Length);

                list.Add(new ModbusItem(addr, binary));
            }

            return list.ToArray();
        }
    }
}
