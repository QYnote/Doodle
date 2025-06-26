
using DotNet.Comm.Controls.IOPorts;
using DotNet.DrawImage.Controls;
using DotNet.Utils.Controls;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace DotNet.DrawImage.Controls
{
    internal class ProgramPort
    {
        /// <summary>Port Log 전송 Delegate</summary>
        /// <param name="Msg"></param>
        internal delegate void PortLogDelegate(string Msg);
        /// <summary>Port Log 작성용 이벤트</summary>
        internal PortLogDelegate PortLogHandler { get; set; }



        /// <summary>
        /// 프로그램 Ethernet Port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="portNo"></param>
        internal ProgramPort(string ip, int portNo)
        {
            this.PCPort = new PortEthernet(ip, portNo);

            SetDefaultValue();
        }



        /// <summary>
        /// User가 Open했는지에대한 확인
        /// </summary>
        internal bool IsUserOpen { get; private set; }
        /// <summary>
        /// Port 연결 상태
        /// </summary>
        internal bool IsOpen { get { return this.PCPort.IsOpen; } }
        /// <summary>
        /// Program Port명
        /// </summary>
        internal string PortName { get { return this.PCPort.PortName; } }
        /// <summary>
        /// 프로그램 - Client 사이의 Port
        /// </summary>
        internal IOPortBase PCPort { get; private set; }
        /// <summary>
        /// Data 송,수신 처리할 Background Thread
        /// </summary>
        internal BackgroundWorker BgWorker { get; private set; }

        /// <summary>
        /// 읽어들인 Data Buffer
        /// </summary>
        private byte[] Buffer { get; set; }
        internal int FrameLength { get; set; }
        internal int FrameWidth { get; set; }
        private short[] ReadFrame { get; set; }
        private int curWidth {  get; set; }
        



        private void SetDefaultValue()
        {
            this.IsUserOpen = false;

            this.BgWorker = new BackgroundWorker();
            this.BgWorker.WorkerSupportsCancellation = true;

            this.FrameLength = 512;
            this.FrameWidth = 512;
            this.ReadFrame = new short[this.FrameLength * this.FrameWidth];
            this.curWidth = 0;

            this.PCPort.LogHandler = (msg) => { this.PortLogHandler?.Invoke(msg); };
        }

        /// <summary>
        /// 통신 열기
        /// </summary>
        internal void Open()
        {
            if (this.PCPort.IsOpen == true) return;

            if(this.PCPort.Open() == true)
            {
                this.IsUserOpen = true;

                this.BgWorker.RunWorkerAsync();

                this.PortLogHandler?.Invoke(string.Format("({0}) Port Connected", this.PortName));
            }
            else
                this.PortLogHandler?.Invoke(string.Format("[ERROR]({0}) Port Connected", this.PortName));

        }
        /// <summary>
        /// 통신 닫기
        /// </summary>
        internal void Close()
        {
            if (this.PCPort.IsOpen == false) return;

            if(this.PCPort.Close() == true)
            {
                this.IsUserOpen = false;
                this.BgWorker.CancelAsync();

                this.PortLogHandler?.Invoke(string.Format("({0}) Port Disconnected", this.PortName));
            }
            else
                this.PortLogHandler?.Invoke(string.Format("[ERROR]({0}) Port Disconnected", this.PortName));
        }
        /// <summary>
        /// 통신에 쓰기
        /// </summary>
        /// <param name="frame"></param>
        internal void Write(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;

            //Data 전송
            this.PCPort.Write(bytes);

            this.PortLogHandler?.Invoke(string.Format("({0}) Port Write\r\nWrite Data : {1}\r\n", this.PortName, this.ByteArrayToString(bytes)));
        }
        /// <summary>
        /// 통신에서 Data 읽기
        /// </summary>
        /// <param name="frame"></param>
        internal Image Read()
        {
            if (this.PCPort.IsOpen == false) return null;

            this.Buffer = this.PCPort.Read(this.Buffer);

            //읽은 Buffer가 없으면 종료
            if (this.Buffer == null || this.Buffer.Length == 0) return null;

            //최소 Line만큼 읽었는지 확인
            if (this.Buffer.Length < this.FrameLength * sizeof(short)) return null;

            //최소 Line만큼 뒤로 당기고 그자리에 새로읽은 byte[] 채우기
            System.Buffer.BlockCopy(this.ReadFrame, 0, this.ReadFrame, this.FrameLength * sizeof(short), (this.ReadFrame.Length - this.FrameLength) * sizeof(short));
            System.Buffer.BlockCopy(this.Buffer,    0, this.ReadFrame, 0, this.FrameLength * sizeof(short));

            //읽은 byte Buffer 당기기
            byte[] temp = new byte[this.Buffer.Length - (this.FrameLength * sizeof(short))];
            System.Buffer.BlockCopy(this.Buffer, this.FrameLength * sizeof(short), temp, 0, temp.Length);
            this.Buffer = temp;

            //이미지 처리하기
            Bitmap bitmap = new Bitmap(this.FrameWidth, this.FrameLength, PixelFormat.Format16bppRgb555);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, this.FrameWidth, this.FrameLength),
                ImageLockMode.WriteOnly,
                PixelFormat.Format16bppRgb555);
            IntPtr ptr = bitmapData.Scan0;
            byte[] bytes = new byte[this.FrameLength * this.FrameWidth * sizeof(short)];
            int x = 0, y = 0, idx;
            byte[] test = new byte[this.FrameLength * sizeof(short)];

            foreach (short v in this.ReadFrame)
            {
                if (this.IsUserOpen == false) return null;

                Color color = UtilImage.ValueToColor(v);

                short rgb555 = (short)(((color.R >> 3) << 10) | ((color.G >> 3) << 5) | (color.B >> 3));

                idx = ((y * this.FrameWidth) + x) * 2;

                bytes[idx + 1] = (byte)((rgb555 >> 8) & 0xFF);
                bytes[idx] = (byte)(rgb555 & 0xFF);

                test[2 * y] = bytes[idx];
                test[(2 * y) + 1] = bytes[idx + 1];
                y++;
                if(y >= this.FrameLength)
                {
                    x++;
                    y = 0;
                }
            }

            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        /// <summary>
        /// Byte Array 각 값들 string으로 표기
        /// </summary>
        /// <param name="bytes">번역할 Byte Array</param>
        /// <returns>번역된 string값</returns>
        private string ByteArrayToString(byte[] bytes)
        {
            string str = string.Empty;
            if (bytes == null) return str;

            foreach (byte b in bytes)
            {
                //Dec(Hex)
                str += b.ToString() + "(" + b.ToString("X2") + ") ";
            }

            return str;
        }
    }
}
