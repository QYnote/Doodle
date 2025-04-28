using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetFrameWork.Communication.Ports
{
    public class QY_SerialPort: PCPort
    {
        public override bool IsOpen
        {
            get
            {
                if (this.Port == null || this.Port.IsOpen == false)
                    return false;   
                else
                    return true;
            }
        }

        public override int ReadBufferLength
        { 
            get
            {
                if (this.IsOpen == false)
                    return 0;
                else
                    return this.Port.ReadBufferSize;
            }
        }

        private SerialPort Port { get; set; }
        public int BaudRate {  get; set; }
        public int DataBits {  get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }

        public QY_SerialPort(string comName):base(comName)
        {
            this.Port = new SerialPort();
            this.BaudRate = 9600;
            this.DataBits = 8;
            this.Parity = Parity.None;
            this.StopBits = StopBits.One;

            Initialize();
        }

        public override void Initialize()
        {
            this.Port.PortName = base.Name;
            this.Port.BaudRate = this.BaudRate;
            this.Port.DataBits = this.DataBits;
            this.Port.Parity = this.Parity;
            this.Port.StopBits = this.StopBits;
        }

        /// <summary>
        /// Port 열기
        /// </summary>
        /// <returns>true: 열기 완료, false: 열기 오류</returns>
        public override bool Open()
        {
            if (this.Port == null)
            {
                this.Port = new SerialPort();
            }
            else
            {
                if (this.Port.IsOpen)
                {
                    Debug.WriteLine(string.Format("[Alarm]({0}) Port Already Open", base.Name));
                    return true;
                }
            }

            try
            {
                Initialize();

                this.Port.Open();
            }
            catch
            {
                Debug.WriteLine(string.Format("[Error]({0}) Port Open Fail", base.Name));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Port 닫기
        /// </summary>
        /// <returns>true: 닫기 완료, false: 닫기 오류</returns>
        public override bool Close()
        {
            if (this.Port == null) return true;

            try
            {
                if (this.Port.IsOpen) this.Port.Close();
            }
            catch
            {
                Debug.WriteLine(string.Format("[Error]({0}) Port Close Fail", base.Name));
                return false;
            }
            finally
            {
                this.Port = null;
            }

            return true;
        }

        /// <summary>
        /// Port 읽기
        /// </summary>
        /// <param name="lastBuffer">담아갈 byte Array</param>
        /// <returns>읽어서 누적된 ByteArray</returns>
        public override byte[] Read(byte[] lastBuffer)
        {
            try
            {
                if (this.IsOpen)
                {
                    if (this.Port.BytesToRead > 0)
                    {
                        //Data 읽기
                        byte[] portBuffer = new byte[this.Port.BytesToRead];

                        this.Port.Read(portBuffer, 0, portBuffer.Length);

                        if (lastBuffer != null || lastBuffer.Length != 0)
                        {
                            //Data 누적
                            byte[] tempBuffer = new byte[lastBuffer.Length + portBuffer.Length];

                            Buffer.BlockCopy(lastBuffer, 0, tempBuffer, 0, lastBuffer.Length);
                            Buffer.BlockCopy(lastBuffer, 0, tempBuffer, lastBuffer.Length, portBuffer.Length);

                            lastBuffer = tempBuffer;
                        }
                        else
                        {
                            lastBuffer = portBuffer;
                        }
                    }
                }
                else
                {
                    throw new Exception(string.Format("[Error]({0})Port Closed", base.Name));
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("({0})Port Read Error\nMessage: {1}\n\nTrack: {2}",
                    base.Name, e.Message, e.StackTrace));
            }

            return lastBuffer;
        }

        /// <summary>
        /// Port Data 쓰기
        /// </summary>
        /// <param name="buffer">전송할 Data Byte Array</param>
        /// <returns>Write 성공 여부</returns>
        public override bool Write(byte[] buffer)
        {
            try
            {
                if (this.IsOpen)
                {
                    this.Port.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    throw new Exception(string.Format("[Error]({0})Port Closed", base.Name));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("({0})Port Read Error\nMessage: {1}\n\nTrack: {2}",
                    base.Name, e.Message, e.StackTrace));

                return false;
            }

            return true;
        }
    }
}
