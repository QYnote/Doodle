using DotNet.Comm.Servers;
using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Server.Model
{
    internal class M_Server_HY_TeraHz
    {
        internal const int DEFAULT_SENSOR_COUNT = 64;
        internal const int DEFAULT_SENSOR_OFFSET_OBJECT = 500;
        internal const int DEFAULT_SENSOR_OFFSET_MAX = 1000;
        internal const int DEFAULT_SENSOR_OFFSET_BOUNDSCALE = 200;
        internal const int SENSOR_PER_CHIP = 16;

        public event Update_WithParam ServerLog;

        private int _device_sensor_count = DEFAULT_SENSOR_COUNT;
        private bool _device_sensor_apply_object = false;
        private Int16 _device_sensor_offset_object = DEFAULT_SENSOR_OFFSET_OBJECT;
        private bool _device_sensor_apply_max = false;
        private Int16 _device_sensor_offset_max = DEFAULT_SENSOR_OFFSET_MAX;
        private bool _device_sensor_apply_random = true;
        private Int16 _device_sensor_offset_boundScale = DEFAULT_SENSOR_OFFSET_BOUNDSCALE;

        private TCPServer _server = new TCPServer();
        private Int16[] _write_min_values = new Int16[DEFAULT_SENSOR_COUNT];
        private Random _write_rnd = new Random();
        private bool _write_isValidating = false;
        private byte[] _read_buffer = null;

        private bool _cmd_send_allow = false;
        private bool _cmd_send_cali_allow = false;
        

        public string IP { get => this._server.IP; set => this._server.IP = value; }
        public int PortNo { get => this._server.PortNo; set => this._server.PortNo = value; }
        public int SensorCount
        {
            get => _device_sensor_count;
            set
            {
                //2의 제곱승에 속하는지 검사
                if ((value > 0 && ((value & (value - 1)) == 0)) == false)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this._write_isValidating = true;

                this._device_sensor_count = value;
                this._write_min_values = new Int16[value];
                this.UpdateSensorCount();

                this._write_isValidating = false;
            }
        }

        public bool ApplyObject { get => _device_sensor_apply_object; set => _device_sensor_apply_object = value; }
        public short OffsetObject { get => _device_sensor_offset_object; set => _device_sensor_offset_object = value; }
        public bool ApplyMax { get => _device_sensor_apply_max; set => _device_sensor_apply_max = value; }
        public short OffsetMax { get => _device_sensor_offset_max; set => _device_sensor_offset_max = value; }
        public bool ApplyRandom { get => _device_sensor_apply_random; set => _device_sensor_apply_random = value; }
        public short OffsetBoundScale { get => _device_sensor_offset_boundScale; set => _device_sensor_offset_boundScale = value; }

        internal M_Server_HY_TeraHz()
        {
            this.IP = "127.0.0.1";
            this.PortNo = 5000;
            this._server.Log += (msg) => { this.ServerLog?.Invoke(msg); };
            this._server.PeriodicSendEvent += _server_PeriodicSendEvent;
            this._server.CreateResponseEvent += _server_CreateResponseEvent;

            this.UpdateSensorCount();
        }

        private void UpdateSensorCount()
        {
            double rndValue = 1;
            for (int i = 0; i < this._device_sensor_count; i++)
            {
                if (this._device_sensor_apply_random)
                {
                    if (i % SENSOR_PER_CHIP == 0)
                        rndValue = this._write_rnd.Next(0, 10000) / 10000d;

                    this._write_min_values[i] = Convert.ToInt16(0x7FFF * rndValue);
                }
                else
                {
                    this._write_min_values[i] = (Int16)(0x7FFF * (i / (float)this._device_sensor_count));
                }

                if (this._write_min_values[i] < 0)
                    this._write_min_values[i] = 0;
                else if (this._write_min_values[i] > 0x7FFF)
                    this._write_min_values[i] = 0x7FFF;
            }
        }

        private byte[] _server_PeriodicSendEvent()
        {
            if (this._cmd_send_allow == false || this._write_isValidating) return null;

            Int16[] outputValue = new Int16[this._device_sensor_count];
            byte[] returnBuffer = new byte[this._device_sensor_count * sizeof(Int16)];

            for (int i = 0; i < this._device_sensor_count; i++)
            {
                int temp = this._write_min_values[i];

                if (this._device_sensor_apply_random)
                    temp += Convert.ToInt16(this._device_sensor_offset_boundScale * (this._write_rnd.Next(-1000, 1000) / 1000d));

                //Max교정값 전송
                if (this._device_sensor_apply_max)
                    temp += this._device_sensor_offset_max;

                //물체 감지 적용
                if (this._device_sensor_apply_object)
                    temp += this._device_sensor_offset_object;

                if (temp > 0x7FFF)
                    outputValue[i] = 0x7FFF;
                else if (temp < 0)
                    outputValue[i] = 0;
                else
                    outputValue[i] = (Int16)temp;
            }

            Buffer.BlockCopy(outputValue, 0, returnBuffer, 0, returnBuffer.Length);

            System.Threading.Thread.Sleep(2);

            return returnBuffer;
        }

        private byte[] _server_CreateResponseEvent(byte[] request)
        {
            if (request == null || this._write_isValidating) return null;

            //0. Test용 수신 Buffer 표기
            this.ServerLog?.Invoke($"Buffer: {ByteToString(request)}");

            //1. Buffer 보관
            if (this._read_buffer == null)
                this._read_buffer = request;
            else
            {
                byte[] temp = new byte[this._read_buffer.Length + request.Length];
                Buffer.BlockCopy(this._read_buffer, 0, temp, 0, this._read_buffer.Length);
                Buffer.BlockCopy(request, 0, temp, this._read_buffer.Length, request.Length);

                this._read_buffer = temp;
            }


            //2. Reqeust Frame 추출
            if (this._read_buffer != null)
            {
                if (this._read_buffer.Length < 6) return null;

                int handle = QYUtils.Find(this._read_buffer, Encoding.ASCII.GetBytes("TS"));
                if (handle < 0)
                {
                    this._read_buffer = null;
                    this.ServerLog?.Invoke($"비정상 Request: {ByteToString(this._read_buffer)}");

                    this._read_buffer = null;
                    return null;
                }

                byte[] reqBytes = new byte[6];
                Buffer.BlockCopy(this._read_buffer, handle, reqBytes, 0, reqBytes.Length);
                string reqStr = Encoding.ASCII.GetString(reqBytes);
                string cmdG = reqStr.Substring(0, 3);
                int cmdN = Convert.ToInt32(reqStr.Substring(4, 2));
                string resStr = string.Empty;

                if (cmdG == "TSN")
                {
                    if (cmdN == 0)
                    {
                        resStr = $"TSN,OK,01,{(this._cmd_send_cali_allow ? "0001" : "0000")}";
                    }
                    else if (cmdN == 1)
                    {
                        resStr = "TSN,OK";
                        this._cmd_send_allow = true;
                    }
                    else if (cmdN == 2)
                    {
                        resStr = "TSN,OK";
                        this._cmd_send_allow = false;
                    }
                    else if (cmdN == 3)
                    {
                        this._cmd_send_cali_allow = true;
                        resStr = "TSN,OK";
                    }
                    else if (cmdN == 4)
                    {
                        this._cmd_send_cali_allow = false;
                        resStr = "TSN,OK";
                    }
                    else if (cmdN == 5 || cmdN == 6)
                    {
                        resStr = $"TSN,OK,{this._device_sensor_count.ToString("D4")}";
                        for (int i = 0; i < this._device_sensor_count; i++)
                            resStr += string.Format(",0000");
                    }
                }
                else if (cmdG == "TST")
                {
                    resStr = "TST,OK";
                }

                if (resStr != string.Empty)
                {
                    byte[] resBytes = Encoding.ASCII.GetBytes(resStr);
                    this.ServerLog?.Invoke(string.Format("Response Bytes: {0}", ByteToString(resBytes)));

                    this._read_buffer = null;
                    return resBytes;
                }

                //초기화
                this._read_buffer = null;
            }


            return null;
        }

        private string ByteToString(byte[] bytes)
        {
            string str = string.Empty;

            if (bytes != null && bytes.Length != 0)
            {
                foreach (var b in bytes)
                    str += string.Format(" {0:X2}", b);
            }

            return str;
        }

        internal void Open()
        {
            this.UpdateSensorCount();

            this._server.Open();
        }

        internal void Close() => this._server.Close();
    }
}
