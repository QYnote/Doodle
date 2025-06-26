using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DotNet.Comm
{
    //프로그램 실행동안 가질 데이터
    internal static class RuntimeData
    {
        internal readonly static string DataPath = string.Format("{0}Data\\", AppDomain.CurrentDomain.BaseDirectory);   //일단 만들어둔 Default Path
        internal static string LangType = "Ko";
        private static Dictionary<string, string> dicTextList = new Dictionary<string, string>();

        static RuntimeData()
        {
            CreateDtImsi();
        }

        /// <summary>
        /// 코드에따른 Database 언어 가져오기
        /// </summary>
        /// <param name="strCode">호출할 코드</param>
        /// <returns>언어별 코드 Value</returns>
        internal static string String(string strCode)
        {
            if (dicTextList.ContainsKey(strCode))
            {
                return dicTextList[strCode];
            }

            return strCode;
        }

        private static void CreateDtImsi()
        {
            //글로벌 DB 적용전 임시 Table
            DataTable dtimsi = new DataTable();
            dtimsi.TableName = "Base";

            dtimsi.Columns.Add("Code");
            dtimsi.Columns.Add("Ko");

            //Form 순서(2), Type 순서(2), Control 순서(2), ...
            //메뉴
            dtimsi.Rows.Add("F00", "메인화면");
            dtimsi.Rows.Add("F0000", "Message Box");
            dtimsi.Rows.Add("F000000", "파일이 열려있습니다.");
            dtimsi.Rows.Add("F000001", "저장이 완료되었습니다.");
            dtimsi.Rows.Add("F0001", "텍스트 메뉴");
            dtimsi.Rows.Add("F000100", "기초");
            dtimsi.Rows.Add("F00010000", "유닛 설정");
            dtimsi.Rows.Add("F000101", "파일");
            dtimsi.Rows.Add("F00010100", "XML 저장");
            dtimsi.Rows.Add("F00010101", "XML 저장");
            dtimsi.Rows.Add("F000102", "통신");
            dtimsi.Rows.Add("F00010200", "통신 생성");
            dtimsi.Rows.Add("F00010201", "통신 열기");
            dtimsi.Rows.Add("F00010202", "통신 닫기");
            dtimsi.Rows.Add("F0002", "아이콘 메뉴");
            dtimsi.Rows.Add("F000200", "XML 저장");
            dtimsi.Rows.Add("F000201", "XML 저장");
            dtimsi.Rows.Add("F000202", "통신 생성");
            dtimsi.Rows.Add("F000203", "통신 열기");
            dtimsi.Rows.Add("F000204", "통신 닫기");
            dtimsi.Rows.Add("F0003", "트리 메뉴");
            dtimsi.Rows.Add("F000300", "통신 생성");
            dtimsi.Rows.Add("F000301", "통신 삭제");
            dtimsi.Rows.Add("F000302", "통신 수정");
            dtimsi.Rows.Add("F000303", "유닛 생성");
            dtimsi.Rows.Add("F000304", "유닛 수정");
            dtimsi.Rows.Add("F000305", "통신 열기");
            dtimsi.Rows.Add("F000306", "통신 닫기");
            dtimsi.Rows.Add("F0004", "속성 Grid");
            dtimsi.Rows.Add("F000400", "이름");
            dtimsi.Rows.Add("F000401", "값");

            dtimsi.Rows.Add("F01", "통신 관리");
            dtimsi.Rows.Add("F0100", "Message Box");
            dtimsi.Rows.Add("F010000", "이미 만들어진 Port의 Port명은 변경할 수 없습니다.");
            dtimsi.Rows.Add("F010001", "통신 생성에 실패하였습니다.");
            dtimsi.Rows.Add("F0101", "버튼리스트");
            dtimsi.Rows.Add("F010100", "저장");
            dtimsi.Rows.Add("F010101", "취소");
            dtimsi.Rows.Add("F0102", "Serial Port");
            dtimsi.Rows.Add("F010200", "Port 이름");
            dtimsi.Rows.Add("F010201", "통신 규격");
            dtimsi.Rows.Add("F010202", "BaudRate");
            dtimsi.Rows.Add("F010203", "DataBits");
            dtimsi.Rows.Add("F010204", "StopBit");
            dtimsi.Rows.Add("F010205", "Parity");
            dtimsi.Rows.Add("F0103", "Ethernet Port");
            dtimsi.Rows.Add("F010300", "Port 번호");
            dtimsi.Rows.Add("F010301", "IP 주소");

            dtimsi.Rows.Add("F02", "Unit 관리");
            dtimsi.Rows.Add("F0200", "Message Box");
            dtimsi.Rows.Add("F020000", "Unit 생성에 실패하였습니다.");
            dtimsi.Rows.Add("F020001", "사용중인 번호입니다.");
            dtimsi.Rows.Add("F020002", "통신Protocol을 지원하지 않는 Model입니다.");
            dtimsi.Rows.Add("F0201", "버튼리스트");
            dtimsi.Rows.Add("F020100", "저장");
            dtimsi.Rows.Add("F020101", "취소");
            dtimsi.Rows.Add("F0202", "속성");
            dtimsi.Rows.Add("F020200", "번호");
            dtimsi.Rows.Add("F020201", "구분");
            dtimsi.Rows.Add("F020202", "모델");
            dtimsi.Rows.Add("F020203", "이름(사용자지정)");
            dtimsi.Rows.Add("F020204", "새로 만들기");
            dtimsi.Rows.Add("F0203", "Unit Grid");
            dtimsi.Rows.Add("F020300", "번호");
            dtimsi.Rows.Add("F020301", "이름");


            dtimsi.Rows.Add("F03", "Unit 설정");
            dtimsi.Rows.Add("F0300", "Message Box");
            dtimsi.Rows.Add("F030000", "Unit 구분명이 입력되지 않았습니다.");
            dtimsi.Rows.Add("F030001", "이미 존재하는 구분명입니다.");
            dtimsi.Rows.Add("F030002", "선택된 구분이 없습니다.");
            dtimsi.Rows.Add("F030003", "중복되는 Address가 있습니다.");
            dtimsi.Rows.Add("F030004", "중복되는 Item이 있습니다.");
            dtimsi.Rows.Add("F0301", "Unit 속성");
            dtimsi.Rows.Add("F030100", "Unit 구분");
            dtimsi.Rows.Add("F030101", "Unit 모델");
            dtimsi.Rows.Add("F030102", "지원 통신Protocol");
            dtimsi.Rows.Add("F030103", "Registry 통신");
            dtimsi.Rows.Add("F030104", "Registry GridView");
            dtimsi.Rows.Add("F03010400", "Address\n[Decimal]");
            dtimsi.Rows.Add("F03010401", "Address\n[Hex]");
            dtimsi.Rows.Add("F03010402", "이름");
            dtimsi.Rows.Add("F03010403", "값 속성");
            dtimsi.Rows.Add("F03010404", "기본값");
            dtimsi.Rows.Add("F03010405", "Read Only");
            dtimsi.Rows.Add("F030105", "Registry SubItem");
            dtimsi.Rows.Add("F03010500", "소수점 위치");
            dtimsi.Rows.Add("F03010501", "최대값");
            dtimsi.Rows.Add("F03010502", "최소값");
            dtimsi.Rows.Add("F03010503", "최대 글자수");
            dtimsi.Rows.Add("F0302", "Excel");
            dtimsi.Rows.Add("F030201", "설명란");
            dtimsi.Rows.Add("F03020100", "[필수]Parameter 이름");
            dtimsi.Rows.Add("F03020101", "입력 방법 ※1");
            dtimsi.Rows.Add("F03020102", "기본값");
            dtimsi.Rows.Add("F03020103", "TRUE : 읽기 전용, FALSE : 읽기/쓰기");
            
            dtimsi.Rows.Add("F03020104", "문자값 입력 ※2"); //입력방법
            dtimsi.Rows.Add("F03020105", "숫자값 입력 ※3");
            dtimsi.Rows.Add("F03020106", "선택 입력 ※4");

            dtimsi.Rows.Add("F03020107", "입력값의 최대치를 지정합니다.");
            dtimsi.Rows.Add("F03020108", "입력값의 최소치를 지정합니다.");
            dtimsi.Rows.Add("F03020109", "최대 입력 글자 수를 지정합니다.");
            dtimsi.Rows.Add("F0302010A", "선택 항목 목록입니다. 항목사이에 띄어쓰기 없이 쉼표(,)로 구분합니다.");

            dicTextList = dtimsi.AsEnumerable().ToDictionary(
                row => row.Field<string>(0),
                row => row.Field<string>(1)
                );
        }

    }
}
