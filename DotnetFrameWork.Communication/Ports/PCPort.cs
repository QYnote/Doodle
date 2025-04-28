using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetFrameWork.Communication.Ports
{
    public abstract class PCPort
    {
        

        public abstract bool IsOpen { get; }
        public abstract int ReadBufferLength { get; }
        public string Name { get; private set; }

        public PCPort(string name)
        {
            this.Name = name;
        }

        public abstract void Initialize();
        public abstract bool Close();
        public abstract bool Open();
        public abstract byte[] Read(byte[] buffer);
        public abstract bool Write(byte[] buffer);
    }
}
