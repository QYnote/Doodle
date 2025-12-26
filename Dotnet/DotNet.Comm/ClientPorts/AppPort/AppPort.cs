using DotNet.Utils.Controls.Utils;
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
    public abstract class AppPort : QYBindingBase
    {
        /// <summary>
        /// 통신 Port Log
        /// </summary>
        /// <remarks>
        /// 이 이벤트가 있는 Class는 통신 Port가 아닌 Application Port임<br/>
        /// Param[0] = Log 내용
        /// </remarks>
        public event Utils.Controls.Utils.Update_WithParam ComPortLog;

        private PortType _commType = PortType.Serial;
        private OSPort.OSPortBase _osPort = new OSPort.QYSerialPort();
        private bool _isAppOpen = false;

        /// <summary>
        /// OS Port 종류
        /// </summary>
        /// <remarks>
        /// 기존 종류와 다를경우 통신Port 신규 생성
        /// </remarks>
        public PortType PortType
        {
            get => this._commType;
            set
            {
                if(this._commType != value)
                {
                    if (value == PortType.Serial)
                        this._osPort = new OSPort.QYSerialPort();
                    else if (value == PortType.Ethernet)
                        this._osPort = new OSPort.QYEthernet(false);
                    this._osPort.Log += (msg) => { this.ComPortLog?.Invoke(msg); };

                    this._commType = value;

                    base.OnPropertyChanged(nameof(this.PortType));
                }
            }
        }
        /// <summary>
        /// OS ↔ Deivce 통신 Port
        /// </summary>
        public OSPort.OSPortBase OSPort { get => this._osPort; }
        /// <summary>
        /// Application Port Open여부
        /// </summary>
        public bool IsAppOpen
        {
            get => this._isAppOpen;
            protected set
            {
                if(this.IsAppOpen != value)
                {
                    this._isAppOpen = value;

                    base.OnPropertyChanged(nameof(this.IsAppOpen));
                }
            }
        }
        /// <summary>
        /// Port 연결
        /// </summary>
        /// <returns>연결 결과</returns>
        public abstract bool Connect();
        /// <summary>
        /// Port 연결 해제
        /// </summary>
        /// <returns>해제 결과</returns>
        public abstract bool Disconnect();
        /// <summary>
        /// OS Port에서 Data 읽기
        /// </summary>
        /// <returns>읽은 Data</returns>
        public abstract byte[] Read();
        /// /// <summary>
        /// OS Port에 Data 쓰기
        /// </summary>
        /// <param name="bytes">전송할 Data</param>
        /// <returns>실행 결과</returns>
        public abstract void Write(byte[] bytes);
        public abstract void Initialize();
    }
}
