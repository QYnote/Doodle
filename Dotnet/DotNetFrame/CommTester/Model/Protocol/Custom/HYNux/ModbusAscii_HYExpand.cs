using DotNet.CommTester.Model.Protocol;
using DotNet.CommTester.Model.Protocol.Custom.HYNux.Extractor;
using DotNet.CommTester.Model.Protocol.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Protocol.Custom.HYNux
{
    internal class ModbusAscii_HYExpand : ModbusBase
    {
        DotNet.Comm.Protocols.ModbusAscii origin = new DotNet.Comm.Protocols.ModbusAscii();
        ModbusExpExtractor unpacker = new ModbusExpExtractor();

        internal ModbusAscii_HYExpand(CommPort handler):base(handler) { }

        public override byte[] Parse(byte[] buffer) => this.origin.Parse(buffer);

        public override byte[] CreateCheckSum(byte[] frame) => this.origin.CreateLRC(frame);

        public override IProtocolResult Extraction(byte[] frame)
        {
            ProtocolResult<ModbusItem> result = new ProtocolResult<ModbusItem>(base.Request);
            result.Response = frame;

            //1. LRC 검사
            if (this.origin.CheckLRC(frame))
            {
                result.Type = ResultType.CheckSum_Error;
                result.ErrorMessage = "CRC Mismatch";
            }
            else
            {
                result.Type = ResultType.Success;

                //2. RTU변환
                byte[] ascii_full_req = new byte[base.Request.Length - 5];
                byte[] ascii_full_res = new byte[frame.Length - 5];

                Buffer.BlockCopy(base.Request, 1, ascii_full_req, 0, ascii_full_req.Length);
                Buffer.BlockCopy(frame, 1, ascii_full_res, 0, ascii_full_res.Length);

                byte[] rtu_req = this.origin.AsciiToRTU(ascii_full_req);
                byte[] rtu_res = this.origin.AsciiToRTU(ascii_full_res);

                //3. PDU 추출
                byte[] req_PDU = new byte[rtu_req.Length - 1];
                byte[] res_PDU = new byte[rtu_res.Length - 1];

                Buffer.BlockCopy(rtu_req, 1, req_PDU, 0, req_PDU.Length);
                Buffer.BlockCopy(rtu_res, 1, res_PDU, 0, res_PDU.Length);

                //4. ErrorCode 검사
                if (res_PDU[0] > 0x80)
                {
                    result.Type = ResultType.Protocol_Exception;
                    result.ErrorMessage = "Protocol Error";
                }
                else
                {
                    //5. Item 추출
                    ModbusItem[] items = this.unpacker.Parse(req_PDU, res_PDU);
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
            }

            return result;
        }

        public override void Initialize()
        {
            base.Request = null;
            this.origin.Initialize();
        }
    }
}
