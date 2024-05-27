using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Comm.Protocols
{
    internal class CRC
    {
        /// <summary>CRC 방법</summary>
        public enum CRCType
        {
            ModbusNormal
        }

        internal static void AddModbusCRC(byte[] frame, CRCType type = CRCType.ModbusNormal)
        {

        }
    }
}
