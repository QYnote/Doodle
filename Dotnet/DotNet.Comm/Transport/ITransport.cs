using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Comm.Transport
{
    public enum PortType
    {
        Serial,
        Socket
    }

    public interface ITransport
    {
        bool IsOpen { get; }
        /// <summary>Port 연결</summary>
        void Open();
        /// <summary>Port 닫기</summary>
        void Close();
        /// <summary>데이터 전송</summary>
        void Write(byte[] data);
        /// <summary>데이터 읽기</summary>
        byte[] Read();
        /// <summary>초기화</summary>
        void Initialize();
    }
}
