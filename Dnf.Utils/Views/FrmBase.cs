using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Utils.Views
{
    /// <summary>
    /// 화면 열리기 전, 데이터 읽어오거나 하는 Progress Bar 기초기능 보관소
    /// </summary>
    public partial class FrmBase : Form
    {
        private bool FlagOpenReady = true;
        private ProgressBar ProgressBar;

        public FrmBase()
        {
            InitializeComponent();
        }

        public void StartReady()
        {
            FlagOpenReady = false;
        }

        public void EndRead()
        {
            FlagOpenReady = true;
        }
    }
}
