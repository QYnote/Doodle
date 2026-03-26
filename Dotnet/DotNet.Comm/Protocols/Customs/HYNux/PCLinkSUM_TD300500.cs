using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    /// <summary>
    /// PCLink TD300, TD500 Custom
    /// </summary>
    /// <remarks>
    /// Address[2] → [3], Unit Address 뒤에 ',(쉼표)' 추가, 전용 Command
    /// </remarks>
    public class PCLinkSUM_TD300500 :PCLinkSUM
    {
        public override string[] Commands => new string[]
        {
            "WHO",
            "RCS", "RPI",
            "RLG", "RDR", "RRD", "RPD", "RSD",
            "WLG", "WDR", "WRD", "WPD", "WSD",
        };

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Request 정보        //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        #region TD3,500 PCLink Request 생성

        /// <summary>
        /// RDR 연속 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_RDR(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 64);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3},RDR,{addrAry.Length},{addrAry[0]}";

                //3. Frame 생성
                byte[] frame = base.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        /// <summary>
        /// RRD 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public List<byte[]> CreateRequest_RRD(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 64);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D3},RRD,{addrAry.Length}";
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);
                bodyStr += ",";

                //3. Frame 생성
                byte[] frame = base.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion TD3,500 PCLink Request 생성
    }
}
