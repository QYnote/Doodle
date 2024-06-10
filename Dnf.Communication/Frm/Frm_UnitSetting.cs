using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using Dnf.Utils.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;

namespace Dnf.Communication.Frm
{
    internal partial class Frm_UnitSetting : TabPageBase
    {
        #region UnitSetting 구조

        protected class UnitType
        {
            internal UnitType(string name) { Name = name; }

            /// <summary>Unit의 Type명</summary>
            internal string Name { get; set; }
            /// <summary>유형에 해당하는 Model</summary>
            internal Dictionary<string, UnitModel> Models = new Dictionary<string, UnitModel>();
        }

        protected class UnitModel
        {
            /// <summary>모델명</summary>
            internal string ModelName { get; set; }
            /// <summary>지원하는 Protocol 여부</summary>
            internal Dictionary<uProtocolType, bool> SupportProtocol { get; set; }
            /// <summary>Registry Map</summary>
            /// 1개의 Model에 Protocol에 따라 Registry Map이 달라질 수 있음
            /// 버전까지는 힘드니 여기까지 타협
            internal Dictionary<uProtocolType, Dictionary<int, UnitRegistry>> RegistryMap = new Dictionary<uProtocolType, Dictionary<int, UnitRegistry>>();

            internal UnitModel()
            {
                SupportProtocol = new Dictionary<uProtocolType, bool>();

                //지원 Protocol 기본틀
                foreach (uProtocolType protocol in UtilCustom.EnumToItems<uProtocolType>())
                {
                    SupportProtocol.Add(protocol, false);
                }

            }
        }

        protected class UnitRegistry
        {
            /// <summary>Registry 주소값[Dec]</summary>
            internal int AddressDec { get; set; }
            /// <summary>Registry 이름</summary>
            internal string RegName { get; set; }

            private string valueType;
            /// <summary>Value 속성</summary>
            internal string ValueType
            {
                get
                {
                    return valueType;
                }
                set
                {
                    valueType = value;
                    //ValueType 지정 후 SubItem 생성
                    if (value == "Numeric"
                        || value == "Combo"
                        || value == "Text")
                    {
                        if (RegSubItem == null)
                        {
                            RegSubItem = new RegistrySubItem();
                        }
                    }
                }
            }
            

            /// <summary>기본값</summary>
            internal string DefaultValue { get; set; }
            /// <summary>읽기전용 bool</summary>
            internal bool ReadOnly { get; set; }
            /// <summary>Registry SubItem</summary>
            internal RegistrySubItem RegSubItem { get; set; }
        }

        protected class RegistrySubItem
        {
            //Combo
            /// <summary>ComboBox Item</summary>
            internal List<string> ComboItems = new List<string>();
            //Numeric
            /// <summary>Numeric 소수점 위치</summary>
            internal int DotPosition { get; set; }
            /// <summary>Numeric 최대값</summary>
            internal int MaxValue { get; set; }
            /// <summary>Numeric 최소값</summary>
            internal int MinValue { get; set; }
            //Text
            /// <summary>Text 최대 길이</summary>
            internal int MaxLength { get; set; }
        }

        #endregion Unit Setting 구조
        #region Controls
        private int marginValue = 3;

        //Menu
        private ToolStrip IconMenu = new ToolStrip();
        private ToolStripButton IconMenu_ExcelUpload = new ToolStripButton();
        private ToolStripButton IconMenu_ExcelDownload = new ToolStripButton();

        //Unit Type
        private Label LblUnitType = new Label();
        private TextBox TxtUnitType = new TextBox();
        private Button btnUnitTypeAdd = new Button();
        private Button btnUnitTypeDel = new Button();
        private ListBox LbxUnitType = new ListBox();
        //Unit 모델
        private Label LblUnitModel = new Label();
        private TextBox TxtUnitModel = new TextBox();
        private Button btnUnitModelAdd = new Button();
        private Button btnUnitModelDel = new Button();
        private ListBox LbxUnitModel = new ListBox();

        //지원 Protocol
        private Label LblSupportProtocol = new Label();
        private CheckedListBox CLbxSupportProtocol = new CheckedListBox();

        //Registry
        private ucControlBox CboProtocol = new ucControlBox(CtrlType.ComboBox);             //Registry 통신타입
        private Button btnRegSave = new Button();                                           //Registry 저장버튼
        private Button btnRegExcelUpload = new Button();                                    //Registry 엑셀 저장
        private Button btnRegExcelDownload = new Button();                                  //Registry 엑셀 불러오기
        private DataGridView gvRegistry = new DataGridView();                               //Registry 정보 GridView
        private DataGridViewTextBoxColumn colAddrDec = new DataGridViewTextBoxColumn();     //주소(10진법)
        private DataGridViewTextBoxColumn colAddrHex = new DataGridViewTextBoxColumn();     //주소(16진법)
        private DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();        //이름
        private DataGridViewComboBoxColumn colValueType = new DataGridViewComboBoxColumn(); //값 속성
        private DataGridViewTextBoxColumn colDefaultValue = new DataGridViewTextBoxColumn();//기본 값(Default Value)
        private DataGridViewCheckBoxColumn colRW = new DataGridViewCheckBoxColumn();        //Read/Write 모드
        private DataGridViewImageColumn colErase = new DataGridViewImageColumn();           //삭제

        //Registry SubItem
        private Label lblSubItem = new Label();
        //Numeric
        private ucControlBox NumDotPosition = new ucControlBox(CtrlType.Numberic);
        private ucControlBox NumMaxValue = new ucControlBox(CtrlType.Numberic);
        private ucControlBox NumMinValue = new ucControlBox(CtrlType.Numberic);
        //Text
        private ucControlBox NumMaxLength = new ucControlBox(CtrlType.Numberic);
        //Combo
        private TextBox TxtCboItems = new TextBox();
        private Button btnCboItemsAdd = new Button();
        private Button btnCboItemsDel = new Button();
        private ListBox LbxCboItems = new ListBox();
        #endregion Controls End

        private Dictionary<string, UnitType> dicUnitTypes;
        private UnitType SelectedType = null;
        private UnitModel SelectedModel = null;
        private uProtocolType SelectedProtocol;   //기본값 RTU
        private bool CellValueChangedFlag = true;   //CellValueChagned시 다른값 변경시킬때 방지용

        private string InfoFilePath = RuntimeData.DataPath + "UnitInfo.xml";

        internal Frm_UnitSetting() : base(RuntimeData.String("F03"), RuntimeData.String("F03"))
        {
            InitializeComponent();
            InitializeForm();

            this.Text = this.Name;
            this.SizeChanged += FrmSizeChanged;
            this.BeforeRemovePageHandler += UnitInfoSave;
        }

        #region Initialize

        /// <summary>
        /// Form 기초 셋팅
        /// </summary>
        private void InitializeForm()
        {
            InitializeIconMenu();
            InitializeUnitInfo();
            InitializeRegistry();
            InitializeRegistrySubItem();

            SetPositionSize();
            SetText();

            Load_UnitInfo();
        }

        private void InitializeIconMenu()
        {
            IconMenu.ImageScalingSize = new Size(32, 32);

            IconMenu_ExcelDownload.Name = "IconMenu_ExcelDownload";
            IconMenu_ExcelUpload.Name = "IconMenu_ExcelUpload";

            IconMenu_ExcelDownload.DisplayStyle = ToolStripItemDisplayStyle.Image;
            IconMenu_ExcelUpload.DisplayStyle = ToolStripItemDisplayStyle.Image;

            IconMenu_ExcelDownload.Image = Dnf.Utils.Properties.Resources.Test_32x32;
            IconMenu_ExcelUpload.Image = Dnf.Utils.Properties.Resources.Test_32x32;

            IconMenu.Items.AddRange(new ToolStripItem[] {IconMenu_ExcelDownload, IconMenu_ExcelUpload});
            this.Controls.Add(IconMenu);

            IconMenu_ExcelDownload.Click += (sender, e) => {  };
            IconMenu_ExcelUpload.Click += (sender, e) => { SaveExcel(); };
        }

        /// <summary>
        /// Unit Type Control 생성
        /// </summary>
        private void InitializeUnitInfo()
        {
            LblUnitType.AutoSize = false;
            LblUnitType.TextAlign = ContentAlignment.MiddleCenter;
            LblUnitType.BorderStyle = BorderStyle.FixedSingle;

            TxtUnitType.AutoSize = false;
            TxtUnitType.MaxLength = 20;

            btnUnitTypeAdd.Text = "+";
            btnUnitTypeDel.Text = "-";

            LbxUnitType.AutoSize = false;
            LbxUnitType.Sorted = true;
            LbxUnitType.IntegralHeight = false; //Size 조절 시 Item Hight 영향 false
            LbxUnitType.MinimumSize = new Size(160 + (marginValue * 2), 50);

            //Unit 모델
            LblUnitModel.AutoSize = false;
            LblUnitModel.TextAlign = ContentAlignment.MiddleCenter;
            LblUnitModel.BorderStyle = BorderStyle.FixedSingle;

            TxtUnitModel.AutoSize = false;
            TxtUnitModel.MaxLength = 20;

            btnUnitModelAdd.Text = "+";
            btnUnitModelDel.Text = "-";

            LbxUnitModel.AutoSize = false;
            LbxUnitModel.Sorted = true;
            LbxUnitModel.IntegralHeight = false;    //Size 조절 시 Item Hight 영향 false
            LbxUnitModel.MinimumSize = new Size(160 + (marginValue * 2), 100);

            //지원 Protocl
            LblSupportProtocol.AutoSize = false;
            LblSupportProtocol.TextAlign = ContentAlignment.MiddleCenter;
            LblSupportProtocol.BorderStyle = BorderStyle.FixedSingle;

            CLbxSupportProtocol.AutoSize = false;
            CLbxSupportProtocol.CheckOnClick = true;    //항목 선택 시 Check 바로 변경
            CLbxSupportProtocol.IntegralHeight = false; //Size 조절 시 Item Hight 영향 false
            CLbxSupportProtocol.MinimumSize = new Size(160 + (marginValue * 2), 50);
            CLbxSupportProtocol.Items.AddRange(UtilCustom.EnumToItems<uProtocolType>());

            this.Controls.Add(TxtUnitType);
            this.Controls.Add(LblUnitType);
            this.Controls.Add(btnUnitTypeAdd);
            this.Controls.Add(btnUnitTypeDel);
            this.Controls.Add(LbxUnitType);

            this.Controls.Add(TxtUnitModel);
            this.Controls.Add(LblUnitModel);
            this.Controls.Add(btnUnitModelAdd);
            this.Controls.Add(btnUnitModelDel);
            this.Controls.Add(LbxUnitModel);

            this.Controls.Add(LblSupportProtocol);
            this.Controls.Add(CLbxSupportProtocol);

            btnUnitTypeAdd.Click += (sender, e) => { UnitTypeAdd(); };
            btnUnitTypeDel.Click += (sender, e) => { UnitTypeRemove(); };
            btnUnitModelAdd.Click += (sender, e) => { UnitModelAdd(); };
            btnUnitModelDel.Click += (sender, e) => { UnitModelRemove(); };
            LbxUnitType.SelectedIndexChanged += UnitTypeSelectedChanged;
            LbxUnitModel.SelectedIndexChanged += UnitModelSelectedChanged;
            CLbxSupportProtocol.SelectedValueChanged += (sender, e) => { ProtocolFlagChanged(); };
            (CboProtocol.ctrl as ComboBox).SelectedIndexChanged += CboProtocol_SelectedIndexChanged_SetFormSelectedProtocol;
            (CboProtocol.ctrl as ComboBox).SelectedIndexChanged += (sender, e) => { SelectedProtocol_SetGridRow(); };
        }

        /// <summary>
        /// Unit - Protocol - Registry Control 생성
        /// </summary>
        private void InitializeRegistry()
        {
            CboProtocol.AutoSize = false;
            CboProtocol.LblWidth = 80;
            CboProtocol.Height = 30;
            CboProtocol.Width = 200;

            btnRegSave.AutoSize = false;
            btnRegExcelUpload.AutoSize = false;
            btnRegExcelDownload.AutoSize = false;

            gvRegistry.AllowUserToResizeRows = false;   //Row 사이즈 조절
            gvRegistry.AutoGenerateColumns = false;     //DataSoure 자동추가
            gvRegistry.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;  //Column Header 높이
            gvRegistry.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; //Column Header Text 정렬
            gvRegistry.MultiSelect = false; //다중선택
            gvRegistry.EditMode = DataGridViewEditMode.EditOnEnter;

            colAddrDec.Width = 70;
            colAddrHex.Width = 60;
            colName.Width = 100;
            colValueType.Width = 80;
            colDefaultValue.Width = 60;
            colRW.Width = 40;
            colErase.Width = 20;

            colAddrDec.DisplayIndex = 0;
            colAddrHex.DisplayIndex = 1;
            colName.DisplayIndex = 2;
            colValueType.DisplayIndex = 4;
            colDefaultValue.DisplayIndex = 5;
            colRW.DisplayIndex = 6;
            colErase.DisplayIndex = 7;

            colAddrDec.Name = "colAddrDec";
            colAddrHex.Name = "colAddrHex";
            colName.Name = "colName";
            colValueType.Name = "colValueType";
            colDefaultValue.Name = "colDefaultValue";
            colRW.Name = "colRW";
            colErase.Name = "colErase";

            colValueType.Items.AddRange("Numeric", "Combo", "Text", "Bool");

            colErase.Image = Dnf.Utils.Properties.Resources.Erase_16x16;

            colAddrDec.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colAddrHex.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colValueType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDefaultValue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colRW.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            colAddrDec.SortMode = DataGridViewColumnSortMode.Programmatic;
            colAddrHex.SortMode = DataGridViewColumnSortMode.Programmatic;
            colName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colValueType.SortMode = DataGridViewColumnSortMode.NotSortable;
            colDefaultValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colRW.SortMode = DataGridViewColumnSortMode.NotSortable;

            //숫자만 입력하게 하기
            UtilCustom.ColumnOnlyNumeric(gvRegistry, colAddrDec.Name);
            UtilCustom.ColumnOnlyNumeric(gvRegistry, colAddrHex.Name, "Hex");

            gvRegistry.Columns.AddRange(colAddrDec, colAddrHex, colName, colValueType, colDefaultValue, colRW, colErase);

            gvRegistry.ColumnHeaderMouseClick += Gv_SortAddr;                                   //Address 정렬
            gvRegistry.SortCompare += Gv_SortCompare;                                           //Address 정렬 비교
            gvRegistry.CellValidating += Gv_CellValidating_AddrDecimalDuplicateConfirm;         //Address 중복검사
            gvRegistry.CellValueChanged += Gv_CellValueChanged_DecimalToHex;                    //Address Dec -> Hex 자동입력
            gvRegistry.CellValueChanged += Gv_CellValueChanged_HexToDecimal;                    //Address Hex -> Dec 자동입력
            gvRegistry.CellValueChanged += Gv_CellValueChanged_VisibleSubItem;                  //ValueType SubItem Visible 처리
            gvRegistry.CellValueChanged += GvRegistry_CellValueChanged_SaveRegistry; ;          //Cell값 입력 시 Registry 저장
            gvRegistry.EditingControlShowing += Gv_EditingControlShowing_SetCboSelectedEvent;   //ValueType ComboBox Item 선택하면 바로 GridCell에 적용
            gvRegistry.CellMouseClick += Gv_CellClick_RWChange;                                 //RW 체크박스 클릭 안해도 체크 변경
            gvRegistry.CellContentClick += EraseColumnClick;                                    //Row삭제이미지 클릭 이벤트
            gvRegistry.CellFormatting += GvRegistry_CellFormatting_NewRowImageSet;              //NewRow 이미지 X로나오는거 수정
            gvRegistry.SelectionChanged += GvRegistry_SelectionChanged_VisibleSubItem;          //선택 Row 변경 시 SubItem Visible 변경

            this.Controls.Add(CboProtocol);
            this.Controls.Add(gvRegistry);
        }

        private void InitializeRegistrySubItem()
        {
            lblSubItem.AutoSize = false;
            lblSubItem.TextAlign = ContentAlignment.MiddleCenter;
            lblSubItem.BorderStyle = BorderStyle.FixedSingle;

            NumDotPosition.Name = "NumDotPosition";
            NumMaxValue.Name = "NumMaxValue";
            NumMinValue.Name = "NumMinValue";
            NumMaxLength.Name = "NumMaxLength";
            btnCboItemsAdd.Name = "btnCboItemsAdd";

            NumDotPosition.LblWidth = 80;
            (NumDotPosition.ctrl as ucNumeric).Value = 0;   //Default Value
            NumMaxValue.LblWidth = 80;
            (NumMaxValue.ctrl as ucNumeric).Value = 0;  //Default Value
            NumMinValue.LblWidth = 80;
            (NumMinValue.ctrl as ucNumeric).MinValue = int.MinValue;
            (NumMinValue.ctrl as ucNumeric).Value = -1; //Default Value
            NumDotPosition.Visible = false;
            NumMaxValue.Visible = false;
            NumMinValue.Visible = false;

            NumMaxLength.LblWidth = 80;
            (NumMaxLength.ctrl as ucNumeric).MinValue = 1;
            NumMaxLength.Visible = false;

            TxtCboItems.AutoSize = false;

            btnCboItemsAdd.Text = "+";
            btnCboItemsAdd.Size = new Size(30, 30);
            btnCboItemsDel.Text = "-";
            btnCboItemsDel.Size = new Size(30, 30);

            LbxCboItems.AutoSize = false;
            LbxCboItems.Sorted = true;
            LbxCboItems.IntegralHeight = false;    //Size 조절 시 Item Hight 영향 false

            this.Controls.Add(lblSubItem);
            this.Controls.Add(NumDotPosition);
            this.Controls.Add(NumMaxValue);
            this.Controls.Add(NumMinValue);
            this.Controls.Add(NumMaxLength);
            this.Controls.Add(TxtCboItems);
            this.Controls.Add(btnCboItemsAdd);
            this.Controls.Add(btnCboItemsDel);
            this.Controls.Add(LbxCboItems);

            (NumDotPosition.ctrl as ucNumeric).ValueChanged += SubItem_ValueChanged_SetRegistryClass;
            (NumMaxValue.ctrl as ucNumeric).ValueChanged += SubItem_ValueChanged_SetRegistryClass;
            (NumMinValue.ctrl as ucNumeric).ValueChanged += SubItem_ValueChanged_SetRegistryClass;
            (NumMaxLength.ctrl as ucNumeric).ValueChanged += SubItem_ValueChanged_SetRegistryClass;
            btnCboItemsAdd.Click += SubItem_ValueChanged_SetRegistryClass;
        }

        private void SetText()
        {
            //Unit
            LblUnitType.Text = RuntimeData.String("F030100");
            LblUnitModel.Text = RuntimeData.String("F030101");
            LblSupportProtocol.Text = RuntimeData.String("F030102");

            //Registry
            CboProtocol.LblText = RuntimeData.String("F030103");
            colAddrDec.HeaderText = RuntimeData.String("F03010400");
            colAddrHex.HeaderText = RuntimeData.String("F03010401");
            colName.HeaderText = RuntimeData.String("F03010402");
            colValueType.HeaderText = RuntimeData.String("F03010403");
            colDefaultValue.HeaderText = RuntimeData.String("F03010404");
            colRW.HeaderText = RuntimeData.String("F03010405");
            colErase.HeaderText = "";

            //Registry SubItem
            lblSubItem.Text = RuntimeData.String("F030105");
            NumDotPosition.LblText = RuntimeData.String("F03010500");
            NumMaxValue.LblText = RuntimeData.String("F03010501");
            NumMinValue.LblText = RuntimeData.String("F03010502");
            NumMaxLength.LblText = RuntimeData.String("F03010503");
        }

        private void SetPositionSize()
        {
            /* Form 사용하면 SizeChanged가 발동 안하고
             * TabPage 쓰면 Form Closed가 없고
             * 미쳐버리겠네 진짜
             */
            int margin = marginValue;

            LblUnitType.Location = new Point(margin, IconMenu.Height + margin);
            LblUnitType.Size = new Size(160 + (margin * 2), 30);

            TxtUnitType.Location = new Point(margin,
                LblUnitType.Location.Y + LblUnitType.Height + margin);
            TxtUnitType.Size = new Size(100, 27);

            btnUnitTypeAdd.Location = new Point(TxtUnitType.Location.X + TxtUnitType.Width + margin,
                TxtUnitType.Location.Y - 2);
            btnUnitTypeAdd.Size = new Size(30, 30);

            btnUnitTypeDel.Location = new Point(btnUnitTypeAdd.Location.X + btnUnitTypeAdd.Width + margin,
                btnUnitTypeAdd.Location.Y);
            btnUnitTypeDel.Size = btnUnitTypeAdd.Size;

            LbxUnitType.Location = new Point(margin,
                TxtUnitType.Location.Y +  TxtUnitType.Height + margin);
            LbxUnitType.Size = new Size(LblUnitType.Width,
                ((this.Height / 4) * 1) - (margin + LblUnitType.Height + margin + TxtUnitType.Height + margin));
            // - (margin + LblUnitType.Height + margin + TxtUnitType.Height + margin)

            //Unit 모델
            LblUnitModel.Location = new Point(margin,
                LbxUnitType.Location.Y + LbxUnitType.Size.Height + margin);
            LblUnitModel.Size = LblUnitType.Size;

            TxtUnitModel.Location = new Point(margin,
                LblUnitModel.Location.Y + LblUnitModel.Height + margin);
            TxtUnitModel.Size = TxtUnitType.Size;

            btnUnitModelAdd.Location = new Point(btnUnitTypeAdd.Location.X,
                TxtUnitModel.Location.Y - 2);
            btnUnitModelAdd.Size = btnUnitTypeAdd.Size;

            btnUnitModelDel.Location = new Point(btnUnitTypeDel.Location.X,
                btnUnitModelAdd.Location.Y);
            btnUnitModelDel.Size = btnUnitTypeAdd.Size;

            LbxUnitModel.Location = new Point(margin,
                TxtUnitModel.Location.Y + TxtUnitModel.Height + margin);
            LbxUnitModel.Size = new Size(LbxUnitType.Width,
                ((this.Height / 4) * 2) - (margin + LblUnitType.Height + margin + TxtUnitType.Height));

            //지원 Protocol
            LblSupportProtocol.Location = new Point(LbxUnitModel.Location.X,
                LbxUnitModel.Location.Y + LbxUnitModel.Height + margin);
            LblSupportProtocol.Size = LblUnitType.Size;

            CLbxSupportProtocol.Location = new Point(LblSupportProtocol.Location.X,
                LblSupportProtocol.Location.Y + LblSupportProtocol.Height + margin);
            CLbxSupportProtocol.Size = new Size(LblSupportProtocol.Width,
                this.Height - (CLbxSupportProtocol.Location.Y + margin));

            //Repository
            CboProtocol.Location = new Point(LblUnitType.Location.X + LblUnitType.Width + margin,
                LblUnitType.Location.Y);

            gvRegistry.Location = new Point(CboProtocol.Location.X,
                CboProtocol.Location.Y + CboProtocol.Height + margin);
            gvRegistry.Size = new Size(this.Width - (gvRegistry.Location.X + margin + LblUnitModel.Width + margin),
                this.Height - (gvRegistry.Location.Y + margin));

            //Repository SubItem
            lblSubItem.Location = new Point(gvRegistry.Location.X + gvRegistry.Width + margin,
                gvRegistry.Location.Y);
            lblSubItem.Size = new Size(LblUnitModel.Width, LblUnitModel.Height);

            //Numeric
            NumDotPosition.Location = new Point(lblSubItem.Location.X,
                lblSubItem.Location.Y + lblSubItem.Height + margin);
            NumDotPosition.Size = new Size(lblSubItem.Width, lblSubItem.Height);

            NumMaxValue.Location = new Point(NumDotPosition.Location.X,
                NumDotPosition.Location.Y + NumDotPosition.Height + margin);
            NumMaxValue.Size = NumDotPosition.Size;

            NumMinValue.Location = new Point(NumMaxValue.Location.X,
                NumMaxValue.Location.Y + NumMaxValue.Height + margin);
            NumMinValue.Size = NumMaxValue.Size;

            //Text
            NumMaxLength.Location = NumDotPosition.Location;
            NumMaxLength.Size = NumDotPosition.Size;

            //Combo
            TxtCboItems.Location = NumDotPosition.Location;
            TxtCboItems.Size = new Size(lblSubItem.Width - (margin + btnCboItemsAdd.Width + margin + btnCboItemsDel.Width), TxtUnitType.Height);

            btnCboItemsAdd.Location = new Point(TxtCboItems.Location.X + TxtCboItems.Width + margin,
                TxtCboItems.Location.Y - 2);
            btnCboItemsDel.Location = new Point(btnCboItemsAdd.Location.X + btnCboItemsAdd.Width + margin,
                TxtCboItems.Location.Y - 2);

            LbxCboItems.Location = new Point(TxtCboItems.Location.X,
                TxtCboItems.Location.Y + TxtCboItems.Height + margin);
            LbxCboItems.Size = new Size(lblSubItem.Width, LbxCboItems.Height);
        }

        #endregion Initialize

        #region Event

        /// <summary>
        /// 동적 Size 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmSizeChanged(object sender, EventArgs e)
        {
            SetPositionSize();
        }

        #region UnitType Evnet

        /// <summary>
        /// Unit Type 선택Index 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitTypeSelectedChanged(object sender, EventArgs e)
        {
            if (LbxUnitType.SelectedItem == null) return;

            this.SelectedType = dicUnitTypes[LbxUnitType.SelectedItem as string];

            if(this.SelectedType == null) { return; }

            LbxUnitModel.Items.Clear();
            gvRegistry.Rows.Clear();
            VisibleSubItem("");
            //하위 모델이 있을경우 첫번째 Item 선택
            if (SelectedType.Models.Count > 0)
            {
                //해당Type의 Model리스트 조회
                foreach (string model in SelectedType.Models.Keys)
                {
                    LbxUnitModel.Items.Add(model);
                }

                LbxUnitModel.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Unit Type 생성
        /// </summary>
        private void UnitTypeAdd()
        {
            string unitTypeName = TxtUnitType.Text.Trim();

            //빈칸입력인지 검사
            if (unitTypeName == "")
            {
                MessageBox.Show(RuntimeData.String("F030000"));
                return;
            }
            //동일한 Type명 있는지 검사
            if (dicUnitTypes.Keys.Contains(unitTypeName))
            {
                MessageBox.Show(RuntimeData.String("F030001"));
                return;
            }

            LbxUnitType.Items.Add(unitTypeName);
            dicUnitTypes.Add(unitTypeName, new UnitType(unitTypeName));

            //후처리
            TxtUnitType.Text = "";
            LbxUnitType.SelectedItem = unitTypeName;
        }

        /// <summary>
        /// Unit Type 삭제
        /// </summary>
        private void UnitTypeRemove()
        {
            string selectedType = LbxUnitType.SelectedItem as string;

            if (selectedType != string.Empty || selectedType != "")
            {
                dicUnitTypes.Remove(selectedType);
                LbxUnitType.Items.Remove(selectedType);

                SelectedType = null;
                if (LbxUnitType.Items.Count > 0)
                {
                    LbxUnitType.SelectedIndex = 0;
                }
            }
        }

        #endregion UnitType Evnet End
        #region UnitModel Event

        /// <summary>
        /// Unit Model 선택Index 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitModelSelectedChanged(object sender, EventArgs e)
        {
            this.SelectedModel = SelectedType.Models[LbxUnitModel.SelectedItem as string];

            UnitModel model = this.SelectedModel;
            //지원하는 Protocol
            SetCboSupportProtocolList(model);
            VisibleSubItem("");
        }

        /// <summary>
        /// Unit Model 생성
        /// </summary>
        private void UnitModelAdd()
        {
            string unitModelName = TxtUnitModel.Text.Trim();

            //Type이 선택된 상태인지 검사
            if (SelectedType == null)
            {
                MessageBox.Show(RuntimeData.String("F030002"));
                return;
            }
            //빈칸입력인지 검사
            if (unitModelName == "")
            {
                MessageBox.Show(RuntimeData.String("F030000"));
                return;
            }
            //동일한 Model명 있는지 검사
            if (SelectedType.Models.Keys.Contains(unitModelName))
            {
                MessageBox.Show(RuntimeData.String("F030001"));
                return;
            }

            LbxUnitModel.Items.Add(unitModelName);
            SelectedType.Models.Add(unitModelName, new UnitModel() { ModelName = unitModelName});

            //후처리
            TxtUnitModel.Text = "";
            LbxUnitModel.SelectedItem = unitModelName;
        }

        /// <summary>
        /// Unit Model 삭제
        /// </summary>
        private void UnitModelRemove()
        {
            if ((SelectedType != null)
                && (SelectedModel != null))
            {
                SelectedType.Models.Remove(SelectedModel.ModelName);
                LbxUnitModel.Items.Remove(SelectedModel);

                SelectedModel = null;
                if (LbxUnitModel.Items.Count > 0)
                {
                    LbxUnitModel.SelectedIndex = 0;
                }
            }
        }

        #endregion UnitModel Event End
        #region Protocol Event

        /// <summary>
        /// 통신 Protocol 사용여부 변경 Event
        /// </summary>
        private void ProtocolFlagChanged()
        {
            if(SelectedType != null || SelectedModel != null)
            {
                uProtocolType protocol = (uProtocolType)CLbxSupportProtocol.SelectedItem;

                UnitModel model = this.SelectedModel;
                model.SupportProtocol[protocol] = CLbxSupportProtocol.GetItemChecked(CLbxSupportProtocol.SelectedIndex);

                if(model.SupportProtocol[protocol] == true)
                {
                    model.RegistryMap.Add(protocol, new Dictionary<int, UnitRegistry>());
                }
                else
                {
                    model.RegistryMap.Remove(protocol);
                }

                SetCboSupportProtocolList(model);
            }
        }

        /// <summary>
        /// 지원하는 Protocol Combobox List 수정
        /// </summary>
        /// <param name="model"></param>
        private void SetCboSupportProtocolList(UnitModel model)
        {
            (CboProtocol.ctrl as ComboBox).Items.Clear();
            foreach (uProtocolType protocol in model.SupportProtocol.Keys)
            {
                int idx = CLbxSupportProtocol.Items.IndexOf(protocol);
                bool flag = model.SupportProtocol[protocol];

                //지원 Protocol CheckBox Item 수정
                CLbxSupportProtocol.SetItemChecked(idx, flag);

                //Registry Protocol ComboBox Item 수정
                if (flag) { (CboProtocol.ctrl as ComboBox).Items.Add(protocol); }
            }

            if ((CboProtocol.ctrl as ComboBox).Items.Count > 0)
            {
                (CboProtocol.ctrl as ComboBox).SelectedIndex = 0;
                gvRegistry.Enabled = true;
            }
            else
            {
                gvRegistry.Rows.Clear();
                gvRegistry.Enabled = false;
            }
        }

        /// <summary>
        /// 선택된 Protocol에 따라 DataGridView Row 생성
        /// </summary>
        private void SelectedProtocol_SetGridRow()
        {
            if (SelectedModel == null) return;

            gvRegistry.Rows.Clear();
            VisibleSubItem("");

            DataGridViewRow row = null;
            foreach (UnitRegistry registry in SelectedModel.RegistryMap[SelectedProtocol].Values)
            {
                row = (DataGridViewRow)gvRegistry.Rows[gvRegistry.NewRowIndex].Clone();
                row.Cells[colAddrDec.Index].Value = registry.AddressDec;
                row.Cells[colAddrHex.Index].Value = registry.AddressDec.ToHexString();
                row.Cells[colName.Index].Value = registry.RegName;
                row.Cells[colValueType.Index].Value = registry.ValueType;
                row.Cells[colDefaultValue.Index].Value = registry.DefaultValue;
                row.Cells[colRW.Index].Value = registry.ReadOnly;

                gvRegistry.Rows.Add(row);
            }
        }

        #endregion Protocol Event

        /// <summary>
        /// Registry Protocol 선택된 Protocol Form의 선택된 Protocol로 지정하기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CboProtocol_SelectedIndexChanged_SetFormSelectedProtocol(object sender, EventArgs e)
        {
            object value = CboProtocol.Value;

            this.SelectedProtocol = (uProtocolType)value;
        }

        #region GridEvent

        /// <summary>
        /// Cell값 변경 시 Registry 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void GvRegistry_CellValueChanged_SaveRegistry(object sender, DataGridViewCellEventArgs e)
        {
            if ((CboProtocol.ctrl as ComboBox).Items.Count == 0) return;
            if (gvRegistry.Rows.Count == 1) return; //NewRow만 있으면 진행 return

            DataGridViewRow dr = gvRegistry.Rows[e.RowIndex];

            //Address Dec 입력 안했으면 진행 안함
            if (dr.Cells[colAddrDec.Index].Value?.ToString() == "") return;

            UnitRegistry reg = new UnitRegistry();
            reg.AddressDec = Convert.ToInt32(dr.Cells[colAddrDec.Index].Value);
            reg.ValueType = Convert.ToString(dr.Cells[colValueType.Index].Value);
            reg.DefaultValue = Convert.ToString(dr.Cells[colDefaultValue.Index].Value);
            reg.ReadOnly = Convert.ToBoolean(dr.Cells[colRW.Index].Value);

            SelectedModel.RegistryMap[SelectedProtocol][reg.AddressDec] = reg;
        }

        /// <summary>
        /// Grid 선택된 Row에 해당하는 ValueType에따라 SubItem Visible 및 값 조정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GvRegistry_SelectionChanged_VisibleSubItem(object sender, EventArgs e)
        {
            if (gvRegistry.SelectedCells.Count < 1) return;
            if (gvRegistry.SelectedCells[0].RowIndex < 0) return;
            DataGridViewRow row = gvRegistry.Rows[gvRegistry.SelectedCells[0].RowIndex];
            if (row.IsNewRow) return;

            string valueType = Convert.ToString(row.Cells[colValueType.Index].Value);
            int addrDec = Convert.ToInt32(row.Cells[colAddrDec.Index].Value);

            if (valueType != "")
            {
                VisibleSubItem(valueType);
            }

            if (SelectedModel.RegistryMap[SelectedProtocol].ContainsKey(addrDec))
            {
                RegistrySubItem sub = SelectedModel.RegistryMap[SelectedProtocol][addrDec].RegSubItem;

                if(valueType == "Combo")
                {
                    LbxCboItems.Items.Clear();
                    LbxCboItems.Items.AddRange(sub.ComboItems.ToArray());
                }
                else if(valueType == "Numeric")
                {
                    CellValueChangedFlag = false;

                    NumMinValue.Value = sub.MinValue;
                    NumMaxValue.Value = sub.MaxValue;
                    NumDotPosition.Value = sub.DotPosition;

                    CellValueChangedFlag = true;
                }
                else if(valueType == "Text")
                {
                    CellValueChangedFlag = false;

                    NumMaxLength.Value = sub.MaxLength;

                    CellValueChangedFlag = true;
                }
            }
        }

        #region Address Event

        /// <summary>
        /// GridView Address기준 정렬
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gv_SortAddr(object sender, DataGridViewCellMouseEventArgs e)
        {
            /*작업이력
             * 2024-04-10
             * CellEndEdit에 자동정렬 넣으니 중복실행 생겨서 따로 정렬시키는 방식으로 재작업
             */

            if (e.ColumnIndex == colAddrDec.Index)
            {
                gvRegistry.Sort(colAddrDec, ListSortDirection.Ascending);
            }
        }

        /// <summary>
        /// Address 정렬 보조, int값 자리수 다르면 더 크게 체크시키기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gv_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column == colAddrDec)
            {
                //빈값이랑 비교하는 것 넘기기
                if (e.CellValue1 == null || e.CellValue2 == null) return;

                int a = int.Parse(e.CellValue1.ToString());
                int b = int.Parse(e.CellValue2.ToString());

                e.SortResult = a.CompareTo(b);

                e.Handled = true;
            }
        }

        /// <summary>
        /// Registry Address 중복검사
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gv_CellValidating_AddrDecimalDuplicateConfirm(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == colAddrDec.Index ||
                e.ColumnIndex == colAddrHex.Index)
            {
                //Registry Address 중복 검사
                object changedValue = e.FormattedValue;
                if (changedValue == null || changedValue.ToString() == "") return;
                bool bCancle = false;

                foreach (DataGridViewRow dr in gvRegistry.Rows)
                {
                    if (dr.Index == e.RowIndex) continue;
                    if (dr.Index == gvRegistry.NewRowIndex) continue;
                    if (dr.Cells[colAddrHex.Index].Value == null) continue;

                    if (e.ColumnIndex == colAddrDec.Index)
                    {
                        //Decimal
                        /*작업내역
                         * ==쓰면 true 안떠서 Equals 사용
                         */
                        if (dr.Cells[colAddrDec.Index].Value.Equals(changedValue))
                        {
                            bCancle = true;
                            break;
                        }
                    }
                    else if (e.ColumnIndex == colAddrHex.Index)
                    {
                        //Hex
                        if (dr.Cells[colAddrHex.Index].Value.Equals(changedValue))
                        {
                            bCancle = true;
                            break;
                        }
                    }
                }

                if (bCancle)
                {
                    e.Cancel = true;
                    MessageBox.Show(RuntimeData.String("F030003"));
                }
            }
        }

        private void Gv_CellValueChanged_DecimalToHex(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == colAddrDec.Index)
            {
                if (CellValueChangedFlag)
                {
                    CellValueChangedFlag = false;

                    object changedValue = gvRegistry.Rows[e.RowIndex].Cells[colAddrDec.Index].Value;
                    int value = Convert.ToInt32(changedValue);

                    gvRegistry.Rows[e.RowIndex].Cells[colAddrHex.Index].Value = value.ToString("X");
                    gvRegistry.RefreshEdit();  //변경값 적용시키기

                    CellValueChangedFlag = true;
                }
            }
        }

        private void Gv_CellValueChanged_HexToDecimal(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == colAddrHex.Index)
            {
                if (CellValueChangedFlag)
                {
                    CellValueChangedFlag = false;

                    object changedValue = gvRegistry.Rows[e.RowIndex].Cells[colAddrHex.Index].Value;
                    string value = Convert.ToString(changedValue);

                    gvRegistry.Rows[e.RowIndex].Cells[colAddrDec.Index].Value = Convert.ToInt32(value, 16);
                    gvRegistry.RefreshEdit();  //변경값 적용시키기

                    CellValueChangedFlag = true;
                }
            }
        }

        #endregion Address Event
        #region ValueType Event

        /// <summary>
        /// Cell에서 ComboBox 선택 시 바로 적용되도록 하는 Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gv_EditingControlShowing_SetCboSelectedEvent(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox)
            {
                ComboBox cbo = e.Control as ComboBox;

                cbo.SelectedIndexChanged -= Cbo_SelectedIndexChanged_VisibleSubItem;
                cbo.SelectedIndexChanged += Cbo_SelectedIndexChanged_VisibleSubItem;

                //왠지 모르겠는데 이거 안하면 배경 검정으로 변함
                e.CellStyle.BackColor = this.gvRegistry.DefaultCellStyle.BackColor;
            }
        }

        /// <summary>
        /// 선택한 ComboBox에서 선택한 Item 바로 Cell에 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cbo_SelectedIndexChanged_VisibleSubItem(object sender, EventArgs e)
        {
            DataGridViewComboBoxCell cell = gvRegistry.CurrentCell as DataGridViewComboBoxCell;

            if (cell != null)
            {
                cell.Value = cell.EditedFormattedValue;
            }
        }

        /// <summary>
        /// 값 속성 변경 시 SubItem Visible 변경
        /// </summary>
        /// <param name="sendder"></param>
        /// <param name="e"></param>
        private void Gv_CellValueChanged_VisibleSubItem(object sendder, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == colValueType.Index)
            {
                object changedValue = gvRegistry.Rows[e.RowIndex].Cells[colValueType.Index].Value;
                string value = Convert.ToString(changedValue);

                //"Numeric", "Combo", "Text", "Bool"
                VisibleSubItem(value);
            }
        }

        #endregion ValueType Event End
        #region ReadWrite Event

        /// <summary>
        /// ReadOnly Column 셀만 클릭해도 바뀌도록하는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gv_CellClick_RWChange(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == colRW.Index)
            {
                if (CellValueChangedFlag)
                {
                    CellValueChangedFlag = false;

                    DataGridViewCell cell = gvRegistry.Rows[e.RowIndex].Cells[colRW.Index];

                    if (Convert.ToBoolean(cell.Value) == true)
                    {
                        cell.Value = false;
                    }
                    else
                    {
                        cell.Value = true;
                    }

                    gvRegistry.RefreshEdit();  //변경값 적용시키기

                    CellValueChangedFlag = true;
                }
            }
        }


        #endregion ReadWrite Event
        #region Erase Event

        /// <summary>
        /// Grid Erase NewRow행 CellImage 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GvRegistry_CellFormatting_NewRowImageSet(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //이거 안해주면 빨간 엑스표이미지가뜸
            if (e.ColumnIndex == colErase.Index)
            {
                e.Value = colErase.Image;
            }
        }

        /// <summary>
        /// Grid에서 삭제아이콘 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EraseColumnClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (gvRegistry.Rows[e.RowIndex].IsNewRow) return;

            if (e.ColumnIndex == colErase.Index)
            {
                //RemoveAt 진행
                CellValueChangedFlag = true;

                SelectedModel.RegistryMap[SelectedProtocol].Remove(Convert.ToInt32(gvRegistry.Rows[e.RowIndex].Cells[colAddrDec.Index].Value));
                gvRegistry.Rows.RemoveAt(e.RowIndex);

                CellValueChangedFlag = false;
            }
        }

        #endregion Erase Event End

        private void VisibleSubItem(string valueType)
        {
            if (valueType == "Numeric")
            {
                //Numeric
                NumDotPosition.Visible = true;
                NumMaxValue.Visible = true;
                NumMinValue.Visible = true;
                //Text
                NumMaxLength.Visible = false;
                //Combo
                TxtCboItems.Visible = false;
                btnCboItemsAdd.Visible = false;
                btnCboItemsDel.Visible = false;
                LbxCboItems.Visible = false;
            }
            else if (valueType == "Text")
            {
                //Numeric
                NumDotPosition.Visible = false;
                NumMaxValue.Visible = false;
                NumMinValue.Visible = false;
                //Text
                NumMaxLength.Visible = true;
                //Combo
                TxtCboItems.Visible = false;
                btnCboItemsAdd.Visible = false;
                btnCboItemsDel.Visible = false;
                LbxCboItems.Visible = false;
            }
            else if (valueType == "Combo")
            {
                //Numeric
                NumDotPosition.Visible = false;
                NumMaxValue.Visible = false;
                NumMinValue.Visible = false;
                //Text
                NumMaxLength.Visible = false;
                //Combo
                TxtCboItems.Visible = true;
                btnCboItemsAdd.Visible = true;
                btnCboItemsDel.Visible = true;
                LbxCboItems.Visible = true;
            }
            else
            {
                //Numeric
                NumDotPosition.Visible = false;
                NumMaxValue.Visible = false;
                NumMinValue.Visible = false;
                //Text
                NumMaxLength.Visible = false;
                //Combo
                TxtCboItems.Visible = false;
                btnCboItemsAdd.Visible = false;
                btnCboItemsDel.Visible = false;
                LbxCboItems.Visible = false;
            }
        }

        #endregion GridEvent End
        #region SubItem Event

        /// <summary>
        /// SubItem 값 변경 시 Registry SubItem 넣기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubItem_ValueChanged_SetRegistryClass(object sender, EventArgs e)
        {
            if ((CboProtocol.ctrl as ComboBox).Items.Count == 0) return;
            if (gvRegistry.CurrentRow.IsNewRow || gvRegistry.CurrentRow == null) return; //NewRow만 있으면 진행 return
            if (CellValueChangedFlag == false) return;

            DataGridViewRow dr = gvRegistry.CurrentRow;
            uProtocolType protocol = Convert.ToString(CboProtocol.Value).StringToEnum<uProtocolType>();
            int addr = Convert.ToInt32(dr.Cells[colAddrDec.Index].Value);
            RegistrySubItem subItem = SelectedModel.RegistryMap[protocol][addr].RegSubItem;

            string ctrlName = (sender as Control).Name;
            if (ctrlName == "NumDotPosition") { int value = Convert.ToInt32(NumDotPosition.Value); subItem.DotPosition = value; }
            else if (ctrlName == "NumMaxValue") { int value = Convert.ToInt32(NumMaxValue.Value); subItem.MaxValue = value; }
            else if (ctrlName == "NumMinValue") { int value = Convert.ToInt32(NumMinValue.Value); subItem.MinValue = value; }
            else if (ctrlName == "NumMaxLength") { int value = Convert.ToInt32(NumMaxLength.Value); subItem.MaxLength = value; }
            else if (ctrlName == "btnCboItemsAdd")
            {
                string itemName = TxtCboItems.Text.Trim();

                if (itemName == "") return;
                if (LbxCboItems.Items.Contains(itemName))
                {
                    MessageBox.Show(RuntimeData.String("F030004"));
                    return;
                }

                subItem.ComboItems.Add(itemName);
                LbxCboItems.Items.Add(itemName);

                TxtCboItems.Text = "";
            }
        }

        #endregion SubItem Event End

        #endregion Event End

        #region XML
        
        /// <summary>
        /// Unit정보 XML 저장
        /// </summary>
        internal void UnitInfoSave()
        {
            //xmlNode 추가인데..... 수정하면 한줄만 추가하는 방식으로는 못만드나?
            //불러온상태에서 추가할떄마다 그 정보만 Update한다던가
            //없는 unit정보일때 Add하는방식
            XmlDocument xdoc = new XmlDocument();
            XmlNode root = xdoc.CreateElement("UnitList");
            xdoc.AppendChild(root);

            foreach (KeyValuePair<string, UnitType> pariType in dicUnitTypes.OrderBy(idx => idx.Key))
            {
                UnitType unitType = pariType.Value;
                /////////////////////////////////////////////////////////////
                /////////////////////////  Unit Type  ///////////////////////
                /////////////////////////////////////////////////////////////
                XmlNode xmlType = xdoc.CreateElement("UnitType");
                //그룹 이름 정의
                XmlAttribute typeName = xdoc.CreateAttribute("Name");
                typeName.Value = unitType.Name;
                xmlType.Attributes.Append(typeName);

                foreach (KeyValuePair<string, UnitModel> pairModel in dicUnitTypes[unitType.Name].Models.OrderBy(idx => idx.Key))
                {
                    UnitModel model = pairModel.Value;
                    /////////////////////////////////////////////////////////////
                    ////////////////////////  Unit Model  ///////////////////////
                    /////////////////////////////////////////////////////////////
                    XmlNode xmlUnitModel = xdoc.CreateElement("Model");
                    //Model 이름 정의
                    XmlAttribute attrModelName = xdoc.CreateAttribute("Name");
                    attrModelName.Value = model.ModelName;
                    xmlUnitModel.Attributes.Append(attrModelName);

                    //지원하는 통신 Protocol
                    /* 지원하는 형식이 추가, 삭제, 순서변경 등을 고려하여
                     * ListNode를 만들고 하위로 Item들을 두어 1,0으로 판단하도록 개발*/
                    XmlNode xmlProtocolTypeList = xdoc.CreateElement("SupportProtocol");
                    foreach (uProtocolType protocol in UtilCustom.EnumToItems<uProtocolType>())
                    {
                        XmlNode xmlProtocolType = xdoc.CreateElement("Protocol");
                        XmlAttribute attrProtocolName = xdoc.CreateAttribute("Name");
                        XmlAttribute attrEnable = xdoc.CreateAttribute("Enable");
                        bool enable = model.SupportProtocol[protocol];
                        //지원하면 1 안하면 0
                        attrProtocolName.Value = protocol.ToString();
                        attrEnable.Value = (enable == true ? 1 : 0).ToString();

                        xmlProtocolType.Attributes.Append(attrProtocolName);
                        xmlProtocolType.Attributes.Append(attrEnable);

                        if (enable == true)
                        {
                            /////////////////////////////////////////////////////////////
                            //////////////////////  Unit Registry  //////////////////////
                            /////////////////////////////////////////////////////////////

                            if (model.RegistryMap[protocol].Count > 0)
                            {
                                XmlNode xmlRegistryMap = xdoc.CreateElement("Registry");

                                foreach (KeyValuePair<int, UnitRegistry> pairReistry in model.RegistryMap[protocol].OrderBy(idx => idx.Key))
                                {
                                    UnitRegistry registry = pairReistry.Value;

                                    XmlNode xmlParameter = xdoc.CreateElement("Parameter");
                                    XmlAttribute attrAddress = xdoc.CreateAttribute("Address");
                                    XmlAttribute attrName = xdoc.CreateAttribute("Name");
                                    XmlAttribute attrValueType = xdoc.CreateAttribute("ValueType");
                                    XmlAttribute attrDefaultvalue = xdoc.CreateAttribute("DefaultValue");
                                    XmlAttribute attrReadOnly = xdoc.CreateAttribute("ReadOnly");

                                    attrAddress.Value = registry.AddressDec.ToString();//필수값
                                    attrName.Value = registry.RegName;
                                    attrValueType.Value = Convert.ToString(registry.ValueType);
                                    attrDefaultvalue.Value = Convert.ToString(registry.DefaultValue);
                                    attrReadOnly.Value = registry.ReadOnly ? "1" : "0";

                                    xmlParameter.Attributes.Append(attrAddress);
                                    xmlParameter.Attributes.Append(attrName);
                                    xmlParameter.Attributes.Append(attrValueType);
                                    xmlParameter.Attributes.Append(attrDefaultvalue);
                                    xmlParameter.Attributes.Append(attrReadOnly);

                                    /////////////////////////////////////////////////////////////
                                    /////////////////////  Registry SubItem  ////////////////////
                                    /////////////////////////////////////////////////////////////
                                    if (attrValueType.Value == "Numeric")
                                    {
                                        int dotPosition = registry.RegSubItem.DotPosition;
                                        int maxValue = registry.RegSubItem.MaxValue;
                                        int minValue = registry.RegSubItem.MinValue;

                                        if (dotPosition != 0 || maxValue != 0 || minValue != 0)
                                        {
                                            XmlNode xmlSubItem = xdoc.CreateElement("SubItem");
                                            XmlNode xmlDp = xdoc.CreateElement("DotPosition");
                                            XmlNode xmlMax = xdoc.CreateElement("MaxValue");
                                            XmlNode xmlMin = xdoc.CreateElement("MinValue");

                                            xmlDp.InnerText = dotPosition.ToString();
                                            xmlMax.InnerText = maxValue.ToString();
                                            xmlMin.InnerText = minValue.ToString();

                                            xmlSubItem.AppendChild(xmlDp);
                                            xmlSubItem.AppendChild(xmlMax);
                                            xmlSubItem.AppendChild(xmlMin);

                                            xmlParameter.AppendChild(xmlSubItem);
                                        }
                                    }
                                    else if (attrValueType.Value == "Combo")
                                    {
                                        //0개면 생성안하도록 수정
                                        if (registry.RegSubItem.ComboItems.Count > 0)
                                        {
                                            XmlNode xmlSubItem = xdoc.CreateElement("SubItem");
                                            XmlNode xmlComboItems = xdoc.CreateElement("ComboItem");

                                            string items = string.Empty;
                                            for(int i =0; i<registry.RegSubItem.ComboItems.Count; i++)
                                            {
                                                string item = registry.RegSubItem.ComboItems[i];

                                                if(i == 0) { items += item; }
                                                else { items += "," + item; }
                                            }

                                            xmlComboItems.InnerText = items;

                                            xmlSubItem.AppendChild(xmlComboItems);
                                            xmlParameter.AppendChild(xmlSubItem);
                                        }
                                    }
                                    else if (attrValueType.Value == "Text")
                                    {
                                        if (registry.RegSubItem.MaxLength > 0)
                                        {
                                            XmlNode xmlSubItem = xdoc.CreateElement("SubItem");
                                            XmlNode xmlMaxLength = xdoc.CreateElement("MaxLength");

                                            xmlMaxLength.InnerText = registry.RegSubItem.MaxLength.ToString();

                                            xmlSubItem.AppendChild(xmlMaxLength);
                                            xmlParameter.AppendChild(xmlSubItem);
                                        }
                                    }//Registry SubItem End

                                    xmlRegistryMap.AppendChild(xmlParameter);
                                }

                                xmlProtocolType.AppendChild(xmlRegistryMap);
                            }//Unit Registry End
                        }

                        xmlProtocolTypeList.AppendChild(xmlProtocolType);
                    }//지원하는 통신 Protocol End
                    xmlUnitModel.AppendChild(xmlProtocolTypeList);

                    //그룹 하위로 추가
                    xmlType.AppendChild(xmlUnitModel);
                }//Unit Model End

                root.AppendChild(xmlType);
            }//Unit Type End

            //작성파일 저장
            xdoc.Save(InfoFilePath);
        }

        /// <summary>
        /// 저장되어있는 Unit정보 호출
        /// </summary>
        private void Load_UnitInfo()
        {
            LoadUnitInfoXML();
            InitializeXML();
        }

        /// <summary>
        /// 초기 Unit 정보들 호출
        /// </summary>
        private void LoadUnitInfoXML()
        {
            string filePath = InfoFilePath;

            dicUnitTypes = new Dictionary<string, UnitType>();
            if (System.IO.File.Exists(filePath))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filePath);

                if (xdoc.ChildNodes.Count > 0)
                {
                    XmlNode unitList = xdoc.SelectSingleNode("UnitList");

                    //가져온 Node Dictionary에 추가
                    foreach (XmlNode TypeNode in unitList.ChildNodes)
                    {
                        ///////////////////////////////////////////
                        ////////////   Unit Type 생성  ////////////
                        ///////////////////////////////////////////
                        string TypeName = TypeNode.Attributes["Name"].Value;
                        UnitType unitType = new UnitType(TypeName);

                        foreach (XmlNode ModelNode in TypeNode.ChildNodes)
                        {
                            ///////////////////////////////////////////
                            ////////////   Unit Model 생성  ///////////
                            ///////////////////////////////////////////
                            UnitModel model = new UnitModel();
                            model.ModelName = ModelNode.Attributes["Name"].Value;    //Model이름

                            //지원 통신Protocol
                            foreach (XmlNode nodeProtocol in ModelNode.SelectSingleNode("SupportProtocol").ChildNodes)
                            {
                                uProtocolType protocolType = nodeProtocol.Attributes["Name"].Value.StringToEnum<uProtocolType>();
                                bool enable = nodeProtocol.Attributes["Enable"].Value == 1.ToString() ? true : false;

                                model.SupportProtocol[protocolType] = enable;
                                model.RegistryMap.Add(protocolType, new Dictionary<int, UnitRegistry>());

                                if(enable == true && nodeProtocol.SelectSingleNode("Registry") != null)
                                {
                                    /////////////////////////////////////////////////////////////
                                    //////////////////////  Unit Registry  //////////////////////
                                    /////////////////////////////////////////////////////////////
                                    foreach (XmlNode nodeParam in nodeProtocol.SelectSingleNode("Registry").ChildNodes)
                                    {
                                        UnitRegistry param = new UnitRegistry();
                                        param.AddressDec = Convert.ToInt32(nodeParam.Attributes["Address"].Value);
                                        param.RegName = nodeParam.Attributes["Name"].Value;
                                        param.ValueType = nodeParam.Attributes["ValueType"].Value;
                                        param.DefaultValue = nodeParam.Attributes["DefaultValue"].Value;
                                        param.ReadOnly = nodeParam.Attributes["ReadOnly"].Value == "1" ? true : false;

                                        /////////////////////////////////////////////////////////////
                                        /////////////////////  Registry SubItem  ////////////////////
                                        /////////////////////////////////////////////////////////////
                                        XmlNode nodeSub = nodeParam.SelectSingleNode("SubItem");

                                        if (nodeSub != null)
                                        {
                                            if (param.ValueType == "Combo")
                                            {
                                                XmlNode nodeItem = nodeSub.SelectSingleNode("ComboItem");

                                                string[] items = nodeSub.InnerText.Split(',');

                                                param.RegSubItem.ComboItems = items.ToList();
                                            }
                                            else if (param.ValueType == "Numeric")
                                            {
                                                foreach (XmlNode item in nodeSub.ChildNodes)
                                                {
                                                    if (item.Name == "DotPosition") { param.RegSubItem.DotPosition = Convert.ToInt32(item.InnerText); }
                                                    else if (item.Name == "MaxValue") { param.RegSubItem.MaxValue = Convert.ToInt32(item.InnerText); }
                                                    else if (item.Name == "MinValue") { param.RegSubItem.MinValue = Convert.ToInt32(item.InnerText); }
                                                }
                                            }
                                            else if (param.ValueType == "Text")
                                            {
                                                XmlNode nodeItem = nodeSub.SelectSingleNode("MaxLength");

                                                param.RegSubItem.MaxLength = Convert.ToInt32(nodeSub.InnerText);
                                            }
                                        }//Registry SubItem End

                                        model.RegistryMap[protocolType].Add(param.AddressDec, param);
                                    }//Registry End
                                }
                            }//지원 통신 Protocol End

                            //Registry Map

                            //Dictionary에 추가
                            unitType.Models.Add(model.ModelName, model);
                        }//Model 생성 End

                        dicUnitTypes.Add(TypeName, unitType);
                    }//Type 생성 End
                }
            }
            else
            {
                Debug.Write("Frm_UnitSetting.cs / LoadUnitInfoXml / 'File not exist' ");
            }
        }

        /// <summary>
        /// Unit정보 불러오기(RuntimeData에 가지고있음)
        /// </summary>
        private void InitializeXML()
        {
            foreach (string TypeName in dicUnitTypes.Keys)
            {
                //Type
                UnitType unitType = dicUnitTypes[TypeName];
                LbxUnitType.Items.Add(TypeName);
                //나머지는 Selected Event로 만들어짐
            }

            if (LbxUnitType.Items.Count > 0)
            {
                LbxUnitType.SelectedIndex = 0;
            }
        }

        #endregion XML End

        #region Excel

        private void SaveExcel()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "엑셀 통합 파일(.xlsx)|*.xlsx|97-03통합(.xls)|*.xls";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            string savePath = saveFileDialog.FileName;

            if (UtilCustom.CheckFileOpend(savePath) == true) return;


            Excel.Application excelApp = new Excel.Application();
            excelApp.DisplayAlerts = false; //중복이름 저장시 알림 삭제
            Excel.Workbook wb = excelApp.Workbooks.Add();
            Excel.Worksheet sheet = wb.Worksheets.get_Item(1) as Excel.Worksheet;

            try
            {
                //Column Header Name
                sheet.Cells[2,  1] = "Model Type";
                sheet.Cells[3,  1] = "Model Name";
                sheet.Cells[4,  1] = "Registry Protocol";

                sheet.Cells[1,  4] = "Address";
                sheet.Cells[1,  5] = "Name";
                sheet.Cells[1,  6] = "Value Type";
                sheet.Cells[1,  7] = "Default Value";
                sheet.Cells[1,  8] = "ReadOnly";
                sheet.Cells[1, 10] = "[Numeric]\nMaxValue";
                sheet.Cells[1, 11] = "[Numeric]\nMinValue";
                sheet.Cells[1, 12] = "[Text]\nMaxLength";
                sheet.Cells[1, 13] = "[Combo]\nItems";

                //Cell 정렬
                //Header Cell
                sheet.Cells[1,  4].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //Address
                sheet.Cells[1,  5].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //Name
                sheet.Cells[1,  6].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //Value Type
                sheet.Cells[1,  7].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //Default Value
                sheet.Cells[1,  8].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //Read Only
                sheet.Cells[1, 10].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //[Numeric]MaxValue
                sheet.Cells[1, 11].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //[Numeric]MinValue
                sheet.Cells[1, 12].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //[Text]MaxLength
                sheet.Cells[1, 13].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //[Combo]Items
                //값 Cell
                sheet.Range[sheet.Cells[2, 4] , sheet.Cells[65536, 4]] .HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //Address
                sheet.Range[sheet.Cells[2, 5] , sheet.Cells[65536, 5]] .HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //Value Type
                sheet.Range[sheet.Cells[2, 10], sheet.Cells[65536, 10]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //[Numeric]MaxValue
                sheet.Range[sheet.Cells[2, 11], sheet.Cells[65536, 11]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //[Numeric]MinValue
                sheet.Range[sheet.Cells[2, 12], sheet.Cells[65536, 12]].HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;  //[Text]MaxLength

                //Column 너비 조정
                sheet.Columns[1].ColumnWidth = 15;  
                sheet.Columns[2].ColumnWidth = 20;  
                sheet.Columns[3].ColumnWidth = 5;  
                sheet.Columns[5].ColumnWidth = 10;  //Name
                sheet.Columns[6].ColumnWidth = 13;  //ValueType
                sheet.Columns[7].ColumnWidth = 13;  //Default Value
                sheet.Columns[8].ColumnWidth = 9;   //ReadOnly
                sheet.Columns[9].ColumnWidth = 5;   //SubItem 사이 공간
                sheet.Columns[10].ColumnWidth = 10;  //[Numeric]MaxValue
                sheet.Columns[11].ColumnWidth = 10;  //[Numeric]MinValue
                sheet.Columns[12].ColumnWidth = 10;  //[Text]MaxLength


                sheet.Cells[2, 2] = SelectedType.Name;
                sheet.Cells[3, 2] = SelectedModel.ModelName;
                sheet.Cells[4, 2] = SelectedProtocol.ToString();

                int idx = 2;
                foreach (UnitRegistry reg in SelectedModel.RegistryMap[SelectedProtocol].Values)
                {
                    //Registry
                    sheet.Cells[idx, 4] = reg.AddressDec;
                    sheet.Cells[idx, 5] = reg.RegName;
                    sheet.Cells[idx, 6] = reg.ValueType;
                    sheet.Cells[idx, 7] = reg.DefaultValue;
                    sheet.Cells[idx, 8] = reg.ReadOnly;

                    //Registry SubItem
                    if(reg.ValueType == "Numeric")
                    { 
                        sheet.Cells[idx, 10] = reg.RegSubItem.MaxValue;
                        sheet.Cells[idx, 11] = reg.RegSubItem.MinValue;
                    }
                    else if (reg.ValueType == "Text")
                    {
                        sheet.Cells[idx, 12] = reg.RegSubItem.MaxLength;
                    }
                    else if (reg.ValueType == "Combo")
                    {
                        if (reg.RegSubItem.ComboItems.Count > 0)
                        {
                            string items = string.Empty;
                            for (int i = 0; i < reg.RegSubItem.ComboItems.Count; i++)
                            {
                                string item = reg.RegSubItem.ComboItems[i];

                                if (i == 0) { items += item; }
                                else { items += "," + item; }
                            }

                            sheet.Cells[idx, 13] = items;
                        }
                    }

                    idx++;
                }

                //설명부
                sheet.Cells[7, 1] = RuntimeData.String("F030201");  //설명란
                sheet.Cells[8, 1] = "Name";
                sheet.Cells[8, 2] = RuntimeData.String("F03020100");
                sheet.Cells[9, 1] = "Value Type";
                sheet.Cells[9, 2] = RuntimeData.String("F03020101");
                sheet.Cells[10, 1] = "Default Value";
                sheet.Cells[10, 2] = RuntimeData.String("F03020102");
                sheet.Cells[11, 1] = "ReadOnly";
                sheet.Cells[11, 2] = RuntimeData.String("F03020103");

                sheet.Cells[13, 1] = RuntimeData.String("F03020101");   //입력방법 ※1
                sheet.Cells[14, 1] = "Text";
                sheet.Cells[14, 2] = RuntimeData.String("F03020104");   //문자값 입력 ※2
                sheet.Cells[15, 1] = "Numeric";
                sheet.Cells[15, 2] = RuntimeData.String("F03020105");   //숫자값 입력 ※3
                sheet.Cells[16, 1] = "Combo";
                sheet.Cells[16, 2] = RuntimeData.String("F03020106");   //선택 입력 ※4

                sheet.Cells[18, 1] = RuntimeData.String("F03020104");   //문자값 입력 ※2
                sheet.Cells[19, 1] = "MaxValue";
                sheet.Cells[19, 2] = RuntimeData.String("F03020107");
                sheet.Cells[20, 1] = "MinValue";
                sheet.Cells[20, 2] = RuntimeData.String("F03020108");
                sheet.Cells[21, 1] = RuntimeData.String("F03020105");   //숫자값 입력 ※3
                sheet.Cells[22, 1] = "MaxLength";
                sheet.Cells[22, 2] = RuntimeData.String("F03020109");
                sheet.Cells[23, 1] = RuntimeData.String("F03020106");   //선택 입력 ※4
                sheet.Cells[24, 1] = "Items";
                sheet.Cells[24, 2] = RuntimeData.String("F0302010A");

                //확장자 구분
                string[] extention = savePath.Split('.');
                if (extention[extention.Length - 1] == "xlsx")
                {
                    wb.SaveAs(savePath, Excel.XlFileFormat.xlWorkbookDefault);
                }
                else if (extention[extention.Length - 1] == "xls")
                {
                    wb.SaveAs(savePath, Excel.XlFileFormat.xlWorkbookNormal);
                }

                //엑셀 열린거 종료
                wb.Close();
                excelApp.Quit();

                MessageBox.Show(RuntimeData.String("F000001"));
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                // Clean up
                ReleaseExcelObject(sheet);
                ReleaseExcelObject(wb);
                ReleaseExcelObject(excelApp);
            }
        }

        /// <summary>
        /// Background Excel 종료
        /// </summary>
        /// <param name="obj"></param>
        private static void ReleaseExcelObject(object obj)
        {
            try
            {
                if (obj != null)
                {
                    Marshal.ReleaseComObject(obj);
                    obj = null;
                }
            }
            catch (Exception ex)
            {
                obj = null;
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
        }

        #endregion Excel End
    }
}
