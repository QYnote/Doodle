using DotNet.Utils.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame
{
    public partial class FrmSolution : Form
    {
        #region UI Controls

        private QYLeftMenu leftMenu = new QYLeftMenu();
        private QYLeftMenuItem btnCommTester = new QYLeftMenuItem();
        private QYLeftMenuItem btnDataBase = new QYLeftMenuItem();
        private QYLeftMenuItem btnServer = new QYLeftMenuItem();

        private Panel pnlTitleBar = new Panel();
        private Label lblTitleText = new Label();
        private Button btnClose = new Button();
        private Button btnMinimize = new Button();
        private Button btnSize = new Button();

        private Panel pnlBody = new Panel();

        #endregion UI Controls End

        private Form _curForm = null;
        private string txtTitle = "QYDoodleProgram";

        public FrmSolution()
        {
            InitializeComponent();
            InitUI();

            Load += (s, e) => { CallForm("Main"); };
        }

        private void InitUI()
        {
            this.ControlBox = false;
            this.Text = string.Empty;
            this.Size = new Size(1150, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizedBounds = Screen.FromControl(this).WorkingArea;

            #region Left Menu

            this.leftMenu.BackColor = Color.LightSkyBlue;
            this.leftMenu.TopMainClick = (s, e) => { CallForm("Main"); };
            this.leftMenu.ItemClick += (s, e) => { CallForm((s as Control).Name); };

            this.btnCommTester.Name = "CommTest";
            this.btnCommTester.Image = DotNet.Utils.Properties.Resources.Connect_32x32;
            this.btnCommTester.Text = "통신 테스터기";

            this.btnDataBase.Name = "DBConnector";
            this.btnDataBase.Image = DotNet.Utils.Properties.Resources.Server_32x32;
            this.btnDataBase.Text = "Database 접속기";

            this.btnServer.Name = "Server";
            this.btnServer.Image = DotNet.Utils.Properties.Resources.Comm_32x32;
            this.btnServer.Text = "임시서버 생성기";

            #endregion Left Menu End
            #region Title Bar

            this.pnlTitleBar.Dock = DockStyle.Top;
            this.pnlTitleBar.BackColor = Color.LightSkyBlue;
            this.pnlTitleBar.MouseDown += (s, e) =>
            {
                if(e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
            this.pnlTitleBar.Height = 24;

            this.lblTitleText.Dock = DockStyle.Left;
            this.lblTitleText.TextAlign = ContentAlignment.MiddleLeft;
            this.lblTitleText.Width = 300;

            this.btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, this.btnClose.BackColor);
            this.btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, this.btnClose.BackColor);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Image = DotNet.Utils.Properties.Resources.FrmTitle_Button_16x16;
            this.btnClose.ImageAlign = ContentAlignment.MiddleCenter;
            this.btnClose.Size = new Size(22, 22);
            this.btnClose.Location = new Point(this.pnlTitleBar.Width - this.btnClose.Width, 0);
            this.btnClose.Click += (s, e) => { Application.Exit(); };

            this.btnSize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnSize.FlatAppearance.BorderSize = 0;
            this.btnSize.FlatAppearance.MouseDownBackColor = this.btnClose.FlatAppearance.MouseDownBackColor;
            this.btnSize.FlatAppearance.MouseOverBackColor = this.btnClose.FlatAppearance.MouseOverBackColor;
            this.btnSize.FlatStyle = FlatStyle.Flat;
            this.btnSize.Image = DotNet.Utils.Properties.Resources.FrmTitle_Button_16x16;
            this.btnSize.ImageAlign = ContentAlignment.MiddleCenter;
            this.btnSize.Size = this.btnClose.Size;
            this.btnSize.Location = new Point(this.btnClose.Location.X - this.btnSize.Width, 0);
            this.btnSize.Click += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                }
            };

            this.btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseDownBackColor = this.btnClose.FlatAppearance.MouseDownBackColor;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = this.btnClose.FlatAppearance.MouseOverBackColor;
            this.btnMinimize.FlatStyle = FlatStyle.Flat;
            this.btnMinimize.Image = DotNet.Utils.Properties.Resources.FrmTitle_Button_16x16;
            this.btnMinimize.ImageAlign = ContentAlignment.MiddleCenter;
            this.btnMinimize.Size = this.btnClose.Size;
            this.btnMinimize.Location = new Point(this.btnSize.Location.X - this.btnMinimize.Width, 0);
            this.btnMinimize.Click += (s, e) => { this.WindowState = FormWindowState.Minimized; };

            #endregion Title Bar End

            this.pnlBody.Dock = DockStyle.Fill;

            this.Controls.Add(this.pnlBody);
            this.pnlTitleBar.Controls.Add(this.lblTitleText);
            this.pnlTitleBar.Controls.Add(this.btnClose);
            this.pnlTitleBar.Controls.Add(this.btnSize);
            this.pnlTitleBar.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.pnlTitleBar);
            this.leftMenu.Items.Add(this.btnServer);
            this.leftMenu.Items.Add(this.btnDataBase);
            this.leftMenu.Items.Add(this.btnCommTester);
            this.Controls.Add(this.leftMenu);
        }

        private void CallForm(string frmName)
        {
            //1. 기존 Form 숨김
            if(this._curForm != null)
                this._curForm.Hide();

            //2. 이미 열린 Form 탐색
            foreach (var ctrl in this.pnlBody.Controls)
            {
                if(ctrl is Form frm
                    && frm.Name == frmName)
                {
                    this._curForm = frm;
                    this.lblTitleText.Text = string.Format("{0} - {1}", this.txtTitle, frmName);
                    frm.Show();
                    return;
                }
            }

            //3. 신규 Form 생성
            switch(frmName)
            {
                case "Main": this._curForm = new Frm.FrmMain() { Name = frmName }; break;
                case "CommTest": this._curForm = new Frm.FrmCommTester() { Name = frmName }; break;
                case "DBConnector": this._curForm = new Frm.FrmDataBase() { Name = frmName }; break;
                case "Server": this._curForm = new Frm.FrmServer() { Name = frmName }; break;
            }

            if(this._curForm != null)
            {
                this.lblTitleText.Text = string.Format("{0} - {1}", this.txtTitle, frmName);

                this._curForm.Dock = DockStyle.Fill;
                this._curForm.FormBorderStyle = FormBorderStyle.None;
                this._curForm.TopLevel = false;
                this.pnlBody.Controls.Add(this._curForm);

                this._curForm.Show();
            }
        }

        /// <summary>
        /// 다른 Control의 Mouse상태 해제
        /// </summary>
        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        /// <summary>
        /// WindowHandle 메시지 전송
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="wMsg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0x112;  //비 클라이언트 영역 클릭
        private const int HTCAPTION = 0xF012; //TitleBar
    }
}
