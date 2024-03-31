using Dnf.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Utils.Controls
{
    static public class UtilCustom
    {
        /// <summary>
        /// Byte Array 합치기
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

        /// <summary>
        /// DataGridView Column의 Value에 숫자만 입력하도록 하기
        /// </summary>
        /// <param name="gv">해당기능을 넣은 DataGridView</param>
        /// <param name="colName">해당기능을 적용할 Column Name</param>
        static public void ColumnOnlyNumeric(DataGridView gv, string colName)
        {
            gv.EditingControlShowing += (sender, e) =>
            {
                e.Control.KeyPress -= new KeyPressEventHandler(SlaveAddr_KeyPress);
                if (gv.CurrentCell.ColumnIndex == gv.Columns[colName].Index)
                {
                    TextBox txt = e.Control as TextBox;
                    if (txt != null)
                    {
                        txt.KeyPress += new KeyPressEventHandler(SlaveAddr_KeyPress);
                    }
                }
            };
        }

        /// <summary>
        /// ColumnOnlyNumeric의 Key검사용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void SlaveAddr_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        static public void TextBox_IP(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string[] DotSplit = textBox.Text.Split('.');

            //제어값(BackSpace, Delete 등)
            if (!char.IsControl(e.KeyChar))
            {
                if(e.KeyChar == '.' && DotSplit.Length == 4)
                {
                    //.을 입력했을떄 이미 4번째주소를 입력한 상태면 불가능
                    e.Handled = true;
                }
                else
                {
                    if (DotSplit[DotSplit.Length - 1].Length == 3)
                    {
                        //입력값이 3개이상 입력된 상태인지 점검
                        if(e.KeyChar !=  '.')
                        {
                            //3개입력된 이후 .이 아니면 입력불가능
                            e.Handled= true;
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

        /// <summary>
        /// string값 Enum값으로 변경
        /// </summary>
        /// <typeparam name="T">변경될 Enum</typeparam>
        /// <param name="str">변경할 string</param>
        /// <returns>Enum값</returns>
        static public T StringToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        /// <summary>
        /// Enum값들을 object[]값으로 변경
        /// </summary>
        /// <typeparam name="T">변환할 Enum</typeparam>
        /// <returns>ItemList</returns>
        static public object[] EnumToItems<T>()
        {
            return Enum.GetValues(typeof(T)).OfType<object>().ToArray();
        }

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

        static public int ToInt32_Custom(this object obj)
        {
            if (obj == null || obj == DBNull.Value || obj.ToString() == "")
                return -1;
            else
                return Convert.ToInt32(obj);
        }
    }
}
