using DotNet.Utils.Controls.Utils;
using DotNetFrame.Chart.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Charts.ViewModel
{
    internal enum FilterType
    {
        [System.ComponentModel.Description("미사용")]
        None,
        [System.ComponentModel.Description("이동평균")]
        MAF,
        [System.ComponentModel.Description("가중이동평균")]
        WAF,
    }

    internal class ChartHandler : QYBindingBase
    {
        private DataCreater_CPU _creater = new DataCreater_CPU();
        private DataFilter _filter = new DataFilter();

        private FilterType _filter_type_current;
        private int _filter_kernal_size;
        private int _filter_process_count;

        private int _peak_kernal_size;
        private double _peak_detect_value;
        private bool _peak_show_all;
        private bool _peak_show_detect_value;

        private List<QYUtils.EnumItem<FilterType>> _filter_type_list;

        public int Creater_Interval
        {
            get => this._creater.Interval;
            set
            {
                if (this._creater.Interval != value)
                {
                    this._creater.Interval = value;
                    base.OnPropertyChanged(nameof(this.Creater_Interval));
                }
            }
        }
        public int Creater_Time
        {
            get => this._creater.Time;
            set => this._creater.Time = value;
        }

        public FilterType FilterType
        {
            get => _filter_type_current;
            set
            {
                if(_filter_type_current != value)
                {
                    this._filter_type_current = value;
                    base.OnPropertyChanged(nameof(FilterType));
                }
            }
        }
        public int Filter_Kernal_Size { get => _filter_kernal_size; set => _filter_kernal_size = value; }
        public int Filter_Process_Count { get => _filter_process_count; set => _filter_process_count = value; }

        public int Peak_Kernal_Size { get => _peak_kernal_size; set => _peak_kernal_size = value; }
        public double Peak_Detect_Value { get => _peak_detect_value; set => _peak_detect_value = value; }
        public bool Peak_Show_All { get => _peak_show_all; set => _peak_show_all = value; }
        public bool Peak_Show_Detect_Value { get => _peak_show_detect_value; set => _peak_show_detect_value = value; }
        internal List<QYUtils.EnumItem<FilterType>> Filter_Type_List { get => _filter_type_list; }

        internal ChartHandler()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this._filter_type_list = QYUtils.GetEnumItems<FilterType>().ToList();

            this.FilterType = FilterType.WAF;
            this.Filter_Kernal_Size = DataFilter.DEFAULT_FILTER_KERNAL_SIZE;
            this.Filter_Process_Count = 5;

            this.Peak_Kernal_Size = DataFilter.DEFAULT_PEAK_KERNAL_SIZE;
            this.Peak_Detect_Value = 15;
            this.Peak_Show_All = false;
            this.Peak_Show_Detect_Value = true;
        }

        internal (DateTime, double)[] Creater_Data_Get() => this._creater.CPU_Create_Data();

        internal double[] Filter_Apply(double[] rawData)
        {
            if (rawData == null
                || (rawData != null && rawData.Length == 0)) return null;

            //데이터 필터링
            for (int i = 0; i < this.Filter_Process_Count; i++)
            {
                if (this.FilterType == FilterType.MAF)
                    rawData = this._filter.MAF(rawData, this.Filter_Kernal_Size);
                else if (this.FilterType == FilterType.WAF)
                    rawData = this._filter.WAF(rawData, this.Filter_Kernal_Size);
            }

            return rawData;
        }

        internal List<int> Peak_Index_List_Get(double[] ary) => this._filter.GetPeakIndexList(ary, this.Peak_Kernal_Size);
    }
}
