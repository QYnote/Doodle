using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Dnf.Comm.Protocols.ServerProtocol
{
    /// <summary>Modbus Server → Client</summary>
    public class ModbusServer
    {
        private Dictionary<short, short> imsiRegistryData = new Dictionary<short, short>();

        /// <summary>
        /// Modbus Server → Client
        /// </summary>
        /// <param name="dict">Server에 있는 Registry Data Dictionary(Address, Value)</param>
        public ModbusServer(Dictionary<short, short> dict)
        {
            imsiRegistryData = dict;
        }

        public byte[] GetRequestData(byte[] reqData)
        {
            if(reqData.Length < 2) return null;
            //1. Client Address, Func 확인
            byte clientAddress = reqData[0];
            byte func = reqData[1];

            //2. Func별 Response Byte[] 생성
            byte[] dataBytes = null;
            switch (func)
            {
                /*개선해야할 사항
                 * 존재하지 않는 Registry 이거나,
                 * 다른 Error 발생시 Error Code를 전송하는 Process가 없는 상황
                 */
                case 0x03:
                    dataBytes = CreateRes_Read(reqData);
                    break;
                case 0x10:
                    dataBytes = CreateRes_Write(reqData);
                    break;
            }

            if (dataBytes == null) return null;

            //3. Response byte[] 합치기
            byte[] resBytes = new byte[] { clientAddress, func };
            resBytes.BytesAppend(dataBytes);

            return resBytes;
        }

        private byte[] CreateRes_Read(byte[] reqData)
        {
            /*개선해야할 사항
             * Server Registry가 1 Address당 2Byte가 되도록 지정되있는 상황
             * 1 ARegistry - Bool처럼 되있는경우 어떻게 처리할지 개선해야함
             */
            //Req : Addr[1] + Func[1] + StartAddr[2] + Read Registry Count[2]
            //Res : Addr[1] + Func[1] + ByteCount[1] + ByteList[N : ByteCount]
            if (reqData.Length < 6) return null;

            short startAddress = (short)((reqData[2] << 8) + reqData[3]);
            short readRegisterCount = (short)((reqData[4] << 8) + reqData[5]);

            List<byte> bytes = new List<byte>() { (byte)(reqData[5] * 2) }; //Response될 bytes, BytesCount 입력상태
            short readAddr = startAddress;
            for(short  i = 0; i < readRegisterCount; i++)
            {
                bytes.Add((byte)(imsiRegistryData[(short)(readAddr + i)] >> 8));
                bytes.Add((byte)imsiRegistryData[(short)(readAddr + i)]);
            }

            return bytes.ToArray();
        }

        private byte[] CreateRes_Write(byte[] reqData)
        {
            //Req : Addr[1] + Func[1] + StartAddr[2] + DataCount[2](max 127) + ByteCount[2](Max 254) + DataValue[N]
            if(reqData.Length < 8) { return null; }

            short startAddress = (short)((reqData[2] << 8) + reqData[3]);
            short writeRegisterCount = (short)((reqData[4] << 8) + reqData[5]);

            short writeAddr = startAddress;
            for (short i = 0; i < writeRegisterCount; i++)
            {
                short value = (short)((reqData[8 + (i * 2)] << 8) + reqData[8 + (i * 2) + 1]);

                imsiRegistryData[(short)(writeAddr + i)] = value;
            }

            return new byte[] { reqData[2], reqData[3], reqData[4], reqData[5] };
        }
    }
}
