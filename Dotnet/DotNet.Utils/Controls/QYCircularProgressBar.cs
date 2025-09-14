using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet.Utils.Controls
{
    public partial class QYCircularProgressBar : UserControl
    {
        private float _value = 0;
        private float _maximum = 100;
        private float _minimum = 0;

        public float Value
        {
            get => _value;
            set
            {
                if (value < this._minimum)
                    this._value = this._minimum;
                else if (this._maximum < value)
                    this._value = this._maximum;
                else
                    this._value = value;

                this.Invalidate();
            }
        }
        public float Maximum
        {
            get => this._maximum;
            set
            {
                //설정할 최대값이 최소값보다 작은경우
                if (value <= this._minimum)
                    this._value = this._maximum = this._minimum + 1;
                //현재값이 설정할 최대값보다 클경우
                else if (value < this._value)
                    this._value = this._maximum = value;
                //일반
                else
                    this._maximum = value;

                this.Invalidate();
            }
        }
        public float Minimum
        {
            get => this._minimum;
            set
            {
                //설정할 최소값이 최대값보다 큰경우
                if (this._maximum <= value)
                    this._value = this._minimum = this._maximum - 1;
                //현재값이 설정할 최소값보다 작은경우
                else if (this._value < value)
                    this._value = this._minimum = value;
                else
                    this._minimum = value;

                this.Invalidate();
            }
        }

        public Color ProgressColor { get; set; } = Color.LightSkyBlue;
        public Color TackColor { get; set; } = Color.FromArgb(200, Color.LightGray);
        public int LineWidth { get; set; } = 10;
        public string ValueUnit { get; set; } = string.Empty;


        public QYCircularProgressBar()
        {
            InitializeComponent();
            this.DoubleBuffered = true;//깜빡임 방지
            this.Size = new Size(100, 100);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int d = Math.Min(this.Width, this.Height) - LineWidth;//지름
            //그려질 Circle Rectangle
            Rectangle rect = new Rectangle(this.LineWidth / 2, this.LineWidth / 2, d, d);

            //배경 원
            using (Pen trackPen = new Pen(this.TackColor, this.LineWidth))
                e.Graphics.DrawEllipse(trackPen, rect);

            //진행도 원
            float sweepAngle = this.Value / this.Maximum * 360f;
            using (Pen progressPen = new Pen(this.ProgressColor, this.LineWidth))
                e.Graphics.DrawArc(progressPen, rect, -90, sweepAngle);
            string text = string.Format("{0:F2} {1}", this.Value, this.ValueUnit);
            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                SizeF textSize = e.Graphics.MeasureString(text, this.Font);
                e.Graphics.DrawString(text, this.Font, brush,
                    (this.Width - textSize.Width) / 2,
                    (this.Height - textSize.Height) / 2
                );
            }
        }
    }
}
