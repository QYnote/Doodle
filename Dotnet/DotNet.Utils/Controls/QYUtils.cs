using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dotnet.Utils.Controls
{
    static public class QYUtils
    {
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
        /// <param name="frontBytes">앞에위치할 Byte Array</param>
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
        static public bool DictKeyChange<TKey, TValue>(IDictionary<TKey, TValue> dic, TKey bfKey, TKey afKey)
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
        static public bool Swap<T>(this List<T> list, int keyA, int keyB)
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
            else if (typeof(IList).IsAssignableFrom(type))
            {
                IList originList = (IList)source;
                IList copyList = (IList)Activator.CreateInstance(type);

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
                foreach (var info in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if(info.CanRead && info.CanWrite)
                    {
                        var value = info.GetValue(source, null);
                        info.SetValue(copy, CopyFrom(value), null);
                    }
                }

                //get;set; 없는 Property 복사
                foreach (var info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
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
                if (File.Exists(filePath)) return false;

                //파일 열기 시도
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    //열리면 false 반환
                    stream.Close();
                    return false;
                }
            }
            catch
            {
                //안열리면 열려있다고 알리고 true 반환
                MessageBox.Show("F000000");
                return true;
            }
        }

        /// <summary>
        /// Array에서 Array 찾기
        /// </summary>
        /// <param name="source">찾을 Array</param>
        /// <param name="pattern">검사할 Array</param>
        /// <returns></returns>
        static public int Find(this byte[] source, byte[] pattern, int startIdx = 0)
        {
            if (source == null || pattern == null) return -1;
            if(source.Length == 0 || source.Length < pattern.Length) return -1;

            int idx = -1;

            for (int i = startIdx; i <= source.Length - pattern.Length; i++)
            {
                bool isMatch = true;

                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
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
            StackFrame frame = new StackFrame(2);
            MethodBase method = frame.GetMethod();

            Debug.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss:fff} {1} - {2}() / {3}", DateTime.Now, method.Name, method.DeclaringType.Name, addStr));
        }

        #endregion 검사 End

        #endregion 기능 End

        #region Event

        /// <summary>
        /// DataGridView Column의 Value에 숫자만 입력하도록 하기
        /// </summary>
        /// <param name="gv">해당기능을 넣은 DataGridView</param>
        /// <param name="colName">해당기능을 적용할 Column Name</param>
        /// <param name="type">정수(Decimal): 기본값, 16진수(Hex)</param>
        static public void ColumnOnlyNumeric(DataGridView gv, string colName, string type = "Dec")
        {
            gv.EditingControlShowing += (sender, e) =>
            {
                if (type == "Dec")
                {
                    e.Control.KeyPress -= new KeyPressEventHandler(SlaveAddr_KeyPress_Decimal);
                    if (gv.CurrentCell.ColumnIndex == gv.Columns[colName].Index)
                    {
                        TextBox txt = e.Control as TextBox;
                        if (txt != null)
                        {
                            txt.KeyPress += new KeyPressEventHandler(SlaveAddr_KeyPress_Decimal);
                        }
                    }
                }
                else if(type == "Hex")
                {
                    e.Control.KeyPress -= new KeyPressEventHandler(SlaveAddr_KeyPress_Hex);
                    if (gv.CurrentCell.ColumnIndex == gv.Columns[colName].Index)
                    {
                        TextBox txt = e.Control as TextBox;
                        if (txt != null)
                        {
                            txt.KeyPress += new KeyPressEventHandler(SlaveAddr_KeyPress_Hex);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// ColumnOnlyNumeric의 Key검사용 10진수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void SlaveAddr_KeyPress_Decimal(object sender, KeyPressEventArgs e)
        {
            /*작업내역
             * IsControl : Ender, Backsapce같은 명령어 확인(이런건 먹어야하니까)
             * IsDigit : 10진수 검사
             * e.Handled : true - 막기, false - 가능
             */
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// ColumnOnlyNumeric의 Key검사용 16진수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void SlaveAddr_KeyPress_Hex(object sender, KeyPressEventArgs e)
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
        /// TextBox IP만 입력가능하도록 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static public void TextBox_IP(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string[] DotSplit = textBox.Text.Split('.');

            //제어값(BackSpace, Delete 등)
            if (!char.IsControl(e.KeyChar))
            {
                if (e.KeyChar == '.' && DotSplit.Length == 4)
                {
                    //.을 입력했을떄 이미 4번째주소를 입력한 상태면 불가능
                    e.Handled = true;
                }
                else
                {
                    if (DotSplit[DotSplit.Length - 1].Length == 3)
                    {
                        //입력값이 3개이상 입력된 상태인지 점검
                        if (e.KeyChar != '.')
                        {
                            //3개입력된 이후 .이 아니면 입력불가능
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        //Only 숫자 or .만 입력가능
                        if (!char.IsDigit(e.KeyChar) && !(e.KeyChar == '.'))
                        {
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        #endregion Event End

        #region 공통Type Control생성

        /// <summary>
        /// Panel 경계선 그리기
        /// </summary>
        /// <param name="dock">Dock 방향</param>
        /// <returns></returns>
        static public Label CreateSplitLine(DockStyle dock, int thickness = 4)
        {
            Label lbl = new Label();
            lbl.Dock = dock;
            lbl.Margin = new Padding(3);
            lbl.BackColor = Color.DarkGray;
            lbl.Text = "";
            lbl.BorderStyle = BorderStyle.Fixed3D;
            lbl.AutoSize = false;
            if (dock == DockStyle.Left || dock == DockStyle.Right)
            {
                lbl.Size = new Size(thickness, lbl.Height);
            }
            else if (dock == DockStyle.Top || dock == DockStyle.Bottom)
            {
                lbl.Size = new Size(lbl.Width, thickness);
            }

            return lbl;
        }

        #endregion 공통Type Control생성 End
    }
}
