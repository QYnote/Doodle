using DotNet.CommTester.Model.Protocol;
using DotNet.CommTester.Model.Protocol.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Protocol.Modbus
{
    internal class ModbusTCP : ModbusBase
    {
        DotNet.Comm.Protocols.Customs.HYNux.ModbusTCP origin = new DotNet.Comm.Protocols.Customs.HYNux.ModbusTCP();
        ModbusExtractor unpacker = new ModbusExtractor();

        internal ModbusTCP(CommPort handler) : base(handler)
        {

        }

        public override byte[] Parse(byte[] buffer) => this.origin.Parse(buffer, base.Request);
        public override IProtocolResult Extraction(byte[] frame)
        {
            ProtocolResult<ModbusItem> result = new ProtocolResult<ModbusItem>(base.Request);
            result.Response = frame;

            //2. PDU 추출
            byte[] reqPDU = new byte[base.Request.Length - 7];
            byte[] resPDU = new byte[frame.Length - 7];

            Buffer.BlockCopy(base.Request, 7, reqPDU, 0, reqPDU.Length);
            Buffer.BlockCopy(frame, 7, resPDU, 0, resPDU.Length);

            //3. ErrorCode 검사
            if (resPDU[0] > 0x80)
            {
                result.Type = ResultType.Protocol_Exception;
                result.ErrorMessage = "Protocol Error";
            }
            else
            {
                //4. Item 추출
                ModbusItem[] items = this.unpacker.Parse(reqPDU, resPDU);
                if (items == null)
                {
                    result.Type = ResultType.Protocol_Exception;
                    result.ErrorMessage = "Undeveloped Command or Extract Error";
                }
                else
                {
                    result.Type = ResultType.Success;

                    foreach (var item in items)
                        result.Items.Add(item);
                }
            }

            return result;
        }
        public override byte[] CreateCheckSum(byte[] frame) => null;

        public override void Initialize()
        {
            base.Request = null;
            this.origin.Initialize();
        }
    }
}
