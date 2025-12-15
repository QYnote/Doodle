using DotNet.Comm;
using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using DotNetFrame.Model.CommTester;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.ViewModel.CommTester
{
    internal class VM_CommTester
    {
        /// <summary>
        /// OS Port Log
        /// </summary>
        /// <remarks>
        /// Param[0]: Log Text
        /// </remarks>
        public event Update_WithParam OSPortLog;
        /// <summary>
        /// Request 전송 후 이벤트
        /// </summary>
        /// <remarks>
        /// Param[0] = RequestByte
        /// </remarks>
        public event Update_WithParam AfterSendRequest;
        /// <summary>
        /// 데이터 읽은 후 Port에 누적된 Buffer
        /// </summary>
        /// <remarks>
        /// Param[0]: Port의 현재 누적 Buffer
        /// </remarks>
        public event Update_WithParam PortCurrentBuffer;
        /// <summary>
        /// Request 에러코드 미일치
        /// </summary>
        /// <remarks>
        /// Param[0]: Response Data
        /// </remarks>
        public event Update_WithParam Error_ErrorCode;
        /// <summary>
        /// Request Protocol 에러
        /// </summary>
        /// <remarks>
        /// Param[0]: Response Data
        /// </remarks>
        public event Update_WithParam Error_Protocol;
        /// <summary>
        /// Request 정상종료
        /// </summary>
        /// <remarks>
        /// Param[0]: Request Data<br/>
        /// Param[1]: Response Data
        /// </remarks>
        public event Update_WithParam RequestComplete;
        /// <summary>
        /// Request Timeout
        /// </summary>
        /// <remarks>
        /// Param[0]:<br/>
        /// None Response: Request 응답없음<br/>
        /// Long Response: Receive Data가 끊임없이 들어올 경우<br/>
        /// Stop Response: Receive 중단됨<br/>
        /// <br/>
        /// Param[1]: Port의 누적된 Buffer
        /// </remarks>
        public event Update_WithParam RequestTimeout;

        private M_CommTester _commTester = new M_CommTester();

        internal CommType PortType
        {
            get => this._commTester.PortType;
            set => this._commTester.PortType = value;
        }
        internal ProtocolType Protocol
        {
            get => this._commTester.ProtocolType;
            set => this._commTester.ProtocolType = value;
        }
        internal bool ErrCode_add
        {
            get => this._commTester.ErrCode_add;
            set => this._commTester.ErrCode_add = value;
        }
        internal bool Do_repeat
        {
            get => this._commTester.Do_repeat;
            set => this._commTester.Do_repeat = value;
        }
        internal int Do_repeat_count
        {
            get => this._commTester.Do_repeat_count;
            set => this._commTester.Do_repeat_count = value;
        }
        internal bool Do_repeat_infinity
        {
            get => this._commTester.Do_repeat_infinity;
            set => this._commTester.Do_repeat_infinity = value;
        }
        internal bool IsOpen { get => this._commTester.IsOpen; }
        internal bool IsWriting { get => this._commTester.IsWriting; }

        internal QYSerialPort SerialPort
        {
            get
            {
                if (this._commTester.OSPort is QYSerialPort port)
                    return port;

                return null;
            }
        }
        internal QYEthernet EthernetPort
        {
            get
            {
                if (this._commTester.OSPort is QYEthernet port)
                    return port;

                return null;
            }
        }

        internal VM_CommTester()
        {
            this._commTester.OSPortLog += (obj) => { this.OSPortLog?.Invoke(obj); };
            this._commTester.AfterSendRequest += (obj) => { this.AfterSendRequest?.Invoke(obj); };
            this._commTester.PortCurrentBuffer += (obj) => { this.PortCurrentBuffer?.Invoke(obj); };
            this._commTester.Error_ErrorCode += (obj) => { this.Error_ErrorCode?.Invoke(obj); };
            this._commTester.Error_Protocol += (obj) => { this.Error_Protocol?.Invoke(obj); };
            this._commTester.RequestComplete += (obj) => { this.RequestComplete?.Invoke(obj); };
            this._commTester.RequestTimeout += (obj) => { this.RequestTimeout?.Invoke(obj); };
        }

        internal bool Connect() => this._commTester.Connect();
        internal bool Disconnect() => this._commTester.Disconnect();
        internal bool Register_Data(string text) => this._commTester.Register_Data(text);
    }
}
