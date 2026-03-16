using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Utils.ViewModel
{
    public static class QYUtils_ViewModel
    {
        public static QYItem[] GetEnumItems<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new QYItem(e))
                .ToArray();
        }
    }


    public class QYItem
    {
        public object Value { get; }
        public string DisplayText { get; set; }

        public QYItem(object value, string displayText = "")
        {
            this.Value = value;
            this.DisplayText = displayText == "" ? value.ToString() : displayText;
        }

        public QYItem(Enum item)
        {
            this.Value = item;
            this.DisplayText = this.GetLangCode(item);
        }

        private string GetLangCode(Enum item)
        {
            FieldInfo field = item.GetType().GetField(item.ToString());

            var attr = field?.GetCustomAttribute<QYLang>();

            return attr != null ? attr.Text : item.ToString();
        }
    }
}
