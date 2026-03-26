using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    /// <summary>
    /// PCLink TH300, TH500 Custom
    /// </summary>
    /// <remarks>
    /// Address[2] → [3], 전용 Command, 전용 CheckSum
    /// </remarks>
    public class PCLinkSTD_TH300500 : PCLinkSTD
    {
        protected override byte[] Tail => new byte[] { 0x03, 0x0D, 0x0A };
        public override string[] Commands => new string[] {
            "WHO",
            "RCS", "RCV",
            "RSP", "RRP", "RUP", "RPD", "RSD", "RTD", "RLG",
            "WSP", "WRP", "WUP", "WPD", "WSD", "WTD", "WLG",
        };

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////


        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Request 정보        //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        #region TH3,500 PCLink Request 생성

        /// <summary>
        /// RSP / RRP / RUP / RTD, Data Address 읽기
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="cmd">명령어 Command</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_ReadAddress(int deviceAddr, string cmd, List<int> readList)
        {
            if (readList.Count == 0) return null;
            if (cmd != "RSP" && cmd != "RRP" && cmd != "RUP" && cmd != "RTD")
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadAddress : Protocol Command Error"));

            //1. 연속 Adress목록 추출
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addr in readList)
            {
                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3}{cmd},{addr:D4}";

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }
        /// <summary>
        /// RLG / RCS / RCV, Data Command 읽기
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="cmd">명령어 Command</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_Command(int deviceAddr, string cmd, List<int> readList)
        {
            if (readList.Count == 0) return null;
            if (cmd != "RLG" && cmd != "RCS" && cmd != "RCV")
                throw new Exception(string.Format("[ERROR]CreateRequest - ReadCommand : Protocol Command Error"));


            //1. 연속 Adress목록 추출
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addr in readList)
            {
                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3}{cmd}";

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion TH3,500 PCLink Request 생성
    }
}
