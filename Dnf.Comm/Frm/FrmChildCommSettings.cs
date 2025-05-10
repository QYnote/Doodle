using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Server
{
    public partial class FrmChildCommSettings : Form
    {
        ucControlBox CboServerType = new ucControlBox(CtrlType.ComboBox);
        Button BtnSave = new Button();
        Button BtnCancle = new Button();
        FrmChildComm FrmMain = null;

        public FrmChildCommSettings(FrmChildComm frm)
        {
            FrmMain = frm;

            InitializeComponent();
            InitControl();

            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(300, 400);
        }

        private void InitControl()
        {
            CboServerType.Size = new Size(200, 30);
            CboServerType.LblWidth = 70;
            CboServerType.LblText = "서버 종류";
            (CboServerType.ctrl as ComboBox).Items.AddRange(new string[] {
                "TCP Server"
            });
            (CboServerType.ctrl as ComboBox).SelectedIndex = 0;

            BtnSave.Location = new Point(this.ClientSize.Width - 126, this.ClientSize.Height - 33);
            BtnSave.Size = new Size(60, 30);
            BtnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnSave.Text = "저장";

            BtnCancle.Location = new Point(this.ClientSize.Width - 63, BtnSave.Location.Y);
            BtnCancle.Size = BtnSave.Size;
            BtnCancle.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnCancle.Text = "취소";

            this.Controls.Add(CboServerType);
            this.Controls.Add(BtnSave);
            this.Controls.Add(BtnCancle);

            BtnSave.Click += (sender, e) => { Save(); };
            BtnCancle.Click += (sender, e) => { Cancle(); };
        }

        private void Save()
        {
            FrmMain.ServerType = CboServerType.Value.ToString();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancle()
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
