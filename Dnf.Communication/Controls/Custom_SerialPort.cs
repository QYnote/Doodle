using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    internal class Custom_SerialPort : Port
    {
        //Serial포트 정보
        SerialPort port;
        internal BaudRate BaudRate;   //Baud Rate
        internal int DataBits;        //DataBits
        internal Parity Parity;       //Parity Bit
        internal StopBits StopBIt;    //Stop Bit

        internal Custom_SerialPort(string portName, uProtocolType type, BaudRate baud, int databits, StopBits stopBits, Parity parity) : base(portName, type)
        {
            port = new SerialPort();

            this.BaudRate = baud;
            this.DataBits = databits;
            this.Parity = parity;
            this.StopBIt = stopBits;
        }

        #region SerialPort

        /// <summary>
        /// 포트 연결 상태
        /// </summary>
        internal override bool IsOpen
        { 
            get
            {
                //포트가 없거나 열려있지 않으면 false
                if(this.port == null || this.port.IsOpen == false) 
                    return false;
                else 
                    return true;
            } 
        }

        /// <summary>
        /// SerialPort Open
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        internal override bool Open()
        {
            SerialPort serial = port;

            //포트 사용중 유무 확인
            if (!serial.IsOpen)
            {
                //필수정보 확인
                if (PortName == null)
                {
                    Debug.WriteLine("[ERROR]{0} - Open() - PortName Empty", base.PortName);
                    return false;
                }

                serial.Open();
                base.BgWorker.RunWorkerAsync();
            }
            else
            {
                Debug.WriteLine("[ERROR]{0} - Open() - Port alreay opened", base.PortName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// SerialPort Close
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        internal override bool Close()
        {
            SerialPort serial = port;

            //포트가 미사용 중인지 확인
            if (!serial.IsOpen)
            {
                Debug.WriteLine("[ERROR]{0} - Close() - Port Not Open", base.PortName);
                return false;
            }

            serial.Close();
            base.BgWorker.CancelAsync();

            return true;
        }

        /// <summary>
        /// SerialPort Write
        /// </summary>
        internal override void Write(byte[] bytes)
        {
            try
            {
                SerialPort serial = port;

                if (serial.IsOpen)
                {
                    serial.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    Debug.WriteLine(string.Format("[ERROR]{0} - PortWrite() - PortClose", base.PortName));
                    return ;
                }

                return ;
            }
            catch
            {
                Debug.WriteLine(string.Format("[ERROR]{0} - PortWrite() - Try Error", base.PortName));
                return ;
            }
        }

        /// <summary>
        /// SerialPort Read
        /// </summary>
        /// <param name="ClassBuffer">담아갈 Byte[]</param>
        internal override void Read()
        {
            try
            {
                if (this.port != null && this.port.IsOpen == true)
                {
                    if(this.port.BytesToRead > 0)
                    {
                        byte[] bufffer = new byte[this.port.BytesToRead];

                        this.port.Read(bufffer, 0, bufffer.Length);

                        if (base.ReadingBuffer != null)
                        {
                            base.ReadingBuffer.BytesAppend(bufffer);
                        }
                        else
                            base.ReadingBuffer = bufffer;
                    }
                    return;
                }
                else
                {
                    UtilCustom.DebugWrite(string.Format("{0} Port 닫힘", base.PortName));
                }
            }
            catch(Exception ex)
            {
                UtilCustom.DebugWrite(string.Format("Try Error : {0}", ex.Message));
            }

            return;
        }

        #region ModbusRTU

        #region Query보낼 시 Modebus 구조

        private void ModbusRTU_Write(SerialPort serial)
        {
            byte[] cmd = CmdQuery_Base();

            serial.Write(cmd, 0, cmd.Length);

            Debug.WriteLine("Write Onetime");
        }

        private byte[] CmdQuery_Base()
        {
            byte[] cmd;
            //ADU - Address(Slave ID)
            int address = 1;
            byte addr = Convert.ToByte(address);

            //ADU,PDU - FunctionCode
            int functionCode = 2;
            byte funcCode = Convert.ToByte(functionCode);

            //Start Address
            int startAddr = 3;
            byte addrHi = (byte)(startAddr >> 8);
            byte addrLo = (byte)startAddr;

            int value = 4;
            byte valueHi = (byte)(value >> 8);
            byte valueLo = (byte)value;

            cmd = new byte[] { addr, funcCode, addrHi, addrLo, valueHi, valueLo };

            if (functionCode == 10)
            {
                cmd = CmdQuery_Multi(cmd);
            }

            return cmd;
        }

        private byte[] CmdQuery_Multi(byte[] bytesHead)
        {
            //Func : 01[0x01], 02[0x02], 03[0x03], 04[0x04], 06[0x06]
            //bytesHead = bytesHead.Concat(new byte[] { addrHi, addrLo }).ToArray();  //기존 Byte 배열에 신규 추가

            ////CRC
            //byte crcLow = readBytes[readBytes.Length - 2];
            //byte crcHi = readBytes[readBytes.Length - 1];
            //debugStr += " / CRC : " + crcLow + crcHi;

            return bytesHead;
        }

        #endregion Query보낼 시 Modebus 구조 End

        private void ModbusRTU_Read()
        {
            SerialPort serial = this.port;

            /*DataReceived이벤트가 아니라 Data Read를 사용하는 이유
             * 이벤트를 통해 Read 할경우 데이터를 buffer에 저장하는데
             * 데이터가 너무 많이 도착하면 오버플로우가 발생하여 데이터 손실이 생길 수 있고 메모리 사용량이 증가하기 때문*/
            byte[] readBytes = new byte[serial.BytesToRead]; //Read할 수 잇는 수만큼 byte 설정
            serial.Read(readBytes, 0, serial.BytesToRead);   //Receive된 Byte를 0번쨰부터 개수만큼 readbuff에 복사

            //J1C에서는 1byte 32이하의 문자들은 전송이 불가능하므로 통신 시에 1byte만 받으니 당황하지 말고 2byte 데이터 받으려 하지 말것
            //byte(8bit) -> int
            string addr = string.Format("{0:D2}", readBytes[0]);
            string func = string.Format("{0:D2}", readBytes[1]);

            //Slave ID(Address)
            //Function Code

            //Function Code에 따라 Protocol 구조가 다름에 따른 Frame 구분
            switch (func)
            {
                case "03": break;
                default: CmdResponse_ReadRegister(readBytes); break;
            }

            //ModbusASCII용
            //byte 10진수 -> 16진수 변환
            //str += string.Format("{0:X2} ", b);
        }

        #region Response시 Modbus구조

        private void CmdResponse_Coil(byte[] readBytes)
        {
            //Func : 01[0x01], 02[0x02]
            //Data Length
            int DataCnt = readBytes[2];
            //Test시 입력가능한 ! 가 33이라 데이터 최소 33개부터 가져와야함

            //Data

            for (int i = 0; i < DataCnt; i++)
            {
                //RTU 통신일 경우 byte ASCII 변환(ex. 'a' 데이터 Receive : 받는 데이터 '97', ASCII 변환 시 'a'
            }

            ////CRC
            //byte crcLow = readBytes[readBytes.Length - 2];
            //byte crcHi = readBytes[readBytes.Length - 1];
            //debugStr += " / CRC : " + crcLow + crcHi;
        }

        private void CmdResponse_ReadRegister(byte[] readBytes)
        {
            //Func : 03[0x03], 04[0x04]
            //Data Length
            int DataCnt = readBytes[2];
            //Test시 입력가능한 ! 가 33이라 데이터 최소 33개부터 가져와야함

            ////CRC
            //byte crcLow = readBytes[readBytes.Length - 2];
            //byte crcHi = readBytes[readBytes.Length - 1];
            //debugStr += " / CRC : " + crcLow + crcHi;
        }

        private void CmdResponse_WriteRegister(byte[] readBytes)
        {
            //Func : 06[0x06], 16[0x10]
            //Data Length
            string regAddr = "";
            regAddr += string.Format("{0:D2}", readBytes[2]) + string.Format("{0:D2}", readBytes[3]);

            //Data
            string regData = "";
            regData += string.Format("{0:D2}", readBytes[4]) + string.Format("{0:D2}", readBytes[5]);

            ////CRC
            //byte crcLow = readBytes[readBytes.Length - 2];
            //byte crcHi = readBytes[readBytes.Length - 1];
            //debugStr += " / CRC : " + crcLow + crcHi;
        }

        #endregion Response시 Modbus구조 End

        #endregion ModbusRTU

        #endregion SerialPort End
    }
}
