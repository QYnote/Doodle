using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class PCLinkSTD
    {
        protected virtual byte[] Tail => new byte[] { 0x0D, 0x0A };
        public virtual string[] Commands => new string[]
        {
            "WHO",
            "DWS", "DWR", "IWS", "IWR", "DMS", "DMC", "IMS", "IMC",
            "DRS", "DRR", "IRS", "IRR",
        };

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Response 정보       //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////
        protected byte[] _buffer = null;

        public virtual byte[] Parse(byte[] buffer, byte[] req)
        {
            this.StackBuffer(buffer);

            int startIdx,
                idxHandle = 0,
                endStartIdx = -1,
                endLastIdx = -1;

            while (idxHandle < this._buffer.Length - 1)
            {
                //1. Header(0x02, STX) Receive 검사
                startIdx = Array.IndexOf(this._buffer, 0x02, idxHandle++);
                if (startIdx < 0) continue;

                //2. Tail Receive 검사
                endStartIdx = Utils.Controls.Utils.QYUtils.Find(this._buffer, this.Tail, startIdx + 1);
                if (endStartIdx < 0) continue;

                endLastIdx = endStartIdx + this.Tail.Length - 1;
                if (this._buffer.Length < endLastIdx + 1) continue;

                //3. Frame 추출
                byte[] frameByte = new byte[endLastIdx - startIdx + 1];
                Buffer.BlockCopy(this._buffer, startIdx, frameByte, 0, frameByte.Length);

                return frameByte;
            }

            return null;
        }

        protected void StackBuffer(byte[] buffer)
        {
            if (buffer == null) return;

            if (this._buffer == null)
            {
                byte[] temp = new byte[buffer.Length];
                Buffer.BlockCopy(buffer, 0, temp, 0, buffer.Length);

                this._buffer = temp;
            }
            else
            {
                byte[] temp = new byte[this._buffer.Length + buffer.Length];
                Buffer.BlockCopy(this._buffer, 0, temp, 0, this._buffer.Length);
                Buffer.BlockCopy(buffer, 0, temp, this._buffer.Length, buffer.Length);

                this._buffer = temp;
            }
        }

        public void Initialize()
        {
            this._buffer = null;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////                            //////////////////////////////
        ////////////////////////////        Request 정보        //////////////////////////////
        ////////////////////////////                            //////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 연속적인 Address목록별 추출
        /// </summary>
        /// <param name="list">전송 할 Address 목록</param>
        /// <param name="maxFrameCount">1회 연속으로 보낼 수 있는 Frame 수</param>
        /// <returns>연속적인 Address 목록</returns>
        protected List<int[]> SortAddress(List<int> list, int maxFrameCount = 63)
        {
            list.Sort();
            List<int> continuousAddr = new List<int>();
            List<int[]> frameList = new List<int[]>();

            for (int i = 0; i < list.Count; i++)
            {
                int curAddr = list[i];

                if (list.Count == 1)
                {
                    //List가 1개만 있는경우
                    continuousAddr.Add(curAddr);
                    frameList.Add(continuousAddr.ToArray());
                }
                else if (i == 0)
                {
                    //첫번째 Address일 경우
                    continuousAddr.Add(curAddr);
                }
                else if (i == list.Count - 1)
                {
                    //여러 Address 중 마지막 Address일 경우
                    if (curAddr - 1 == continuousAddr.Last())
                    {
                        //연속되는 Address일 경우
                        continuousAddr.Add(curAddr);
                        frameList.Add(continuousAddr.ToArray());
                    }
                    else
                    {
                        //비연속 Address일 경우
                        frameList.Add(continuousAddr.ToArray());

                        continuousAddr.Clear();
                        continuousAddr.Add(curAddr);
                        frameList.Add(continuousAddr.ToArray());
                    }
                }
                else
                {
                    if (continuousAddr.Count == 0)
                    {
                        //연속진행 중 개수초과로 초기화 되었을 경우
                        continuousAddr.Add(curAddr);
                    }
                    else if (curAddr - 1 == continuousAddr.Last())
                    {
                        //연속 Address일경우
                        continuousAddr.Add(curAddr);

                        if (continuousAddr.Count >= maxFrameCount)
                        {
                            frameList.Add(continuousAddr.ToArray());
                            continuousAddr.Clear();
                        }
                    }
                    else
                    {
                        //연속 Address가 아닐경우
                        frameList.Add(continuousAddr.ToArray());
                        continuousAddr.Clear();

                        continuousAddr.Add(curAddr);
                    }
                }
            }

            return frameList;
        }

        protected virtual byte[] Build(string body)
        {
            byte[] stx = new byte[] { 0x02 };
            byte[] frame = Encoding.ASCII.GetBytes(body);
            byte[] build = new byte[stx.Length + frame.Length + this.Tail.Length];

            //STX
            Buffer.BlockCopy(stx, 0, build, 0, stx.Length);
            //Frame
            Buffer.BlockCopy(frame, 0, build, stx.Length, frame.Length);
            //Tail
            Buffer.BlockCopy(this.Tail, 0, build, stx.Length + frame.Length, this.Tail.Length);

            return build;
        }

        #region 일반 PCLink Request 생성

        /// <summary>
        /// IRR 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_IRR(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D2}IRR{addrAry.Length:D2}";
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0) return frameList;
            return null;
        }

        /// <summary>
        /// DRS 연속 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_DRS(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D2}DRS{addrAry.Length:D2}";

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        /// <summary>
        /// DRR 랜덤 Address 읽기 Request byte Array List 생성
        /// </summary>
        /// <param name="deviceAddr">장비 통신주소</param>
        /// <param name="readList">읽으려는 Registry Address 목록</param>
        /// <returns>Requeset Byte Array List</returns>
        public virtual List<byte[]> CreateRequest_DRR(int deviceAddr, List<int> readList)
        {
            //1. 연속 Adress목록 추출
            List<int[]> addrList = this.SortAddress(readList, 32);
            List<byte[]> frameList = new List<byte[]>();

            foreach (var addrAry in addrList)
            {
                if (addrAry.Length == 0) continue;

                //2. Main Frame 생성
                string bodyStr = $"{deviceAddr:D2}DRR{addrAry.Length:D2}";
                foreach (var addr in addrAry)
                    bodyStr += string.Format(",{0:D4}", addr);

                //3. Frame 생성
                byte[] frame = this.Build(bodyStr);
                frameList.Add(frame);
            }

            if (frameList.Count > 0)
                return frameList;

            return null;
        }

        #endregion 일반 PCLink Request 생성 End

        protected byte[] HexToByte(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2));

            return bytes;
        }
    }
}
