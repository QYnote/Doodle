using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFramework
{
    public partial class MainForm : Form
    {
        private ToolStripButton btnCommunication;
        private ToolStripButton btnCommTester;
        private ToolStripButton btnServer;
        private ToolStripButton btnSensorToImage;
        private ToolStripButton btnTest;

        public MainForm()
        {
            InitializeComponent();

            this.IsMdiContainer = true;
            this.Text = ".Net FrameWork(WinForm)";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1200, 800);

            this.IsMdiContainer = true;

            InitUI();
        }

        private void InitUI()
        {
            //MenuBar
            ToolStrip TopMenu = new ToolStrip();
            TopMenu.ImageScalingSize = new Size(32, 32);
            TopMenu.ItemClicked += (sender, e) => { MdiOpen(e.ClickedItem.Name); };

            this.btnCommunication = new ToolStripButton() {DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnCommunication.Name = "Communication";
            this.btnCommunication.Image = Dnf.Utils.Properties.Resources.Connect_32x32;
            this.btnCommunication.ToolTipText = "통신";

            this.btnCommTester = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnCommTester.Name = "CommTester";
            this.btnCommTester.Image = Dnf.Utils.Properties.Resources.Connect_32x32;
            this.btnCommTester.ToolTipText = "통신테스터기";

            this.btnServer = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnCommunication.Name = "Serer";
            this.btnServer.Image = Dnf.Utils.Properties.Resources.Server_32x32;
            this.btnServer.ToolTipText = "서버";

            this.btnSensorToImage = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnCommunication.Name = "SensorToImage";
            this.btnSensorToImage.Image = Dnf.Utils.Properties.Resources.Image_32x32;
            this.btnSensorToImage.ToolTipText = "센서이미지화";

            this.btnTest = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image }; //통신
            this.btnCommunication.Name = "Test";
            this.btnTest.Image = Dnf.Utils.Properties.Resources.Test_32x32;
            this.btnTest.ToolTipText = "테스트";

            //메뉴 추가
            TopMenu.Items.AddRange(new ToolStripItem[] {
                this.btnCommunication,
                this.btnCommTester,
                this.btnServer,
                this.btnSensorToImage,
                this.btnTest
            });

            this.Controls.Add(TopMenu);
        }

        private void MdiOpen(string btnName)
        {
            Form frm = null;
            bool isOpen = false;

            //이미 열린 Form인지 탐색
            foreach (Form frmChild in this.MdiChildren)
            {
                if (btnName == frmChild.Name)
                {
                    isOpen = true;
                    frmChild.Focus();
                    break;
                }
            }

            if (!isOpen)
            {
                //Form 생성
                if (btnName == btnCommunication.Name) { frm = new Dnf.Comm.MainForm() { Name = btnCommunication.Name }; }
                else if (btnName == btnCommTester.Name) { frm = new HY.Comm.MainForm() { Name = btnCommTester.Name }; }
                else if (btnName == btnServer.Name) { frm = new Dnf.Server.FrmChildComm() { Name = btnServer.Name }; }
                else if (btnName == btnSensorToImage.Name) { frm = new Dnf.DrawImage.FrmMain_DrawImage() { Name = btnSensorToImage.Name }; }
                else if (btnName == btnTest.Name)
                {
                    frm = null;
                }

                //이미 틀어져 있는지 검색
                if (frm != null)
                {
                    frm.MdiParent = this;
                    frm.WindowState = FormWindowState.Maximized;
                    frm.Show();
                }
            }
        }
    }
}
