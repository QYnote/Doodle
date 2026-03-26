using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols.Customs.HYNux
{
    public class ModbusTCP
    {
        // =============================
        // 
        // CheckSum 정보
        // 
        // =============================
        /// <summary>
        /// 통신 수신 검사
        /// </summary>
        /// <param name="bytes">검사 할 Frame</param>
        /// <returns>true: 에러발생 / false : 정상</returns>
        /// <remarks>
        /// ErrorCode가 포함 된 Frame으로 진행
        /// </remarks>
        public bool CheckCRC(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;
            int errStartIdx = bytes.Length - 2;


            for (int Index = 0; Index < errStartIdx; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            for (int i = 0; i < chkCd.Length; i++)
            {
                if (bytes[errStartIdx + i] != chkCd[i])
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// ErrorCode 생성
        /// </summary>
        /// <param name="bytes">생성 할 Frame</param>
        /// <returns>생성된 Error Code</returns>
        public byte[] CreateCRC(byte[] bytes)
        {
            byte[] chkCd = new byte[2];

            int nPolynominal = 40961;//&HA001
            int sum = 65535;
            int nXOR_Poly = 0;


            for (int Index = 0; Index < bytes.Length; Index++)
            {
                sum = sum ^ bytes[Index];
                for (int j = 0; j <= 7; j++)
                {
                    nXOR_Poly = sum % 2;
                    sum = sum / 2;

                    if (nXOR_Poly != 0)
                        sum = sum ^ nPolynominal;
                }
            }

            chkCd[0] = (byte)(sum % 256);
            chkCd[1] = (byte)((sum / 256) % 256);

            return chkCd;
        }

        // =============================
        // 
        // Response 정보
        // 
        // =============================
        protected byte[] _buffer = null;

        public byte[] Parse(byte[] buffer, byte[] request)
        {
            this.StackBuffer(buffer);

            //1. 최소 Header 길이 탐색
            //Header: Transaction[2] + ProtocolID[2] + Length[2] + Client Identifier[1]
            if (this._buffer.Length < 7)
            {
                this._buffer = null;
                return null;
            }

            int idxHandle = 0;
            while(idxHandle < this._buffer.Length)
            {
                //TransactionID, ProtocolID 검사
                byte[] header = new byte[4];
                Buffer.BlockCopy(request, 0, header, 0, header.Length);

                int startIndex = QYUtils.Find(this._buffer, header, idxHandle++);
                if (startIndex < 0) continue;

                //2. 남은 Frame 길이 탐색
                int frameLen = -1;
                byte[] lenBin = new byte[2];
                Buffer.BlockCopy(this._buffer, startIndex + 4, lenBin, 0, lenBin.Length);
                UInt16 remainLen = BitConverter.ToUInt16(lenBin, 0);

                frameLen = 6 + remainLen;

                if (this._buffer.Length < startIndex + frameLen)
                {
                    this._buffer = null;
                    return null;
                }

                //3. Frame 추출
                byte[] frameByte = new byte[frameLen];
                Buffer.BlockCopy(this._buffer, startIndex, frameByte, 0, frameLen);

                //4. 추출 후처리
                byte[] remain = new byte[this._buffer.Length - frameByte.Length];
                Buffer.BlockCopy(this._buffer, startIndex + frameByte.Length, remain, 0, remain.Length);

                this._buffer = remain;

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
    }
}
