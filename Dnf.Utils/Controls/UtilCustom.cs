using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Utils.Controls
{
    static public class UtilCustom
    {
        /// <summary>
        /// Byte Array 합치ㅣㄱ
        /// </summary>
        /// <param name="frontBytes">앞에위치할 Byte Array</param>
        /// <param name="backBytes">뒤에 위치할 Byte Array</param>
        /// <returns></returns>
        static public byte[] BytesAppend(byte[] frontBytes, byte[] backBytes)
        {
            byte[] outputByts = new byte[frontBytes.Length + backBytes.Length];

            //옮길 Array, 옮길 Array 시작 index, 넘겨받은 Array, 넘겨받을 Array index, 옮길 Array 수
            Buffer.BlockCopy(frontBytes, 0, outputByts, 0, frontBytes.Length);
            Buffer.BlockCopy(backBytes, 0, outputByts, frontBytes.Length, backBytes.Length);

            return outputByts;
        }

        /// <summary>
        /// int -> 2byte로 변환
        /// </summary>
        /// <param name="value">변환할 int Value(Max 65535)</param>
        /// <returns></returns>
        static public byte[] IntToByte2(int value)
        {
            if(value > 65535) return null;

            //int Bit Shift
            return new byte[] { (byte)(value >> 8), (byte)value };
        }

        /// <summary>
        /// Dictionary Key값 수정
        /// </summary>
        /// <typeparam name="TKey">Dictionary Key</typeparam>
        /// <typeparam name="TValue">Dictionary value</typeparam>
        /// <param name="dic">바꿀 Dictionary</param>
        /// <param name="bfKey">기존 Key</param>
        /// <param name="afKey">바꿀 Key</param>
        /// <returns>true : Success / false : Fail</returns>
        static public bool DictKeyChange<TKey, TValue>(IDictionary<TKey, TValue> dic, TKey bfKey, TKey afKey)
        {
            if(dic.ContainsKey(afKey)) { return false; }

            TValue value = dic[bfKey];
            dic.Remove(bfKey);
            dic[afKey] = value;

            return true;
        }
    }
}
