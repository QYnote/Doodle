using Dnf.Communication.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    internal class Unit
    {
        internal ConnectionState State;
        internal readonly Port ParentPort;  //등록된 Port

        internal int SlaveAddr;       //Unit 주소
        internal string UnitType;   //Unit 구분
        internal string UnitModel;   //Unit 이름
        internal string UnitName;    //Unit 사용자 지정명
        internal Dictionary<int, object> UnitRegistry;    //Unit Registry정보<주소[Decimal], 정보List>

        internal Unit(Port port, int addr, string type, string model, string modelName = null)
        { 
            ParentPort = port;
            SlaveAddr = addr;
            UnitType = type;
            UnitModel = model;
            UnitName = modelName == null || modelName == "" ? model.ToString() : modelName;
            State = ConnectionState.Closed;
            UnitRegistry = new Dictionary<int, object>();
        }
    }
}
