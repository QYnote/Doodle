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

            this._port.OnSendBinary += _port_OnSendBinary;
            this._port.NoneProtocolReceive += _port_NoneProtocolReceive;
            this._port.OnResult += _port_OnResult;
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


        private void _port_OnSendBinary(byte[] binary)
        {
            base.SyncContext.Post(_ =>
            {
                string txt = this.BinaryToString(binary);
                this._log.Add($"{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffZ}/Request{Environment.NewLine}{txt}");
            }, null);
        }

        private void _port_NoneProtocolReceive(byte[] binary)
        {
            base.SyncContext.Post(_ =>
            {
                string txt = this.BinaryToString(binary);
                this._log.Add($"{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffZ}/Receive{Environment.NewLine}{txt}");
            }, null);
        }

        private void _port_OnResult(DotNet.CommTester.Model.Protocol.IProtocolResult result)
        {
            base.SyncContext.Post(_ =>
            {
                string txt = this.BinaryToString(result.Response);
                string style = "";
                if (result.Type == DotNet.CommTester.Model.Protocol.ResultType.Timeout)
                {
                    style = "Timeout";
                }
                else if(result.Type == DotNet.CommTester.Model.Protocol.ResultType.CheckSum_Error)
                {
                    style = "CheckSum Error";
                }
                else if (result.Type == DotNet.CommTester.Model.Protocol.ResultType.Protocol_Exception)
                {
                    style = "Protocol Error";
                }
                else if(result.Type == DotNet.CommTester.Model.Protocol.ResultType.Success)
                {
                    style = "Complete";
                }

                this._log.Add($"{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fffZ}/{style}{Environment.NewLine}{txt}");
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
