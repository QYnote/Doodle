using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication
{
    public enum UnitType
    {
        UnitType1,
        UnitType2,
        UnitType3
    }
    public enum UnitModel
    {
        UnitModel1,
        UnitModel2,
        UnitModel3
    }

    public class Unit
    {
        public readonly Port ParentPort;  //등록된 Port
        public int SlaveAddr { get; }       //Unit 주소
        public readonly UnitType UnitModelType;   //Unit 구분
        public readonly UnitModel UnitModelName;   //Unit 구분
        public readonly string UnitModelUserName;    //Unit 모델명
        public List<UnitRegistry> Registries { get; }

        public Unit(Port port, int addr, UnitType type, UnitModel model, string modelName = null)
        { 
            ParentPort = port;
            SlaveAddr = addr;
            UnitModelType = type;
            UnitModelName = model;
            UnitModelUserName = modelName == null || modelName == "" ? model.ToString() : modelName;
            Registries = new List<UnitRegistry>();
        }
    }
}
