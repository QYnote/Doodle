using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public enum TimeoutType
    {
        NONE,
        RES_NONE,
        RES_STOP,
        RES_LONG,
    }

    public class TimeoutChecker
    {
        bool _is_receive = false;
        DateTime _time_send = DateTime.MaxValue;
        DateTime _time_receive = DateTime.MaxValue;

        TimeSpan _timeout_none = TimeSpan.FromMilliseconds(3000);
        TimeSpan _timeout_stop = TimeSpan.FromMilliseconds(5000);
        TimeSpan _timeout_long = TimeSpan.FromMilliseconds(10000);

        public int CheckTimeNone
        {
            get => (int)this._timeout_none.TotalMilliseconds;
            set => this._timeout_none = TimeSpan.FromMilliseconds((int)value);
        }
        public int CheckTimeStop
        {
            get => (int)this._timeout_stop.TotalMilliseconds;
            set => this._timeout_stop = TimeSpan.FromMilliseconds((int)value);
        }
        public int CheckTimeLong
        {
            get => (int)this._timeout_long.TotalMilliseconds;
            set => this._timeout_long = TimeSpan.FromMilliseconds((int)value);
        }


        public void OnSend()
        {
            this._is_receive = false;
            this._time_send = DateTime.Now;
            this._time_receive = DateTime.Now;
        }

        public void OnReceive()
        {
            this._is_receive = true;
            this._time_receive = DateTime.Now;
        }

        public void OnComplete()
        {
            this._is_receive = false;
            this._time_send = DateTime.MaxValue;
            this._time_receive = DateTime.MaxValue;
        }

        public TimeoutType CheckTimeout()
        {
            DateTime now = DateTime.Now;
            if (this._is_receive == false)
            {
                if (now - this._time_send > this._timeout_none)
                {
                    this.OnComplete();
                    return TimeoutType.RES_NONE;
                }
            }
            else
            {
                if (now - this._time_receive > this._timeout_stop)
                {
                    this.OnComplete();
                    return TimeoutType.RES_STOP;
                }
                else if (now - this._time_send > this._timeout_long)
                {
                    this.OnComplete();
                    return TimeoutType.RES_STOP;
                }
            }

            return TimeoutType.NONE;
        }
    }
}
