using DotNetFrame.Chart.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Chart.ViewModel
{
    internal class VM_DataCreater_CPU
    {
        internal const int DEFAULT_DATA_GET_INTERVAL = DataCreater_CPU.DEFAULT_DATA_GET_INTERVAL;
        internal const int DEFAULT_DATA_GET_TIME = DataCreater_CPU.DEFAULT_DATA_GET_TIME;

        private DataCreater_CPU _creater = new DataCreater_CPU();

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
