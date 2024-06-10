using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Dnf.Communication.Controls
{
    internal class PortSerial:PortBase
    {
        internal PortSerial(string COMName, string BaudRate, int DataBits, Parity Parity, StopBits StopBits)
        {
            Port = new SerialPort();

            this.COMName = COMName;
            this.BaudRate = BaudRate;
            this.DataBits = DataBits;
            this.Parity = Parity;
            this.StopBit = StopBits;
        }
        /// <summary>
        /// 서버와 연결할 Serial Port
        /// </summary>
        private SerialPort Port { get; set; }
        /// <summary>
        /// 연결할 Port명
        /// </summary>
        internal string COMName {  get; set; }
        /// <summary>
        /// 통신속도
        /// </summary>
        internal string BaudRate { get; set; }
        /// <summary>
        /// 데이터 길이
        /// </summary>
        internal int DataBits { get; set; }
        /// <summary>
        /// Parity Bit
        /// </summary>
        internal Parity Parity { get; set; }
        /// <summary>
        /// Stop Bit
        /// </summary>
        internal StopBits StopBit { get; set; }
        /// <summary>
        /// Port 연결 상태
        /// </summary>
        internal override bool IsOpen
        {
            get
            {
                if(this.Port == null || this.Port.IsOpen == false)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// Port 열기
        /// </summary>
        /// <returns>true : 정상 열림 / false : 열기 실패</returns>
        internal override bool Open()
        {
            if (this.IsOpen == true)
            {
                this.Port.PortName = this.COMName;
                this.Port.BaudRate = Convert.ToInt32(this.BaudRate);
                this.Port.DataBits = this.DataBits;
                this.Port.Parity = this.Parity;
                this.Port.StopBits = this.StopBit;

                try
                {
                    this.Port.Open();
                }
                catch
                {
                    UtilCustom.DebugWrite("[ERROR]Port Open Fail");
                    return false;
                }
            }
            else
            {
                UtilCustom.DebugWrite("[Alart]Port Already Open");
            }
            return true;
        }
        /// <summary>
        /// Port 닫기
        /// </summary>
        /// <returns>true : 정상 닫기 / false : 닫기 실패</returns>
        internal override bool Close()
        {
            if(this.Port != null && this.Port.IsOpen == true)
            {
                this.Port.Close();
            }
            else
            {
                UtilCustom.DebugWrite("[Alart]Port Already Close");

                return false;
            }

            return true;
        }
        /// <summary>
        /// Port Data 읽어서 PortClass의 ReadingData에 쌓기
        /// </summary>
        /// <param name="buffer">담아갈 byte Array</param>
        internal override void Read(ref byte[] buffer)
        {
            try
            {
                if(this.IsOpen == true)
                {
                    if (this.Port.BytesToRead > 0)
                    {
                        byte[] portBufffer = new byte[this.Port.BytesToRead];

                        this.Port.Read(portBufffer, 0, portBufffer.Length);

                        if (buffer != null)
                            buffer.BytesAppend(portBufffer);
                        else
                            buffer = portBufffer;
                    }
                }
                else
                {
                    Debug.WriteLine(string.Format("[ERROR]{0} - Read() - PortClose", this.COMName));
                }
            }
            catch
            {
                Debug.WriteLine(string.Format("[ERROR]{0} - Read() - Try Error", this.COMName));
            }

            return;
        }
        /// <summary>
        /// Port Data 전송
        /// </summary>
        /// <param name="bytes">전송할 Data byte Array</param>
        internal override void Write(byte[] bytes)
        {
            try
            {
                if (this.IsOpen == true)
                {
                    this.Port.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    Debug.WriteLine(string.Format("[ERROR]{0} - Write() - PortClose", this.COMName));
                }
            }
            catch
            {
                Debug.WriteLine(string.Format("[ERROR]{0} - Write() - Try Error", this.COMName));
            }
        }
    }
}
