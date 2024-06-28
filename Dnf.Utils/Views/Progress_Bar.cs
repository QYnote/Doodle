using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Utils.Views
{
    internal class Progress_Bar : ProgressBase
    {
        private Label LblTitle { get; set; }
        private ProgressBar ProgressBar { get; set; }
        private Label LblDescription { get; set; }
        private Label LblPercent { get; set; }

        private delegate void ProgressBarValue(int value);
        private delegate void DelegateText(string text);

        internal Progress_Bar()
        {
            Init();
        }

        private void Init()
        {
            base.Width = 400;
            base.Height = 83;

            //대제목
            this.LblTitle = new Label();
            this.LblTitle.Size = new Size(base.Width - 20, 23);
            this.LblTitle.Location = new Point(10, 3);
            this.LblTitle.Text = "대제목";
            this.LblTitle.Font = new Font(this.LblTitle.Font.FontFamily, 12.0f, FontStyle.Bold);
            this.LblTitle.TextAlign = ContentAlignment.MiddleLeft;

            //진행도 바
            this.ProgressBar = new ProgressBar();
            this.ProgressBar.Size = new Size(base.Width - 20, 30);
            this.ProgressBar.Location = new Point(this.LblTitle.Location.X, this.LblTitle.Location.Y + this.LblTitle.Size.Height + 2);

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
            base.Controls.Add(this.LblTitle);
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
            this.LblTitle.Text = Text;

            this.LblTitle.Refresh();
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
        internal void SetProgressValue(int Value)
        {
            this.ProgressBar.Value = Value;
            this.LblPercent.Text = string.Format("{0:D0} %", Value);

            this.LblPercent.Refresh();
        }
    }
}
