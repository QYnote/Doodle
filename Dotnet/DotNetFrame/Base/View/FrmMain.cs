using DotNet.Utils.Controls.Utils;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DotNetFrame.Base.View
{
    public partial class FrmMain : Form
    {
        #region UI Controls

        #endregion UI Controls

        private BackgroundWorker _bgWorker = new BackgroundWorker();

        public FrmMain()
        {
            InitializeComponent();
            InitUI();

            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += _bgWorker_DoWork;
            this._bgWorker.RunWorkerAsync();
        }

        private void InitUI()
        {

        }


        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this._bgWorker.CancellationPending || this.IsDisposed) break;

                    UpdateUI();
                }
                catch
                {

                }

                System.Threading.Thread.Sleep(50);
            }
        }

        private void UpdateUI()
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Update_WithoutParam(UpdateUI));
            else
            {

            }
        }
    }
}
