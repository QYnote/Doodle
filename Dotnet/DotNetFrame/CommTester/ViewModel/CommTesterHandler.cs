using DotNet.Comm;
using DotNet.Comm.ClientPorts.OSPort;
using DotNet.Utils.Controls.Utils;
using DotNet.Utils.Views;
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
    internal class CommTesterHandler : QYViewModelHandler
    {
        public event EventHandler<TestDataFrame> FrameUpated;

        private SerialHandler _serial;
        private EthernetHandler _ethernet;

        private M_CommTester _commTester = new M_CommTester();
        private List<QYViewUtils.EnumItem<CommType>> _port_type_list;
        private List<QYViewUtils.EnumItem<ProtocolType>> _port_protocol_type_list;
        private bool _port_protocol_errorcode_add;
        private bool _port_comm_repeat_enable;
        private int _port_comm_repeat_count;
        private bool _port_comm_repeat_infinity;
        private string _port_comm_request;

        internal SerialHandler Serial { get => _serial; }
        internal EthernetHandler Ethernet { get => _ethernet; }
        public List<QYViewUtils.EnumItem<CommType>> Port_Type_List { get => _port_type_list;}
        internal List<QYViewUtils.EnumItem<ProtocolType>> Port_protocol_type_list { get => _port_protocol_type_list;}


        public CommType Port_Type
        {
            get => this._commTester.PortType;
            set
            {
                if (this._commTester.PortType != value)
                {
                    this._commTester.PortType = value;
                    base.OnPopertyChanged(nameof(this.Port_Type));
                }
            }
        }
        public ProtocolType Port_Protocol_Type
        {
            get => this._commTester.ProtocolType;
            set
            {
                if(this._commTester.ProtocolType != value)
                {
                    this._commTester.ProtocolType = value;

                    if (value == ProtocolType.None)
                        this.Port_Protocol_ErrorCode_Add = false;

                    base.OnPopertyChanged(nameof(this.Port_Protocol_Type));
                }
            }
        }
        public bool Port_Protocol_ErrorCode_Add
        {
            get => this._port_protocol_errorcode_add;
            set => this._port_protocol_errorcode_add = value;
        }

        public bool Port_Comm_Repeat_Enable
        {
            get => this._port_comm_repeat_enable;
            set
            {
                if(this._port_comm_repeat_enable != value)
                {
                    this._port_comm_repeat_enable = value;
                    base.OnPopertyChanged(nameof(this.Port_Comm_Repeat_Enable));
                }
            }
        }
        public int Port_Comm_Repeat_Count
        {
            get => this._port_comm_repeat_count;
            set
            {
                if(this._port_comm_repeat_count != value)
                {
                    if (value < 1)
                        this._port_comm_repeat_count = 1;
                    else
                        this._port_comm_repeat_count = value;
                }
            }
        }
        public bool Port_Comm_Repeat_Infinity
        {
            get => this._port_comm_repeat_infinity;
            set
            {
                if(this._port_comm_repeat_infinity != value)
                {
                    this._port_comm_repeat_infinity = value;
                    base.OnPopertyChanged(nameof(this.Port_Comm_Repeat_Infinity));
                }
            }
        }
        public string Port_Comm_Request
        {
            get => this._port_comm_request;
            set => this._port_comm_request = value;
        }

        public bool Port_IsUserOpen
        {
            get => this._commTester.IsOpen_App;
            set
            {
                if (this._commTester.IsOpen_App != value)
                {
                    if (value)
                    {
                        this._commTester.Connect();
                    }
                    else
                    {
                        this._commTester.Disconnect();
                    }

                    base.OnPopertyChanged(nameof(this.Port_IsUserOpen));
                }
            }
        }
        public bool Port_IsPortOpen => this._commTester.IsOpen_OS;
        public Color Port_IsPortOpen_Color
        {
            get
            {
                if (this.Port_IsPortOpen)
                    return Color.Green;
                else
                    return Color.Red;
            }
        }
        public bool Port_Requesting => this._commTester.IsWriting;

        internal CommTesterHandler()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this._commTester.TestFrameUpdated += (s, e) => { this.FrameUpated?.Invoke(s, e.CopyFrom()); };
            this._serial = new SerialHandler(this._commTester.OSPort as QYSerialPort);
            this._ethernet = new EthernetHandler(this._commTester.OSPort as QYEthernet);

            this._port_type_list = QYViewUtils.GetEnumItems<CommType>().ToList();
            this._port_protocol_type_list = QYViewUtils.GetEnumItems<ProtocolType>().ToList();

            this.Port_Type = CommType.Serial;
        }

        internal void Data_Register()
        {
            byte[] bytes = this.ConvertTextToByte(this.Port_Comm_Request);
            if (bytes == null) return;

            if (this.Port_Protocol_ErrorCode_Add)
            {
                byte[] errCode = this._commTester.Create_ErrorCode(bytes);
                if (errCode == null) return;

                bytes = QYUtils.Comm.BytesAppend(bytes, errCode);
            }

            int tryCount = this.Port_Comm_Repeat_Enable ? (this.Port_Comm_Repeat_Infinity ? int.MaxValue : this.Port_Comm_Repeat_Count) : 1;

            this._commTester.Register_Data(bytes, tryCount);
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
