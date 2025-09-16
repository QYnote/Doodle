using Dotnet.Comm;
using DotNet.Comm.ClientPorts;
using DotNet.Comm.Protocols;
using DotNet.Comm.Protocols.Customs.HYNux;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CustomComm.HYNux
{
    internal enum ProtocolType
    {
        None,
        HY_ModbusRTU,
        HY_ModbusRTU_EXP,
        HY_ModbusAscii,
        HY_ModbusAscii_EXP,
        HY_ModbusTCP,
        PCLink_STD,
        PCLink_STD_TH300500,
        PCLink_SUM,
        PCLink_SUM_TD300500,
        PCLink_SUM_TH300500,
    }
    /// <summary>
    /// HY Port
    /// </summary>
    /// <remarks>
    /// 1Port - (1 Request ↔ 1Response) 처리방식
    /// </remarks>
    public class HYCommTesterPort
    {
        public delegate void HYPortLogHandler(string logName, params object[] data);
        public event HYPortLogHandler Log;

        public delegate void ReadWriteHandler(string title, byte[] data);

        private CommType _commType = CommType.Serial;
        private ProtocolType _protocolType = ProtocolType.HY_ModbusRTU;
        private byte[] _sendingBytes = null;
        private byte[] _stackBuffer = null;
        private DateTime _sendingTime = DateTime.MinValue;
        private DateTime _recentReadTime = DateTime.MinValue;
        private int _curBufferLen = 0;
        private int _recentBufferLen = 0;

        public string UserName { get; set; } = string.Empty;
        public CommType CommType
        {
            get { return this._commType; }
            set
            {
                this._commType = value;
                if (value == CommType.Serial)
                {
                    this.ComPort = new QYSerialPort();
                    this.ComPort.Log += (msg) =>
                    {
                        this.Log?.Invoke("ComPortLog", new object[] { msg });
                    };
                }
                else if (value == CommType.Ethernet)
                {
                    this.ComPort = new QYEthernet(false);
                    this.ComPort.Log += (msg) =>
                    {
                        this.Log?.Invoke("ComPortLog", new object[] { msg });
                    };
                }
            }
        }
        internal ProtocolType ProtocolType
        {
            get { return this._protocolType; }
            set
            {
                this._protocolType = value;

                switch (value)
                {
                    case ProtocolType.HY_ModbusRTU:
                        this.Protocol = new HYModbus(true);
                        break;
                    case ProtocolType.HY_ModbusRTU_EXP:
                        this.Protocol = new HYModbus(true) { IsEXP = true };
                        break;
                    case ProtocolType.HY_ModbusAscii:
                        this.Protocol = new HYModbus(true) { IsAscii = true };
                        break;
                    case ProtocolType.HY_ModbusAscii_EXP:
                        this.Protocol = new HYModbus(true) { IsAscii = true, IsEXP = true };
                        break;
                    case ProtocolType.HY_ModbusTCP:
                        this.Protocol = new HYModbus(true) { IsTCP = true };
                        break;
                    case ProtocolType.PCLink_STD:
                        this.Protocol = new PCLink(true);
                        break;
                    case ProtocolType.PCLink_STD_TH300500:
                        this.Protocol = new PCLink(true) { IsTH3500 = true };
                        break;
                    case ProtocolType.PCLink_SUM:
                        this.Protocol = new PCLink(true) { IsSUM = true };
                        break;
                    case ProtocolType.PCLink_SUM_TH300500:
                        this.Protocol = new PCLink(true) { IsSUM = true, IsTH3500 = true };
                        break;
                    case ProtocolType.PCLink_SUM_TD300500:
                        this.Protocol = new PCLink(true) { IsSUM = true, IsTD3500 = true };
                        break;
                    default: this.Protocol = null; break;
                }
            }
        }
        internal Queue<byte[]> RegularQueue { get; } = new Queue<byte[]>();
        internal Queue<byte[]> WriteQueue { get; } = new Queue<byte[]>();
        internal Dictionary<int, Dictionary<int, byte[]>> UnitRegistry { get; } = new Dictionary<int, Dictionary<int, byte[]>>();
        internal CommStatus Status { get; private set; } = CommStatus.DisConnect;
        internal bool IsUserOpen { get; set; } = false;

        public PCPortBase ComPort { get; private set; }
        public ProtocolFrame Protocol { get; set; }
        private BackgroundWorker _bgWorker = new BackgroundWorker();


        internal HYCommTesterPort()
        {
            this.CommType = CommType.Serial;
            this.ProtocolType = ProtocolType.HY_ModbusRTU;
            this._bgWorker.WorkerSupportsCancellation = true;
            this._bgWorker.DoWork += _bgWorker_DoWork;
        }

        internal void Connect()
        {
            try
            {
                if (this.ComPort.IsOpen) return;

                this.ComPort.InitPort();
                this.ComPort.Open();

                this._bgWorker.RunWorkerAsync();
                this.IsUserOpen = true;
            }
            catch
            {
                this.Disconnect();
                throw;
            }
        }

        internal void Disconnect()
        {
            if (this._bgWorker.IsBusy)
                this._bgWorker.CancelAsync();

            this.Status = CommStatus.DisConnect;
            this.ComPort.Close();
            this.ComPort.InitPort();
            this.IsUserOpen = false;
        }

        private void _bgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (this._bgWorker.CancellationPending) return;

                    if (this.ComPort.IsOpen == false)
                    {
                        this.Status = CommStatus.DisConnect;
                        this.ComPort.InitPort();
                        this.ComPort.Open();
                        System.Threading.Thread.Sleep(3000);
                        continue;
                    }

                    if (this._sendingBytes == null)
                    {
                        //비정기 전송
                        if (this.WriteQueue.Count > 0)
                        {
                            this._sendingBytes = this.WriteQueue.Peek();
                        }
                        //정기 전송
                        else if (this.RegularQueue.Count > 0)
                        {
                            this._sendingBytes = this.RegularQueue.Peek();
                        }

                        if (this._sendingBytes != null)
                        {
                            this.SendData(this._sendingBytes);
                            this.Log?.Invoke("Request", new object[] { this._sendingBytes });
                        }
                    }
                    else
                    {
                        //수신
                        //1. Timeout
                        if (this.IsTimeout())
                        {
                            if (this.WriteQueue.Count != 0 && this._sendingBytes == this.WriteQueue.Peek())
                                this.WriteQueue.Dequeue();
                            else if (this.RegularQueue.Count != 0 && this._sendingBytes == this.RegularQueue.Peek())
                                this.RegularQueue.Dequeue();

                            this.Status = CommStatus.DisConnect;
                            this._sendingBytes = null;
                            this._stackBuffer = null;
                            this.ComPort.InitPort();
                            continue;
                        }

                        //2. 수신처리
                        byte[] readBytes = this.ComPort.Read();

                        if (readBytes != null)
                        {
                            if (this._stackBuffer != null)
                            {
                                byte[] temp = new byte[this._stackBuffer.Length + readBytes.Length];
                                Buffer.BlockCopy(this._stackBuffer, 0, temp, 0, this._stackBuffer.Length);
                                Buffer.BlockCopy(readBytes, 0, temp, this._stackBuffer.Length, readBytes.Length);
                                this._stackBuffer = temp;
                            }
                            else
                                this._stackBuffer = readBytes;


                            this._recentReadTime = DateTime.Now;
                            this._curBufferLen = this._stackBuffer.Length;
                            this.Log?.Invoke("StackBuffer", new object[] { this._stackBuffer });

                            //3. Protocol 처리
                            if (this.Protocol == null)
                            {
                                this._sendingBytes = null;
                            }
                            else
                            {
                                byte[] frameBytes = this.Protocol.Response_ExtractFrame(this._stackBuffer, this._sendingBytes);

                                if (frameBytes != null)
                                {
                                    //ErrorCode 확인
                                    bool isErr = false;
                                    if (this.Protocol.ConfirmErrCode(frameBytes) == false)
                                    {
                                        isErr = true;
                                        this.Log?.Invoke("Error-ErrorCode", new object[] { frameBytes });
                                    }
                                    else
                                    {
                                        List<object> readItems = this.Protocol.Response_ExtractData(frameBytes, this._sendingBytes);

                                        if (readItems != null && readItems.Count > 0)
                                        {
                                            if (this.Protocol is HYModbus modbus)
                                            {
                                                foreach (DataFrame_Modbus frame in readItems)
                                                {
                                                    if (frame.FuncCode > 0x80)
                                                    {
                                                        //Protocol Error 처리
                                                        isErr = true;
                                                        this.Log?.Invoke("Error-Protocol", new object[] { frameBytes });
                                                        break;
                                                    }
                                                }
                                            }
                                            else if (this.Protocol is PCLink pcLink)
                                            {

                                            }
                                        }
                                    }
                                    //수신 최종 완료처리
                                    if(isErr == false)
                                        this.Log?.Invoke("Receive End", new object[] { this._sendingBytes, frameBytes });
                                    if (this.WriteQueue.Count != 0 && this._sendingBytes == this.WriteQueue.Peek())
                                        this.WriteQueue.Dequeue();
                                    else if (this.RegularQueue.Count != 0 && this._sendingBytes == this.RegularQueue.Peek())
                                        this.RegularQueue.Dequeue();
                                    this._sendingBytes = null;
                                    this._stackBuffer = null;
                                    this.Status = CommStatus.Connect;
                                }
                            }//Protocol 처리 End
                        }//read 처리 End
                    }//수신 End
                }
                catch
                {
                    this._sendingBytes = null;
                    System.Threading.Thread.Sleep(3000);
                }
                finally
                {
                    System.Threading.Thread.Sleep(20);
                }
            }
        }

        private bool IsTimeout()
        {
            if (this._recentBufferLen <= 0)
            {
                //None Receive Timeout
                TimeSpan ts = DateTime.Now - this._sendingTime;

                if (ts.TotalMilliseconds > 3000)
                {
                    //Receive 없음
                    this.Log?.Invoke("None Response");
                    return true;
                }
            }
            else
            {
                TimeSpan ts = DateTime.Now - this._recentReadTime;

                if (ts.TotalMilliseconds > 10000)
                {
                    if (this._recentBufferLen == this._curBufferLen)
                    {
                        //Receive 중단됨
                        this.Log?.Invoke("Stop Response", new object[] { this._stackBuffer });
                    }
                    else
                    {
                        //Receive 너무 김
                        this.Log?.Invoke("Long Response", new object[] { this._stackBuffer });
                    }

                    return true;
                }
            }

            this._recentBufferLen = this._curBufferLen;

            return false;
        }

        private void SendData(byte[] bytes)
        {
            this._sendingTime = DateTime.Now;
            this._curBufferLen = 0;
            this._recentBufferLen = 0;
            this._stackBuffer = null;

            this.ComPort.Write(bytes);
        }
    }
}
