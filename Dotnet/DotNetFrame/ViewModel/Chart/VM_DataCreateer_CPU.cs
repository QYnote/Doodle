using DotNetFrame.Model.Chart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.ViewModel.Chart
{
    internal class VM_DataCreateer_CPU
    {
        internal const int DEFAULT_DATA_GET_INTERVAL = M_DataCreater_CPU.DEFAULT_DATA_GET_INTERVAL;
        internal const int DEFAULT_DATA_GET_TIME = M_DataCreater_CPU.DEFAULT_DATA_GET_TIME;

        private M_DataCreater_CPU _creater = new M_DataCreater_CPU();

        internal int Interval
        {
            get => this._creater.Interval;
            set => this._creater.Interval = value;
        }

        internal int Time
        {
            get => this._creater.Time;
            set => this._creater.Time = value;
        }


        internal (DateTime, double)[] CPU_Create_Data() => this._creater.CPU_Create_Data();
    }
}
