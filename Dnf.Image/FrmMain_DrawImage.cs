using Dnf.DrawImage.Controls;
using Dnf.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.DrawImage
{
    public partial class FrmMain_DrawImage : QYForm
    {
        private PictureBox PicImage { get; set; }
        private ProgramPort Port { get; set; }
        private Button BtnStartStop { get; set; }
        private Label LblFPS { get; set; }

        public FrmMain_DrawImage()
        {
            InitializeComponent();
            InitForm();
            InitComponent();
        }

        private void InitForm()
        {
            this.PicImage = new PictureBox();
            this.PicImage.Size = new Size(1024, 512);
            this.PicImage.Location = new Point(3, 3);
            this.PicImage.BorderStyle = BorderStyle.FixedSingle;
            this.PicImage.SizeMode = PictureBoxSizeMode.StretchImage;

            this.BtnStartStop = new Button();
            this.BtnStartStop.Size = new Size(80, 30);
            this.BtnStartStop.Location = new Point(this.PicImage.Location.X, this.PicImage.Location.Y + this.PicImage.Size.Height + 3);
            this.BtnStartStop.Text = "핸들";
            this.BtnStartStop.Click += (sender, e) =>
            {
                if (this.Port.IsUserOpen == false)
                    this.Port.Open();
                else
                    this.Port.Close();
            };

            this.LblFPS = new Label();
            this.LblFPS.Size = new Size(80, 30);
            this.LblFPS.Location = new Point(this.BtnStartStop.Location.X, this.BtnStartStop.Location.Y + this.BtnStartStop.Size.Height + 3);
            this.LblFPS.Text = "Test";


            this.Controls.Add(this.PicImage);
            this.Controls.Add(this.BtnStartStop);
            this.Controls.Add(this.LblFPS);
        }

        private void InitComponent()
        {
            this.Port = new ProgramPort("127.0.0.1", 5000);
            this.Port.PortLogHandler += LogMsg;
            this.Port.BgWorker.DoWork += BgWorker_DoWork;

        }

        private void LogMsg(string msg)
        {
            Debug.WriteLine(string.Format("{0: yyyy-MM-dd HH:mm:ss:fff} - {1}", DateTime.Now, msg));
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int frameCount = 0;

            while (true)
            {
                if (this.Port.BgWorker.CancellationPending)
                    break;
                else
                {
                    Thread.Sleep(1);

                    Image img = this.Port.Read();

                    UpdateUI("Image", img);

                    frameCount++;
                    if (sw.ElapsedMilliseconds > 1000)
                    {
                        UpdateUI("FPS", frameCount / ((float)sw.ElapsedMilliseconds / 1000));

                        frameCount = 0;
                        sw.Restart();
                    }
                }
            }
        }

        private void UpdateUI(params object[] args)
        {
            if(this.InvokeRequired)
                this.Invoke(new UpdateUIDelegate(UpdateUI), args);
            else
            {
                if((string)args[0] == "Image")
                {
                    this.PicImage.Image = args[1] as Image;
                }
                else if((string)args[0] == "FPS")
                {
                    this.LblFPS.Text = string.Format("{0:F2} FPS", args[1]);
                }
            }
        }
    }
}
