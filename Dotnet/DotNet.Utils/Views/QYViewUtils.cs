using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// Enum을 DataItem으로 변환
        /// </summary>
        /// <typeparam name="T">변환할 Enum</typeparam>
        /// <returns>변환된 DataItem 목록</returns>
        public static QYItem[] EnumToItem<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new QYItem(e))
                .ToArray();
        }

        /// <summary>
        /// Rectange 안에서 시작, 종료 Point로 Rectangle 안의 Rectangle 구하기
        /// </summary>
        /// <remarks>
        /// Point가 기준 Rect밖에 있을 경우 기준 Rect 끝으로 취급
        /// </remarks>
        /// <param name="baseRect">기준 Rectangle</param>
        /// <param name="sp">시작 Point</param>
        /// <param name="ep">종료 Point</param>
        /// <returns>기준 Rectangle 속 Rectangle</returns>
        static public Rectangle RectInRect(Rectangle baseRect, Point sp, Point ep)
        {
            int spx, spy, epx, epy, x, y, width, height;

            //시작 X좌표 변경
            if (sp.X < baseRect.X)
                //Diagram 좌측 밖 시작
                spx = baseRect.X;
            else if (sp.X > baseRect.X + baseRect.Width)
            {
                //Diagram 우측 밖 시작
                if (ep.X < baseRect.X)
                    //Diagram 좌측 밖 종료
                    spx = baseRect.X;
                else if (ep.X > baseRect.X + baseRect.Width)
                    //Diagram 우측 밖 종료
                    spx = baseRect.X + baseRect.Width;
                else
                    //Diagram 내부 종료
                    spx = ep.X;
            }
            else
            {
                //Diagram 내부 시작
                if (ep.X < baseRect.X)
                    //Diagram 좌측 밖 종료
                    spx = baseRect.X;
                else if (ep.X > baseRect.X + baseRect.Width)
                    //Diagram 우측 밖 종료
                    spx = sp.X;
                else if (sp.X < ep.X)
                    //내부 종료 - 시작X < 종료X
                    spx = sp.X;
                else
                    //내부 종료 - 시작X > 종료X
                    spx = ep.X;
            }
            //시작 Y좌표 변경
            if (sp.Y < baseRect.Y)
                //Diagram 상단 밖 시작
                spy = baseRect.Y;
            else if (sp.Y > baseRect.Y + baseRect.Height)
            {
                //Diagram 하단 밖 시작
                if (ep.Y < baseRect.Y)
                    //Diagram 상단 밖 종료
                    spy = baseRect.Y;
                else if (ep.Y > baseRect.Y + baseRect.Height)
                    //Diagram 하단 밖 종료
                    spy = baseRect.Y + baseRect.Height;
                else
                    //Diagram 내부 종료
                    spy = ep.Y;
            }
            else
            {
                //Diagram 내부 시작
                if (ep.Y < baseRect.Y)
                    //Diagram 상단 밖 종료
                    spy = baseRect.Y;
                else if (ep.Y > baseRect.Y + baseRect.Height)
                    //Diagram 하단 밖 종료
                    spy = sp.Y;
                else if (sp.Y < ep.Y)
                    //내부 종료 - 시작Y < 종료Y
                    spy = sp.Y;
                else
                    //내부 종료 - 시작Y > 종료Y
                    spy = ep.Y;
            }
            //종료 X좌표 변경
            if (sp.X < baseRect.X)
            {
                //Diagram 좌측 밖 시작
                if (ep.X < baseRect.X)
                    //Diagram 좌측 밖 종료
                    epx = baseRect.X;
                else if (ep.X > baseRect.X + baseRect.Width)
                    //Diagram 우측 밖 종료
                    epx = baseRect.X + baseRect.Width;
                else
                    //Diagram 내부 종료
                    epx = ep.X;
            }
            else if (sp.X > baseRect.X + baseRect.Width)
                //Diagram 우측 밖 시작
                epx = baseRect.X + baseRect.Width;
            else
            {
                //Diagram 내부 시작
                if (ep.X < baseRect.X)
                    //Diagram 좌측 밖 종료
                    epx = sp.X;
                else if (ep.X > baseRect.X + baseRect.Width)
                    //Diagram 우측 밖 종료
                    epx = baseRect.X + baseRect.Width;
                else if (sp.X < ep.X)
                    //내부 종료 - 시작X < 종료X
                    epx = ep.X;
                else
                    //내부 종료 - 시작X > 종료X
                    epx = sp.X;
            }
            //종료 Y좌표 변경
            if (sp.Y < baseRect.Y)
            {
                //Diagram 상단 밖 시작
                if (ep.Y < baseRect.Y)
                    //Diagram 상단 밖 종료
                    epy = baseRect.Y;
                else if (ep.Y > baseRect.Y + baseRect.Height)
                    //Diagram 하단 밖 종료
                    epy = baseRect.Y + baseRect.Height;
                else
                    //Diagram 내부 종료
                    epy = ep.Y;
            }
            else if (sp.Y > baseRect.Y + baseRect.Height)
                //Diagram 하단 밖 시작
                epy = baseRect.Y + baseRect.Height;
            else
            {
                //Diagram 내부 시작
                if (ep.Y < baseRect.Y)
                    //Diagram 상단 밖 종료
                    epy = baseRect.Y;
                else if (ep.Y > baseRect.Y + baseRect.Height)
                    //Diagram 하단 밖 종료
                    epy = baseRect.Y + baseRect.Height;
                else if (sp.Y < ep.Y)
                    //내부 종료 - 시작Y < 종료Y
                    epy = ep.Y;
                else
                    //내부 종료 - 시작Y > 종료Y
                    epy = sp.Y;
            }

            x = Math.Min(spx, epx);
            y = Math.Min(spy, epy);
            width = spx < epx ? Math.Abs(spx - epx) : 0;
            height = spy < epy ? Math.Abs(spy - epy) : 0;


            return new Rectangle(
                x,
                y,
                width,
                height
                );
        }
    }

    /// <summary>
    /// ComboBox, RadioGroup 등에 사용되는 DisplayText, Value 묶음
    /// </summary>
    public class QYItem
    {
        /// <summary>
        /// Item 값
        /// </summary>
        public object Value { get; }
        /// <summary>
        /// 표기 Text
        /// </summary>
        public string DisplayText { get; set; }

        public QYItem(object item)
        {
            this.Value = item;
            if (item is Enum e)
                this.DisplayText = this.GetTextCode(e);
            else if (item != null)
                this.DisplayText = item.ToString();
        }
        private string GetTextCode(Enum value)
        {
            System.Reflection.FieldInfo info = value.GetType().GetField(value.ToString());
            QYLangCode[] attributes =
                (QYLangCode[])info.GetCustomAttributes(typeof(QYLangCode), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Code;

            return value.ToString();
        }
    }
}
