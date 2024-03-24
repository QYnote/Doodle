using Dnf.Communication.Controls;
using Dnf.Communication.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Communication
{
    //프로그램 실행동안 가질 데이터
    public static class RuntimeData
    {
        public readonly static string DataPath = string.Format("{0}Data", AppDomain.CurrentDomain.BaseDirectory);   //일단 만들어둔 Default Path
        public static Dictionary<string, Port> Ports = new Dictionary<string, Port>();  //만들어진 Port

        private static DataTable dt = CreateDtImsi();    //DB Code-나라별언어 Table

        private static DataTable CreateDtImsi()
        {
            //글로벌 DB 적용전 임시 Table
            DataTable dtimsi = new DataTable();
            dtimsi.TableName = "Base";

            dtimsi.Columns.Add("Code");
            dtimsi.Columns.Add("Ko");

            //F : Form / A : Alarm Message
            //Form(1)_화면순서(1)_Control순서(1)_Item번호(2)
            //메뉴
            dtimsi.Rows.Add("F0602", "통신그룹");
            dtimsi.Rows.Add("F0600", "파일");
            dtimsi.Rows.Add("F0601", "통신");
            dtimsi.Rows.Add("F0000", "Xml 저장");
            dtimsi.Rows.Add("F0001", "Xml 불러오기");
            //Port 정보
            dtimsi.Rows.Add("F0100", "Port명");
            dtimsi.Rows.Add("F0101", "통신방법");
            dtimsi.Rows.Add("F0102", "BaudRate");
            dtimsi.Rows.Add("F0103", "Data Bits");
            dtimsi.Rows.Add("F0104", "Stop Bits");
            dtimsi.Rows.Add("F0105", "Parity BIt");
            dtimsi.Rows.Add("F0106", "Port 생성");
            //생성 Port
            dtimsi.Rows.Add("F0200", "생성된 Port");
            dtimsi.Rows.Add("F0201", "Port 열기");
            dtimsi.Rows.Add("F0202", "Port 닫기");
            dtimsi.Rows.Add("F0203", "Port 삭제");
            dtimsi.Rows.Add("F0204", "Item 생성");
            //Unit 정보
            dtimsi.Rows.Add("F0300", "Slave Address");
            dtimsi.Rows.Add("F0301", "모델 구분");
            dtimsi.Rows.Add("F0302", "모델");
            dtimsi.Rows.Add("F0303", "모델명(사용자)");
            dtimsi.Rows.Add("F0304", "Unit 생성");
            dtimsi.Rows.Add("F0305", "Unit 삭제");
            //생성 Unit
            dtimsi.Rows.Add("F0400", "생성된 Unit");
            dtimsi.Rows.Add("F0401", "Data 전송");
            //Channel 정보
            dtimsi.Rows.Add("F0700", "Model 채널");
            dtimsi.Rows.Add("F0701", "사용 여부");
            //생성 Channel

            //DataGridView
            dtimsi.Rows.Add("F0500", "속성");
            dtimsi.Rows.Add("F0501", "값");

            //Port, Unit 생성Form
            dtimsi.Rows.Add("F1000", "새로 만들기");
            dtimsi.Rows.Add("F1001", "저장");
            dtimsi.Rows.Add("F1002", "삭제");
            dtimsi.Rows.Add("F1003", "취소");

            //알림
            dtimsi.Rows.Add("A001", "Port 선택됨");
            dtimsi.Rows.Add("A002", "Port가 선택되지 않았습니다.");
            dtimsi.Rows.Add("A003", "Port가 생성되지 않았습니다.");
            dtimsi.Rows.Add("A004", "사용 중인 포트입니다.");
            dtimsi.Rows.Add("A017", "이미 만들어진 Port의 Port명은 변경할 수 없습니다.");
            dtimsi.Rows.Add("A005", "Port 생성됨");
            dtimsi.Rows.Add("A006", "Port 삭제됨");
            dtimsi.Rows.Add("A007", "Port 연결성공");
            dtimsi.Rows.Add("A008", "Port 연결실패");
            dtimsi.Rows.Add("A009", "Port 닫기성공");
            dtimsi.Rows.Add("A010", "Port 닫기실패");
            dtimsi.Rows.Add("A011", "Unit 모델이 선택되지 않았습니다.");
            dtimsi.Rows.Add("A012", "사용 중인 Slave Address입니다.");
            dtimsi.Rows.Add("A012", "이미 생성된 Unit의 Slave Address는 변경할 수 없습니다.");
            dtimsi.Rows.Add("A013", "Unit 생성됨");
            dtimsi.Rows.Add("A014", "XML Data 저장됨");
            dtimsi.Rows.Add("A015", "XML Data 불러옴");
            dtimsi.Rows.Add("A016", "파일이 존재하지 않습니다.");
            dtimsi.Rows.Add("A018", "변경내역이 저장되었습니다.");
            dtimsi.Rows.Add("A018", "변경내역이 초기화 됩니다. 취소하시겠습니까?");

            return dtimsi;
        }

        public static string String(string strCode)
        {
            return Convert.ToString(dt.Select($"Code = '{strCode}'")[0]["Ko"]);
        }
    }
}
