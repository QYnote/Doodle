using Dnf.Communication;
using Dnf.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFramework.Communication
{
    internal class FrmItemEdit : TabPage
    {
        MainForm mainForm;

        //Port 정보
        Panel pnlPort;
        GroupBox gbPort;
        ucControlBox cboPortList;
        ucControlBox cboProtocolType;
        ucControlBox cboBaudRate;
        ucControlBox numDataBits;
        ucControlBox cboParity;
        ucControlBox cboStopBit;

        Panel pnlPortBtn;
        Button BtnPortNew;
        Button BtnPortSave;
        Button BtnPortDel;

        //Unit 정보
        Panel pnlUnit;
        GroupBox gbUnit;
        ucControlBox numUnitAddress;
        ucControlBox cboUnitType;
        ucControlBox cboUnitModel;
        ucControlBox txtUnitName;

        Panel pnlUnitBtn;
        Button BtnUnitNew;
        Button BtnUnitSave;
        Button BtnUnitDel;

        public FrmItemEdit(MainForm form)
        {
            this.mainForm = form;

            CreateInitial();
        }


        private void CreateInitial()
        {
            CreateUnitInfo();
            this.Controls.Add(CreateSplitLine(DockStyle.Left));
            CreatePortInfo();

            InitGroupBox();
        }

        #region ControlInitialize

        private void CreatePortInfo()
        {
            pnlPort = new Panel();
            pnlPort.Dock = DockStyle.Left;
            pnlPort.Size = new Size(300, pnlPort.Height);
            pnlPort.MinimumSize = new Size(300, pnlPort.Height);

            gbPort = new GroupBox();
            gbPort.Dock = DockStyle.Top;
            gbPort.Size = new Size(gbPort.Width, 200);
            gbPort.Text = Data_Runtime.String("F0200");

            int portLabelWidth = 100;

            //연결된 포트
            cboPortList = new ucControlBox(CtrlType.ComboBox);
            cboPortList.Dock = DockStyle.Top;
            cboPortList.LblText = Data_Runtime.String("F0100");
            cboPortList.LblWidth = portLabelWidth;
            (cboPortList.ctrl as ComboBox).Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            if ((cboPortList.ctrl as ComboBox).Items.Count > 0)
            {
                (cboPortList.ctrl as ComboBox).SelectedIndex = 0;
            }

            //통신방법 구분
            cboProtocolType = new ucControlBox(CtrlType.ComboBox);
            cboProtocolType.LblText = Data_Runtime.String("F0101");
            cboProtocolType.LblWidth = portLabelWidth;
            cboProtocolType.Dock = DockStyle.Top;
            (cboProtocolType.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(ProtocolType)).OfType<object>().ToArray());    //포트타입 enum을 통해 추가
            (cboProtocolType.ctrl as ComboBox).SelectedIndex = 0;  //Default Value

            //BaudRate
            cboBaudRate = new ucControlBox(CtrlType.ComboBox);
            cboBaudRate.LblText = Data_Runtime.String("F0102");
            cboBaudRate.LblWidth = portLabelWidth;
            cboBaudRate.Dock = DockStyle.Top;
            (cboBaudRate.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(BaudRate)).OfType<object>().ToArray());    //enum을 통해 추가
            (cboBaudRate.ctrl as ComboBox).SelectedIndex = 0;  //Default Value

            //Data Bits
            numDataBits = new ucControlBox(CtrlType.NumbericUpDown);
            numDataBits.LblText = Data_Runtime.String("F0103");
            numDataBits.LblWidth = portLabelWidth;
            numDataBits.Dock = DockStyle.Top;
            (numDataBits.ctrl as NumericUpDown).Maximum = 8;
            (numDataBits.ctrl as NumericUpDown).Minimum = 5;
            (numDataBits.ctrl as NumericUpDown).Value = 8;  //Default Value

            //StopBit
            cboStopBit = new ucControlBox(CtrlType.ComboBox);
            cboStopBit.LblText = Data_Runtime.String("F0104");
            cboStopBit.LblWidth = portLabelWidth;
            cboStopBit.Dock = DockStyle.Top;
            (cboStopBit.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(StopBits)).OfType<object>().ToArray());    //enum을 통해 추가
            (cboStopBit.ctrl as ComboBox).SelectedIndex = 0;  //Default Value

            //ParityBit
            cboParity = new ucControlBox(CtrlType.ComboBox);
            cboParity.LblText = Data_Runtime.String("F0105");
            cboParity.LblWidth = portLabelWidth;
            cboParity.Dock = DockStyle.Top;
            (cboParity.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(Parity)).OfType<object>().ToArray());    //enum을 통해 추가
            (cboParity.ctrl as ComboBox).SelectedIndex = 0;  //Default Value

            //Active 버튼
            pnlPortBtn = new Panel();
            pnlPortBtn.Dock = DockStyle.Bottom;
            pnlPortBtn.Size = new Size(pnlPortBtn.Width, 30);

            BtnPortNew = new Button();
            BtnPortNew.Dock = DockStyle.Right;
            BtnPortNew.Size = new Size(100, BtnPortNew.Height);
            BtnPortNew.Text = Data_Runtime.String("F1000");

            BtnPortSave = new Button();
            BtnPortSave.Dock = DockStyle.Right;
            BtnPortSave.Size = new Size(100, BtnPortSave.Height);
            BtnPortSave.Text = Data_Runtime.String("F1001");

            BtnPortDel = new Button();
            BtnPortDel.Dock = DockStyle.Right;
            BtnPortDel.Size = new Size(100, BtnPortDel.Height);
            BtnPortDel.Text = Data_Runtime.String("F1002");


            pnlPort.Controls.Add(cboParity);
            pnlPort.Controls.Add(cboStopBit);
            pnlPort.Controls.Add(numDataBits);
            pnlPort.Controls.Add(cboBaudRate);
            pnlPort.Controls.Add(cboProtocolType);
            pnlPort.Controls.Add(cboPortList);
            pnlPort.Controls.Add(gbPort);

            pnlPortBtn.Controls.Add(BtnPortNew);
            pnlPortBtn.Controls.Add(BtnPortSave);
            pnlPortBtn.Controls.Add(BtnPortDel);
            pnlPort.Controls.Add(pnlPortBtn);

            this.Controls.Add(pnlPort);

            BtnPortSave.Click += (sender, e) => { PortCreate(); };
            BtnPortDel.Click += (sender, e) => { PortDelete(); };
            BtnPortNew.Click += (sender, e) => { PortNew(); };
        }

        private void CreateUnitInfo()
        {
            pnlUnit = new Panel();
            pnlUnit.Dock = DockStyle.Left;
            pnlUnit.Size = new Size(300, pnlUnit.Height);
            pnlUnit.MinimumSize = new Size(300, pnlUnit.Height);

            gbUnit = new GroupBox();
            gbUnit.Dock = DockStyle.Top;
            gbUnit.Size = new Size(gbUnit.Width, 200);
            gbUnit.Text = Data_Runtime.String("F0400");

            int unitLabelWIdth = 100;

            //Unit Slave Address
            numUnitAddress = new ucControlBox(CtrlType.NumbericUpDown);
            numUnitAddress.Dock = DockStyle.Top;
            numUnitAddress.LblText = Data_Runtime.String("F0300");
            numUnitAddress.LblWidth = unitLabelWIdth;
            (numUnitAddress.ctrl as NumericUpDown).Maximum = 128;
            (numUnitAddress.ctrl as NumericUpDown).Minimum = 1;
            (numUnitAddress.ctrl as NumericUpDown).Value = 1;   //Default Value

            //Unit 구분
            cboUnitType = new ucControlBox(CtrlType.ComboBox);
            cboUnitType.Dock = DockStyle.Top;
            cboUnitType.LblText = Data_Runtime.String("F0301");
            cboUnitType.LblWidth = unitLabelWIdth;
            (cboUnitType.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(UnitType)).OfType<object>().ToArray());
            (cboUnitType.ctrl as ComboBox).SelectedIndex = 0;

            //Unit Model명
            cboUnitModel = new ucControlBox(CtrlType.ComboBox);
            cboUnitModel.Dock = DockStyle.Top;
            cboUnitModel.LblText = Data_Runtime.String("F0302");
            cboUnitModel.LblWidth = unitLabelWIdth;
            (cboUnitModel.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(UnitModel)).OfType<object>().ToArray());
            if ((cboUnitType.ctrl as ComboBox).Items.Count > 0)
            {
                (cboUnitType.ctrl as ComboBox).SelectedIndex = 0;
            }

            //Unit Model명(사용자지정)
            txtUnitName = new ucControlBox(CtrlType.TextBox);
            txtUnitName.Dock = DockStyle.Top;
            txtUnitName.LblText = Data_Runtime.String("F0303");
            txtUnitName.LblWidth = unitLabelWIdth;
            (txtUnitName.ctrl as TextBox).Text = Convert.ToString((cboUnitModel.ctrl as ComboBox).Items[0]);

            //Active 버튼
            pnlUnitBtn = new Panel();
            pnlUnitBtn.Dock = DockStyle.Bottom;
            pnlUnitBtn.Size = new Size(pnlUnitBtn.Width, 30);

            BtnUnitNew = new Button();
            BtnUnitNew.Dock = DockStyle.Right;
            BtnUnitNew.Size = new Size(100, BtnUnitNew.Height);
            BtnUnitNew.Text = Data_Runtime.String("F1000");

            BtnUnitSave = new Button();
            BtnUnitSave.Dock = DockStyle.Right;
            BtnUnitSave.Size = new Size(100, BtnUnitSave.Height);
            BtnUnitSave.Text = Data_Runtime.String("F1001");

            BtnUnitDel = new Button();
            BtnUnitDel.Dock = DockStyle.Right;
            BtnUnitDel.Size = new Size(100, BtnUnitDel.Height);
            BtnUnitDel.Text = Data_Runtime.String("F1002");

            pnlUnit.Controls.Add(txtUnitName);
            pnlUnit.Controls.Add(cboUnitModel);
            pnlUnit.Controls.Add(cboUnitType);
            pnlUnit.Controls.Add(numUnitAddress);
            pnlUnit.Controls.Add(gbUnit);

            pnlUnitBtn.Controls.Add(BtnUnitNew);
            pnlUnitBtn.Controls.Add(BtnUnitSave);
            pnlUnitBtn.Controls.Add(BtnUnitDel);
            pnlUnit.Controls.Add(pnlUnitBtn);

            this.Controls.Add(pnlUnit);

            BtnUnitSave.Click += (sender, e) => { UnitCreate(); };
            BtnUnitDel.Click += (sender, e) => { UnitDelete(); };
            BtnUnitNew.Click += (sender, e) => { UnitNew(); };
        }

        /// <summary>
        /// GroupBox Controls초기화 후 생성하기
        /// </summary>
        private void InitGroupBox()
        {
            //GroupBox Clear
            gbPort.Controls.Clear();
            gbUnit.Controls.Clear();

            //GroupBox들에 Item 생성
            foreach (Port port in mainForm.ports.Values)
            {
                foreach (Unit unit in port.Units.Values)
                {
                    RadioButton rdoUnit = new RadioButton();
                    rdoUnit.Name = port.PortName + unit.SlaveAddr;  //GroupBox Controls 탐색방식
                    rdoUnit.Text = unit.UnitModelUserName;
                    rdoUnit.Dock = DockStyle.Top;
                    rdoUnit.Visible = true;
                    rdoUnit.Tag = unit;

                    gbUnit.Controls.Add(rdoUnit);
                }

                RadioButton rdoPort = new RadioButton();
                rdoPort.Name = port.PortName;
                rdoPort.Text = port.PortName;
                rdoPort.Dock = DockStyle.Top;
                rdoPort.Visible = true;
                rdoPort.Tag = port;

                rdoPort.CheckedChanged += (sender, e) => { VisibleUnitGroupBox(rdoPort); };

                gbPort.Controls.Add(rdoPort);
            }

            //Item 정렬
            SortGroupBox();
        }

        /// <summary>
        /// GroupBox의 선택된 Port에따라 Unit GroupBox Visible 처리
        /// </summary>
        /// <param name="rdo">선택된 Port RadioButton</param>
        private void VisibleUnitGroupBox(RadioButton rdo)
        {
            //선택된 Port UnitBox 숨김 해제
            if (rdo.Checked)
            {
                Port port = rdo.Tag as Port;

                foreach (Control ctrl in gbUnit.Controls)
                {
                    RadioButton rdoUnit = ctrl as RadioButton;
                    Unit gbUnit = rdoUnit.Tag as Unit;

                    //전체 Visible 숨김 처리
                    rdoUnit.Visible = false;

                    foreach (Unit portUnit in port.Units.Values)
                    {
                        //GroupBox Unit이 선택된 Port의 Unit에 있으면 보이기
                        if (portUnit.ParentPort == gbUnit.ParentPort)
                        {
                            rdoUnit.Visible = true;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GroupBox 정렬
        /// </summary>
        private void SortGroupBox()
        {
            //Port Sort
            foreach (var dicPairPort in mainForm.ports.OrderBy(x => x.Key))
            {
                int idxPort = 0;

                //Port Name == Group Box Name 비교
                foreach (Control ctrl in gbPort.Controls)
                {
                    if (ctrl.Name == dicPairPort.Key)
                    {
                        gbPort.Controls.SetChildIndex(ctrl, idxPort);
                        break;
                    }
                }

                //Unit Sort
                foreach (var dicPairUnit in dicPairPort.Value.Units.OrderBy(x => x.Key))
                {
                    int idxUnit = 0;

                    //Unit 유저지정명 == GroupBox Name 비교
                    foreach (Control ctrl in gbUnit.Controls)
                    {
                        if (ctrl.Text == dicPairUnit.Value.UnitModelUserName)
                        {
                            gbUnit.Controls.SetChildIndex(ctrl, idxUnit);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Panel 경계선 그리기
        /// </summary>
        /// <param name="dock">Dock 방향</param>
        /// <returns></returns>
        private Label CreateSplitLine(DockStyle dock)
        {
            Label lbl = new Label();
            lbl.Dock = dock;
            lbl.Margin = new Padding(3);
            lbl.BackColor = Color.DarkGray;
            lbl.Text = "";
            lbl.BorderStyle = BorderStyle.Fixed3D;
            lbl.AutoSize = false;
            lbl.Size = new Size(4, lbl.Height);

            return lbl;
        }

        #endregion ControlInitialize End

        #region Port

        private void PortCreate()
        {
            try
            {
                ProtocolType getProtocolType = (ProtocolType)(cboProtocolType.ctrl as ComboBox).SelectedItem;

                if (getProtocolType == ProtocolType.ModBusRTU
                     || getProtocolType == ProtocolType.ModBusAscii)
                {
                    string portName = Convert.ToString((cboPortList.ctrl as ComboBox).SelectedItem);
                    BaudRate baudRate = (BaudRate)(cboBaudRate.ctrl as ComboBox).SelectedItem;
                    int dataBits = Convert.ToInt32((numDataBits.ctrl as NumericUpDown).Value);
                    Parity pairty = (Parity)(cboParity.ctrl as ComboBox).SelectedItem;
                    StopBits stopBits = (StopBits)(cboStopBit.ctrl as ComboBox).SelectedItem;

                    //Error 체크
                    if (portName == "")
                    {
                        //lblStatus.Text = Data_Runtime.String("A002");
                        return;
                    }

                    //중복 포트인지 확인
                    if (mainForm.ports.ContainsKey(portName))
                    {
                        //lblStatus.Text = Data_Runtime.String("A004");
                        throw new Exception(Data_Runtime.String("A004"));
                    }

                    Port port = new Port(portName, getProtocolType, baudRate, dataBits, pairty, stopBits);
                    //Port 등록
                    mainForm.ports.Add(portName, port);

                    //Port 등록 후처리
                    InitGroupBox(); //GroupBox 재생성

                    (gbPort.Controls[port.PortName] as RadioButton).Checked = true;

                    mainForm.InitTreeItem();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PortDelete()
        {
            //선택중인 RadioButton 가져오기
            RadioButton rdo = GetSelectGroupBoxItem("Port");
            Port port = (rdo.Tag as Port);

            //등록된 Port 삭제
            mainForm.ports.Remove(port.PortName);

            //삭제 후처리
            //GroupBox 조정
            InitGroupBox();

            //메인폼 Tree 재지정
            mainForm.InitTreeItem();
        }

        private void PortNew()
        {
            (cboBaudRate.ctrl as ComboBox).SelectedIndex = 0;
            (cboParity.ctrl as ComboBox).SelectedIndex = 0;
            (cboStopBit.ctrl as ComboBox).SelectedIndex = 0;

            ComboBox cboList = cboPortList.ctrl as ComboBox;
            if (cboList.Items.Count > 0) { cboList.SelectedIndex = 0; }
            else { cboList.SelectedIndex = -1; }

            NumericUpDown num = numDataBits.ctrl as NumericUpDown;
            num.Value = (int)num.Minimum;
        }


        #endregion Port End

        #region Unit

        private void UnitCreate()
        {
            //선택된 Port에 Unit 추가
            try
            {
                Port port = GetSelectGroupBoxItem("Port").Tag as Port;
                int slaveAddr = (int)(numUnitAddress.ctrl as NumericUpDown).Value;
                UnitType type = (UnitType)(cboUnitType.ctrl as ComboBox).SelectedItem;
                UnitModel model = (UnitModel)(cboUnitModel.ctrl as ComboBox).SelectedItem;
                string name = (txtUnitName.ctrl as TextBox).Text;

                //선택된 Port없는지 검사
                if (port == null)
                {
                    return;
                }

                //사용중인 SlaveAddress 검색
                if (port.Units.ContainsKey(slaveAddr))
                {
                    return;
                }

                Unit unit = new Unit(port, slaveAddr, type, model, name);
                //Unit 등록
                port.Units.Add(slaveAddr, unit);

                //이하 Unit등록 후처리
                InitGroupBox();

                (gbPort.Controls[port.PortName] as RadioButton).Checked = true;
                (gbUnit.Controls[port.PortName + unit.SlaveAddr] as RadioButton).Checked = true;

                //메인폼 Tree 재지정
                mainForm.InitTreeItem();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UnitDelete()
        {
            RadioButton rdo = GetSelectGroupBoxItem("Unit");
            Unit unit = rdo.Tag as Unit;

            //등록된 Unit 삭제
            mainForm.ports[unit.ParentPort.PortName].Units.Remove(unit.SlaveAddr);

            //GroupBox 조정
            InitGroupBox();

            (gbPort.Controls[unit.ParentPort.PortName] as RadioButton).Checked = true;

            mainForm.InitTreeItem();
        }

        private void UnitNew()
        {
            (numUnitAddress.ctrl as NumericUpDown).Value = 1;
            (cboUnitType.ctrl as ComboBox).SelectedIndex = 0;
            (cboUnitModel.ctrl as ComboBox).SelectedIndex = 0;
            (txtUnitName.ctrl as TextBox).Text = "";
        }

        #endregion Unit End

        /// <summary>
        /// GoupBox에 선택된 RadioButton 찾기
        /// </summary>
        /// <param name="type">Port, Unit 택 1</param>
        /// <returns>선택된 RadioButton</returns>
        private RadioButton GetSelectGroupBoxItem(string type)
        {
            GroupBox box = null;
            RadioButton rdoPort = null;

            //GroupBox 어떤건지 고르기
            if (type == "Port") box = gbPort;
            else if (type == "Unit") box = gbUnit;
            else return null;

            //해당 GroupBOx에서 검색
            foreach (Control ctrl in box.Controls)
            {
                rdoPort = ctrl as RadioButton;

                if (rdoPort.Checked)
                {
                    return rdoPort;
                }
            }

            return null;
        }
    }
}
