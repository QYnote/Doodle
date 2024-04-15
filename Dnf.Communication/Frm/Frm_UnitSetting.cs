using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using Dnf.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Dnf.Communication.Frm
{
    internal partial class Frm_UnitSetting : TabPage
    {
        #region Controls
        private int marginValue = 3;
        
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
        private ucControlBox CboProtocol = new ucControlBox(CtrlType.ComboBox);                 //Registry 통신타입
        private Button btnRegSave = new Button();                                               //Registry 저장버튼
        private Button btnRegExcelUpload = new Button();                                        //Registry 엑셀 저장
        private Button btnRegExcelDownload = new Button();                                      //Registry 엑셀 불러오기
        private DataGridView gvRegistry = new DataGridView();                                   //Registry 정보 GridView
        private DataGridViewTextBoxColumn colAddrDecimal = new DataGridViewTextBoxColumn();     //주소(10진법)
        private DataGridViewTextBoxColumn colAddrHex = new DataGridViewTextBoxColumn();     //주소(16진법)
        private DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();            //이름
        private DataGridViewComboBoxColumn colEditor = new DataGridViewComboBoxColumn();        //값 속성
        private DataGridViewTextBoxColumn colDefaultValue = new DataGridViewTextBoxColumn();    //기본 값(Default Value)
        private DataGridViewCheckBoxColumn colRW = new DataGridViewCheckBoxColumn();            //Read/Write 모드

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
        private CheckedListBox LbxCboItems = new CheckedListBox();
        #endregion Controls End

        private string SelectedType = string.Empty;
        private string SelectedModel = string.Empty;
        private string SelectedProtocol = string.Empty;

        private string InfoFilePath = RuntimeData.DataPath + "UnitInfo.xml";

        internal Frm_UnitSetting()
        {
            InitializeComponent();
            InitializeForm();

            this.Name = RuntimeData.String("F03");
            this.Text = this.Name;
            this.SizeChanged += FrmSizeChanged;

            UnitInfoLoad(); ;
        }

        /// <summary>
        /// Form 기초 셋팅
        /// </summary>
        private void InitializeForm()
        {
            InitializeUnitInfo();
            InitializeRegistry();
            InitializeRegistrySubItem();
            SetPositionSize();
            SetText();
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

            colAddrDecimal.Width = 70;
            colAddrHex.Width = 60;
            colName.Width = 100;
            colEditor.Width = 80;
            colDefaultValue.Width = 60;
            colRW.Width = 40;

            colAddrDecimal.DisplayIndex = 0;
            colAddrHex.DisplayIndex = 1;
            colName.DisplayIndex = 2;
            colEditor.DisplayIndex = 4;
            colDefaultValue.DisplayIndex = 5;
            colRW.DisplayIndex = 6;

            colAddrDecimal.Name = "colAddrDecimal";
            colAddrHex.Name = "colAddrHex";
            colName.Name = "colName";
            colEditor.Name = "colEditor";
            colDefaultValue.Name = "colDefaultValue";
            colRW.Name = "colRW";

            colEditor.Items.AddRange("Numeric", "Combo", "Text", "Bool");

            colAddrDecimal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colAddrHex.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colEditor.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDefaultValue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colRW.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            colAddrDecimal.SortMode = DataGridViewColumnSortMode.Programmatic;
            colAddrHex.SortMode = DataGridViewColumnSortMode.Programmatic;
            colName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colEditor.SortMode = DataGridViewColumnSortMode.NotSortable;
            colDefaultValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colRW.SortMode = DataGridViewColumnSortMode.NotSortable;

            //숫자만 입력하게 하기
            UtilCustom.ColumnOnlyNumeric(gvRegistry, colAddrDecimal.Name);
            UtilCustom.ColumnOnlyNumeric(gvRegistry, colAddrHex.Name, "Hex");

            gvRegistry.Columns.AddRange(colAddrDecimal, colAddrHex, colName, colEditor, colDefaultValue, colRW);

            (CboProtocol.ctrl as ComboBox).SelectedIndexChanged += CboProtocol_SelectedIndexChanged_SetFormSelectedProtocol;
            gvRegistry.ColumnHeaderMouseClick += Gv_SortAddr;
            gvRegistry.SortCompare += Gv_SortCompare;
            gvRegistry.CellValidating += Gv_CellValidating_AddrDecimalDuplicateConfirm;
            gvRegistry.CellValueChanged += Gv_CellValueChanged_DecimalToHex;
            gvRegistry.CellValueChanged += Gv_CellValueChanged_HexToDecimal;
            gvRegistry.CellValueChanged += Gv_CellValueChanged_VisibleSubItem;
            gvRegistry.CellMouseClick += Gv_CellClick_RWChange;
            gvRegistry.EditingControlShowing += Gv_EditingControlShowing_SetCboSelectedEvent;

            this.Controls.Add(CboProtocol);
            this.Controls.Add(gvRegistry);
        }

        private void InitializeRegistrySubItem()
        {
            lblSubItem.AutoSize = false;
            lblSubItem.TextAlign = ContentAlignment.MiddleCenter;
            lblSubItem.BorderStyle = BorderStyle.FixedSingle;

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
        }

        private void SetText()
        {
            //Unit
            LblUnitType.Text = RuntimeData.String("F030100");
            LblUnitModel.Text = RuntimeData.String("F030101");
            LblSupportProtocol.Text = RuntimeData.String("F030102");

            //Registry
            CboProtocol.LblText = RuntimeData.String("F030103");
            colAddrDecimal.HeaderText = RuntimeData.String("F03010400");
            colAddrHex.HeaderText = RuntimeData.String("F03010401");
            colName.HeaderText = RuntimeData.String("F03010402");
            colEditor.HeaderText = RuntimeData.String("F03010403");
            colDefaultValue.HeaderText = RuntimeData.String("F03010404");
            colRW.HeaderText = RuntimeData.String("F03010405");

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

            LblUnitType.Location = new Point(margin, margin);
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

        #region Event

        /// <summary>
        /// 동적 Size 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmSizeChanged(object sender, EventArgs e)
        {
            SetPositionSize();
            //this.pnlUnitType.Size = new Size(pnlUnitType.Width, this.Size.Height / 2);
        }

        /// <summary>
        /// Unit Type 선택Index 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitTypeSelectedChanged(object sender, EventArgs e)
        {
            this.SelectedType = LbxUnitType.SelectedItem as string;
            LbxUnitModel.Items.Clear();

            //하위 모델이 있을경우 첫번째 Item 선택
            if(this.SelectedType == null || this.SelectedType == string.Empty) { return; }

            if (RuntimeData.dicUnitTypes[SelectedType].Count > 0)
            {
                //해당Type의 Model리스트 조회
                foreach (string model in RuntimeData.dicUnitTypes[SelectedType].Keys)
                {
                    LbxUnitModel.Items.Add(model);
                }

                LbxUnitModel.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Unit Model 선택Index 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitModelSelectedChanged(object sender, EventArgs e)
        {
            this.SelectedModel = LbxUnitModel.SelectedItem as string;

            UnitModel model = RuntimeData.dicUnitTypes[SelectedType][SelectedModel];
            //지원하는 Protocol
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

            if((CboProtocol.ctrl as ComboBox).Items.Count > 0)
            {
                (CboProtocol.ctrl as ComboBox).SelectedIndex = 0;
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
            if (RuntimeData.dicUnitTypes.Keys.Contains(unitTypeName))
            {
                MessageBox.Show(RuntimeData.String("F030001"));
                return;
            }

            LbxUnitType.Items.Add(unitTypeName);
            RuntimeData.dicUnitTypes.Add(unitTypeName, new Dictionary<string, UnitModel>());

            //후처리
            TxtUnitType.Text = "";
            LbxUnitType.SelectedItem = unitTypeName;
        }

        /// <summary>
        /// Unit Type 삭제
        /// </summary>
        private void UnitTypeRemove()
        {
            if(SelectedType != string.Empty || SelectedType != "")
            {
                RuntimeData.dicUnitTypes.Remove(SelectedType);
                LbxUnitType.Items.Remove(SelectedType);

                SelectedType = string.Empty;
                if(LbxUnitType.Items.Count > 0)
                {
                    LbxUnitType.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Unit Model 생성
        /// </summary>
        private void UnitModelAdd()
        {
            string unitModelName = TxtUnitModel.Text.Trim();

            //Type이 선택된 상태인지 검사
            if (SelectedType == null || SelectedType == string.Empty || SelectedType == "")
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
            if (RuntimeData.dicUnitTypes[SelectedType].Keys.Contains(unitModelName))
            {
                MessageBox.Show(RuntimeData.String("F030001"));
                return;
            }

            LbxUnitModel.Items.Add(unitModelName);
            RuntimeData.dicUnitTypes[SelectedType].Add(unitModelName, new UnitModel() { ModelName = unitModelName});

            //후처리
            TxtUnitModel.Text = "";
            LbxUnitModel.SelectedItem = unitModelName;
        }

        /// <summary>
        /// Unit Model 삭제
        /// </summary>
        private void UnitModelRemove()
        {
            if ((SelectedType != string.Empty || SelectedType != "")
                && (SelectedModel != string.Empty || SelectedModel != ""))
            {
                RuntimeData.dicUnitTypes[SelectedType].Remove(SelectedModel);
                LbxUnitModel.Items.Remove(SelectedModel);

                SelectedModel = string.Empty;
                if (LbxUnitModel.Items.Count > 0)
                {
                    LbxUnitModel.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// 통신 Protocol 사용여부 변경 Event
        /// </summary>
        private void ProtocolFlagChanged()
        {
            if(SelectedType != string.Empty || SelectedModel != string.Empty)
            {
                uProtocolType protocol = (uProtocolType)CLbxSupportProtocol.SelectedItem;

                UnitModel model = RuntimeData.dicUnitTypes[SelectedType][SelectedModel];
                model.SupportProtocol[protocol] = CLbxSupportProtocol.GetItemChecked(CLbxSupportProtocol.SelectedIndex);
            }
        }

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

            if (e.ColumnIndex == colAddrDecimal.Index)
            {
                gvRegistry.Sort(colAddrDecimal, ListSortDirection.Ascending);
            }
        }

        /// <summary>
        /// Address 정렬 보조, int값 자리수 다르면 더 크게 체크시키기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gv_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column == colAddrDecimal)
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
            if (e.ColumnIndex == colAddrDecimal.Index ||
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

                    if (e.ColumnIndex == colAddrDecimal.Index)
                    {
                        //Decimal
                        /*작업내역
                         * ==쓰면 true 안떠서 Equals 사용
                         */
                        if (dr.Cells[colAddrDecimal.Index].Value.Equals(changedValue))
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

        private bool CellValueChangedFlag = true;   //CellValueChagned시 다른값 변경시킬때 방지용
        private void Gv_CellValueChanged_DecimalToHex(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == colAddrDecimal.Index)
            {
                if (CellValueChangedFlag)
                {
                    CellValueChangedFlag = false;

                    object changedValue = gvRegistry.Rows[e.RowIndex].Cells[colAddrDecimal.Index].Value;
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

                    gvRegistry.Rows[e.RowIndex].Cells[colAddrDecimal.Index].Value = Convert.ToInt32(value, 16);
                    gvRegistry.RefreshEdit();  //변경값 적용시키기

                    CellValueChangedFlag = true;
                }
            }
        }

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

            if(cell != null)
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

            if (e.ColumnIndex == colEditor.Index)
            {
                object changedValue = gvRegistry.Rows[e.RowIndex].Cells[colEditor.Index].Value;
                string value = Convert.ToString(changedValue);

                //"Numeric", "Combo", "Text", "Bool"
                if (value == "Numeric")
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
                else if (value == "Text")
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
                else if (value == "Combo")
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

                //Default값 적용
                //NumDotPosition.Value = 0;
                //NumMaxValue.Value = 0;
                //NumMinValue.Value = -1;
                //NumMaxLength.Value = 0;
                //TxtCboItems.Text = "";
                //LbxCboItems.Items.Clear();
            }
        }

        /// <summary>
        /// Registry Protocol 선택된 Protocol Form의 선택된 Protocol로 지정하기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CboProtocol_SelectedIndexChanged_SetFormSelectedProtocol(object sender, EventArgs e)
        {
            object value = CboProtocol.Value;

            this.SelectedProtocol = value.ToString();
        }

        #endregion Event End

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

            foreach (string unitType in RuntimeData.dicUnitTypes.Keys)
            {
                XmlNode xmlType = xdoc.CreateElement("UnitType");
                //그룹 이름 정의
                XmlAttribute typeName = xdoc.CreateAttribute("Name");
                typeName.Value = unitType;
                xmlType.Attributes.Append(typeName);

                foreach (UnitModel model in RuntimeData.dicUnitTypes[unitType].Values)
                {
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
                        XmlNode xmlProtocolType = xdoc.CreateElement(protocol.ToString());
                        //지원하면 1 안하면 0
                        xmlProtocolType.InnerText = (model.SupportProtocol[protocol] == true ? 1 : 0).ToString();

                        xmlProtocolTypeList.AppendChild(xmlProtocolType);
                    }
                    xmlUnitModel.AppendChild(xmlProtocolTypeList);

                    //그룹 하위로 추가
                    xmlType.AppendChild(xmlUnitModel);
                }

                root.AppendChild(xmlType);
            }

            //작성파일 저장
            xdoc.Save(InfoFilePath);
        }

        /// <summary>
        /// Unit정보 불러오기(RuntimeData에 가지고있음)
        /// </summary>
        private void UnitInfoLoad()
        {
            foreach(string TypeName in RuntimeData.dicUnitTypes.Keys)
            {
                LbxUnitType.Items.Add(TypeName);

                foreach(string modelName in RuntimeData.dicUnitTypes[TypeName].Keys)
                {
                    LbxUnitModel.Items.Add(modelName);
                }
            }

            if(LbxUnitType.Items.Count > 0)
            {
                LbxUnitType.SelectedIndex = 0;
            }
        }
    }
}
