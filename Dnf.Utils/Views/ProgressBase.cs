using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Utils.Views
{
    internal abstract partial class ProgressBase : Form
    {
        /// <summary>
        /// Process 실행 시 로딩화면 표시용
        /// </summary>
        internal ProgressBase()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Text = "로딩 Form";
        }
        /// <summary>
        /// Form Title 설정
        /// </summary>
        /// <param name="Text"></param>
        internal void SetTitle(string Text)
        {
            this.Text = Text;
        }
        /// <summary>
        /// 로딩 대제목 설정
        /// </summary>
        /// <param name="Text">입력할 Text</param>
        internal abstract void SetCaption(string Text);
        /// <summary>
        /// 로딩 설명 Text 설정
        /// </summary>
        /// <param name="Text">입력할 Text</param>
        internal abstract void SetDescription(string Text);

    }
}
