using Dnf.Communication.Controls;
using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using Dnf.Utils.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Communication.Frm
{
    internal partial class FrmUnit : Form
    {
        /// <summary>
        /// Form Open 형태, New : 신규생성, Edit : 수정
        /// </summary>
        FrmEditType OpenType;
        /// <summary>
        /// Form 내부 Port
        /// </summary>
        Port BasePort;
        /// <summary>
        /// 선택된 Unit
        /// </summary>
        Unit SelectedUnit;

        #region Controls
        Panel pnlButton = new Panel();
        Button BtnOK = new Button();
        Button BtnCancel = new Button();

        Panel pnlControlBox = new Panel();
        ucControlBox cboProtocolType = new ucControlBox(CtrlType.ComboBox);
        ucControlBox numUnitAddr = new ucControlBox(CtrlType.NumbericUpDown);
        ucControlBox cboUnitType = new ucControlBox(CtrlType.ComboBox);
        ucControlBox cboUnitModel = new ucControlBox(CtrlType.ComboBox);
        ucControlBox txtUnitName = new ucControlBox(CtrlType.TextBox);
        Button BtnNew = new Button();

        DataGridView gv = new DataGridView();
        DataGridViewTextBoxColumn colSlaveAddr = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnitName = new DataGridViewTextBoxColumn();
        DataGridViewImageColumn colErase = new DataGridViewImageColumn();
        #endregion Controls End

        /// <summary>
        /// Row Remove하면 SelectionRow가 사라져서 쓰는 Flag
        /// </summary>
        bool removeFlag = false;
        /// <summary>
        /// Form이 틀어지거나 Unit 선택할때 Value Changed이벤트 막는용도
        /// </summary>
        bool EditFlag = false;

        internal FrmUnit(FrmEditType type, Port port, Unit unit = null)
        {
            OpenType = type;
            BasePort = port;
            SelectedUnit = unit;

            InitializeComponent();
            InitialForm();
        }

        private void InitialForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.ShowInTaskbar = false;
            this.Size = new Size(280, 400);
            this.Text = RuntimeData.String("F02");

            InitializeButton();
            InitializeControlBox();
            InitializeDockIndex();

            SetGridData();
            SetText();
            SetDefaultValue();
            SetEnable();
        }

        /// <summary>
        /// Button Control 생성
        /// </summary>
        private void InitializeButton()
        {
            pnlButton.Dock = DockStyle.Bottom;
            pnlButton.Size = new Size(pnlButton.Width, 30);

            BtnOK.Dock = DockStyle.Right;
            BtnCancel.Dock = DockStyle.Right;

            BtnOK.Size = new Size(100, BtnOK.Height);
            BtnCancel.Size = new Size(100, BtnCancel.Height);

            //Button 추가
            pnlButton.Controls.Add(BtnOK);
            pnlButton.Controls.Add(BtnCancel);
            //이벤트
            BtnOK.Click += ClickButton_Check;
            BtnCancel.Click += ClickButton_Cancel;
            //정렬
            BtnCancel.BringToFront();
            BtnOK.BringToFront();

            this.Controls.Add(pnlButton);
        }

        /// <summary>
        /// 조작 Control 생성
        /// </summary>
        private void InitializeControlBox()
        {
            pnlControlBox.Dock = DockStyle.Top;
            pnlControlBox.MinimumSize = new Size(pnlControlBox.Width, 135);

            //Control 명(Control Type - Item구분 - 담당Property)
            cboProtocolType.Name = "cboProtocolType";
            numUnitAddr.Name = "numUnitAddr";
            cboUnitType.Name = "cboUnitType";
            cboUnitModel.Name = "cboUnitModel";
            txtUnitName.Name = "txtUnitName";

            //Items
            (cboProtocolType.ctrl as ComboBox).Items.AddRange(UtilCustom.EnumToItems<uProtocolType>());;
            object[] UnitTypeArr = RuntimeData.dicUnitTypes.Keys.ToArray();
            (cboUnitType.ctrl as ComboBox).Items.AddRange(UnitTypeArr);
            object[] UnitModelArr = new object[] { };
            if (UnitTypeArr.Length > 0)
            {
                UnitModelArr = RuntimeData.dicUnitTypes[UnitTypeArr[0].ToString()].Keys.ToArray();    //Type 첫번째값으로 임시지정
            }
            (cboUnitModel.ctrl as ComboBox).Items.AddRange(UnitModelArr);

            (numUnitAddr.ctrl as NumericUpDown).Minimum = 1;
            (numUnitAddr.ctrl as NumericUpDown).Maximum = 255;

            //Dock
            cboProtocolType.Dock = DockStyle.Top;
            numUnitAddr.Dock = DockStyle.Top;
            cboUnitType.Dock = DockStyle.Top;
            cboUnitModel.Dock = DockStyle.Top;
            txtUnitName.Dock = DockStyle.Top;
            BtnNew.Dock = DockStyle.Fill;

            //Label Width
            int portLabelWidth = 100;
            cboProtocolType.LblWidth = portLabelWidth;
            numUnitAddr.LblWidth = portLabelWidth;
            cboUnitType.LblWidth = portLabelWidth;
            cboUnitModel.LblWidth = portLabelWidth;
            txtUnitName.LblWidth = portLabelWidth;
            BtnNew.Height = 30;

            Label splitLine1 = UtilCustom.CreateSplitLine(DockStyle.Top);

            pnlControlBox.Controls.Add(splitLine1);
            pnlControlBox.Controls.Add(numUnitAddr);
            pnlControlBox.Controls.Add(cboProtocolType);
            pnlControlBox.Controls.Add(cboUnitType);
            pnlControlBox.Controls.Add(cboUnitModel);
            pnlControlBox.Controls.Add(txtUnitName);
            pnlControlBox.Controls.Add(BtnNew);

            cboProtocolType.BringToFront();
            splitLine1.BringToFront();
            numUnitAddr.BringToFront();
            cboUnitType.BringToFront();
            cboUnitModel.BringToFront();
            txtUnitName.BringToFront();
            BtnNew.BringToFront();

            BtnNew.Click += Click_NewRow;
            (numUnitAddr.ctrl as NumericUpDown).ValueChanged += Changed_UnitProperty;
            (cboUnitModel.ctrl as ComboBox).SelectedIndexChanged += Changed_UnitProperty;
            (cboUnitType.ctrl as ComboBox).SelectedIndexChanged += Changed_UnitProperty;
            (txtUnitName.ctrl as TextBox).TextChanged += Changed_UnitProperty;


            //Grid는 내용이 많아서 따로 생성
            InitializeUnitGrid();

            this.Controls.Add(pnlControlBox);
        }

        /// <summary>
        /// DataGridView Control 셋팅
        /// </summary>
        private void InitializeUnitGrid()
        {
            gv.Dock = DockStyle.Fill;
            gv.AllowUserToAddRows = false;
            gv.AllowUserToOrderColumns = false;
            gv.AllowUserToResizeRows = false;
            gv.AutoGenerateColumns = false;
            gv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gv.MultiSelect = false;

            //Column Setting
            colSlaveAddr.Width = 60;
            colUnitName.Width = 120;
            colErase.Width = 20;

            colSlaveAddr.ReadOnly = true;
            colUnitName.ReadOnly = true;

            colSlaveAddr.Name = "colSlaveAddr";
            colUnitName.Name = "colUnitName";
            colErase.Name = "colErase";

            colSlaveAddr.DisplayIndex = 0;
            colUnitName.DisplayIndex = 1;
            colErase.DisplayIndex = 2;

            colSlaveAddr.ValueType = typeof(byte);
            colUnitName.ValueType = typeof(string);

            colSlaveAddr.DataPropertyName = "SlaveAddr";
            colUnitName.DataPropertyName = "UnitName";

            colErase.Image = Dnf.Utils.Properties.Resources.Erase_16x16;

            colSlaveAddr.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSlaveAddr.SortMode = DataGridViewColumnSortMode.NotSortable;
            colUnitName.SortMode = DataGridViewColumnSortMode.NotSortable;

            gv.Columns.AddRange(new DataGridViewColumn[] { colSlaveAddr, colUnitName, colErase });

            gv.CellContentClick += EraseColumnClick;
            gv.SelectionChanged += RowSelected;

            this.Controls.Add(gv);
        }

        /// <summary>
        /// Dock 순서 조정
        /// </summary>
        private void InitializeDockIndex()
        {
            pnlControlBox.BringToFront();
            pnlButton.BringToFront();
            gv.BringToFront();
        }

        /// <summary>
        /// Grid DataSource 셋팅
        /// </summary>
        private void SetGridData()
        {
            BindingSource binding = new BindingSource();
            DataTable dt = new DataTable();
            dt.Columns.Add("SlaveAddr", typeof(short));
            dt.Columns.Add("UnitName", typeof(string));
            dt.Columns.Add("Unit", typeof(Unit));

            foreach (Unit unit in BasePort.Units.Values)
            {
                dt.Rows.Add(new object[] { unit.SlaveAddr, unit.UnitName, unit });
            }

            binding.DataSource = dt;
            gv.DataSource = binding;
        }

        /// <summary>
        /// Controls 기본값 지정
        /// </summary>
        private void SetDefaultValue()
        {
            EditFlag = false;

            //Port Protocol 정보
            (cboProtocolType.ctrl as ComboBox).SelectedItem = BasePort.ProtocolType;

            if (OpenType == FrmEditType.New)
            {
                (numUnitAddr.ctrl as NumericUpDown).Value = 1;
                ComboBox unitType = (cboUnitType.ctrl as ComboBox);
                if (unitType.Items.Count > 0)
                {
                    unitType.SelectedIndex = 0;
                }
                ComboBox unitModel = (cboUnitModel.ctrl as ComboBox);
                if (unitModel.Items.Count > 0)
                {
                    unitModel.SelectedIndex = 0;
                }
                (txtUnitName.ctrl as TextBox).Text = "";
            }
            else if (OpenType == FrmEditType.Edit && SelectedUnit != null)
            {
                numUnitAddr.Value = SelectedUnit.SlaveAddr;
                cboUnitType.Value = SelectedUnit.UnitType;
                cboUnitModel.Value = SelectedUnit.UnitModel;
                txtUnitName.Value = SelectedUnit.UnitName;
            }

            EditFlag = true;
        }

        /// <summary>
        /// Controls 비활성화 처리
        /// </summary>
        private void SetEnable()
        {
            if (gv.Rows.Count < 1)
            {
                cboProtocolType.Enabled = false;
                numUnitAddr.Enabled = false;
                cboUnitType.Enabled = false;
                cboUnitModel.Enabled = false;
                txtUnitName.Enabled = false;
            }
            else
            {
                cboProtocolType.Enabled = true;
                numUnitAddr.Enabled = true;
                cboUnitType.Enabled = true;
                cboUnitModel.Enabled = true;
                txtUnitName.Enabled = true;
            }
        }

        /// <summary>
        /// Controls Text 지정
        /// </summary>
        private void SetText()
        {
            //Button 정의
            BtnOK.Text     = RuntimeData.String("F020100");
            BtnCancel.Text = RuntimeData.String("F020101");

            //Label 표기 Text
            cboProtocolType.LblText = RuntimeData.String("F010201");
            numUnitAddr.LblText     = RuntimeData.String("F020200");
            cboUnitType.LblText     = RuntimeData.String("F020201");
            cboUnitModel.LblText    = RuntimeData.String("F020202");
            txtUnitName.LblText     = RuntimeData.String("F020203");
            BtnNew.Text             = RuntimeData.String("F020204");

            //Grid
            colSlaveAddr.HeaderText = RuntimeData.String("F020300");
            colUnitName.HeaderText  = RuntimeData.String("F020301");
            colErase.HeaderText = "";
        }

        #region Event

        /// <summary>
        /// Grid에서 삭제아이콘 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EraseColumnClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex == gv.NewRowIndex) return;

            if(e.ColumnIndex == colErase.Index)
            {
                /*RemoveAt진행 시 SelectedIndex 이벤트가 진행되는것 방지*/
                removeFlag = true;
                gv.Rows.RemoveAt(e.RowIndex);
                removeFlag = false;

                SetEnable();
            }
        }

        /// <summary>
        /// 신규추가 Button Click 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_NewRow(object sender, EventArgs e)
        {
            BindingSource binding = gv.DataSource as BindingSource;
            DataTable dt = binding.DataSource as DataTable;
            DataRow dr = dt.NewRow();

            if(dt.Rows.Count == 0)
            {
                dr["SlaveAddr"] = 1;
            }
            else
            {
                //Address값 마지막값 + 1로
                int maxAddr = 1;
                foreach (DataRow row in dt.Rows)
                {
                    int addr = row["SlaveAddr"].ToInt32_Custom();
                    if (maxAddr <= addr) { maxAddr = addr + 1; continue; }
                }

                dr["SlaveAddr"] = maxAddr;
            }
            dr["UnitName"] = "";
            string unitType = cboUnitType.Value.ToString();
            string unitModel = cboUnitModel.Value.ToString();

            dr["Unit"] = new Unit(BasePort, dr["SlaveAddr"].ToInt32_Custom(), unitType, unitModel);

            dt.Rows.Add(dr);

            binding.ResetBindings(false);
            SetEnable();
        }

        /// <summary>
        /// Grid Row 선택 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void RowSelected(object sender, EventArgs e)
        {
            if (gv.SelectedRows.Count == 0) return;
            if (removeFlag) return;

            DataRow dr = GetSelectedGridDataRow();
            Unit unit = dr["Unit"] as Unit;
            EditFlag = false;

            numUnitAddr.Value = unit.SlaveAddr;
            cboUnitType.Value = unit.UnitType;
            cboUnitModel.Value = unit.UnitModel;
            txtUnitName.Value = unit.UnitName;

            EditFlag = true;
        }

        /// <summary>
        /// 저장버튼 Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickButton_Check(object sender, EventArgs e)
        {
            DataTable dt = (gv.DataSource as BindingSource).DataSource as DataTable;

            BasePort.Units.Clear();
            foreach (DataRow dr in dt.Rows)
            {
                Unit unit = dr["Unit"] as Unit;
                if (unit == null)
                {
                    MessageBox.Show(RuntimeData.String("F020000"));
                    return;
                }

                BasePort.Units.Add(unit.SlaveAddr, unit);
            }

            this.DialogResult = DialogResult.OK;
        }

        private void ClickButton_Cancel(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Unit속성 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Changed_UnitProperty(object sender, EventArgs e)
        {
            if (EditFlag)
            {
                DataRow dr = GetSelectedGridDataRow();
                string CtrlName = (sender as Control).Parent.Name;

                if (dr == null) return;
                Unit unit = dr["Unit"] as Unit;

                //CtrlName별로 동작 구분
                if (CtrlName == "numUnitAddr")
                {
                    //Slave Address 변경
                    int afAddr = (int)(sender as NumericUpDown).Value;
                    bool useAddr = true;

                    //변경될 값이 DataTble에 있는지 확인
                    foreach (DataRow drComp in dr.Table.Rows)
                    {
                        if (drComp != dr)
                        {
                            if (drComp["SlaveAddr"].ToInt32_Custom() == afAddr)
                            {
                                MessageBox.Show(RuntimeData.String("F020001"));
                                useAddr = false;
                                return;
                            }
                        }
                    }

                    //중복Addr 없으면 적용
                    if (useAddr)
                    {
                        unit.SlaveAddr = afAddr;
                        dr["SlaveAddr"] = unit.SlaveAddr;
                    }
                }
                else if (CtrlName == "cboUnitType")
                {
                    //UnitModel 리스트 변경
                    object selItem = cboUnitType.Value;
                    if (selItem == null) return;

                    object[] UnitModelArr = RuntimeData.dicUnitTypes[selItem.ToString()].Keys.ToArray();
                    ComboBox cboModel = (cboUnitModel.ctrl as ComboBox);
                    cboModel.Items.Clear();
                    cboModel.Items.AddRange(UnitModelArr);
                    if(cboModel.Items.Count > 0)
                    {
                        cboModel.SelectedIndex = 0;
                    }

                    unit.UnitType = selItem.ToString();
                }
                else if (CtrlName == "cboUnitModel")
                {
                    //Protocol 사용할 수 있는 Model인지 확인절차
                    object type = cboUnitType.Value;
                    object model = (sender as ComboBox).SelectedItem;
                    object protocol = cboProtocolType.Value;

                    /*모델정보 Dictionary -> Type검색 -> Model 검색 -> 지원 Protocol 검색 -> 지원여부값 추출*/
                    bool chkSupport = RuntimeData.dicUnitTypes[type.ToString()][model.ToString()].SupportProtocol[(uProtocolType)protocol];
                    if (!chkSupport)
                    {
                        EditFlag = false;
                        (sender as ComboBox).SelectedIndex = 0;
                        MessageBox.Show(RuntimeData.String("F020002"));
                        EditFlag = true;
                        return;
                    }

                    //Model 적용
                    unit.UnitModel = (sender as ComboBox).SelectedItem.ToString();
                }
                else if (CtrlName == "txtUnitName")
                {
                    //UnitName 변경
                    unit.UnitName = (sender as TextBox).Text;
                    dr["UnitName"] = unit.UnitName;
                }
            }
        }

        #endregion Event End

        private DataRow GetSelectedGridDataRow()
        {
            if (gv.SelectedRows.Count == 0) return null;

            DataTable dt = (gv.DataSource as BindingSource).DataSource as DataTable;
            DataGridViewRow drv = gv.SelectedRows[0];

            foreach (DataRow dr in dt.Rows)
            {
                //Addr이 같은 DataRow 탐색
                if (dr["SlaveAddr"].ToInt32_Custom() == drv.Cells["colSlaveAddr"].Value.ToInt32_Custom())
                {
                    return dr;
                }
            }

            return null;
        }

        private Unit CreateUnit(DataRow dr)
        {
            Unit unit = dr["Unit"] as Unit;

            int addr = unit.SlaveAddr;
            string unitType = unit.UnitType;
            string unitModel = unit.UnitModel;
            string unitName = unit.UnitName;

            return new Unit(BasePort, addr, unitType, unitModel, unitName);
        }
    }
}
