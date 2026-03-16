using DotNet.Comm.Transport;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.ViewModel;
using DotNet.Utils.Views;
using DotNetFrame.CommTester.Model;
using DotNetFrame.CommTester.Model.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.CommTester.ViewModel.Port
{
    internal class PortVM : QYViewModel
    {
        //Fields
        private CommPort _port = new CommPort();

        private List<QYItem> _list_port_type = new List<QYItem>();
        private List<QYItem> _list_protocol_type = new List<QYItem>();
        private BindingList<string> _log = new BindingList<string>();
        private string _log_text = string.Empty;

        private bool _reapeat_enable = true;
        private int _reapeat_count = 3;
        private bool _reapeat_infinity = false;

        private TransportVM _port_config = null;

        //Property
        public List<QYItem> PortTypeList => this._list_port_type;
        public List<QYItem> ProtocolTypeList => this._list_protocol_type;
        public string LogText => this._log_text;

        public PortType PortType
        {
            get => this._port.PortType;
            set
            {
                if(this.PortType != value)
                {
                    this._port.PortType = value;

                    base.OnPropertyChanged(nameof(PortType));
                }
            }
        }
        public ProtocolType ProtocolType
        {
            get => this._port.ProtocolType;
            set
            {
                if (this.ProtocolType != value)
                {
                    this._port.ProtocolType = value;

                    base.OnPropertyChanged(nameof(ProtocolType));
                }
            }
        }
        public bool Repeat_Enable
        {
            get => this._reapeat_enable;
            set
            {
                if (this._reapeat_enable != value)
                {
                    this._reapeat_enable = value;

                    if (value) this._port.MaxCount = this._reapeat_count;
                    else this._port.MaxCount = 1;

                        base.OnPropertyChanged(nameof(Repeat_Enable));
                    base.OnPropertyChanged(nameof(Repeat_Infinity));
                    base.OnPropertyChanged(nameof(Repeat_Count));
                }
            }
        }
        public bool Repeat_Infinity
        {
            get
            {
                if (this.Repeat_Enable) return this._reapeat_infinity;
                else return false;
            }
            set
            {
                if(this.Repeat_Infinity != value &&
                    this.Repeat_Enable == true)
                {
                    this._reapeat_infinity = value;

                    if (value) this._port.MaxCount = int.MaxValue;
                    else this._port.MaxCount = this._reapeat_count;

                    base.OnPropertyChanged(nameof(Repeat_Infinity));
                    base.OnPropertyChanged(nameof(Repeat_Count));
                }
            }
        }
        public int Repeat_Count
        {
            get => this._reapeat_count;
            set
            {
                if(this.Repeat_Count != value &&
                    this.Repeat_Enable == true &&
                    this.Repeat_Infinity == false)
                {
                    this._port.MaxCount = 
                    this._reapeat_count = value;

                    base.OnPropertyChanged(nameof(Repeat_Count));
                }
            }
        }
        public bool CreateErrorCode
        {
            get => this._port.CreateErrorCode;
            set
            {
                if(this.CreateErrorCode != value &&
                    this.ProtocolType != ProtocolType.None)
                {
                    this._port.CreateErrorCode = value;

                    base.OnPropertyChanged(nameof(CreateErrorCode));
                }
            }
        }

        public bool ErrorCode_Enable => this.ProtocolType != ProtocolType.None;
        public bool Repeat_Count_Enable => this.Repeat_Enable && !this.Repeat_Infinity;
        public bool IsSending => this._port.IsSending;

        public TransportVM PortConfig => this._port_config;


        internal PortVM()
        {
            this._list_port_type = QYViewUtils.EnumToItem<PortType>().ToList();
            this._list_protocol_type = QYViewUtils.EnumToItem<ProtocolType>().ToList();
            this._log.ListChanged += _log_ListChanged;
            this._port.Log += _port_Log;
            this.PropertyChanged += PortVM_PropertyChanged;
        }


        public void Send(string text)
        {
            if (this._port.IsSending)
            {
                this._port.Stop();
            }
            else
            {
                this._log.Clear();
                this._log_text = string.Empty;

                this._port.Send(text);
            }

            base.OnPropertyChanged(nameof(this.IsSending));
        }

        private void _log_ListChanged(object sender, ListChangedEventArgs e)
        {
            if(e.ListChangedType == ListChangedType.ItemAdded)
            {
                this._log_text +=
                    $"{this._log[e.NewIndex]}" +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    $"";

                base.OnPropertyChanged(nameof(this.LogText));
            }
        }

        private void _port_Log(CommResult rst)
        {
            base.SyncContext.Post(_ =>
            {
                string text = "";
                switch (rst.Type)
                {
                    case ResultType.Write:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Send{Environment.NewLine}{this.BinaryToString(rst.Req)}";
                        break;
                    case ResultType.Read:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Read{Environment.NewLine}{this.BinaryToString(rst.Rcv)}";
                        break;
                    case ResultType.Timeout_Stop:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Timeout - Stop{Environment.NewLine}{this.BinaryToString(rst.Rcv)}";
                        break;
                    case ResultType.Timeout_Long:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Timeout - Long{Environment.NewLine}{this.BinaryToString(rst.Rcv)}";
                        break;
                    case ResultType.Timeout_None:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Timeout - None";
                        break;
                    case ResultType.Protocol_Success:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Success{Environment.NewLine}{this.BinaryToString(rst.Rcv)}";
                        break;
                    case ResultType.Protocol_Error_CheckSum:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Protocol Error - CheckSum{Environment.NewLine}{this.BinaryToString(rst.Rcv)}";
                        break;
                    case ResultType.Protocol_Error_Exception:
                        text = $"{rst.Time:yyyy-MM-ddTHH:mm:ss.fffZ}/Protocol Error - Exception{Environment.NewLine}{this.BinaryToString(rst.Rcv)}";
                        break;
                }

                this._log.Add(text);
            }, null);
        }

        private string BinaryToString(byte[] bytes)
        {
            if (bytes == null) return "";

            string text = "";

            foreach (byte b in bytes)
            {
                text += $"{b:X2} ";
            }

            return text;
        }

        private void PortVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.PortType))
            {
                switch (this.PortType)
                {
                    case PortType.Serial:
                        this._port_config = new SerialVM((QYSerialPort)this._port.Transport);
                        break;
                    case PortType.Socket:
                        this._port_config = new SocketVM((QYSocketPort)this._port.Transport);
                        break;
                    default:
                        this._port_config = null;
                        break;
                }

                base.OnPropertyChanged(nameof(PortConfig));
            }
        }
    }

    public abstract class TransportVM : QYViewModel { }
}
