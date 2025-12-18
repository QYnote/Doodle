using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Chart.Model
{
    internal class DataCreater_CPU
    {
        internal const int DEFAULT_DATA_GET_INTERVAL = 100;
        internal const int DEFAULT_DATA_GET_TIME = 60;

        private int _data_get_interval = DEFAULT_DATA_GET_INTERVAL;
        private int _data_get_time = DEFAULT_DATA_GET_TIME;

        private System.Diagnostics.PerformanceCounter _cpu_cre_counter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
        private System.Collections.Concurrent.ConcurrentQueue<(DateTime, double)> _cpu_cre_queue = new System.Collections.Concurrent.ConcurrentQueue<(DateTime, double)>();
        private System.Timers.Timer _cpu_cre_timer = new System.Timers.Timer();

        public int Interval
        { 
            get => (int)this._cpu_cre_timer.Interval;
            set
            {
                if (_data_get_interval != value)
                {
                    if (value < 50)
                        this._cpu_cre_timer.Interval = 50;
                    else
                        this._cpu_cre_timer.Interval = value;
                }
            }
        }
        public int Time
        {
            get => _data_get_time;
            set
            {
                if (_data_get_time != value)
                {
                    if (value < 1)
                        this._data_get_time = 1;
                    else 
                        _data_get_time = value;
                }
            }
        }

        internal DataCreater_CPU()
        {
            this.InitComponent();
        }

        private void InitComponent()
        {
            this._cpu_cre_timer = new System.Timers.Timer(DEFAULT_DATA_GET_INTERVAL);
            this._cpu_cre_timer.Elapsed += _cpu_cre_timer_Elapsed;

            this._cpu_cre_counter.NextValue();
            System.Threading.Thread.Sleep(DEFAULT_DATA_GET_INTERVAL / 2);
            this._cpu_cre_timer.Start();
        }

        private void _cpu_cre_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                double curUsage = this._cpu_cre_counter.NextValue();
                DateTime curTime = DateTime.Now,
                        minTime = curTime.AddSeconds(-this.Time);

                this._cpu_cre_queue.Enqueue((curTime, curUsage));

                while (true)
                {
                    this._cpu_cre_queue.TryPeek(out var result);

                    if (result.Item1 < minTime)
                        this._cpu_cre_queue.TryDequeue(out var oldData);
                    else break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CPU 데이터 수집 에러: {ex.Message}\r\nTrace:{ex.StackTrace}");
            }
        }

        internal (DateTime, double)[] CPU_Create_Data() => this._cpu_cre_queue.ToArray();
    }
}
