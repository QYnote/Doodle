using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication
{
    /// <summary>
    /// 통신방법 종류
    /// </summary>
    public enum ProtocolType
    {
        ModBusAscii,
        ModBusRTU,
        ModBusTcpIp
    }

    public enum BaudRate
    {
        _9600,
        _14400
    }

    public class Port
    {
        public Component portComponent; //포트 구조 ex) 직렬 - SerialPort인지, EthernetPort인지
        public Dictionary<int, Unit> Units { get; }        //Port에 연결된 하위 Unit들(ex. PLC, 센서 등), <slaveAddr, Unit>

        //Serial포트 정보
        public string PortName { get; }     //UI에 표시되는 Port 이름(COM3 COM4 등)
        public BaudRate BaudRate { get; }   //Baud Rate
        public int DataBits { get; }        //DataBits
        public Parity Parity { get; }       //Parity Bit
        public StopBits StopBIt { get; }    //Stop Bit
        public ProtocolType ProtocolType { get; }   //통신방법

        //개발용
        private string DebugStr;

        public Port(string portName, ProtocolType type, BaudRate baud, int databits, Parity parity, StopBits stopBits)
        {
            if (type == ProtocolType.ModBusRTU
                || type == ProtocolType.ModBusAscii)
            {
                portComponent = new SerialPort();

                PortName = portName;
                ProtocolType = type;
                BaudRate = baud;
                DataBits = databits;
                Parity = parity;
                StopBIt = stopBits;
            }

            Units = new Dictionary<int, Unit>();
        }

        /// <summary>
        /// 연결된 포트 구분하여 열기
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        public bool PortOpen()
        {
            if(this.ProtocolType == ProtocolType.ModBusRTU
                || this.ProtocolType == ProtocolType.ModBusAscii)
            {
                return SerialPort_Open();
            }

            return false;
        }

        /// <summary>
        /// 연결된 포트 구분하여 닫기
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        public bool PortClose()
        {
            if (this.ProtocolType == ProtocolType.ModBusRTU
                || this.ProtocolType == ProtocolType.ModBusAscii)
            {
                return SerialPort_Close();
            }

            return false;
        }

        public bool PortSend()
        {
            if (this.ProtocolType == ProtocolType.ModBusRTU
                || this.ProtocolType == ProtocolType.ModBusAscii)
            {
                return SerialPort_Send();
            }

            return false;
        }

        #region SerialPort

        /// <summary>
        /// SerialPort Open
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        private bool SerialPort_Open()
        {
            //Serial Open인데 다른걸로 지정되있는지 확인
            if(portComponent == null || portComponent.GetType() != typeof(SerialPort))
            {
                Console.WriteLine("({0})Port Componet is null or Not SerialPort / ProtocolType : {1}", this.PortName, this.ProtocolType);
                return false;
            }

            SerialPort serial = portComponent as SerialPort;

            //포트 사용중 유무 확인
            if (!serial.IsOpen)
            {
                //필수정보 확인
                if(PortName == null)
                {
                    Console.WriteLine("Port Name is empty");
                    return false;
                }

                serial.PortName = PortName;

                serial.DataReceived += SerialPort_Recived;
                serial.Open();
            }
            else
            {
                Console.WriteLine("({0}) Port is alreay opened", this.PortName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// SerialPort Close
        /// </summary>
        /// <returns>true : Success / false : Fail</returns>
        private bool SerialPort_Close()
        {
            //Serial Open인데 다른걸로 지정되있는지 확인
            if (portComponent == null || portComponent.GetType() != typeof(SerialPort))
            {
                Console.WriteLine("({0})Port Componet is null or Not SerialPort / ProtocolType : {1}", this.PortName, this.ProtocolType);
                return false;
            }

            SerialPort serial = portComponent as SerialPort;

            //포트가 미사용 중인지 확인
            if (!serial.IsOpen)
            {
                Console.WriteLine("({0}) Port is alreay closed", this.PortName);
                return false;
            }


            serial.DataReceived -= SerialPort_Recived;
            serial.Close();

            return true;

        }

        #region ModbusRTU

        /// <summary>
        /// SerialPort 데이터 전송
        /// </summary>
        /// <returns></returns>
        private bool SerialPort_Send()
        {
            try
            {
                SerialPort serial = portComponent as SerialPort;

                if (serial.IsOpen)
                {
                    ModbusRTU_Write(serial);
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SerialPort_Recived(object sender, SerialDataReceivedEventArgs e)
        {
            ModbusRTU_Read();

            Console.WriteLine("Recived Onetime");
        }

        #region Query보낼 시 Modebus 구조

        private void ModbusRTU_Write(SerialPort serial)
        {
            byte[] cmd = CmdQuery_Base();

            serial.Write(cmd, 0, cmd.Length);

            Console.WriteLine(DebugStr);
            Console.WriteLine("Write Onetime");
        }

        private byte[] CmdQuery_Base()
        {
            byte[] cmd;
            DebugStr = "";

            //ADU - Address(Slave ID)
            int address = 1;
            byte addr = Convert.ToByte(address);
            DebugStr += "Slave Address : " + address;

            //ADU,PDU - FunctionCode
            int functionCode = 2;
            byte funcCode = Convert.ToByte(functionCode);
            DebugStr += " / Function Code : " + functionCode + "\n";

            //Start Address
            int startAddr = 3;
            byte addrHi = (byte)(startAddr >> 8);
            byte addrLo = (byte)startAddr;
            DebugStr += string.Format("Start Reg Address : {0}", startAddr);

            int value = 4;
            byte valueHi = (byte)(value >> 8);
            byte valueLo = (byte)value;
            DebugStr += string.Format(" / Write Value : {0}", value);


            cmd = new byte[] { addr, funcCode, addrHi, addrLo, valueHi, valueLo };

            if(functionCode == 10)
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
            SerialPort serial = this.portComponent as SerialPort;
            DebugStr = "";

            byte[] readBytes = new byte[serial.BytesToRead]; //Recieve된 Byte들
            serial.BaseStream.Read(readBytes, 0, serial.BytesToRead);    //Recieve된 Byte를 0번쨰부터 개수만큼 readbuff에 복사

            //J1C에서는 1byte 32이하의 문자들은 전송이 불가능하므로 통신 시에 1byte만 받으니 당황하지 말고 2byte 데이터 받으려 하지 말것
            //byte(8bit) -> int
            string addr = string.Format("{0:D2}", readBytes[0]);
            string func = string.Format("{0:D2}", readBytes[1]);

            //Slave ID(Address)
            DebugStr += "Addr : " + addr + " / ";
            //Function Code
            DebugStr += "Func : " + func + "\n";

            //Function Code에 따라 Protocol 구조가 다름에 따른 Frame 구분
            switch (func)
            {
                case "03": break;
                default: CmdResponse_ReadRegister(readBytes); break;
            }

            Console.WriteLine(DebugStr);
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
            DebugStr = "Data Byte Count : " + DataCnt;

            //Data
            DebugStr += " / Data : ";

            for (int i = 0; i < DataCnt; i++)
            {
                //RTU 통신일 경우 byte ASCII 변환(ex. 'a' 데이터 Receive : 받는 데이터 '97', ASCII 변환 시 'a'
                DebugStr += readBytes[i + 3] + " ";
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
            DebugStr = "Data Byte Count : " + string.Format("{0:D2}", DataCnt) + "\n";

            for (int i = 0; i < DataCnt; i+=2)
            {
                DebugStr += string.Format("Data{0:D2} : ", (i / 2) + 1);
                DebugStr += readBytes[3 + i];
                DebugStr += readBytes[4 + i];
                DebugStr += "\n";
            }

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
            DebugStr += "Reg Start Addr : " + regAddr;

            //Data
            string regData = "";
            regData += string.Format("{0:D2}", readBytes[4]) + string.Format("{0:D2}", readBytes[5]);
            DebugStr += " / Data : " + regData;

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