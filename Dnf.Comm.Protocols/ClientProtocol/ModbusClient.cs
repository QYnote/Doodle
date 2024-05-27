using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Comm.Protocols.ClientProtocol
{
    /// <summary>Modbus Client → Server</summary>
    public class ModbusClient
    {
        /// <summary>생성된 전송 Frame Queue</summary>
        public Queue<byte[]> ReqFrameQueue = new Queue<byte[]>();
        /// <summary>수신 확인용 전송 Frame Queue</summary>
        public Queue<byte[]> SendingFrameQueue = new Queue<byte[]>();
        /// <summary>수신한 수신 Frame Queue</summary>
        public Queue<byte[]> RcvFrameQueue = new Queue<byte[]>();

        #region Request

        /// <summary>
        /// Request Read
        /// / Func : 01(0x01), 02(0x02), 03(0x03), 04(0x04)
        /// / Default : 03(0x03)
        /// </summary>
        /// <param name="clientAddr">Client Address 번호</param>
        /// <param name="dic">변경할 (Registry Address, Data) Dictionary</param>
        /// <param name="func">전송 Fuction Code / Default : 03(0x03)</param>
        public void CreateReq_Read(int clientAddr, Dictionary<short, short> dic, byte func = 0x03)
        {
            //Addr[1] + Func[1] + StartAddr[2] + DataCount[2]
            byte byteCliAddr = byte.Parse(clientAddr.ToString());   //Client Address byte형태 전환
            byte[] headBytes = new byte[] { byteCliAddr, func };    //고정 Header생성
            List<ArrangeStruct> dataList = ArrangeDictionary(dic);  //Dictionary 구조 변경

            foreach (ArrangeStruct arrange in dataList)
            {
                byte[] frame = new byte[0];
                //데이터 변환
                byte[] byteStartAddr = UtilCustom.IntToByte2(arrange.StartAddr);    //Start Address
                byte[] byteCnt = UtilCustom.IntToByte2(arrange.DataList.Count);     //Write Register Count

                //합치기
                frame.BytesAppend(headBytes);
                frame.BytesAppend(byteStartAddr);
                frame.BytesAppend(byteCnt);
                CRC.AddModbusCRC(frame);

                //전송 Queue 저장
                ReqFrameQueue.Enqueue(frame);
                SendingFrameQueue.Enqueue(frame);
            }
        }

        /// <summary>
        /// Request Write / Fuc : 0x10
        /// </summary>
        /// <param name="clientAddr">Client Address 번호</param>
        /// <param name="dic">변경할 (Registry Address, Data) Dictionary</param>
        /// <param name="func">전송할 FuctionCode</param>
        public void CreateReq_Write(int clientAddr, Dictionary<short, short> dic, byte func = 0x10)
        {
            //Addr[1] + Func[1] + StartAddr[2] + DataCount[2](max 127) + ByteCount[2](Max 254) + DataValue[N]
            byte byteCliAddr = byte.Parse(clientAddr.ToString());   //Client Address byte형태 전환
            byte[] headBytes = new byte[] { byteCliAddr, func };    //고정 Header생성
            List<ArrangeStruct> dataList = ArrangeDictionary(dic);  //Dictionary 구조 변경

            foreach (ArrangeStruct arrange in dataList)
            {
                byte[] frame = new byte[0];
                //데이터 변환
                byte[] byteStartAddr = UtilCustom.IntToByte2(arrange.StartAddr);    //Start Address
                byte[] regCnt = UtilCustom.IntToByte2(arrange.DataList.Count);      //Write Register Count
                byte[] byteCnt = UtilCustom.IntToByte2(arrange.DataList.Count * 2); //Write Byte Count

                //합치기
                frame.BytesAppend(headBytes);
                frame.BytesAppend(byteStartAddr);
                frame.BytesAppend(regCnt);
                frame.BytesAppend(byteCnt);

                foreach (short regData in arrange.DataList)
                {
                    frame.BytesAppend(UtilCustom.IntToByte2(regData));
                }

                //전송 Queue 저장
                ReqFrameQueue.Enqueue(frame);
                SendingFrameQueue.Enqueue(frame);
            }
        }

        /// <summary>전송 Data 구조</summary>
        private struct ArrangeStruct
        {
            /// <summary>시작 Registry Address</summary>
            internal short StartAddr;
            /// <summary>전송할 Data List / Read시 : Data 개수 확인, Write 시 : Data 개수 및 List 보관용</summary>
            internal List<short> DataList;
        }

        /// <summary>
        /// 전송 할 Data Dictionary 정리,
        /// 연속되는 경우 Registr의 Address일 경우 Address는 삭제하고 Data는 List에 보관하여 새로운 struct에 저장
        /// </summary>
        /// <param name="dic"></param>
        /// <returns>정리된 Dictionary 신규 구조</returns>
        private List<ArrangeStruct> ArrangeDictionary(Dictionary<short, short> dic)
        {
            //Registry Address로 정렬한 Address, Value List
            KeyValuePair<short, short>[] pairList = dic.OrderBy(key => key).ToArray();
            ArrangeStruct tempArrange = new ArrangeStruct();    //초기 struct 생성
            List<ArrangeStruct> sendList = new List<ArrangeStruct>();   //정리된 전송데이터 List

            foreach (KeyValuePair<short, short> pair in pairList)
            {
                //마지막 Pair일 경우
                if(pair.Key == pairList.Last().Key)
                {
                    if (tempArrange.DataList.Count > 0)
                    {
                        //마지막이자 처음인 Pair일 경우
                        tempArrange.StartAddr = pair.Key;
                        tempArrange.DataList.Add(pair.Value);

                        sendList.Add(tempArrange);
                    }
                    else
                    {
                        //그냥 마지막 Pair인 경우
                        tempArrange.DataList.Add(pair.Value);

                        sendList.Add(tempArrange);
                    }
                }
                else if(pair.Key == pairList.First().Key)
                {
                    //첫 Pair일 경우
                    tempArrange.StartAddr = pair.Key;
                    tempArrange.DataList.Add(pair.Value);
                }
                else
                //중간 Pair일 경우
                {
                    if(pair.Key == tempArrange.StartAddr + tempArrange.DataList.Count)
                    //연속되는 Pair일 경우
                    {
                        if (tempArrange.DataList.Count >= 127)
                        //연속되기는 하는데 개수가 127개보다 커질경우
                        //Modbus가 127개까지만 지원함
                        { 
                            sendList.Add(tempArrange);

                            tempArrange = new ArrangeStruct();
                            tempArrange.StartAddr = pair.Key;
                            tempArrange.DataList.Add(pair.Value);
                        }
                        else
                        //연속되고 연속 개수가 127미만일 경우
                        {
                            tempArrange.StartAddr = pair.Key;
                            tempArrange.DataList.Add(pair.Value);
                        }
                    }
                    else
                    //연속되지 않는 Pair일 경우
                    {
                        sendList.Add(tempArrange);

                        tempArrange = new ArrangeStruct();
                        tempArrange.StartAddr = pair.Key;
                        tempArrange.DataList.Add(pair.Value);
                    }
                }
            }//End For

            return sendList;
        }

        #endregion Request
        #region Receive

        public void RcvData(byte[] rcvBytes, byte[] reqBytes)
        {
            //누적된 Receive Data에서 Protocol 길이만큼 추출
            //1. Client Address Index 찾기
            int headerStartIdx = Array.IndexOf(rcvBytes, new byte[] { reqBytes[0], reqBytes[1] });
            if (headerStartIdx == -1) return;

            //2. Func확인
            byte func = rcvBytes[headerStartIdx + 1];
            //3, Func에 맞는 해석 진행
            int dataLastIdx = -1;
            switch (func)
            {
                //case 0x01: //Dec : 1
                //case 0x02: //Dec : 2
                case 0x03: //Dec : 3
                           //case 0x04: //Dec : 4
                    dataLastIdx = InteruptData_Read(rcvBytes, reqBytes, headerStartIdx);
                    break;
                case 0x10:  //Dec : 16
                    dataLastIdx = InteruptData_Write(rcvBytes, reqBytes, headerStartIdx);
                    break;
            }

            //3-1. Func에서 Header 파악 후 데이터 길이만큼 안들어왔으면 중단
            if (dataLastIdx == -1) return;

            //4. HeaderStartIdx 앞부분 자르고 뒤에있는 데이터들 앞으로 당기기
            byte[] remainBytes = new byte[dataLastIdx - headerStartIdx];
            Buffer.BlockCopy(rcvBytes, headerStartIdx, remainBytes, 0, rcvBytes.Length - dataLastIdx);
            rcvBytes = remainBytes;
        }

        /// <summary>
        /// Receive 0x10 해석
        /// </summary>
        /// <param name="rcvBytes">Receive 받은 Bytes</param>
        /// <param name="reqBytes">비교용 Request Bytes</param>
        /// <param name="headerStartIdx">Receive Bytes Header 시작 Index</param>
        /// <returns>Receive Bytes 마지막 Data Index</returns>
        private int InteruptData_Write(byte[] rcvBytes, byte[] reqBytes, int headerStartIdx)
        {
            //Client Address[1] + Func[1] + Start Register Address[2] + Write Register Count[2] + CRC[2]
            if (rcvBytes.Length < headerStartIdx + 7) return -1;

            //Request와 같은부분 검사
            if (rcvBytes[headerStartIdx] != reqBytes[0]         //Client Address
                || rcvBytes[headerStartIdx + 1] != reqBytes[1]  //Func
                || rcvBytes[headerStartIdx + 2] != reqBytes[2]  //Start Register Address Hi
                || rcvBytes[headerStartIdx + 3] != reqBytes[3]  //Start Register Address Lo
                || rcvBytes[headerStartIdx + 4] != reqBytes[4]  //Write Register Count Hi
                || rcvBytes[headerStartIdx + 5] != reqBytes[5]  //Write Register Count Lo
                )
            {
                return -1;
            }

            //Receive Data 자를 마지막 Index 구하기
            return headerStartIdx + 7;
        }

        /// <summary>
        /// Receive 0x03 해석
        /// </summary>
        /// <param name="rcvBytes">Receive 받은 Bytes</param>
        /// <param name="reqBytes">비교용 Request Bytes</param>
        /// <param name="headerStartIdx">Receive Bytes Header 시작 Index</param>
        /// <returns>Receive Bytes 마지막 Data Index</returns>
        private int InteruptData_Read(byte[] rcvBytes, byte[] reqBytes, int headerStartIdx)
        {
            //Client Address[1] + Func[1] + Read Byte Count[1] + Read Register Value[N : Read Byte Count] + CRC[2]
            if (rcvBytes.Length < 3 + (reqBytes[5] * 2) + 2) return -1;

            //정상Process 실행
            short getAddress = (short)((reqBytes[2] << 8) + reqBytes[3]); //Start Address Hi부 Bit Shift 8 + Lo부
            short regCount = (short)((reqBytes[4] << 8) + reqBytes[5]);   //Hi부 Bit Shift 8 + Lo부

            //Data부 가져오기
            Dictionary<short, short> getValues = new Dictionary<short, short>();
            for (int i = 0; i < regCount; i++)
            {
                byte byteHi = rcvBytes[headerStartIdx + 3 + (i * 2)];       //Value부 Hi
                byte byteLo = rcvBytes[headerStartIdx + 3 + (i * 2) + 1];   //Value부 Lo

                getValues[getAddress++] = (short)((byteHi << 8) + byteLo);
            }

            //Receive Data 자를 마지막 Index 구하기
            return headerStartIdx + 3 + (regCount * 2) + 2;
        }

        #endregion Receive
    }
}
