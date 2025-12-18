using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using DotNetFrame.Charts.ViewModel;
using System;
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
        private GroupBox gbx_creater = new GroupBox();
        private Label lbl_cre_interval = new Label();
        private Label lbl_cre_maxseconds = new Label();

        private GroupBox gbx_filter = new GroupBox();
        private GroupBox gbx_filter_type = new GroupBox();
        private Label lbl_filter_kernal_size = new Label();
        private NumericUpDown num_filter_kernal_size = new NumericUpDown();
        private Label lbl_filter_quantity = new Label();
        private NumericUpDown num_filter_quantity = new NumericUpDown();

        private GroupBox gbx_peak = new GroupBox();
        private Label lbl_peak_kernal_size = new Label();
        private Label lbl_peak_reference = new Label();
        private CheckBox chk_peak_show_all = new CheckBox();
        private CheckBox chk_peak_show_reference = new CheckBox();


        private System.Windows.Forms.DataVisualization.Charting.Chart chart = new System.Windows.Forms.DataVisualization.Charting.Chart();

        private ChartHandler _chartHandler = new ChartHandler();
        private BackgroundWorker _bgWorker = new BackgroundWorker();

        public FrmChart()
        {
            InitializeComponent();
            this.InitText();
            this.InitUI();
            this.InitComponent();
        }

        private void InitUI()
        {
            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Panel1.Padding = new Padding(3);
            split.SplitterDistance = 34;

            this.gbx_creater.Dock = DockStyle.Top;
            this.InitUI_Creater(this.gbx_creater);

            this.gbx_filter.Dock = DockStyle.Top;
            this.InitUI_Filter(this.gbx_filter);

            this.gbx_peak.Dock = DockStyle.Top;
            this.InitUI_Peak(this.gbx_peak);

            this.InitUI_Chart(split.Panel2);

            split.Panel1.Controls.Add(this.gbx_peak);
            split.Panel1.Controls.Add(this.gbx_filter);
            split.Panel1.Controls.Add(this.gbx_creater);
            this.Controls.Add(split);
        }

        private void InitUI_Creater(GroupBox gbx)
        {
            this.lbl_cre_interval.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
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
            num_cre_interval.DataBindings.Add("Value", this._chartHandler, nameof(this._chartHandler.Creater_Interval), true, DataSourceUpdateMode.OnPropertyChanged);

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
            num_cre_maxseconds.DataBindings.Add("Value", this._chartHandler, nameof(this._chartHandler.Creater_Time), true, DataSourceUpdateMode.OnPropertyChanged);

            gbx.Height = this.lbl_cre_maxseconds.Bottom + 3;

            gbx.Controls.Add(this.lbl_cre_interval);
            gbx.Controls.Add(num_cre_interval);
            gbx.Controls.Add(this.lbl_cre_maxseconds);
            gbx.Controls.Add(num_cre_maxseconds);
        }

        private void InitUI_Filter(GroupBox gbx)
        {
            this.gbx_filter_type.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            RadioButton[] rdo_filter_type = QYViewUtils.CreateEnumRadioButton<FilterType>();
            for (int i = 0; i < rdo_filter_type.Length; i++)
            {
                RadioButton rdo = rdo_filter_type[i];
                rdo.Dock = DockStyle.Top;
                QYViewUtils.BindingRadioButton(rdo, this._chartHandler, nameof(this._chartHandler.FilterType), rdo.Tag);

                this.gbx_filter_type.Controls.Add(rdo);
                rdo.BringToFront();
            }
            this.gbx_filter_type.Height = 92;

            
            this.lbl_filter_kernal_size.Left = this.gbx_filter_type.Left;
            this.lbl_filter_kernal_size.Top = this.gbx_filter_type.Bottom + 3;
            this.lbl_filter_kernal_size.Width = this.lbl_cre_maxseconds.Width;
            this.lbl_filter_kernal_size.TextAlign = ContentAlignment.MiddleLeft;
            this.num_filter_kernal_size.Left = this.lbl_filter_kernal_size.Right + 3;
            this.num_filter_kernal_size.Top = this.lbl_filter_kernal_size.Top;
            this.num_filter_kernal_size.Width = 80;
            this.num_filter_kernal_size.DecimalPlaces = 0;
            this.num_filter_kernal_size.TextAlign = HorizontalAlignment.Right;
            this.num_filter_kernal_size.Minimum = 0;
            this.num_filter_kernal_size.Maximum = int.MaxValue;
            this.num_filter_kernal_size.DataBindings.Add("Value", this._chartHandler, nameof(this._chartHandler.Filter_Kernal_Size), true, DataSourceUpdateMode.OnPropertyChanged);


            this.lbl_filter_quantity.Left = this.lbl_filter_kernal_size.Left;
            this.lbl_filter_quantity.Top = this.lbl_filter_kernal_size.Bottom + 3;
            this.lbl_filter_quantity.Width = this.lbl_cre_maxseconds.Width;
            this.lbl_filter_quantity.TextAlign = ContentAlignment.MiddleLeft;
            this.num_filter_quantity.Left = this.lbl_filter_quantity.Right + 3;
            this.num_filter_quantity.Top = this.lbl_filter_quantity.Top;
            this.num_filter_quantity.Width = this.num_filter_kernal_size.Width;
            this.num_filter_quantity.DecimalPlaces = 0;
            this.num_filter_quantity.TextAlign = HorizontalAlignment.Right;
            this.num_filter_quantity.Minimum = 0;
            this.num_filter_quantity.Maximum = int.MaxValue;
            this.num_filter_quantity.DataBindings.Add("Value", this._chartHandler, nameof(this._chartHandler.Filter_Process_Count), true, DataSourceUpdateMode.OnPropertyChanged);

            this.gbx_filter_type.Width = this.num_filter_kernal_size.Right - this.gbx_filter_type.Left;
            gbx.Height = this.lbl_filter_quantity.Bottom + 3;

            gbx.Controls.Add(this.gbx_filter_type);
            gbx.Controls.Add(this.lbl_filter_kernal_size);
            gbx.Controls.Add(this.num_filter_kernal_size);
            gbx.Controls.Add(this.lbl_filter_quantity);
            gbx.Controls.Add(this.num_filter_quantity);
        }

        private void InitUI_Peak(GroupBox gbx)
        {
            this.lbl_peak_kernal_size.Location = new Point(3, (int)QYViewUtils.GroupBox_Caption_Hight(gbx) + 3);
            this.lbl_peak_kernal_size.Width = this.lbl_filter_kernal_size.Width;
            this.lbl_peak_kernal_size.TextAlign = ContentAlignment.MiddleLeft;
            NumericUpDown num_peak_kernal_size = new NumericUpDown();
            num_peak_kernal_size.Left = this.lbl_peak_kernal_size.Right + 3;
            num_peak_kernal_size.Top = this.lbl_peak_kernal_size.Top;
            num_peak_kernal_size.Width = 80;
            num_peak_kernal_size.DecimalPlaces = 0;
            num_peak_kernal_size.TextAlign = HorizontalAlignment.Right;
            num_peak_kernal_size.Minimum = 0;
            num_peak_kernal_size.Maximum = int.MaxValue;
            num_peak_kernal_size.DataBindings.Add("Value", this._chartHandler, nameof(this._chartHandler.Peak_Kernal_Size), true, DataSourceUpdateMode.OnPropertyChanged);

            this.lbl_peak_reference.Left = this.lbl_peak_kernal_size.Left;
            this.lbl_peak_reference.Top = this.lbl_peak_kernal_size.Bottom + 3;
            this.lbl_peak_reference.Width = this.lbl_peak_kernal_size.Width;
            this.lbl_peak_reference.TextAlign = ContentAlignment.MiddleLeft;
            NumericUpDown num_peak_reference = new NumericUpDown();
            num_peak_reference.Left = this.lbl_peak_reference.Right + 3;
            num_peak_reference.Top = this.lbl_peak_reference.Top;
            num_peak_reference.Width = num_peak_kernal_size.Width;
            num_peak_reference.DecimalPlaces = 0;
            num_peak_reference.TextAlign = HorizontalAlignment.Right;
            num_peak_reference.Minimum = 0;
            num_peak_reference.Maximum = int.MaxValue;
            num_peak_reference.DataBindings.Add("Value", this._chartHandler, nameof(this._chartHandler.Peak_Detect_Value), true, DataSourceUpdateMode.OnPropertyChanged);

            this.chk_peak_show_all.Left = this.lbl_peak_reference.Left;
            this.chk_peak_show_all.Top = this.lbl_peak_reference.Bottom + 3;
            this.chk_peak_show_all.Width = this.lbl_peak_reference.Width + 20;
            this.chk_peak_show_all.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_peak_show_all.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_peak_show_all.DataBindings.Add("Checked", this._chartHandler, nameof(this._chartHandler.Peak_Show_All), true, DataSourceUpdateMode.OnPropertyChanged);

            this.chk_peak_show_reference.Left = this.chk_peak_show_all.Left;
            this.chk_peak_show_reference.Top = this.chk_peak_show_all.Bottom + 3;
            this.chk_peak_show_reference.Width = this.chk_peak_show_all.Width;
            this.chk_peak_show_reference.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_peak_show_reference.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_peak_show_reference.DataBindings.Add("Checked", this._chartHandler, nameof(this._chartHandler.Peak_Show_Detect_Value), true, DataSourceUpdateMode.OnPropertyChanged);

            gbx.Height = this.chk_peak_show_reference.Bottom + 3;

            gbx.Controls.Add(this.lbl_peak_kernal_size);
            gbx.Controls.Add(num_peak_kernal_size);
            gbx.Controls.Add(this.lbl_peak_reference);
            gbx.Controls.Add(num_peak_reference);
            gbx.Controls.Add(this.chk_peak_show_all);
            gbx.Controls.Add(this.chk_peak_show_reference);
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
            this.gbx_creater.Text = AppData.Lang("chart.cre.title");
            this.lbl_cre_interval.Text = AppData.Lang("chart.cre.interval");
            this.lbl_cre_maxseconds.Text = AppData.Lang("chart.cre.length");

            this.gbx_filter.Text = AppData.Lang("chart.filter.title");
            this.gbx_filter_type.Text = AppData.Lang("chart.filter.type");
            this.lbl_filter_kernal_size.Text = AppData.Lang("chart.filter.kernal");
            this.lbl_filter_quantity.Text = AppData.Lang("chart.filter.count");

            this.gbx_peak.Text = AppData.Lang("chart.peak.title");
            this.lbl_peak_kernal_size.Text = AppData.Lang("chart.peak.kernal");
            this.lbl_peak_reference.Text = AppData.Lang("chart.peak.reference.value");
            this.chk_peak_show_all.Text = AppData.Lang("chart.peak.show.all");
            this.chk_peak_show_reference.Text = AppData.Lang("chart.peak.show.reference");
        }


        private void InitComponent()
        {
            this._chartHandler.PropertyChanged += _chartHandler_PropertyChanged;

            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += _bgWorker_DoWork;
            this._bgWorker.RunWorkerAsync();
        }

        private void _chartHandler_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(this._chartHandler.FilterType))
            {
                if(this._chartHandler.FilterType == FilterType.None)
                {
                    this.num_filter_kernal_size.Enabled = false;
                    this.num_filter_quantity.Enabled = false;
                }
                else
                {
                    this.num_filter_kernal_size.Enabled = true;
                    this.num_filter_quantity.Enabled = true;
                }
            }
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
                (DateTime, double)[] data = this._chartHandler.Creater_Data_Get();
                if (data == null
                    || (data != null && data.Length == 0)) return;

                double[] arys = new double[data.Length];
                for (int i = 0; i < data.Length; i++)
                    arys[i] = data[i].Item2;

                arys = this._chartHandler.Filter_Apply(arys);

                for (int i = 0; i < data.Length; i++)
                    data[i].Item2 = arys[i];

                Series series = this.chart.Series["CPU"];
                ChartArea area = this.chart.ChartAreas[series.ChartArea];
                series.Points.Clear();
                series.MarkerSize = 0;
                area.AxisY.StripLines.Clear();

                //데이터 입력
                foreach (var item in data)
                    series.Points.AddXY(item.Item1, item.Item2);

                //X축 범위 설정
                DateTime now = DateTime.Now;
                this.chart.ChartAreas[0].AxisX.Maximum = now.ToOADate();
                this.chart.ChartAreas[0].AxisX.Minimum = now.AddSeconds(-this._chartHandler.Creater_Time).ToOADate();
                //Y축 범위 설정
                double yMax = data.Max(x => x.Item2),
                       yMin = data.Min(x => x.Item2);
                if (yMin < yMax)
                {
                    double range = yMax - yMin,
                               offset = range / 10;

                    if (series.YAxisType == AxisType.Primary)
                    {
                        area.AxisY.Minimum = yMin < offset ? 0 : yMin - offset;
                        area.AxisY.Maximum = yMax + offset;
                    }
                }

                //이상치 검출선
                if (this._chartHandler.Peak_Show_Detect_Value)
                {
                    StripLine lineAvgValue = new StripLine();
                    lineAvgValue.Interval = 0;
                    lineAvgValue.IntervalOffset = this._chartHandler.Peak_Detect_Value;
                    lineAvgValue.StripWidth = 0;
                    lineAvgValue.BorderWidth = 2;
                    lineAvgValue.BorderColor = Color.BlueViolet;
                    lineAvgValue.BorderDashStyle = ChartDashStyle.Dash;
                    lineAvgValue.Text = $"{AppData.Lang("chart.peak.outlier.line")}: {lineAvgValue.IntervalOffset:F3}";
                    lineAvgValue.TextOrientation = TextOrientation.Horizontal;
                    lineAvgValue.Tag = true;
                    area.AxisY.StripLines.Add(lineAvgValue);
                }

                foreach (var idx in this._chartHandler.Peak_Index_List_Get(arys))
                {
                    if (this._chartHandler.Peak_Show_All)
                    {
                        series.Points[idx].MarkerSize = 8;
                        series.Points[idx].MarkerStyle = MarkerStyle.Circle;
                        series.Points[idx].MarkerColor = Color.DarkGray;
                    }

                    if (series.Points[idx].YValues[0] > this._chartHandler.Peak_Detect_Value)
                    {
                        series.Points[idx].MarkerSize = 8;
                        series.Points[idx].MarkerStyle = MarkerStyle.Circle;
                        series.Points[idx].MarkerColor = Color.Black;
                    }
                }
            }
        }
    }
}
