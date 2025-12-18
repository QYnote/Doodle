using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet.Utils.Views
{
    public static class QYViewUtils
    {
        /// <summary>
        /// GroupBox의 Caption 높이 가져오기
        /// </summary>
        /// <param name="gbx">확인할 GroupBox</param>
        /// <returns>Caption 높이</returns>
        public static float GroupBox_Caption_Hight(GroupBox gbx) => gbx.CreateGraphics().MeasureString(gbx.Text, gbx.Font).Height;

        /// <summary>
        /// RadioButton.Check에 DataBinding
        /// </summary>
        /// <param name="rdo">Binding 할 RadioButton</param>
        /// <param name="dataSource">Binding할 DataSource</param>
        /// <param name="dataMember">Binding할 Property</param>
        /// <param name="value">RadioButton 담당 Value</param>
        public static void BindingRadioButton(RadioButton rdo, object dataSource, string dataMember, object value)
        {
            Binding binding = new Binding("Checked", dataSource, dataMember, true, DataSourceUpdateMode.OnPropertyChanged);

            binding.Format += (s, e) => { e.Value = e.Value.Equals(value); };
            binding.Parse += (s, e) => { if((bool)e.Value) e.Value = value; };

            rdo.DataBindings.Add(binding);
        }
        /// <summary>
        /// Enum을 RadioButton Array로 변환
        /// </summary>
        /// <typeparam name="T">변환할 Enum</typeparam>
        /// <returns>변환된 RadioButton Array</returns>
        public static RadioButton[] CreateEnumRadioButton<T>() where T : Enum
        {
            EnumItem<T>[] items = GetEnumItems<T>();

            RadioButton[] ary = new RadioButton[items.Length];
            for (int i = 0; i < ary.Length; i++)
            {
                ary[i] = new RadioButton();
                ary[i].Tag = items[i].Value;
                ary[i].Text = items[i].DisplayText;
            }

            return ary;
        }
        /// <summary>
        /// Enum을 DataItem으로 변환
        /// </summary>
        /// <typeparam name="T">변환할 Enum</typeparam>
        /// <returns>변환된 DataItem 목록</returns>
        public static EnumItem<T>[] GetEnumItems<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new EnumItem<T>(e))
                .ToArray();
        }
        /// <summary>
        /// Enum Item 목록
        /// </summary>
        /// <typeparam name="T">사용된 Enum</typeparam>
        public class EnumItem<T> where T : Enum
        {
            /// <summary>
            /// Enum값
            /// </summary>
            public T Value { get; }
            /// <summary>
            /// Enum Text
            /// </summary>
            public string DisplayText { get; }

            public EnumItem(T item)
            {
                this.Value = item;
                this.DisplayText = GetDescription(item);
            }

            private string GetDescription(Enum value)
            {
                System.Reflection.FieldInfo info = value.GetType().GetField(value.ToString());
                System.ComponentModel.DescriptionAttribute[] attributes =
                    (System.ComponentModel.DescriptionAttribute[])info.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                    return attributes[0].Description;

                return value.ToString();
            }
        }
    }
}
