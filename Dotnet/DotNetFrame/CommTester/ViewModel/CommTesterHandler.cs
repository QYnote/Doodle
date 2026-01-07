using DotNet.Comm;
using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
using DotNetFrame.Base.Model;
using DotNetFrame.CommTester.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DotNetFrame.CommTester.ViewModel
{
    internal class CommTesterHandler : QYBindingBase
    {
        public event EventHandler<TestDataFrame> FrameUpated;

        private SerialHandler _serialHandler;
        private EthernetHandler _ethernetHandler;

        private Model.CommTester _commTester = new Model.CommTester();
        private List<QYViewUtils.EnumItem<PortType>> _port_type_list;
        private List<QYViewUtils.EnumItem<ProtocolType>> _port_protocol_type_list;
        private string _port_comm_request = string.Empty;

        internal SerialHandler SerialHandler { get => _serialHandler; }
        internal EthernetHandler EthernetHandler { get => _ethernetHandler; }
        public List<QYViewUtils.EnumItem<PortType>> Port_Type_List { get => _port_type_list;}
        internal List<QYViewUtils.EnumItem<ProtocolType>> Port_protocol_type_list { get => _port_protocol_type_list;}
        public Model.CommTester CommTester => this._commTester;

        public string ConnectionText
        {
            get
            {
                if(this.CommTester.IsAppOpen)
                    return AppData.Lang("commtester.portproperty.disconnect.text");
                else
                    return AppData.Lang("commtester.portproperty.connect.text");
            }
        }
        public Color ConnectionColor
        {
            get
            {
                if (this.CommTester.OSPort.IsOpen)
                    return Color.Green;
                else
                    return Color.Red;
            }
        }

        public bool Reg_AddErrCode_Enable
            => this.CommTester.ProtocolType != ProtocolType.None;
        public bool Reg_Repeat_Count_Enable
            => this.CommTester.Reg_Repeat_Enable
            && this.CommTester.Reg_Repeat_Infinity == false;
        public bool Reg_Repeat_Infinity_Enable
            => this.CommTester.Reg_Repeat_Enable;

        public string Text
        {
            get => this._port_comm_request;
            set
            {
                if (this.Text != value)
                {
                    this._port_comm_request = value;
                }
            }
        }
        public string RequestButtonText
        {
            get
            {
                if(this.CommTester.IsWriting)
                    return AppData.Lang("commtester.commproperty.stop.text");
                else
                    return AppData.Lang("commtester.commproperty.send.text");
            }
        }


        internal CommTesterHandler()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this._commTester.TestFrameUpdated += (s, e) => { this.FrameUpated?.Invoke(s, e.CopyFrom()); };
            this._commTester.PropertyChanged += _commTester_PropertyChanged;
            this._serialHandler = new SerialHandler(this._commTester.OSPort);
            this._ethernetHandler = new EthernetHandler(this._commTester.OSPort);

            this._port_type_list = QYViewUtils.GetEnumItems<PortType>().ToList();
            this._port_protocol_type_list = QYViewUtils.GetEnumItems<ProtocolType>().ToList();

            this.CommTester.PortType = PortType.Serial;
            this.CommTester.ProtocolType = ProtocolType.None;
        }

        private void _commTester_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender is Model.CommTester tester)
            {
                if(e.PropertyName == nameof(tester.IsAppOpen))
                {
                    base.OnPropertyChanged(nameof(this.ConnectionText));
                }
                else if(e.PropertyName == nameof(tester.ProtocolType))
                {
                    base.OnPropertyChanged(nameof(this.Reg_AddErrCode_Enable));
                }
                else if(e.PropertyName == nameof(tester.Reg_Repeat_Enable))
                {
                    base.OnPropertyChanged(nameof(this.Reg_Repeat_Count_Enable));
                    base.OnPropertyChanged(nameof(this.Reg_Repeat_Infinity_Enable));
                }
                else if(e.PropertyName == nameof(tester.Reg_Repeat_Infinity))
                {
                    base.OnPropertyChanged(nameof(this.Reg_Repeat_Count_Enable));
                }
                else if(e.PropertyName == nameof(tester.IsWriting))
                {
                    base.OnPropertyChanged(nameof(this.RequestButtonText));
                }
            }
            else if(sender is OSPortBase osPort)
            {
                if(e.PropertyName == nameof(osPort.IsOpen))
                {
                    base.OnPropertyChanged(nameof(this.ConnectionColor));
                }
            }
        }

        public void Connection()
        {
            if (this.CommTester.IsAppOpen)
                this.CommTester.Disconnect();
            else
                this.CommTester.Connect();
        }


        internal void Data_Register()
        {
            if (this.Text == null || this.Text == string.Empty) return;

            byte[] bytes = this.ConvertTextToByte(this.Text);
            if (bytes == null) return;

            this._commTester.Register_Data(bytes);
        }
        
        /// <summary>
        /// Text → Byte 변환
        /// </summary>
        /// <param name="text">변환할 Text</param>
        /// <returns>변환 된 Byte Array</returns>
        private byte[] ConvertTextToByte(string text)
        {
            int handle = 0;
            List<byte> bytes = new List<byte>();

            while (handle < text.Length)
            {
                char c = text[handle];
                int len;

                //범위 지정
                if (c == '@') len = 3;
                else if (c == '#') len = 2;
                else
                {
                    if (++handle > text.Length) break;
                    else continue;
                }

                if (++handle + len > text.Length) break;

                //변환 시도
                bool tryResult = false;
                string byteStr = text.Substring(handle, len);
                byte b = 0;
                if (c == '@')
                    tryResult = byte.TryParse(byteStr, out b);
                else if (c == '#')
                    tryResult = byte.TryParse(byteStr,
                        System.Globalization.NumberStyles.HexNumber,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out b);


                if (tryResult)
                {
                    bytes.Add(b);
                    handle += len;
                }
            }

            if (bytes.Count == 0)
                return null;
            else
                return bytes.ToArray();
        }
    }
}
