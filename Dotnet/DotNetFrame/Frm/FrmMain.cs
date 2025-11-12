using DotNet.Comm;
using DotNet.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace DotNetFrame.Frm
{
    public partial class FrmMain : Form
    {
        #region UI Controls

        QYCircularProgressBar progressCPU = new QYCircularProgressBar();
        QYCircularProgressBar progressMemory = new QYCircularProgressBar();
        PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);

        Chart chart = new Chart();

        System.Threading.Timer timerUI = null;

        #endregion UI Controls

        BackgroundWorker _bgWorker = new BackgroundWorker();

        public FrmMain()
        {
            InitializeComponent();
            InitUI();

            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += _bgWorker_DoWork;
            this._bgWorker.RunWorkerAsync();
            timerUI = new System.Threading.Timer(UpdatProcess, null, 0, 1000);
        }

        private void InitUI()
        {
            this.progressCPU.ValueUnit = "%";

            this.progressMemory.Maximum = Process.GetCurrentProcess().PagedMemorySize64;
            this.progressMemory.Location = new Point(this.progressCPU.Location.X + this.progressCPU.Width + 3, this.progressCPU.Location.Y);
            this.progressMemory.ValueUnit = "MB";

            this.chart.Series.Clear();
            this.chart.ChartAreas.Clear();
            this.chart.Legends.Clear();
            ChartArea area =  this.chart.ChartAreas.Add("Default");
            area.AxisX.LabelStyle.Format = "HH:mm:ss";
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(200, area.AxisX.MajorGrid.LineColor);
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY.LabelStyle.Format = "F1";
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(200, area.AxisY.MajorGrid.LineColor);
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY2.LabelStyle.Format = "F1";
            area.AxisY2.MajorGrid.LineColor = Color.FromArgb(200, area.AxisY2.MajorGrid.LineColor);
            area.AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

            Series seriesCPU = this.chart.Series.Add("CPU");
            seriesCPU.ChartType = SeriesChartType.FastLine;
            seriesCPU.XValueType = ChartValueType.DateTime;
            seriesCPU.IsVisibleInLegend = true;

            Series seriesMemory = this.chart.Series.Add("Memory");
            seriesMemory.ChartType = SeriesChartType.FastLine;
            seriesMemory.XValueType = ChartValueType.DateTime;
            seriesMemory.YAxisType = AxisType.Secondary;
            seriesMemory.IsVisibleInLegend = true;

            Legend legend = this.chart.Legends.Add("Legend");
            legend.LegendStyle = LegendStyle.Table;
            legend.Docking = Docking.Top;
            legend.IsDockedInsideChartArea = true;
            legend.Alignment = StringAlignment.Far;

            //this.Controls.Add(this.progressCPU);
            //this.Controls.Add(this.progressMemory);
            this.Controls.Add(this.chart);
        }

        private void UpdatProcess(object state)
        {
            this.progressCPU.Value = this.cpuCounter.NextValue() / Environment.ProcessorCount;//CPU 코어수
            long memBytes = Process.GetCurrentProcess().PrivateMemorySize64;
            this.progressMemory.Value = Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024f);
        }


        private void _bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this._bgWorker.CancellationPending) break;

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
                this.BeginInvoke(new UpdateUI(UpdateUI));
            else
            {
                DateTime now = DateTime.Now;
                this.chart.Series["CPU"].Points.AddXY(now, this.cpuCounter.NextValue() / Environment.ProcessorCount);
                this.chart.Series["Memory"].Points.AddXY(now, Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024f));

                DateTime minDate = now.AddSeconds(-60);
                double min_Mem = double.MaxValue,
                       max_Mem = double.MinValue,
                       min_CPU = double.MaxValue,
                       max_CPU = double.MinValue;
                for (int i = 0; i < this.chart.Series["CPU"].Points.Count; i++)
                {
                    DataPoint p = this.chart.Series["CPU"].Points[i];
                    if ((DateTime.FromOADate(p.XValue)-minDate).TotalSeconds < -1)
                    {
                        this.chart.Series["CPU"].Points.Remove(p);
                        i--;
                        continue;
                    }

                    //AxisY 최대 최소 설정
                    if (min_CPU > p.YValues[0]) min_CPU = p.YValues[0];
                    if (max_CPU < p.YValues[0]) max_CPU = p.YValues[0];
                }

                for (int i = 0; i < this.chart.Series["Memory"].Points.Count; i++)
                {
                    DataPoint p = this.chart.Series["Memory"].Points[i];
                    if ((DateTime.FromOADate(p.XValue) - minDate).TotalSeconds < -1)
                    {
                        this.chart.Series["Memory"].Points.Remove(p);
                        i--;
                    }

                    //AxisY 최대 최소 설정
                    if (min_Mem > p.YValues[0]) min_Mem = p.YValues[0];
                    if (max_Mem < p.YValues[0]) max_Mem = p.YValues[0];
                }

                this.chart.ChartAreas[0].AxisX.Maximum = now.ToOADate();    
                this.chart.ChartAreas[0].AxisX.Minimum = minDate.ToOADate();
                double defaultAxisYMin = min_CPU - (Math.Abs(max_CPU - min_CPU) / 10);
                this.chart.ChartAreas[0].AxisY.Minimum = defaultAxisYMin < 0 ? 0 : defaultAxisYMin;
                if(max_CPU != double.MinValue)
                    this.chart.ChartAreas[0].AxisY.Maximum = max_CPU + (Math.Abs(max_CPU - min_CPU) / 10);
                this.chart.ChartAreas[0].AxisY2.Minimum = min_Mem - (Math.Abs(max_Mem - min_Mem) / 10);
                this.chart.ChartAreas[0].AxisY2.Maximum = max_Mem + (Math.Abs(max_Mem - min_Mem) / 10);
            }
        }
    }
}
