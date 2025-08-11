using System;
using System.Linq;

namespace DotNet.Utils.Controls
{
    static public class QYUtils
    {
        public const string Regex_IP = @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";

        #region 데이터 형태 변환

        /// <summary>
        /// DataRow값이나 object값 int32형태로 변환
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static public int ToInt32(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString() == "")
                return -1;
            else
                return Convert.ToInt32(obj);
        }

        /// <summary>
        /// int값 string형태의 Hex값으로 변환
        /// </summary>
        /// <param name="value">변환할 int값</param>
        /// <param name="format">format 값</param>
        /// <returns></returns>
        static public string ToHexString(this int value, int format = 0)
        {
            return value.ToString(string.Format("X{0}", format));
        }

        /// <summary>
        /// int -> 2byte로 변환
        /// </summary>
        /// <param name="value">변환할 int Value(Max 65535)</param>
        /// <returns></returns>
        static public byte[] IntToByte2(int value)
        {
            if (value > 65535) return null;

            //int Bit Shift
            return new byte[] { (byte)(value >> 8), (byte)value };
        }
        /// <summary>
        /// String Hex값 byte로 변환
        /// <para>ex. 1F → 31</para>
        /// </summary>
        /// <param name="str">2글자 String</param>
        /// <returns>변환된 byte값, 2글자 이상일 시 0</returns>
        static public byte StringHexToByte(this string str)
        {
            if (str.Length > 2) return 0;
            foreach (char c in str)
            {
                if (!(char.IsDigit(c)
                    || c == 'A' || c == 'B'
                    || c == 'C' || c == 'D'
                    || c == 'E' || c == 'F'
                    ))
                    return 0;
            }

            return Convert.ToByte(str, 16);
        }
        /// <summary>
        /// String Dec값 byte로 변환
        /// </summary>
        /// <param name="str">3글자 String</param>
        /// <returns>변환된 byte값, 3글자 이상일 시 0</returns>
        static public byte StringDecToByte(this string str)
        {
            if (str.Length > 3) return 0;
            foreach (char c in str)
            {
                if (!char.IsDigit(c)) return 0;
            }

            return Convert.ToByte(str);
        }

        /// <summary>
        /// string -> Enum값으로 변경
        /// </summary>
        /// <typeparam name="T">변경될 Enum</typeparam>
        /// <param name="str">변경할 string</param>
        /// <returns>Enum값</returns>
        static public T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        /// <summary>
        /// Enum List -> object[]값으로 변경
        /// </summary>
        /// <typeparam name="T">변환할 Enum</typeparam>
        /// <returns>ItemList</returns>
        static public object[] EnumToItems<Tenum>()
        {
            return Enum.GetValues(typeof(Tenum)).OfType<object>().ToArray();
        }

        #endregion 데이터 형태 변환 End

        #region 기능

        #region 병합

        /// <summary>
        /// Byte Array 합치기
        /// </summary>
        /// <param name="baseBytes">앞에위치할 Byte Array</param>
        /// <param name="backBytes">뒤에 위치할 Byte Array</param>
        /// <returns></returns>
        static public byte[] BytesAppend(this byte[] baseBytes, byte[] backBytes)
        {
            if (backBytes == null) return baseBytes;
            byte[] containByts = new byte[baseBytes.Length + backBytes.Length];

            //옮길 Array, 옮길 Array 시작 index, 넘겨받은 Array, 넘겨받을 Array index, 옮길 Array 수
            Buffer.BlockCopy(baseBytes, 0, containByts, 0, baseBytes.Length);
            Buffer.BlockCopy(backBytes, 0, containByts, baseBytes.Length, backBytes.Length);

            baseBytes = containByts;

            //메모리 초기화
            containByts = null;
            return baseBytes;
        }

        #endregion 병합 End
        #region 수정

        /// <summary>
        /// Dictionary Key값 수정
        /// </summary>
        /// <typeparam name="TKey">Dictionary Key</typeparam>
        /// <typeparam name="TValue">Dictionary value</typeparam>
        /// <param name="dic">바꿀 Dictionary</param>
        /// <param name="bfKey">기존 Key</param>
        /// <param name="afKey">바꿀 Key</param>
        /// <returns>true : Success / false : Fail</returns>
        static public bool DictKeyChange<TKey, TValue>(System.Collections.Generic.IDictionary<TKey, TValue> dic, TKey bfKey, TKey afKey)
        {
            if (dic.ContainsKey(afKey) || !dic.ContainsKey(bfKey)) { return false; }

            TValue value = dic[bfKey];
            dic.Remove(bfKey);
            dic[afKey] = value;

            return true;
        }

        /// <summary>
        /// List 항목 순서 변경
        /// </summary>
        /// <typeparam name="T">Array Type</typeparam>
        /// <param name="list">변경할 List</param>
        /// <param name="keyA">변경 Index A</param>
        /// <param name="keyB">변경 Index B</param>
        /// <returns></returns>
        static public bool Swap<T>(this System.Collections.Generic.List<T> list, int keyA, int keyB)
        {
            if(keyA < 0 || keyB < 0
                || list.Count - 1 < keyA || list.Count - 1 < keyB)
                return false;

            T temp = list[keyB];
            list[keyB] = list[keyA];
            list[keyA] = temp;

            return true;
        }

        /// <summary>
        /// Array 항목 순서 변경
        /// </summary>
        /// <typeparam name="T">Array Type</typeparam>
        /// <param name="ary">변경할 List</param>
        /// <param name="keyA">변경 Index A</param>
        /// <param name="keyB">변경 Index B</param>
        /// <returns></returns>
        static public bool Swap<T>(this T[] ary, int keyA, int keyB)
        {
            if (keyA < 0 || keyB < 0
                || ary.Length - 1 < keyA || ary.Length - 1 < keyB)
                return false;

            T temp = ary[keyB];
            ary[keyB] = ary[keyA];
            ary[keyA] = temp;

            return true;
        }

        /// <summary>
        /// 원본에 손상이 가지 않도록 복제품 생성
        /// </summary>
        /// <typeparam name="T">복사 Source Type</typeparam>
        /// <param name="source">복사 원본</param>
        /// <returns>복사 결과물</returns>
        static public T CopyFrom<T>(this T source)
        {
            if (source == null) return default(T);

            Type type = typeof(T);

            //값, 문자열
            if (type.IsValueType || type == typeof(string))
                return source;

            //리스트 인경우
            else if (typeof(System.Collections.IList).IsAssignableFrom(type))
            {
                System.Collections.IList originList = (System.Collections.IList)source;
                System.Collections.IList copyList = (System.Collections.IList)Activator.CreateInstance(type);

                foreach (var item in originList)
                {
                    copyList.Add(CopyFrom(item));
                }

                return (T)copyList;
            }

            //Class 복사
            else
            {
                object copy = Activator.CreateInstance(type);

                //get;set; Property 복사
                foreach (var info in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if(info.CanRead && info.CanWrite)
                    {
                        var value = info.GetValue(source, null);
                        info.SetValue(copy, CopyFrom(value), null);
                    }
                }

                //get;set; 없는 Property 복사
                foreach (var info in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var value = info.GetValue(source);
                    info.SetValue(copy, CopyFrom(value));
                }

                return (T)copy;
            }
        }


        #endregion 수정 End
        #region 검사

        /// <summary>
        /// 파일 열려있는지 검사
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>true : 열려있음, false : 없거나 닫혀있음</returns>
        static public bool CheckFileOpend(string filePath)
        {
            try
            {
                //존재하지 않으면 false 반환
                if (System.IO.File.Exists(filePath)) return false;

                //파일 열기 시도
                using (System.IO.Stream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None))
                {
                    //열리면 false 반환
                    stream.Close();
                    return false;
                }
            }
            catch
            {
                //안열리면 열려있다고 알리고 true 반환
                System.Windows.Forms.MessageBox.Show("F000000");
                return true;
            }
        }

        /// <summary>
        /// Array에서 Array의 시작 Index 찾기
        /// </summary>
        /// <param name="source">찾을 Array</param>
        /// <param name="pattern">검사할 Array</param>
        /// <param name="startIdx">시작 Index</param>
        /// <returns></returns>
        static public int Find<T>(this T[] source, T[] pattern, int startIdx = 0) where T : IEquatable<T>
        {
            if (source == null || pattern == null) return -1;
            if (source.Length == 0 || source.Length < pattern.Length) return -1;

            int idx = -1;

            for (int i = startIdx; i <= source.Length - pattern.Length; i++)
            {
                bool isMatch = true;

                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j].Equals(pattern[j]) == false)
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch) return i;
            }

            return idx;
        }

        /// <summary>실행시킨 Class와 실행시킨 Method Name Debug로 뿌려주기</summary>
        static public void DebugWrite(string addStr = "Debug")
        {
            System.Diagnostics.StackFrame frame = new System.Diagnostics.StackFrame(2);
            System.Reflection.MethodBase method = frame.GetMethod();

            System.Diagnostics.Debug.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss:fff} {1} - {2}() / {3}", DateTime.Now, method.Name, method.DeclaringType.Name, addStr));
        }

        #endregion 검사 End

        #endregion 기능 End

        #region Event

        /// <summary>
        /// ColumnOnlyNumeric의 Key검사용 16진수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void SlaveAddr_KeyPress_Hex(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)
                && !(e.KeyChar == 'a' || e.KeyChar == 'A'
                    || e.KeyChar == 'b' || e.KeyChar == 'B'
                    || e.KeyChar == 'c' || e.KeyChar == 'C'
                    || e.KeyChar == 'd' || e.KeyChar == 'D'
                    || e.KeyChar == 'e' || e.KeyChar == 'E'
                    || e.KeyChar == 'f' || e.KeyChar == 'F'))
            {
                e.Handled = true;
            }
            else if(e.KeyChar == 'a'
                    || e.KeyChar == 'b'
                    || e.KeyChar == 'c'
                    || e.KeyChar == 'd'
                    || e.KeyChar == 'e'
                    || e.KeyChar == 'f')
            {
                if(e.KeyChar == 'a') { e.KeyChar = 'A'; }
                else if(e.KeyChar == 'b') { e.KeyChar = 'B'; }
                else if(e.KeyChar == 'c') { e.KeyChar = 'C'; }
                else if(e.KeyChar == 'd') { e.KeyChar = 'D'; }
                else if(e.KeyChar == 'e') { e.KeyChar = 'E'; }
                else if(e.KeyChar == 'f') { e.KeyChar = 'F'; }
            }
        }

        /// <summary>
        /// .NET Winform TextBox IP만 입력가능하도록 변경
        /// </summary>
        static public void TextBox_IP(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            //1. 숫자 or . 을 입력한 것인지 검사
            //제어값 확인(Backspace, Delete 등)
            if (char.IsControl(e.KeyChar))
                return; //제어키면 그냥 진행시키기
            else
            {
                //숫자 or . 인지 확인
                if (!char.IsDigit(e.KeyChar) && !(e.KeyChar == '.'))
                {
                    e.Handled = true;
                    return;
                }
            }

            //2. IP주소에 맞게 입력하는지 검사
            System.Windows.Forms.TextBox txtbox = sender as System.Windows.Forms.TextBox;
            string text = txtbox.Text + e.KeyChar;
            int dotCnt = text.Count(c => c == '.'); //점 입력 개수
            /*Regex문자 해석
             * ^ : 시작문자
             * ( : 그룹 시작
             *   \d{1,2}    : 1 or 2자리 숫자
             *  |1\d\d      : 100 ~ 199
             *  |2[0-4]\d   : 200 ~ 249
             *  |25[0-5]    : 250 ~ 255
             * ) : 그룹 종료
             * $ : 종료문자
             */
            string strRegex = "(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])";
            string pattern = string.Empty;

            //구분 점에따라 확인 문자열 변경
            switch (dotCnt)
            {
                case 0:
                    pattern = string.Format("^{0}$", strRegex);
                    break;
                case 1:
                    if (e.KeyChar == '.') pattern = string.Format("^{0}.$", strRegex);
                    else pattern = string.Format("^{0}.{0}$", strRegex);
                    break;
                case 2:
                    if (e.KeyChar == '.') pattern = string.Format("^{0}.{0}.$", strRegex);
                    else pattern = string.Format("^{0}.{0}.{0}$", strRegex);
                    break;
                case 3:
                    if (e.KeyChar == '.') pattern = string.Format("^{0}.{0}.{0}.$", strRegex);
                    else pattern = string.Format("^{0}.{0}.{0}.{0}$", strRegex);
                    break;
            }

            if (pattern == string.Empty || System.Text.RegularExpressions.Regex.IsMatch(text, pattern) == false)
            {
                e.Handled = true;
                return;
            }
        }

        #endregion Event End

        #region 공통Type Control생성

        /// <summary>
        /// Panel 경계선 그리기
        /// </summary>
        /// <param name="dock">Dock 방향</param>
        /// <returns></returns>
        static public System.Windows.Forms.Label CreateSplitLine(System.Windows.Forms.DockStyle dock, int thickness = 4)
        {
            System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
            lbl.Dock = dock;
            lbl.Margin = new System.Windows.Forms.Padding(3);
            lbl.BackColor = System.Drawing.Color.DarkGray;
            lbl.Text = "";
            lbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lbl.AutoSize = false;
            if (dock == System.Windows.Forms.DockStyle.Left || dock == System.Windows.Forms.DockStyle.Right)
            {
                lbl.Size = new System.Drawing.Size(thickness, lbl.Height);
            }
            else if (dock == System.Windows.Forms.DockStyle.Top || dock == System.Windows.Forms.DockStyle.Bottom)
            {
                lbl.Size = new System.Drawing.Size(lbl.Width, thickness);
            }

            return lbl;
        }

        #endregion 공통Type Control생성 End
    }
}
