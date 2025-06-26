using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dotnet.Utils.Views
{
    public enum ProgressFormType
    {
        ProgressBar,
        Loading
    }

    /// <summary>
    /// 화면 열리기 전, 데이터 읽어오거나 하는 Progress Bar 기초기능 보관소
    /// </summary>
    public partial class QYForm : Form
    {
        private delegate void ProgressVoidDelegate();
        private delegate void ProgressValueDelegate(int value);
        private delegate void ProgressStringDelegate(int value);

        public delegate void UpdateUIDelegate(params object[] obj);

        /// <summary>
        /// 로딩화면 Form
        /// </summary>
        private ProgressBase Progress { get; set; }

        /// <summary>
        /// Form 기본틀
        /// </summary>
        public QYForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 실행한 Process Form 잠그고 로딩화면 열기
        /// </summary>
        /// <param name="Type">로딩화면 종류</param>
        public void ShowProgressForm(ProgressFormType Type)
        {
            this.Enabled = false;

            if (Type == ProgressFormType.ProgressBar)
            {
                this.Progress = new Progress_Bar();
            }

            this.Invoke(new Action(() => { this.Progress.Show(); this.Progress.Refresh(); }));
            
        }
        /// <summary>
        /// 로딩화면 닫기
        /// </summary>
        public void CloseProgressForm()
        {
            if (this.InvokeRequired)
                this.Invoke(new ProgressVoidDelegate(CloseProgressForm));
            else
            {
                this.Progress.Dispose();
                this.Focus();
            }

            this.Enabled = true;
        }
        /// <summary>
        /// 로딩화면 진행도 입력
        /// </summary>
        /// <param name="value">진행도 값</param>
        public void SetProgressBarValue(int value)
        {
            if (this.InvokeRequired)
                this.Invoke(new ProgressValueDelegate(SetProgressBarValue), value);
            else
            {
                if (this.Progress is Progress_Bar)
                {
                    (this.Progress as Progress_Bar).SetProgressValue(value);
                }
            }
        }
        /// <summary>
        /// 로딩화면 제목 입력
        /// </summary>
        /// <param name="caption">제목 Text</param>
        public void SetProgressCaption(string caption)
        {
            if (this.InvokeRequired)
                this.Invoke(new ProgressStringDelegate(SetProgressBarValue), caption);
            else
            {
                this.Progress.SetCaption(caption);
            }
        }
        /// <summary>
        /// 로딩화면 설명 입력
        /// </summary>
        /// <param name="text">설명 Text</param>
        public void SetProgressDescriptioin(string text)
        {
            if (this.InvokeRequired)
                this.Invoke(new ProgressStringDelegate(SetProgressBarValue), text);
            else
            {
                this.Progress.SetDescription(text);
            }
        }
    }
}
