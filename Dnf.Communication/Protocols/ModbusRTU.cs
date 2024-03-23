using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Communication.Protocols
{
    internal class ModbusRTU
    {
        string DebugStr = "";
        /* 코일/레지스터 번호   데이터 주소  유형  테이블 이름
         * 1     ~  9999        0000 ~ 270E  R/W   개별 출력 코일
         * 10001 ~ 19999        0000 ~ 270E  R     개별 입력 접점
         * 30001 ~ 39999        0000 ~ 270E  R     아날로그 입력 레지스터
         * 40001 ~ 49999        0000 ~ 270E  R/W   아날로그 출력 홀딩 레지스터
         */
        public byte[] Frame;
        public Queue<byte[]> ReqFrameData = new Queue<byte[]>();

        /// <summary>
        /// 전송할 Data 구조 기초공사
        /// </summary>
        /// <param name="slaveAddress">Slave Address</param>
        /// <param name="functionCode">Modbus 기능코드</param>
        internal void CreateFrameBase(int slaveAddress, int functionCode)
        {
            byte byteAddr = (byte)slaveAddress;
            byte byteFunc = (byte)functionCode;

            Frame = new byte[] { byteAddr, byteFunc };
        }

        /// <summary>
        /// Request Func01(0x01), 02(0x02), 03(0x03), 04(0x04), 06(0x06)
        /// </summary>
        /// <param name="startAddress">시작 데이터 주소</param>
        /// <param name="dataCnt">데이터 개수</param>
        internal void CreateFrame_Request(int startAddress, int dataCnt)
        {
            //Function Code : 01(0x01), 02(0x02), 03(0x03), 04(0x04), 06(0x06)

            byte[] byteAddr = UtilCustom.IntToByte2(startAddress);
            byte[] byteCnt = UtilCustom.IntToByte2(dataCnt);

            Frame = UtilCustom.BytesAppend(Frame, byteAddr);
            Frame = UtilCustom.BytesAppend(Frame, byteCnt);

            //생성된 데이터 저장
            ReqFrameData.Enqueue(Frame);
            Frame = null;
        }

        /// <summary>
        /// Request Func16(0x10)
        /// </summary>
        /// <param name="startAddress">시작 데이터 주소</param>
        /// <param name="regQuantity">시작 레지스트리 번호</param>
        /// <param name="dataList">읽어올 데이터 주소 리스트</param>
        internal void CreateFrame_RequestMulti(int startAddress, int regQuantity, int[] dataList)
        {
            //Function Code : 16(0x10)
            byte[] byteAddr = UtilCustom.IntToByte2(startAddress);
            byte[] byteReg = UtilCustom.IntToByte2(regQuantity);
            byte byteQuantity = (byte)dataList.Length;

            Frame = UtilCustom.BytesAppend(Frame, byteAddr);
            Frame = UtilCustom.BytesAppend(Frame, byteReg);
            Frame = UtilCustom.BytesAppend(Frame, new byte[] { byteQuantity });

            foreach (int data in  dataList)
            {
                byte[] byteData = UtilCustom.IntToByte2(data);

                Frame = UtilCustom.BytesAppend(Frame, byteData);
            }

            //생성된 데이터 저장
            ReqFrameData.Enqueue(Frame);
            Frame = null;
        }

        /// <summary>
        /// Recieve된 Data 검증 및 데이터 적용
        /// </summary>
        /// <param name="recvData"></param>
        /// <returns></returns>
        internal void CheckFrame_Response(byte[] recvData)
        {
            //전송한 Data 중 먼저 보낸 Data
            byte[] reqData = ReqFrameData.Peek();

            //Receive데이터가 Request데이터에 맞는 데이터인지 확인
            if (reqData[0] == recvData[0] && reqData[1] == recvData[1])
            {
                DebugStr += string.Format("RecieveData - SlaveAddr : {0} / Func : {1}", recvData[0], recvData[1]);
                byte func = recvData[1];
                switch (func)
                {
                    case 0x01:
                    case 0x02:
                    case 0x03:
                    case 0x04:
                        InteruptData_Common(recvData, reqData);
                        break;
                    case 0x06:
                        InteruptData_06(recvData);
                        break;
                }

                Console.WriteLine(DebugStr); DebugStr = "";
                ReqFrameData.Dequeue();
            }
        }

        /// <summary>
        /// Response 데이터 해석 / Func01(0x01), 02(0x02), 03(0x03), 04(0x04)
        /// </summary>
        /// <param name="recvData">Response Data</param>
        /// <param name="reqData">Request Data</param>
        internal void InteruptData_Common(byte[] recvData, byte[] reqData)
        {
            //시작주소 해석(AddrHi Bit Shift + AddrLo)
            int startAddress = (reqData[2] << 8) + reqData[3];
            int DataCnt = recvData[2];
            DebugStr += string.Format("Data Byte Count : {0}\n", DataCnt);

            //Data주소 핸들
            int addressHandle = startAddress;
            int applyData;

            for (int i = 0; i < DataCnt; i+=2)
            {
                //가져온 Data 해석(DataHi Bit Shift + DataLo)
                applyData = (recvData[3 + i] << 8) + recvData[4 + i];

                //임시
                int dataAddr = addressHandle;
                int data = applyData;
                DebugStr += string.Format("Data Addr : {0:D2} - Data : {1}\n", addressHandle, applyData);

                addressHandle++;
            }
        }

        /// <summary>
        /// Response 데이터 해석 / Func06(0x06)
        /// </summary>
        /// <param name="recvData">Response Data</param>
        internal void InteruptData_06(byte[] recvData)
        {
            //시작주소 해석(AddrHi Bit Shift + AddrLo)
            int RegAddr = (recvData[3] << 8) + recvData[4];
            int RegData = (recvData[5] << 8) + recvData[6];

            DebugStr += string.Format("Register Addr : {0:D2} - PresitData : {1}\n", RegAddr, RegData);
        }
    }
}
