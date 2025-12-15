using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Base.View
{
    public partial class FrmSolution : Form
    {
        #region UI Controls

        private QYLeftMenu leftMenu = new QYLeftMenu();
        private QYLeftMenuItem btnCommTester = new QYLeftMenuItem();
        private QYLeftMenuItem btnDataBase = new QYLeftMenuItem();
        private QYLeftMenuItem btnServer = new QYLeftMenuItem();
        private QYLeftMenuItem btnChart = new QYLeftMenuItem();

        private Panel pnlTitleBar = new Panel();
        private Label lblTitleText = new Label();
        private Button btnClose = new Button();
        private Button btnMinimize = new Button();
        private Button btnSize = new Button();

        private Panel pnlBody = new Panel();

        #endregion UI Controls End

        private Dictionary<string, Form> _openForm = new Dictionary<string, Form>();
        private Form _curForm = null;
        private string txtTitle = AppData.Lang("sol.title");

        public FrmSolution()
        {
            InitializeComponent();
            InitUI();
            InitText();

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
            this.btnCommTester.Image = DotNet.Utils.Properties.Resources.Connector_32x32;

            this.btnDataBase.Name = "DBConnector";
            this.btnDataBase.Image = DotNet.Utils.Properties.Resources.Database_32x32;

            this.btnServer.Name = "Server";
            this.btnServer.Image = DotNet.Utils.Properties.Resources.Comm_32x32;

            this.btnChart.Name = "Chart";
            this.btnChart.Image = DotNet.Utils.Properties.Resources.Chart_32x32;

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

            this.btnClose.Dock = DockStyle.Right;
            this.btnClose.Width = this.pnlTitleBar.Height + 1;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, this.btnClose.BackColor);
            this.btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, this.btnClose.BackColor);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Image = DotNet.Utils.Properties.Resources.Button_Close_16x16;
            this.btnClose.ImageAlign = ContentAlignment.MiddleCenter;
            this.btnClose.RightToLeft = RightToLeft.No;
            this.btnClose.TextImageRelation = TextImageRelation.Overlay;
            this.btnClose.Location = new Point(this.pnlTitleBar.Width - this.btnClose.Width, 0);
            this.btnClose.Click += (s, e) =>
            {
                if (this._curForm == null)
                    Application.Exit();
                else
                {
                    this._curForm.Close();
                    this._openForm.Remove(this._curForm.Name);
                    this._curForm = null;

                    foreach (var frm in this._openForm.Values)
                    {
                        this._curForm = frm;
                        this._curForm.Show();
                        break;
                    }
                }
            };

            this.btnSize.Dock = DockStyle.Right;
            this.btnSize.Width = this.btnClose.Width;
            this.btnSize.FlatAppearance.BorderSize = 0;
            this.btnSize.FlatAppearance.MouseDownBackColor = this.btnClose.FlatAppearance.MouseDownBackColor;
            this.btnSize.FlatAppearance.MouseOverBackColor = this.btnClose.FlatAppearance.MouseOverBackColor;
            this.btnSize.FlatStyle = FlatStyle.Flat;
            this.btnSize.Image = DotNet.Utils.Properties.Resources.Button_Max_16x16;
            this.btnSize.ImageAlign = ContentAlignment.MiddleCenter;
            this.btnSize.Location = new Point(this.btnClose.Location.X - this.btnSize.Width, 0);
            this.btnSize.Click += (s, e) =>
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.btnSize.Image = DotNet.Utils.Properties.Resources.Button_Normal_16x16;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.btnSize.Image = DotNet.Utils.Properties.Resources.Button_Max_16x16;
                }
            };

            this.btnMinimize.Dock = DockStyle.Right;
            this.btnMinimize.Width = this.btnClose.Width;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseDownBackColor = this.btnClose.FlatAppearance.MouseDownBackColor;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = this.btnClose.FlatAppearance.MouseOverBackColor;
            this.btnMinimize.FlatStyle = FlatStyle.Flat;
            this.btnMinimize.Image = DotNet.Utils.Properties.Resources.Button_Min_16x16;
            this.btnMinimize.ImageAlign = ContentAlignment.MiddleCenter;
            this.btnMinimize.Size = this.btnClose.Size;
            this.btnMinimize.Location = new Point(this.btnSize.Location.X - this.btnMinimize.Width, 0);
            this.btnMinimize.Click += (s, e) => { this.WindowState = FormWindowState.Minimized; };

            #endregion Title Bar End

            this.pnlBody.Dock = DockStyle.Fill;

            this.Controls.Add(this.pnlBody);
            this.pnlTitleBar.Controls.Add(this.lblTitleText);
            this.pnlTitleBar.Controls.Add(this.btnMinimize);
            this.pnlTitleBar.Controls.Add(this.btnSize);
            this.pnlTitleBar.Controls.Add(this.btnClose);
            this.Controls.Add(this.pnlTitleBar);
            this.leftMenu.Items.Add(this.btnChart);
            this.leftMenu.Items.Add(this.btnServer);
            this.leftMenu.Items.Add(this.btnDataBase);
            this.leftMenu.Items.Add(this.btnCommTester);
            this.Controls.Add(this.leftMenu);
        }

        private void InitText()
        {
            this.btnCommTester.Text = AppData.Lang("sol.leftmenu.button.commtester.text");
            this.btnDataBase.Text = AppData.Lang("sol.leftmenu.button.dbconnector.text");
            this.btnServer.Text = AppData.Lang("sol.leftmenu.button.server.text");
            this.btnChart.Text = AppData.Lang("sol.leftmenu.button.chart.text");
        }

        private void CallForm(string frmName)
        {
            //1. 기존 Form 숨김
            if(this._curForm != null)
                this._curForm.Hide();

            //2. 이미 열린 Form 탐색
            foreach (var pair in this._openForm)
            {
                if(pair.Key == frmName)
                {
                    this._curForm = pair.Value;
                    this.lblTitleText.Text = $"{this.txtTitle} - {this._curForm.Text}";
                    this._curForm.Show();
                    return;
                }
            }

            //3. 신규 Form 생성
            switch(frmName)
            {
                case "Main": this._curForm = new FrmMain() { Name = frmName, Text = AppData.Lang("메인화면") }; break;
                case "CommTest": this._curForm = new CommTester.View.FrmCommTester() { Name = frmName, Text = this.btnCommTester.Text }; break;
                case "DBConnector": this._curForm = new DataBase.View.FrmDataBase() { Name = frmName, Text = this.btnDataBase.Text }; break;
                case "Server": this._curForm = new Server.View.FrmServer() { Name = frmName, Text = this.btnServer.Text }; break;
                case "Chart": this._curForm = new Chart.View.FrmChart() { Name = frmName, Text = this.btnChart.Text }; break;
            }

            if(this._curForm != null)
            {
                //열림폼 등록
                this._openForm[this._curForm.Name] = this._curForm;

                //Title 설정
                this.lblTitleText.Text = $"{this.txtTitle} - {this._curForm.Text}";

                //Form 열기
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
