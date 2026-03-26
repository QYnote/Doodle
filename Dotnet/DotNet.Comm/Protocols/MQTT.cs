using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Protocols
{
    public enum MQTTPacketType
    {
        Reserved = 0,
        /// <summary>연결요청</summary>
        /// <remarks>Client → Server</remarks>
        CONNECT = 1,
        /// <summary>연결요청 답변</summary>
        /// <remarks>Server → Client</remarks>
        CONNACK = 2,
        PUBLISH = 3,
        PUBACK = 4,
        PUBREC = 5,
        PUBREL = 6,
        PUBCOMP = 7,
        /// <summary>Data받기 등록</summary>
        SUBSCRIBE = 8,
        SUBACK = 9,
        /// <summary>Data받기 등록해제</summary>
        UNSUBSCRIBE = 10,
        UNSUBACK = 11,
        PINGREQ = 12,
        PINGRESP = 13,
        /// <summary>연결 해제</summary>
        DISCONNECT = 14,
        AUTH = 15,
    }

    public class MQTTConfig
    {
        public string ClientID { get; set; } = Guid.NewGuid().ToString();
        public bool CleanSession {  get; set; } = true;
        public UInt16 KeepAlive { get; set; } = 60; //초

        public string Username { get; set; }
        public string Password { get; set; }

        //Will(유언장)
        public bool WillFlag { get; set; } = false;
        public string WillTopic {  get; set; }
        public string WillMesssgae { get; set; }
        public byte WillQos { get; set; } = 0;
        public bool WillRetain { get; set; } = false;
    }

    public class MQTT
    {
        public byte[] Connect(MQTTConfig config)
        {
            //Protocol Name
            var payload = new List<byte>();
            payload.AddRange(new byte[] { 0x00, 0x04 });
            payload.AddRange(Encoding.UTF8.GetBytes("MQTT"));

            //Protocol 버전
            if(true)
                //5.0.0
                payload.Add(0x05);
            else
                //3.1.1
                payload.Add(0x04);

            //Connect Flags
            byte flag = 0;
            if (config.CleanSession) flag |= 0b00000010;
            if (config.WillFlag) flag |= 0b00000100;
            if (config.WillFlag && config.WillRetain) flag |= 0b00100000;
            if (config.WillFlag) flag |= (byte)((config.WillQos & 0b11) << 3);
            if (string.IsNullOrEmpty(config.Username) == false) flag |= 0b10000000;
            if (string.IsNullOrEmpty(config.Username) == false &&
                string.IsNullOrEmpty(config.Password) == false) flag |= 0b01000000;
            payload.Add(flag);

            //Keep Alive
            payload.Add((byte)((config.KeepAlive >> 8) & 0xFF));
            payload.Add((byte)(config.KeepAlive & 0xFF));

            //순서 지키기 필수
            //1. Client ID
            payload.AddRange(this.LengthToBytes(config.ClientID.Length));
            payload.AddRange(Encoding.UTF8.GetBytes(config.ClientID));

            //2. Will Topic
            if (config.WillFlag)
            {
                payload.AddRange(this.LengthToBytes(config.WillTopic.Length));
                payload.AddRange(Encoding.UTF8.GetBytes(config.WillTopic));

                payload.AddRange(this.LengthToBytes(config.WillMesssgae.Length));
                payload.AddRange(Encoding.UTF8.GetBytes(config.WillMesssgae));
            }

            //3. Username / Password
            if (string.IsNullOrEmpty(config.Username) == false)
            {
                payload.AddRange(this.LengthToBytes(config.Username.Length));
                payload.AddRange(Encoding.UTF8.GetBytes(config.Username));

                if (string.IsNullOrEmpty(config.Password) == false)
                {
                    payload.AddRange(this.LengthToBytes(config.Password.Length));
                    payload.AddRange(Encoding.UTF8.GetBytes(config.Password));
                }
            }

            //Fixed Header 생성
            var packet = new List<byte>();
            packet.Add((byte)MQTTPacketType.CONNECT);
            packet.AddRange(this.LengthToBytes(payload.Count));
            packet.AddRange(payload);

            return packet.ToArray();
        }

        protected byte[] LengthToBytes(int length)
        {
            int curLen = length;
            List<byte> bytes = new List<byte>();

            do
            {
                byte b = (byte)(curLen & 0x7F);
                curLen = (curLen >> 7);

                if(curLen > 0)
                    b = (byte)(b | 0x80);

                bytes.Add(b);
            } while (curLen > 0);

            return bytes.ToArray();
        }
    }
}
