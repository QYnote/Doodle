using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public abstract class ProtocolFrame
    {
        protected int ErrCodeLength { get; set; }
        protected bool IsClient { get; set; }

        public ProtocolFrame(bool isClient)
        {
            this.IsClient = isClient;
        }

        #region Response
        
        /// <summary>
        /// 읽은 Buffer에서 Response Frame 추출
        /// </summary>
        /// <param name="buffer">여태 읽은 Buffer</param>
        /// <param name="subData">추출에 필요한 Sub Data</param>
        /// <returns>추출한 Response Frame</returns>
        public abstract byte[] Response_ExtractFrame(byte[] buffer, params object[] subData);
        /// <summary>
        /// Response Frame에서 추출한 Data 목록
        /// </summary>
        /// <param name="frame">추출할 Response Frame</param>
        /// <param name="subData">추출에 필요한 Sub Data</param>
        /// <returns>추출한 Data 목록</returns>
        /// T를 사용하고 싶지만 사용 불가능하여 object로 처리
        public abstract List<object> Response_ExtractData(byte[] frame, params object[] subData);

        #endregion Response End
        #region Request

        /// <summary>
        /// 읽은 Buffer에서 Request Frame 추출
        /// </summary>
        /// <param name="buffer">여태 읽은 Buffer</param>
        /// <param name="subData">추출에 필요한 Sub Data</param>
        /// <returns>추출한 Request Frame</returns>
        public abstract byte[] Request_ExtractFrame(byte[] buffer, params object[] subData);
        /// <summary>
        /// Request Frame을통한 Response Data 생성
        /// </summary>
        /// <param name="reqFrame">수신된 Request Frame</param>
        /// <param name="subData">생성에 필요한 Sub Data</param>
        /// <returns>생성한 Resonse Frame</returns>
        public abstract byte[]Request_CreateResponse(byte[] reqFrame, params object[] subData);

        #endregion Request End
        #region Error Code

        /// <summary>
        /// ErrorCode 검사
        /// </summary>
        /// <param name="frame">검사할 Data Frame</param>
        /// <returns>true: 정상 / false: 에러</returns>
        public abstract bool ConfirmErrCode(byte[] frame);
        /// <summary>
        /// Error Code 생성
        /// </summary>
        /// <param name="frame">생성할 Data Frame</param>
        /// <returns>생성된 Error Code</returns>
        public abstract byte[] CreateErrCode(byte[] frame);

        #endregion Error Code

        /// <summary>
        /// 연속적인 Address목록별 추출
        /// </summary>
        /// <param name="list"></param>
        /// <param name="maxFrameCount"></param>
        /// <returns>연속적인 Address 목록</returns>
        protected List<int[]> SortContinuouseAddress(List<int> list, int maxFrameCount = 63)
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
    }
}
