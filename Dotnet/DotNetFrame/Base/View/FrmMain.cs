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
        }

        private void InitUI()
        {

        }
    }
}
