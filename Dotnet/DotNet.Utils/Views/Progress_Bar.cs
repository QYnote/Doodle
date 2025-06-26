using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet.Utils.Views
{
    internal class Progress_Bar : ProgressBase
    {
        /// <summary>
        /// 제목
        /// </summary>
        private Label LblCaption { get; set; }
        /// <summary>
        /// 진행도 Bar
        /// </summary>
        private ProgressBar ProgressBar { get; set; }
        /// <summary>
        /// 진행도 Text
        /// </summary>
        private Label LblPercent { get; set; }
        /// <summary>
        /// 설명
        /// </summary>
        private Label LblDescription { get; set; }

        /// <summary>
        /// 로딩화면 ProgressBar 형태
        /// </summary>
        internal Progress_Bar()
        {
            Init();
        }

        private void Init()
        {
            base.Width = 400;
            base.Height = 83;

            //대제목
            this.LblCaption = new Label();
            this.LblCaption.Size = new Size(base.Width - 20, 23);
            this.LblCaption.Location = new Point(10, 3);
            this.LblCaption.Text = "대제목";
            this.LblCaption.Font = new Font(this.LblCaption.Font.FontFamily, 12.0f, FontStyle.Bold);
            this.LblCaption.TextAlign = ContentAlignment.MiddleLeft;

            //진행도 바
            this.ProgressBar = new ProgressBar();
            this.ProgressBar.Size = new Size(base.Width - 20, 30);
            this.ProgressBar.Location = new Point(this.LblCaption.Location.X, this.LblCaption.Location.Y + this.LblCaption.Size.Height + 2);

            //설명
            this.LblDescription = new Label();
            this.LblDescription.Size = new Size(base.Width - 20, 23);
            this.LblDescription.Location = new Point(this.ProgressBar.Location.X, this.ProgressBar.Location.Y + this.ProgressBar.Size.Height + 2);
            this.LblDescription.Text = "설명란";
            this.LblDescription.TextAlign = ContentAlignment.MiddleLeft;

            //진행도
            this.LblPercent = new Label();
            this.LblPercent.Size = new Size(40, 23);
            this.LblPercent.Location = new Point(this.ProgressBar.Location.X + this.ProgressBar.Size.Width - this.LblPercent.Size.Width, this.ProgressBar.Location.Y + this.ProgressBar.Size.Height + 2);
            this.LblPercent.Text = "0 %";
            this.LblPercent.TextAlign = ContentAlignment.MiddleRight;
            this.LblPercent.Font = new Font(this.LblPercent.Font, FontStyle.Bold);

            //Control Add
            base.Controls.Add(this.LblCaption);
            base.Controls.Add(this.ProgressBar);
            base.Controls.Add(this.LblDescription);
            base.Controls.Add(this.LblPercent);

            this.LblDescription.BringToFront();
            this.LblPercent.BringToFront();
        }
        /// <summary>
        /// 로딩 대제목 설정
        /// </summary>
        /// <param name="Text">입력할 Text</param>
        internal override void SetCaption(string Text)
        {
            this.LblCaption.Text = Text;

            this.LblCaption.Refresh();
        }
        /// <summary>
        /// 로딩 설명 Text 설정
        /// </summary>
        /// <param name="Text">입력할 Text</param>
        internal override void SetDescription(string Text)
        {
            this.LblDescription.Text = Text;

            this.LblDescription.Refresh();
        }
        /// <summary>
        /// 로딩 진행도 값 설정
        /// </summary>
        /// <param name="Value"></param>
        internal void SetProgressValue(int Value)
        {
            this.ProgressBar.Value = Value;
            this.LblPercent.Text = string.Format("{0:D0} %", Value);

            this.LblPercent.Refresh();
        }
    }
}
