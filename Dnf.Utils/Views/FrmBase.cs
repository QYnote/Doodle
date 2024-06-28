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

namespace Dnf.Utils.Views
{
    public enum ProgressFormType
    {
        ProgressBar,
        Loading
    }

    /// <summary>
    /// 화면 열리기 전, 데이터 읽어오거나 하는 Progress Bar 기초기능 보관소
    /// </summary>
    public partial class FrmBase : Form
    {
        private delegate void ProgressVoidDelegate();
        private delegate void ProgressValueDelegate(int value);
        private delegate void ProgressStringDelegate(int value);


        private ProgressBase Progress { get; set; }

        public FrmBase()
        {
            InitializeComponent();
        }

        public void ShowProgressForm(ProgressFormType Type)
        {
            this.Enabled = false;

            if (Type == ProgressFormType.ProgressBar)
            {
                this.Progress = new Progress_Bar();
            }

            this.Invoke(new Action(() => { this.Progress.Show(); this.Progress.Refresh(); }));
            
        }

        public void CloseProgressForm()
        {
            if (this.InvokeRequired)
                this.Invoke(new ProgressVoidDelegate(CloseProgressForm));
            else
            {
                this.Progress.Dispose();
            }

            this.Enabled = true;
        }
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
        public void SetProgressCaption(string caption)
        {
            if (this.InvokeRequired)
                this.Invoke(new ProgressStringDelegate(SetProgressBarValue), caption);
            else
            {
                this.Progress.SetCaption(caption);
            }
        }
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
