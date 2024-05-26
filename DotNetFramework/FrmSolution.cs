using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFramework
{
    public partial class FrmSolution : Form
    {
        ToolStripButton BtnCommunication;
        ToolStripButton BtnServer;
        ToolStripButton BtnTest;

        public FrmSolution()
        {
            InitializeComponent();
            InitializeMyContents();

            this.IsMdiContainer = true;
            this.Text = ".Net FrameWork(WinForm)";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1200, 800);
        }

        private void InitializeMyContents()
        {
            //MenuBar
            ToolStrip TopMenu = new ToolStrip();
            TopMenu.ImageScalingSize = new Size(32, 32);
            TopMenu.ItemClicked += (sender, e) => { MdiOpen(e.ClickedItem.Name); };

            BtnCommunication = new ToolStripButton() { Name = "Communication", DisplayStyle = ToolStripItemDisplayStyle.Image}; //통신
            BtnServer = new ToolStripButton() { Name = "Serer", DisplayStyle = ToolStripItemDisplayStyle.Image}; //통신
            BtnTest = new ToolStripButton() { Name = "Test", DisplayStyle = ToolStripItemDisplayStyle.Image}; //통신

            //아이콘
            BtnCommunication.Image = Dnf.Utils.Properties.Resources.Connect_32x32;
            BtnServer.Image = Dnf.Utils.Properties.Resources.Server_32x32;
            BtnTest.Image = Dnf.Utils.Properties.Resources.Test_32x32;

            //설명
            BtnCommunication.ToolTipText = "통신";
            BtnServer.ToolTipText = "서버";
            BtnTest.ToolTipText = "테스트";

            //메뉴 추가
            TopMenu.Items.AddRange(new ToolStripItem[] { 
                BtnCommunication,
                BtnServer,
                BtnTest 
            });

            this.Controls.Add(TopMenu);
        }

        private void MdiOpen(string btnName)
        {
            Form frm = null;
            bool isOpen = false;

            //이미 열린 Form인지 탐색
            foreach(Form frmChild in this.MdiChildren)
            {
                if(btnName == frmChild.Name)
                {
                    isOpen = true;
                    frmChild.Focus();
                    break;
                }
            }

            if (!isOpen)
            {
                //Form 생성
                if (btnName == BtnCommunication.Name) { frm = new Dnf.Communication.MainForm() { Name = BtnCommunication.Name }; }
                else if (btnName == BtnServer.Name) { frm = new Dnf.Server.FrmMain() { Name = BtnServer.Name }; }
                else if (btnName == BtnTest.Name) { frm = null; }

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
