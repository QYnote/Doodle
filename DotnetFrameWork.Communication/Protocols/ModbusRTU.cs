using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetFrameWork.Communication.Protocols
{
    public class ModbusRTU : ProtocolBase
    {
        private int CustomCode { get; }
        public int IDLength { get; set; }
        public int RegistryAddressLength { get; set; }

        public ModbusRTU(int customCode = 0)
        {
            this.CustomCode = customCode;

            switch (this.CustomCode)
            {
                case 0:
                    this.IDLength = 1;
                    this.RegistryAddressLength = 1;
                    break;
            }
        }

        public void Request_Read_Holding_Register(int slaveID, int startAddress, int quantity)
        {
            if (slaveID > 0xFF) throw new Exception("[Error]ID Size Over");
            if (startAddress > 0xFFFF) throw new Exception("[Error]Address Size Over");
            if (quantity > 0xFF) throw new Exception("[Error]Quantity Size Over");

            byte func = 0x03;
        }

        public override Queue<byte[]> ParseData(byte[] readBuffer, out int parseCode)
        {
            throw new NotImplementedException();
        }
    }
}
