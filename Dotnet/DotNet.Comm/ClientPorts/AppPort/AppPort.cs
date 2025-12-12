using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.ClientPorts.AppPort
{
    /// <summary>
    /// Application Port 기준
    /// </summary>
    /// <remarks>
    /// Application ↔ OS 통신 Port
    /// </remarks>
    public class AppPort
    {
        /// <summary>
        /// 통신 Port Log
        /// </summary>
        /// <remarks>
        /// 이 이벤트가 있는 Class는 통신 Port가 아닌 Application Port임<br/>
        /// Param[0] = Log 내용
        /// </remarks>
        public event Utils.Controls.Utils.UpdateUI_WithParam ComPortLog;

        private CommType _commType = CommType.Serial;
        private OSPort.OSPortBase _osPort = new OSPort.QYSerialPort();
        private bool _isUserOpen = false;

        /// <summary>
        /// 통신 Port 종류
        /// </summary>
        /// <remarks>
        /// 기존 종류와 다를경우 통신Port 신규 생성
        /// </remarks>
        public CommType CommType
        {
            get => this._commType;
            set
            {
                if(this._commType != value)
                {
                    if (value == CommType.Serial)
                        this._osPort = new OSPort.QYSerialPort();
                    else if (value == CommType.Ethernet)
                        this._osPort = new OSPort.QYEthernet(false);

                    this._osPort.Log += (msg) => { this.ComPortLog?.Invoke(msg); };

                    this._commType = value;
                }
            }
        }
        /// <summary>
        /// OS ↔ Deivce 통신 Port
        /// </summary>
        public OSPort.OSPortBase OSPort { get => this._osPort; }
        /// <summary>
        /// 사용자의 Port Open여부
        /// </summary>
        public bool IsUserOpen { get => this._isUserOpen; }
        /// <summary>
        /// Port 연결
        /// </summary>
        /// <returns>연결 결과</returns>
        public bool Connect()
        {
            if (this.OSPort.IsOpen) return false;

            this._isUserOpen = true;

            this.OSPort.Open();

            return true;
        }
        /// <summary>
        /// Port 연결 해제
        /// </summary>
        /// <returns>해제 결과</returns>
        public bool Disconnect()
        {
            this._isUserOpen = false;

            this.OSPort.Close();
            this.OSPort.InitPort();

            return true;
        }
        /// <summary>
        /// OS Port에서 Data 읽기
        /// </summary>
        /// <returns>읽은 Data</returns>
        public byte[] Read() => this.OSPort.Read();
        /// /// <summary>
        /// OS Port에 Data 쓰기
        /// </summary>
        /// <param name="bytes">전송할 Data</param>
        /// <returns>실행 결과</returns>
        public void Write(byte[] bytes) => this.OSPort.Write(bytes);

        public void Initialize() => this.OSPort.InitPort();
    }
}
