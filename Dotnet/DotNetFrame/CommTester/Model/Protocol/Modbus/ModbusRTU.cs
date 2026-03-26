using DotNet.CommTester.Model.Protocol;
using DotNet.CommTester.Model.Protocol.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.Model.Protocol.Modbus
{
    internal class ModbusRTU : ModbusBase, IDisposable
    {
        DotNet.Comm.Protocols.ModbusRTU origin = new DotNet.Comm.Protocols.ModbusRTU();
        ModbusExtractor unpacker = new ModbusExtractor();

        internal ModbusRTU(CommPort handler) : base(handler)
        {
            base.Handler.PropertyChanged += Handler_PropertyChanged;
        }

        public override byte[] Parse(byte[] buffer) => this.origin.Parse(buffer);
        public override IProtocolResult Extraction(byte[] frame)
        {
            ProtocolResult<ModbusItem> result = new ProtocolResult<ModbusItem>(base.Request);
            result.Response = frame;

            //1. CRC 검사
            if (this.origin.CheckCRC(frame))
            {
                result.Type = ResultType.CheckSum_Error;
                result.ErrorMessage = "CRC Mismatch";
            }
            else
            {
                //2. PDU 추출
                byte[] reqPDU = new byte[base.Request.Length - 3];
                byte[] resPDU = new byte[frame.Length - 3];

                Buffer.BlockCopy(base.Request, 1, reqPDU, 0, reqPDU.Length);
                Buffer.BlockCopy(frame, 1, resPDU, 0, resPDU.Length);

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
            }

            return result;
        }
        public override byte[] CreateCheckSum(byte[] frame) => this.origin.CreateCRC(frame);

        private void Handler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //3.5 Char를 계산하기 위한 Baudrate 변경 시 Update 이벤트
            if(e.PropertyName == nameof(DotNet.Comm.Transport.QYSerialPort.BaudRate) &&
                base.Handler.Transport is DotNet.Comm.Transport.QYSerialPort serial)
            {
                this.origin.SetBaudrate(serial.BaudRate);
            }
        }

        public void Dispose()
        {
            //Baudrate Event 해제
            //Event 미해제 시 연결된 Event가 남기때문에 메모리 관리용으로 하는 작업
            base.Handler.PropertyChanged -= Handler_PropertyChanged;
        }

        public override void Initialize()
        {
            base.Request = null;
            this.origin.Initialize();
        }
    }
}
