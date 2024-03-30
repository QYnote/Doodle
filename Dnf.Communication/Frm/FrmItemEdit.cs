using Dnf.Communication.Controls;
using Dnf.Communication.Data;
using Dnf.Utils.Controls;
using Dnf.Utils.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Communication.Frm
{
    internal class FrmItemEdit : TabPage
    {
        //Unit Type 선택 -> Unit Model 선택 -> 통신 Protocl 선택 -> Port 정보 입력 -> Unit Addr 선택 -> Unit Name 입력
        //수정할거 개많네 야팔

        MainForm mainForm;

        #region Controls
        //저장 정보
        Panel pnlTop = new Panel();
        Panel pnlButton = new Panel();
        Button BtnOK = new Button();     //저장    
        Button BtnCancle = new Button();   //초기화

        //Port 정보
        Panel pnlControlBox = new Panel();
        ucControlBox cboPortName = new ucControlBox(CtrlType.ComboBox);      //연결된 포트
        ucControlBox cboProtocolType = new ucControlBox(CtrlType.ComboBox);  //통신방법 구분
        ucControlBox cboBaudRate = new ucControlBox(CtrlType.ComboBox);      //BaudRate
        ucControlBox numDataBits = new ucControlBox(CtrlType.NumbericUpDown);//Data Bits
        ucControlBox cboStopBit = new ucControlBox(CtrlType.ComboBox);       //StopBit
        ucControlBox cboParity = new ucControlBox(CtrlType.ComboBox);        //ParityBit

        ucControlBox txtIPaddr = new ucControlBox(CtrlType.TextBox);        //IP
        ucControlBox txtPortNo = new ucControlBox(CtrlType.MaskedTextBox);  //Port번호

        //Unit 정보
        ucControlBox cboUnitType = new ucControlBox(CtrlType.ComboBox);
        ucControlBox cboUnitModel = new ucControlBox(CtrlType.ComboBox);
        
        DataGridView gv = new DataGridView();
        DataGridViewTextBoxColumn colSlaveAddr = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnitName = new DataGridViewTextBoxColumn();

        #endregion Controls End

        public FrmItemEdit(MainForm form)
        {
            this.mainForm = form;

            CreateInitial();
            SetText();
            SetValue();
            SetVisible();
        }

        private void CreateInitial()
        {
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Size = new Size(pnlTop.Width, 30);

            InitializeButton();
            InitializeControlBox();
            InitializeGrid();

            Label splitLine1 = CreateSplitLine(DockStyle.Left);

            this.Controls.Add(splitLine1);
            this.Controls.Add(pnlTop);

            //pnlTop.BringToFront();
            pnlControlBox.BringToFront();
            splitLine1.BringToFront();
            pnlButton.BringToFront();
            gv.BringToFront();
        }

        #region Control Initialize
        
        private void InitializeButton()
        {
            pnlButton.Dock = DockStyle.Bottom;
            pnlButton.Size = new Size(pnlButton.Width, 30);

            //Button 정의
            BtnOK.Text = RuntimeData.String("F1001");
            BtnCancle.Text = RuntimeData.String("F1003");

            BtnOK.Dock = DockStyle.Right;
            BtnCancle.Dock = DockStyle.Right;

            BtnOK.Size = new Size(100, BtnOK.Height);
            BtnCancle.Size = new Size(100, BtnCancle.Height);

            //Button 추가
            pnlButton.Controls.Add(BtnOK);
            pnlButton.Controls.Add(BtnCancle);
            //이벤트
            BtnOK.Click += ClickButton_Check;
            BtnCancle.Click += ClickButton_Cancle;
            //정렬
            BtnCancle.BringToFront();
            BtnOK.BringToFront();

            this.Controls.Add(pnlButton);

        }

        private void InitializeControlBox()
        {
            pnlControlBox.Dock = DockStyle.Left;
            pnlControlBox.Size = new Size(300, pnlControlBox.Height);
            pnlControlBox.MinimumSize = new Size(300, pnlControlBox.Height);

            //Control 명(Control Type - Item구분 - 담당Property)
            cboUnitType.Name = "Combo-Unit-UnitType";
            cboUnitModel.Name = "Combo-Unit-UnitModel";
            cboPortName.ctrl.Name = "Combo-Port-PortName";
            cboProtocolType.ctrl.Name = "Combo-Port-ProtocolType";
            cboBaudRate.ctrl.Name = "Combo-Port-BaudRate";
            numDataBits.ctrl.Name = "Numeric-Port-DataBits";
            cboStopBit.ctrl.Name = "Combo-Port-StopBits";
            cboParity.ctrl.Name = "Combo-Port-Parity";
            txtIPaddr.ctrl.Name = "Txt-Port-IPaddr";
            txtPortNo.ctrl.Name = "Txt-Port-PortNo";

            //Items
            (cboUnitType.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(UnitType)).OfType<object>().ToArray());
            (cboUnitModel.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(UnitModel)).OfType<object>().ToArray());
            (cboPortName.ctrl as ComboBox).Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            (cboProtocolType.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(uProtocolType)).OfType<object>().ToArray());
            (cboBaudRate.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(BaudRate)).OfType<object>().ToArray());
            (cboStopBit.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(StopBits)).OfType<object>().ToArray());
            (cboParity.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(Parity)).OfType<object>().ToArray());

            (numDataBits.ctrl as NumericUpDown).Maximum = 8;
            (numDataBits.ctrl as NumericUpDown).Minimum = 7;

            (txtPortNo.ctrl as MaskedTextBox).ValidatingType = typeof(short);
            (txtPortNo.ctrl as MaskedTextBox).Mask = "####";

            (txtIPaddr.ctrl as TextBox).KeyPress += UtilCustom.TextBox_IP;

            cboUnitType.Dock = DockStyle.Top;
            cboUnitModel.Dock = DockStyle.Top;
            cboPortName.Dock = DockStyle.Top;
            cboProtocolType.Dock = DockStyle.Top;
            cboBaudRate.Dock = DockStyle.Top;
            numDataBits.Dock = DockStyle.Top;
            cboStopBit.Dock = DockStyle.Top;
            cboParity.Dock = DockStyle.Top;
            txtIPaddr.Dock = DockStyle.Top;
            txtPortNo.Dock = DockStyle.Top;

            //Label Width
            int portLabelWidth = 100;
            cboUnitType.LblWidth = portLabelWidth;
            cboUnitModel.LblWidth = portLabelWidth;
            cboPortName.LblWidth = portLabelWidth;
            cboProtocolType.LblWidth = portLabelWidth;
            cboBaudRate.LblWidth = portLabelWidth;
            numDataBits.LblWidth = portLabelWidth;
            cboStopBit.LblWidth = portLabelWidth;
            cboParity.LblWidth = portLabelWidth;
            txtIPaddr.LblWidth = portLabelWidth;
            txtPortNo.LblWidth = portLabelWidth;

            (cboProtocolType.ctrl as ComboBox).SelectedValueChanged += (sender, e) => { SetVisible(); };

            Label splitLine1 = CreateSplitLine(DockStyle.Top);
            Label splitLine2 = CreateSplitLine(DockStyle.Top);

            pnlControlBox.Controls.Add(splitLine1);
            pnlControlBox.Controls.Add(splitLine2);
            pnlControlBox.Controls.Add(cboUnitType);
            pnlControlBox.Controls.Add(cboUnitModel);
            pnlControlBox.Controls.Add(cboPortName);
            pnlControlBox.Controls.Add(cboProtocolType);
            pnlControlBox.Controls.Add(cboBaudRate);
            pnlControlBox.Controls.Add(numDataBits);
            pnlControlBox.Controls.Add(cboStopBit);
            pnlControlBox.Controls.Add(cboParity);
            pnlControlBox.Controls.Add(txtIPaddr);
            pnlControlBox.Controls.Add(txtPortNo);

            //Control 정렬
            cboUnitType.BringToFront();
            cboUnitModel.BringToFront();
            splitLine1.BringToFront();
            cboProtocolType.BringToFront();
            splitLine2.BringToFront();
            cboPortName.BringToFront();
            cboBaudRate.BringToFront();
            numDataBits.BringToFront();
            cboStopBit.BringToFront();
            cboParity.BringToFront();
            txtIPaddr.BringToFront();
            txtPortNo.BringToFront();

            this.Controls.Add(pnlControlBox);
        }

        private void InitializeGrid()
        {
            gv.Dock = DockStyle.Fill;
            gv.AllowUserToAddRows = false;
            gv.AllowUserToOrderColumns = false;
            gv.AllowUserToResizeRows = false;
            gv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gv.MultiSelect = false;

            DataGridViewCheckBoxColumn colCheck = new DataGridViewCheckBoxColumn();

            colCheck.HeaderText = "";
            colCheck.Width = 30;
            colSlaveAddr.Width = 60;
            colUnitName.Width = 120;
            colSlaveAddr.ReadOnly = true;

            colCheck.Name = "ColUnitCheck";
            colSlaveAddr.Name = "ColSlaveAddr";
            colUnitName.Name = "ColUnitName";

            colCheck.DisplayIndex = 0;
            colSlaveAddr.DisplayIndex = 1;
            colUnitName.DisplayIndex = 2;

            colCheck.ValueType = typeof(bool);
            colSlaveAddr.ValueType = typeof(byte);
            colUnitName.ValueType = typeof(string);

            colCheck.DataPropertyName = "Check";
            colSlaveAddr.DataPropertyName = "SlaveAddr";
            colUnitName.DataPropertyName = "UnitName";

            colSlaveAddr.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSlaveAddr.SortMode = DataGridViewColumnSortMode.NotSortable;
            colUnitName.SortMode = DataGridViewColumnSortMode.NotSortable;

            gv.Columns.AddRange(new DataGridViewColumn[] { colCheck, colSlaveAddr, colUnitName });

            DataTable dt = new DataTable();
            dt.Columns.Add("Check", typeof(bool));
            dt.Columns.Add("SlaveAddr", typeof(short));
            dt.Columns.Add("UnitName", typeof(string));

            for (int i = 0; i < 32; i++)
            {
                dt.Rows.Add(new object[] { false, i + 1, "" });
            }
            gv.DataSource = dt;

            gv.CellClick += Gv_CellClick;

            this.Controls.Add(gv);
        }

        private void Gv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            bool curValue = (bool)gv.Rows[e.RowIndex].Cells["ColUnitCheck"].Value;

            //Slave Address 클릭 시 체크박스 On/Off 처리
            if (e.ColumnIndex == gv.Columns["ColSlaveAddr"].Index)
            {
                if (curValue)
                {
                    gv.Rows[e.RowIndex].Cells["ColUnitCheck"].Value = false;
                }
                else
                {
                    gv.Rows[e.RowIndex].Cells["ColUnitCheck"].Value = true;
                }
            }
        }

        /// <summary>
        /// Panel 경계선 그리기
        /// </summary>
        /// <param name="dock">Dock 방향</param>
        /// <returns></returns>
        private Label CreateSplitLine(DockStyle dock, int thickness = 4)
        {
            Label lbl = new Label();
            lbl.Dock = dock;
            lbl.Margin = new Padding(3);
            lbl.BackColor = Color.DarkGray;
            lbl.Text = "";
            lbl.BorderStyle = BorderStyle.Fixed3D;
            lbl.AutoSize = false;
            if (dock == DockStyle.Left || dock == DockStyle.Right)
            {
                lbl.Size = new Size(thickness, lbl.Height);
            }
            else if(dock == DockStyle.Top || dock == DockStyle.Bottom)
            {
                lbl.Size = new Size(lbl.Width, thickness);
            }

            return lbl;
        }

        private void SetValue()
        {
            //Defulat Value
            if ((cboPortName.ctrl as ComboBox).Items.Count > 0)
            {
                (cboPortName.ctrl as ComboBox).SelectedIndex = 0;
            }
            (cboProtocolType.ctrl as ComboBox).SelectedIndex = 0;
            (cboBaudRate.ctrl as ComboBox).SelectedIndex = 0;
            (cboStopBit.ctrl as ComboBox).SelectedIndex = 0;
            (cboParity.ctrl as ComboBox).SelectedIndex = 0;
            (numDataBits.ctrl as NumericUpDown).Value = 8;

            (cboUnitType.ctrl as ComboBox).SelectedIndex = 0;
            (cboUnitModel.ctrl as ComboBox).SelectedIndex = 0;

            (txtIPaddr.ctrl as TextBox).Text = "192.168.0.1";
            (txtPortNo.ctrl as MaskedTextBox).Text = "0502";
        }

        private void SetText()
        {
            //Label 표기 Text
            cboUnitType.LblText = RuntimeData.String("F0301");
            cboUnitModel.LblText = RuntimeData.String("F0302");

            cboPortName.LblText = RuntimeData.String("F0100");
            cboProtocolType.LblText = RuntimeData.String("F0101");
            cboBaudRate.LblText = RuntimeData.String("F0102");
            numDataBits.LblText = RuntimeData.String("F0103");
            cboStopBit.LblText = RuntimeData.String("F0104");
            cboParity.LblText = RuntimeData.String("F0105");
            txtIPaddr.LblText = RuntimeData.String("F0306");
            txtPortNo.LblText = RuntimeData.String("F0307");

            //GridColumn Header Text
            colSlaveAddr.HeaderText = RuntimeData.String("F0300");
            colUnitName.HeaderText = RuntimeData.String("F0303");
        }

        #endregion ControlInitialize End

        #region Event

        /// <summary>
        /// 저장 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickButton_Check(object sender, EventArgs e)
        {
            if (ConfirmCreate())
            {
                Port port = CreatePort();

                foreach (DataGridViewRow dr in gv.Rows)
                {
                    if (dr.IsNewRow) continue;
                    if (Convert.ToBoolean(dr.Cells["ColUnitCheck"].Value) == false) continue;

                    CreateUnit(port, dr);
                }

                //mainForm 트리 그리기
                mainForm.InitTreeItem();
                //화면 종료
                mainForm.RemoveTabPage(this.Name);
            }
        }

        private void ClickButton_Cancle(object sender, EventArgs e)
        {
            mainForm.RemoveTabPage(this.Name);
        }

        private void SetVisible()
        {
            //Protocol Type에 따라서 Visible 처리
            uProtocolType type = (uProtocolType)(cboProtocolType.ctrl as ComboBox).SelectedItem;

            if (type == uProtocolType.ModBusTcpIp)
            {
                cboPortName.Visible = false;
                cboBaudRate.Visible = false;
                numDataBits.Visible = false;
                cboStopBit.Visible = false;
                cboParity.Visible = false;

                txtIPaddr.Visible = true;
                txtPortNo.Visible = true;
            }
            else
            {
                cboPortName.Visible = true;
                cboBaudRate.Visible = true;
                numDataBits.Visible = true;
                cboStopBit.Visible = true;
                cboParity.Visible = true;

                txtIPaddr.Visible = false;
                txtPortNo.Visible = false;
            }
        }

        #endregion Event End

        /// <summary>
        /// Create 확인 절차
        /// </summary>
        /// <returns>true : OK / false : Error</returns>
        private bool ConfirmCreate()
        {
            //사전 검사
            //Port명 중복 검사
            string portName = (cboPortName.ctrl as ComboBox).SelectedItem.ToString();
            if (portName == "") return false;

            if (RuntimeData.Ports.ContainsKey(portName))
            {
                MessageBox.Show(RuntimeData.String("A004"));
                return false;
            }

            //Unit값 빈값 있는지 검사
            bool checkFlag = false;

            foreach (DataGridViewRow dr in gv.Rows)
            {
                if (Convert.ToBoolean(dr.Cells["ColUnitCheck"].Value) == true) { checkFlag = true; break; }
            }
            if (!checkFlag) { return false; }

            return true;
        }

        private Port CreatePort()
        {
            Port port = null;

            //Port 작업
            string portName = (cboPortName.ctrl as ComboBox).SelectedItem.ToString();
            uProtocolType protocolType = (uProtocolType)(cboProtocolType.ctrl as ComboBox).SelectedItem;
            BaudRate baudRate = (BaudRate)(cboBaudRate.ctrl as ComboBox).SelectedItem;
            int dataBits = Convert.ToInt32((numDataBits.ctrl as NumericUpDown).Value);
            StopBits stopBits = (StopBits)(cboStopBit.ctrl as ComboBox).SelectedItem;
            Parity parity = (Parity)(cboParity.ctrl as ComboBox).SelectedItem;

            port = new Port(portName, protocolType, baudRate, dataBits, stopBits, parity);
            RuntimeData.Ports.Add(portName, port);

            return RuntimeData.Ports[portName];
        }

        private void CreateUnit(Port port, DataGridViewRow dr)
        {
            Unit unit = null;

            byte Addr = Convert.ToByte(dr.Cells["ColSlaveAddr"].Value);
            UnitType unitType = (UnitType)(cboUnitType.ctrl as ComboBox).SelectedItem;
            UnitModel unitModel = (UnitModel)(cboUnitModel.ctrl as ComboBox).SelectedItem;
            string unitName = dr.Cells["ColUnitName"].Value.ToString();

            unit = new Unit(port, Addr, unitType, unitModel, unitName);
            port.Units.Add(Addr, unit);
        }
    }
}
