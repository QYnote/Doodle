using Dnf.Utils.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication
{
    public class Unit
    {
        public ConnectionState State;
        public readonly Port ParentPort;  //등록된 Port
        public int SlaveAddr { get; }       //Unit 주소
        public readonly UnitType UnitModelType;   //Unit 구분
        public readonly UnitModel UnitModelName;   //Unit 구분
        public readonly string UnitModelUserName;    //Unit 모델명
        public UnitRegistry UnitRegistry;
        public List<UnitChannel> Channel;

        public Unit(Port port, int addr, UnitType type, UnitModel model, string modelName = null)
        { 
            ParentPort = port;
            SlaveAddr = addr;
            UnitModelType = type;
            UnitModelName = model;
            UnitModelUserName = modelName == null || modelName == "" ? model.ToString() : modelName;
            State = ConnectionState.Closed;

            UnitRegistry = new UnitRegistry();
            Channel = new List<UnitChannel>();
        }
    }
}
