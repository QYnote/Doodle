using DotNet.Utils.Controls.Utils;
using DotNetFrame.Base.Model;
using DotNetFrame.Chart.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DotNetFrame.Chart.View
{
    public partial class FrmChart : Form
    {
        private GroupBox gbx_cre_property = new GroupBox();
        private Label lbl_cre_interval = new Label();
        private Label lbl_cre_maxseconds = new Label();
        private Label lbl_filter_kernal_size = new Label();
        private CheckBox chk_filter_MAF = new CheckBox();
        private CheckBox chk_filter_WAF = new CheckBox();
        private NumericUpDown num_filter_kernal_size = new NumericUpDown();
        private Label lbl_peak_kernal_size = new Label();
        private System.Windows.Forms.DataVisualization.Charting.Chart chart = new System.Windows.Forms.DataVisualization.Charting.Chart();

        private VM_DataCreater_CPU _data_creater = new VM_DataCreater_CPU();
        private DataFilter _data_filter = new DataFilter();
        private BackgroundWorker _bgWorker = new BackgroundWorker();

        public FrmChart()
        {
            InitializeComponent();
            this.InitUI();
            this.InitText();

            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += _bgWorker_DoWork;
            this._bgWorker.RunWorkerAsync();
        }

        private void InitUI()
        {
            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Panel1.Padding = new Padding(3);

            this.InitUI_Property();
            this.InitUI_Chart(split.Panel2);
            split.Panel1.Controls.Add(this.gbx_cre_property);
            this.Controls.Add(split);
        }

        private void InitUI_Property()
        {
            this.gbx_cre_property.Dock = DockStyle.Fill;
            this.gbx_cre_property.Text = "Data 설정";

            this.lbl_cre_interval.Location = new Point(3, (int)DotNet.Utils.Views.Events.QYEvents.GetCaptionHeight(this.gbx_cre_property) + 3);
            this.lbl_cre_interval.Width = 110;
            this.lbl_cre_interval.TextAlign = ContentAlignment.MiddleLeft;
            NumericUpDown num_cre_interval = new NumericUpDown();
            num_cre_interval.Left = this.lbl_cre_interval.Right + 3;
            num_cre_interval.Top = this.lbl_cre_interval.Top;
            num_cre_interval.Width = 80;
            num_cre_interval.DecimalPlaces = 0;
            num_cre_interval.TextAlign = HorizontalAlignment.Right;
            num_cre_interval.Minimum = 0;
            num_cre_interval.Maximum = int.MaxValue;
            num_cre_interval.Value = VM_DataCreater_CPU.DEFAULT_DATA_GET_INTERVAL;
            num_cre_interval.ValueChanged += Num_cre_interval_ValueChanged;

            this.lbl_cre_maxseconds.Left = this.lbl_cre_interval.Left;
            this.lbl_cre_maxseconds.Top = this.lbl_cre_interval.Bottom + 3;
            this.lbl_cre_maxseconds.Width = this.lbl_cre_interval.Width;
            this.lbl_cre_maxseconds.TextAlign = ContentAlignment.MiddleLeft;
            NumericUpDown num_cre_maxseconds = new NumericUpDown();
            num_cre_maxseconds.Left = this.lbl_cre_maxseconds.Right + 3;
            num_cre_maxseconds.Top = this.lbl_cre_maxseconds.Top;
            num_cre_maxseconds.Width = num_cre_interval.Width;
            num_cre_maxseconds.DecimalPlaces = 0;
            num_cre_maxseconds.TextAlign = HorizontalAlignment.Right;
            num_cre_maxseconds.Minimum = 0;
            num_cre_maxseconds.Maximum = int.MaxValue;
            num_cre_maxseconds.Value = VM_DataCreater_CPU.DEFAULT_DATA_GET_TIME;
            num_cre_maxseconds.ValueChanged += Num_cre_maxseconds_ValueChanged; ;

            this.chk_filter_MAF.Left = this.lbl_cre_maxseconds.Left;
            this.chk_filter_MAF.Top = this.lbl_cre_maxseconds.Bottom + 3;
            this.chk_filter_MAF.Width = this.lbl_cre_maxseconds.Width + 20;
            this.chk_filter_MAF.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_filter_MAF.Checked = false;
            this.chk_filter_MAF.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_filter_MAF.CheckedChanged += Chk_filter_CheckedChanged;

            this.chk_filter_WAF.Left = this.chk_filter_MAF.Left;
            this.chk_filter_WAF.Top = this.chk_filter_MAF.Bottom + 3;
            this.chk_filter_WAF.Width = this.chk_filter_MAF.Width;
            this.chk_filter_WAF.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_filter_WAF.Checked = true;
            this.chk_filter_WAF.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_filter_WAF.CheckedChanged += Chk_filter_CheckedChanged;

            this.lbl_filter_kernal_size.Left = this.chk_filter_WAF.Left;
            this.lbl_filter_kernal_size.Top = this.chk_filter_WAF.Bottom + 3;
            this.lbl_filter_kernal_size.Width = this.lbl_cre_maxseconds.Width;
            this.lbl_filter_kernal_size.TextAlign = ContentAlignment.MiddleLeft;
            this.num_filter_kernal_size.Left = this.lbl_filter_kernal_size.Right + 3;
            this.num_filter_kernal_size.Top = this.lbl_filter_kernal_size.Top;
            this.num_filter_kernal_size.Width = num_cre_interval.Width;
            this.num_filter_kernal_size.DecimalPlaces = 0;
            this.num_filter_kernal_size.TextAlign = HorizontalAlignment.Right;
            this.num_filter_kernal_size.Minimum = 0;
            this.num_filter_kernal_size.Maximum = VM_DataCreater_CPU.DEFAULT_DATA_GET_TIME * 1000 / VM_DataCreater_CPU.DEFAULT_DATA_GET_INTERVAL;
            this.num_filter_kernal_size.Value = 3;
            this.num_filter_kernal_size.ValueChanged += Num_filter_kernal_size_ValueChanged;

            this.lbl_peak_kernal_size.Left = this.lbl_filter_kernal_size.Left;
            this.lbl_peak_kernal_size.Top = this.lbl_filter_kernal_size.Bottom + 3;
            this.lbl_peak_kernal_size.Width = this.lbl_filter_kernal_size.Width;
            this.lbl_peak_kernal_size.TextAlign = ContentAlignment.MiddleLeft;
            NumericUpDown num_peak_kernal_size = new NumericUpDown();
            num_peak_kernal_size.Left = this.lbl_peak_kernal_size.Right + 3;
            num_peak_kernal_size.Top = this.lbl_peak_kernal_size.Top;
            num_peak_kernal_size.Width = num_cre_interval.Width;
            num_peak_kernal_size.DecimalPlaces = 0;
            num_peak_kernal_size.TextAlign = HorizontalAlignment.Right;
            num_peak_kernal_size.Minimum = 0;
            num_peak_kernal_size.Maximum = VM_DataCreater_CPU.DEFAULT_DATA_GET_TIME * 1000 / VM_DataCreater_CPU.DEFAULT_DATA_GET_INTERVAL;
            num_peak_kernal_size.Value = DataFilter.DEFAULT_PEAK_KERNAL_SIZE;
            num_peak_kernal_size.ValueChanged += Num_peak_kernal_size_ValueChanged;

            this.gbx_cre_property.Controls.Add(this.lbl_cre_interval);
            this.gbx_cre_property.Controls.Add(num_cre_interval);
            this.gbx_cre_property.Controls.Add(this.lbl_cre_maxseconds);
            this.gbx_cre_property.Controls.Add(num_cre_maxseconds);
            this.gbx_cre_property.Controls.Add(this.chk_filter_MAF);
            this.gbx_cre_property.Controls.Add(this.chk_filter_WAF);
            this.gbx_cre_property.Controls.Add(this.lbl_filter_kernal_size);
            this.gbx_cre_property.Controls.Add(num_filter_kernal_size);
            this.gbx_cre_property.Controls.Add(this.lbl_peak_kernal_size);
            this.gbx_cre_property.Controls.Add(num_peak_kernal_size);
        }

        private void Num_cre_interval_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._data_creater.Interval = Convert.ToInt32(num.Value);
        }

        private void Num_cre_maxseconds_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._data_creater.Time = Convert.ToInt32(num.Value);
        }

        private void Chk_filter_CheckedChanged(object sender, EventArgs e)
        {
            bool MAF = this.chk_filter_MAF.Checked;
            bool WAF = this.chk_filter_WAF.Checked;

            if(MAF == false && WAF == false)
                this.num_filter_kernal_size.Enabled = false;
            else
                this.num_filter_kernal_size.Enabled = true;
        }

        private void Num_filter_kernal_size_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._data_filter.Filter_KernalSize = Convert.ToInt32(num.Value);
        }

        private void Num_peak_kernal_size_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._data_filter.Peak_KernalSize = Convert.ToInt32(num.Value);
        }

        private void InitUI_Chart(Panel pnl)
        {
            this.chart.Dock = DockStyle.Fill;
            this.chart.Series.Clear();
            this.chart.ChartAreas.Clear();
            this.chart.Legends.Clear();
            ChartArea area = this.chart.ChartAreas.Add("Default");
            area.AxisX.LabelStyle.Format = "HH:mm:ss";
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(200, area.AxisX.MajorGrid.LineColor);
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY.LabelStyle.Format = "F1";
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(200, area.AxisY.MajorGrid.LineColor);
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY2.LabelStyle.Format = "F1";
            area.AxisY2.MajorGrid.LineColor = Color.FromArgb(200, area.AxisY2.MajorGrid.LineColor);
            area.AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.InnerPlotPosition.X = 7;
            area.InnerPlotPosition.Y = 1;
            area.InnerPlotPosition.Width = 90;
            area.InnerPlotPosition.Height = 95;

            Series seriesCPU = this.chart.Series.Add("CPU");
            seriesCPU.LegendText = "CPU[%]";
            seriesCPU.ChartType = SeriesChartType.Line;
            seriesCPU.XValueType = ChartValueType.DateTime;
            seriesCPU.IsVisibleInLegend = true;

            Legend legend = this.chart.Legends.Add("Legend");
            legend.LegendStyle = LegendStyle.Table;
            legend.Docking = Docking.Top;
            legend.IsDockedInsideChartArea = true;
            legend.Alignment = StringAlignment.Far;

            pnl.Controls.Add(this.chart);
        }

        private void InitText()
        {
            this.gbx_cre_property.Text = AppData.Lang("chart.property.text");
            this.lbl_cre_interval.Text = AppData.Lang("chart.cre.interval");
            this.lbl_cre_maxseconds.Text = AppData.Lang("chart.cre.length");
            this.chk_filter_MAF.Text = AppData.Lang("chart.filter.MAF");
            this.chk_filter_WAF.Text = AppData.Lang("chart.filter.WAF");
            this.lbl_filter_kernal_size.Text = AppData.Lang("chart.filter.kernal");
            this.lbl_peak_kernal_size.Text = AppData.Lang("chart.peak.kernal");
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
                (DateTime, double)[] values = this._data_creater.CPU_Create_Data();
                if (values.Length <= 0) return;

                double[] yAry = new double[values.Length];
                for (int i = 0; i < yAry.Length; i++)
                    yAry[i] = values[i].Item2;

                //단순이동 평균 적용
                if (this.chk_filter_MAF.Checked)
                    yAry = this._data_filter.MAF(yAry);
                //가중이동 평균 적용
                if (this.chk_filter_WAF.Checked)
                    yAry = this._data_filter.WAF(yAry);

                for (int i = 0; i < yAry.Length; i++)
                    values[i].Item2 = yAry[i];


                Series series = this.chart.Series["CPU"];
                ChartArea area = this.chart.ChartAreas[series.ChartArea];
                this.chart.BeginInit();
                series.Points.Clear();
                series.MarkerSize = 0;
                area.AxisY.StripLines.Clear();

                double min_CPU = double.MaxValue,
                       max_CPU = double.MinValue;
                DateTime now = DateTime.Now,
                         minDate = now.AddSeconds(-this._data_creater.Time);

                foreach (var pair in values)
                {
                    series.Points.AddXY(pair.Item1, pair.Item2);

                    if (min_CPU > pair.Item2) min_CPU = pair.Item2;
                    if (max_CPU < pair.Item2) max_CPU = pair.Item2;
                }

                if (min_CPU < max_CPU)
                {
                    double range = max_CPU - min_CPU,
                           offset = range / 10;

                    if (series.YAxisType == AxisType.Primary)
                    {
                        area.AxisY.Minimum = min_CPU < offset ? 0 : min_CPU - offset;
                        area.AxisY.Maximum = max_CPU + offset;
                    }
                }
                this.chart.ChartAreas[0].AxisX.Maximum = now.ToOADate();
                this.chart.ChartAreas[0].AxisX.Minimum = minDate.ToOADate();

                //1. Y축 Data 추출
                double[] aryPoints = new double[series.Points.Count];
                for (int i = 0; i < aryPoints.Length; i++)
                    aryPoints[i] = series.Points[i].YValues[0];

                //2-1. 봉우리 목록 추출 및 이상치 값 계산
                List<int> peakIndexList = this._data_filter.GetPeakIndexList(aryPoints);
                double[] peakAry = new double[peakIndexList.Count];
                double anomalyValue = -1;
                if (peakAry.Length > 0)
                {
                    for (int i = 0; i < peakAry.Length; i++)
                        peakAry[i] = series.Points[peakIndexList[i]].YValues[0];

                    anomalyValue = this._data_filter.Calc_Anomaly(aryPoints, peakAry);
                }

                //2-2. 이상치 검출선 표기
                if (true)
                {
                    StripLine lineAvgValue = new StripLine();
                    lineAvgValue.Interval = 0;
                    lineAvgValue.IntervalOffset = anomalyValue;
                    lineAvgValue.StripWidth = 0;
                    lineAvgValue.BorderWidth = 2;
                    lineAvgValue.BorderColor = Color.BlueViolet;
                    lineAvgValue.BorderDashStyle = ChartDashStyle.Dash;
                    lineAvgValue.Text = $"{AppData.Lang("chart.peak.outlier.line")}: {lineAvgValue.IntervalOffset:F3}";
                    lineAvgValue.TextOrientation = TextOrientation.Horizontal;
                    lineAvgValue.Tag = true;
                    area.AxisY.StripLines.Add(lineAvgValue);
                }

                //3. 봉우리 표기 및 이상치 봉우리 추출
                List<DataPoint> outlierList = new List<DataPoint>();
                foreach (var idx in peakIndexList)
                {
                    //모든 봉우리 표기
                    if (true)
                    {
                        series.Points[idx].MarkerSize = 8;
                        series.Points[idx].MarkerStyle = MarkerStyle.Circle;
                        series.Points[idx].MarkerColor = Color.DarkGray;
                    }

                    if (series.Points[idx].YValues[0] > anomalyValue)
                        outlierList.Add(series.Points[idx]);
                }

                //4. 이상치 봉우리 표기
                if (peakIndexList.Count > 0)
                {
                    foreach (var item in outlierList)
                    {
                        item.MarkerSize = 8;
                        item.MarkerStyle = MarkerStyle.Circle;
                        item.MarkerColor = Color.Black;
                    }
                }

                this.chart.EndInit();
            }
        }
    }
}
