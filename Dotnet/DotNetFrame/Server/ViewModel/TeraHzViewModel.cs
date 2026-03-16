using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.Server.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Server.ViewModel
{
    internal class TeraHzViewModel : QYViewModel
    {
        internal enum DataType
        {
            Int16,
            UInt16,
        }

        internal event EventHandler<string> ServerLog;

        private Server_HY_TeraHz _server = new Server_HY_TeraHz();
        private BackgroundWorker _write_worker = new BackgroundWorker();
        private Random _write_rnd = new Random();

        private int _sensor_count = Server_HY_TeraHz.DEFAULT_SENSOR_COUNT;
        private DataType _write_type = DataType.UInt16;
        private List<QYItem> _write_type_list = null;
        private bool _write_sequantial_enable = true;
        private bool _write_detect_enable = false;
        private int _write_detect_offset = Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_OBJECT;
        private bool _write_max_enable = false;
        private int _write_max_offset = Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_MAX;
        private bool _write_random_enable = true;
        private int _write_random_scale = Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_BOUNDSCALE;

        private UInt16[] _write_frame_min = null;
        private object _sensor_lock = new object();

        public string Server_IPAddress
        {
            get => this._server.IP;
            set
            {
                if (this._server.IP != value
                    && this._server.IsOpen == false)
                {
                    this._server.IP = value;
                }
            }
        }
        public int Server_PortNo
        {
            get => this._server.PortNo;
            set
            {
                if(this._server.PortNo != value
                    && this._server.IsOpen == false)
                {
                    this._server.PortNo = value;
                }
            }
        }
        public int Sensor_Count
        {
            get => (int)Math.Log(2, this._sensor_count);
            set
            {
                if (value != this.Sensor_Count)
                {
                    this._sensor_count = (int)Math.Pow(2, value);
                    this.CreateMinFrame();

                    base.OnPropertyChanged(nameof(this.Sensor_Count));
                }
            }
        }

        public DataType WriteType
        {
            get => _write_type;
            set
            {
                if (this.WriteType != value)
                {
                    _write_type = value;
                    this.CreateMinFrame();

                    base.OnPropertyChanged(nameof(this.WriteType));
                }
            }
        }
        public List<QYItem> WriteType_List => this._write_type_list;
        public bool Write_Sequantial_Enable
        {
            get => this._write_sequantial_enable;
            set
            {
                if (this.Write_Sequantial_Enable != value)
                {
                    this._write_sequantial_enable = value;
                    this.CreateMinFrame();

                    base.OnPropertyChanged(nameof(this.Write_Sequantial_Enable));
                }
            }
        }
        public bool Write_Random_Enable
        {
            get => this._write_random_enable;
            set
            {
                if (this.Write_Random_Enable != value)
                {
                    this._write_random_enable = value;

                    base.OnPropertyChanged(nameof(this.Write_Random_Enable));
                }
            }
        }
        public int Write_Random_Scale
        {
            get => this._write_random_scale;
            set
            {
                if (this.Write_Random_Scale != value)
                {
                    if(this.WriteType == DataType.Int16)
                    {
                        if(value > Int16.MaxValue)
                            this._write_random_scale = Int16.MaxValue;
                        else if (value < Int16.MinValue)
                            this._write_random_scale = Int16.MinValue;
                        else
                            this._write_random_scale = value;
                    }
                    else
                    {
                        if (value > UInt16.MaxValue)
                            this._write_random_scale = UInt16.MaxValue;
                        else if (value < UInt16.MinValue)
                            this._write_random_scale = UInt16.MinValue;
                        else
                            this._write_random_scale = value;
                    }

                    base.OnPropertyChanged(nameof(this.Write_Random_Scale));
                }
            }
        }
        public bool Write_Max_Enable
        {
            get => this._write_max_enable;
            set
            {
                if (this.Write_Max_Enable != value)
                {
                    this._write_max_enable = value;

                    base.OnPropertyChanged(nameof(this.Write_Max_Enable));
                }
            }
        }
        public int Write_Max_Offset
        {
            get => this._write_max_offset;
            set
            {
                if (this.Write_Max_Offset != value)
                {
                    if (this.WriteType == DataType.Int16)
                    {
                        if (value > Int16.MaxValue)
                            this._write_max_offset = Int16.MaxValue;
                        else if (value < Int16.MinValue)
                            this._write_max_offset = Int16.MinValue;
                        else
                            this._write_max_offset = value;
                    }
                    else
                    {
                        if (value > UInt16.MaxValue)
                            this._write_max_offset = UInt16.MaxValue;
                        else if (value < UInt16.MinValue)
                            this._write_max_offset = UInt16.MinValue;
                        else
                            this._write_max_offset = value;
                    }

                    base.OnPropertyChanged(nameof(this.Write_Max_Offset));
                }
            }
        }
        public bool Write_Detect_Enable
        {
            get => this._write_detect_enable;
            set
            {
                if (this.Write_Detect_Enable != value)
                {
                    this._write_detect_enable = value;

                    base.OnPropertyChanged(nameof(this.Write_Detect_Enable));
                }
            }
        }
        public int Write_Detect_Offset
        {
            get => this._write_detect_offset;
            set
            {
                if (this.Write_Detect_Offset != value)
                {
                    if (this.WriteType == DataType.Int16)
                    {
                        if (value > Int16.MaxValue)
                            this._write_detect_offset = Int16.MaxValue;
                        else if (value < Int16.MinValue)
                            this._write_detect_offset = Int16.MinValue;
                        else
                            this._write_detect_offset = value;
                    }
                    else
                    {
                        if (value > UInt16.MaxValue)
                            this._write_detect_offset = UInt16.MaxValue;
                        else if (value < UInt16.MinValue)
                            this._write_detect_offset = UInt16.MinValue;
                        else
                            this._write_detect_offset = value;
                    }

                    base.OnPropertyChanged(nameof(this.Write_Detect_Offset));
                }
            }
        }


        internal TeraHzViewModel()
        {
            this._server.ServerLog += (sender, msg) => this.ServerLog.Invoke(this, msg);

            this._write_type_list = QYViewUtils.EnumToItem<DataType>().ToList();

            this.Initialize();
        }

        private void Initialize()
        {
            this.Server_IPAddress = "127.0.0.1";
            this.Server_PortNo = 5000;

            this._write_worker.WorkerSupportsCancellation = true;
            this._write_worker.DoWork += _write_worker_DoWork;

            this.CreateMinFrame();
        }

        internal void Open()
        {
            if (this._write_worker.IsBusy == false)
                this._write_worker.RunWorkerAsync();

            this._server.Open();
        }
        internal void Close()
        {
            this._server.Close();

            if (this._write_worker.IsBusy)
                this._write_worker.CancelAsync();
        }

        private void _write_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (true)
            {
                if (worker.CancellationPending) break;

                try
                {
                    this.UpdateFrame();
                }
                catch
                {

                }
                finally
                {
                }
            }
        }

        private void CreateMinFrame()
        {
            lock (this._sensor_lock)
            {
                int data_scale = UInt16.MaxValue;
                int interval = data_scale / this._sensor_count;
                double rnd_position = 1;

                //Write 신규생성
                this._write_frame_min = new UInt16[this._sensor_count];

                for (int i = 0; i < this._sensor_count; i++)
                {
                    if (this.WriteType == DataType.UInt16)
                    {
                        UInt16 tempValue = 0;

                        if (this.Write_Sequantial_Enable)
                            tempValue = (UInt16)(i * interval);
                        else
                        {
                            if (i % Server_HY_TeraHz.SENSOR_PER_CHIP == 0)
                                rnd_position = this._write_rnd.Next(0, 10000) / 10000d;

                            tempValue = Convert.ToUInt16(data_scale * rnd_position);
                        }


                        if (this._write_frame_min[i] < UInt16.MinValue)
                            tempValue = UInt16.MinValue;
                        else if (this._write_frame_min[i] > UInt16.MaxValue)
                            tempValue = UInt16.MaxValue;

                        this._write_frame_min[i] = tempValue;
                    }
                    else if(this.WriteType == DataType.Int16)
                    {
                        Int16 tempValue = 0;

                        if (this.Write_Sequantial_Enable)
                            tempValue = (Int16)(Int16.MinValue + (i * interval));
                        else
                        {
                            if (i % Server_HY_TeraHz.SENSOR_PER_CHIP == 0)
                                rnd_position = this._write_rnd.Next(0, 10000) / 10000d;

                            tempValue = Convert.ToInt16(Int16.MinValue + (data_scale * rnd_position));
                        }


                        if (tempValue < Int16.MinValue)
                            tempValue = Int16.MinValue;
                        else if (tempValue > Int16.MaxValue)
                            tempValue = Int16.MaxValue;

                        this._write_frame_min[i] = (UInt16)tempValue;
                    }
                }
            }
        }

        private void UpdateFrame()
        {
            if (this._write_frame_min == null) return;

            lock (this._sensor_lock)
            {
                int sensor_count = this._sensor_count;
                UInt16[] write_frame_uint = new UInt16[this._sensor_count];
                byte[] buffer = new byte[this._sensor_count * sizeof(Int16)];

                for (int i = 0; i < this._sensor_count; i++)
                {
                    int temp = this._write_frame_min[i];

                    //Random Data
                    if (this.Write_Random_Enable)
                        temp += Convert.ToInt32(this.Write_Random_Scale * (this._write_rnd.Next(-1000, 1000) / 1000d));

                    if (this.Write_Detect_Enable)
                        //물체 감지값
                        temp += this.Write_Detect_Offset;
                    else if (this.Write_Max_Enable)
                        //Max 교정값
                        temp += this.Write_Max_Offset;

                    if (temp > UInt16.MaxValue)
                        temp = UInt16.MaxValue;
                    else if (temp < UInt16.MinValue)
                        temp = UInt16.MinValue;

                    write_frame_uint[i] = (UInt16)temp;
                }

                Buffer.BlockCopy(write_frame_uint, 0, buffer, 0, buffer.Length);

                this._server.WriteBuffer = buffer;
            }
        }
    }
}
