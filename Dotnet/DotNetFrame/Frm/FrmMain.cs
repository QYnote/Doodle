using DotNet.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Frm
{
    public partial class FrmMain : Form
    {
        #region UI Controls

        QYTokenTextBox txtToken = new QYTokenTextBox();

        QYCircularProgressBar progressCPU = new QYCircularProgressBar();
        QYCircularProgressBar progressMemory = new QYCircularProgressBar();
        PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);

        System.Threading.Timer timerUI = null;

        #endregion UI Controls

        List<string> tokenList = new List<string>();
        BindingList<string> bindingToken = null;

        public FrmMain()
        {
            InitializeComponent();
            InitUI();

            timerUI = new System.Threading.Timer(UpdatUI, null, 0, 1000);
        }

        private void InitUI()
        {
            this.progressCPU.ValueUnit = "%";

            this.progressMemory.Maximum = Process.GetCurrentProcess().PagedMemorySize64;
            this.progressMemory.Location = new Point(this.progressCPU.Location.X + this.progressCPU.Width + 3, this.progressCPU.Location.Y);
            this.progressMemory.ValueUnit = "MB";

            this.txtToken.Location = new Point(this.progressCPU.Location.X, this.progressCPU.Bottom + 3);
            bindingToken = new BindingList<string>(this.tokenList);
            this.tokenList.Add("Test1");
            this.tokenList.Add("Test2");
            this.txtToken.DataSource = bindingToken;
            this.txtToken.Size = new Size(200, 80);

            this.Controls.Add(this.progressCPU);
            this.Controls.Add(this.progressMemory);
            this.Controls.Add(this.txtToken);
        }

        private void UpdatUI(object state)
        {
            this.progressCPU.Value = this.cpuCounter.NextValue() / Environment.ProcessorCount;//CPU 코어수
            long memBytes = Process.GetCurrentProcess().PrivateMemorySize64;
            this.progressMemory.Value = Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024f);
        }
    }
}
