using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication.Controls
{
    internal class UnitRegistry
    {
        //삭제할 수도 있음
        //Registry 프로그램이 가지고있기에는 내용이 많을것 같음
        //Registry 업다운 시화면에 호출시에만 사용하고 그외에는 사용 안할것 같음
        internal int CurrentValue;
        internal int RegAddr;
        internal string RegName;
        internal string ValueType;
        internal string DefaultValue;
        internal bool RWmode;

        //ValueType별 값
        //Numeric
        internal int Dot;       //소수점위치
        internal int MaxValue;  //최댓값
        internal int MinValue;  //최솟값
        //ComboBox
        internal string[] Items;    //ComboBox 목록
        //Text
        internal int MaxLength; //Text 최대길이

        /// <summary>
        /// Unit Registry 정보
        /// </summary>
        /// <param name="Addr">주소값</param>
        internal UnitRegistry(int Addr)
        {
            RegAddr = Addr;
        }
    }
}
