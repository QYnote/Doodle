using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Data
{
    /// <summary>
    /// Unit Model Group(Type)
    /// </summary>
    public enum UnitType
    {
        UnitType1,
        UnitType2,
        UnitType3
    }
    /// <summary>
    /// Unit 모델 종류
    /// </summary>
    public enum UnitModel
    {
        UnitModel1,
        UnitModel2,
        UnitModel3,
        UnitModel4
    }

    /// <summary>
    /// 통신방법 종류
    /// </summary>
    public enum uProtocolType
    {
        ModBusAscii,
        ModBusRTU,
        ModBusTcpIp
    }

    public enum BaudRate
    {
        _9600,
        _14400
    }

    /// <summary>
    /// Channel Value 종류
    /// </summary>
    public enum ChValueType
    {
        CV, //현재값
        LV, //하한값
        HV  //상한값
    }
}
