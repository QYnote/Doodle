using Dnf.Comm.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Comm.Controls
{
    internal class Unit
    {
        /// <summary>Unit 연결 상태</summary>
        internal UnitConnectionState State;
        /// <summary>Unit 상위 Port</summary>
        internal readonly ProgramPort ParentPort;  //등록된 Port

        /// <summary>Unit 주소</summary>
        internal int SlaveAddr;
        /// <summary>Unit 구분</summary>
        internal string UnitType;
        /// <summary>Unit Model명</summary>
        internal string UnitModel;
        /// <summary>Unit 사용자 지정명</summary>
        internal string UnitName;
        /// <summary>Unit Registry [주소Dec, Value])></summary>
        internal Dictionary<int, object> UnitRegistry;

        /// <summary>Unit을 담당하는 Node</summary>
        internal TreeNode Node;

        internal Unit(ProgramPort port, int addr, string type, string model, string modelName = null)
        { 
            ParentPort = port;
            SlaveAddr = addr;
            UnitType = type;
            UnitModel = model;
            UnitName = modelName == null || modelName == "" ? model.ToString() : modelName;
            State = UnitConnectionState.Close_DisConnect;
            UnitRegistry = new Dictionary<int, object>();
        }
    }
}
