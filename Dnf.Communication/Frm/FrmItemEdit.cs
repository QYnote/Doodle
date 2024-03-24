using Dnf.Communication.Controls;
using Dnf.Communication.Data;
using Dnf.Utils.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Communication.Frm
{
    internal class FrmItemEdit : TabPage
    {
        MainForm mainForm;
        /// <summary>
        /// 신규, 수정, 삭제한 데이터 임시 저장용
        /// </summary>
        Dictionary<string, Port> imsiPorts = RuntimeData.Ports;
        bool EditingFlag = false;
        bool isEdit = false;

        #region Controls
        //저장 정보
        Panel pnlButton = new Panel();
        Button BtnSave = new Button();     //저장    
        Button BtnCancle = new Button();   //초기화

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
        Button BtnUnitDel;

        //Channel 정보
        Panel pnlCh;
        GroupBox gbCh;
        ucControlBox chkChEnable;

        Panel  pnlChBtn;
        Button BtnChNew;
        Button BtnChSave;
        Button BtnChDel;


        #endregion Controls End

        public FrmItemEdit(MainForm form)
        {
            this.mainForm = form;

            CreateInitial();
        }

        private void CreateInitial()
        {
            CreateChannelnfo();
            this.Controls.Add(CreateSplitLine(DockStyle.Left));
            CreateUnitInfo();
            this.Controls.Add(CreateSplitLine(DockStyle.Left));
            CreatePortInfo();
            this.Controls.Add(CreateSplitLine(DockStyle.Bottom));
            CreateEditInfo();

            InitGroupBox();
        }

        #region ControlInitialize
        
        private void CreateEditInfo()
        {
            pnlButton.Dock = DockStyle.Bottom;
            pnlButton.Size = new Size(pnlButton.Width, 30);

            BtnSave.Dock = DockStyle.Right;
            BtnSave.Size = new Size(100, BtnSave.Height);
            BtnSave.Text = RuntimeData.String("F1001");

            BtnCancle.Dock = DockStyle.Right;
            BtnCancle.Size = new Size(100, BtnCancle.Height);
            BtnCancle.Text = RuntimeData.String("F1003");

            pnlButton.Controls.Add(BtnSave);
            pnlButton.Controls.Add(BtnCancle);

            BtnSave.Click += SaveEnd;
            BtnCancle.Click += CancleEvent;

            BtnCancle.BringToFront();
            BtnSave.BringToFront();

            this.Controls.Add(pnlButton);
        }


        private void CreatePortInfo()
        {
            pnlPort = new Panel();
            pnlPort.Dock = DockStyle.Left;
            pnlPort.Size = new Size(300, pnlPort.Height);
            pnlPort.MinimumSize = new Size(300, pnlPort.Height);

            gbPort = new GroupBox();
            gbPort.Dock = DockStyle.Fill;
            gbPort.Size = new Size(gbPort.Width, 200);
            gbPort.Text = RuntimeData.String("F0200");

            pnlPort.Controls.Add(gbPort);

            cboPortList = new ucControlBox(CtrlType.ComboBox);      //연결된 포트
            cboProtocolType = new ucControlBox(CtrlType.ComboBox);  //통신방법 구분
            cboBaudRate = new ucControlBox(CtrlType.ComboBox);      //BaudRate
            numDataBits = new ucControlBox(CtrlType.NumbericUpDown);//Data Bits
            cboStopBit = new ucControlBox(CtrlType.ComboBox);       //StopBit
            cboParity = new ucControlBox(CtrlType.ComboBox);        //ParityBit

            //Control 명(Control Type - Item구분 - 담당Property)
            cboPortList.ctrl.Name = "Combo-Port-PortName";
            cboProtocolType.ctrl.Name = "Combo-Port-ProtocolType";
            cboBaudRate.ctrl.Name = "Combo-Port-BaudRate";
            numDataBits.ctrl.Name = "Numeric-Port-DataBits";
            cboStopBit.ctrl.Name = "Combo-Port-StopBits";
            cboParity.ctrl.Name = "Combo-Port-Parity";

            //Label 표기 Text
            cboPortList.LblText = RuntimeData.String("F0100");
            cboProtocolType.LblText = RuntimeData.String("F0101");
            cboBaudRate.LblText = RuntimeData.String("F0102");
            numDataBits.LblText = RuntimeData.String("F0103");
            cboStopBit.LblText = RuntimeData.String("F0104");
            cboParity.LblText = RuntimeData.String("F0105");

            //Items
            (cboPortList.ctrl as ComboBox).Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
            (cboProtocolType.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(uProtocolType)).OfType<object>().ToArray());
            (cboBaudRate.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(BaudRate)).OfType<object>().ToArray());     
            (cboStopBit.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(StopBits)).OfType<object>().ToArray());      
            (cboParity.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(Parity)).OfType<object>().ToArray());         

            //Defulat Value
            if ((cboPortList.ctrl as ComboBox).Items.Count > 0)
            {
                (cboPortList.ctrl as ComboBox).SelectedIndex = 0;
            }
            (cboProtocolType.ctrl as ComboBox).SelectedIndex = 0;
            (cboBaudRate.ctrl as ComboBox).SelectedIndex = 0;   
            (cboStopBit.ctrl as ComboBox).SelectedIndex = 0;    
            (cboParity.ctrl as ComboBox).SelectedIndex = 0;     

            (numDataBits.ctrl as NumericUpDown).Value = 8;      
            (numDataBits.ctrl as NumericUpDown).Maximum = 8;
            (numDataBits.ctrl as NumericUpDown).Minimum = 5;

            cboPortList.Dock = DockStyle.Bottom;
            cboProtocolType.Dock = DockStyle.Bottom;
            cboBaudRate.Dock = DockStyle.Bottom;
            numDataBits.Dock = DockStyle.Bottom;
            cboStopBit.Dock = DockStyle.Bottom;
            cboParity.Dock = DockStyle.Bottom;

            //Label Width
            int portLabelWidth = 100;
            cboPortList.LblWidth = portLabelWidth;
            cboProtocolType.LblWidth = portLabelWidth;
            cboBaudRate.LblWidth = portLabelWidth;
            numDataBits.LblWidth = portLabelWidth;
            cboStopBit.LblWidth = portLabelWidth;
            cboParity.LblWidth = portLabelWidth;

            pnlPort.Controls.Add(cboPortList);
            pnlPort.Controls.Add(cboProtocolType);
            pnlPort.Controls.Add(cboBaudRate);
            pnlPort.Controls.Add(numDataBits);
            pnlPort.Controls.Add(cboStopBit);
            pnlPort.Controls.Add(cboParity);

            (cboPortList.ctrl as ComboBox).SelectedIndexChanged += EditItem;
            (cboProtocolType.ctrl as ComboBox).SelectedIndexChanged += EditItem;
            (cboBaudRate.ctrl as ComboBox).SelectedIndexChanged += EditItem;
            (cboStopBit.ctrl as ComboBox).SelectedIndexChanged += EditItem;
            (cboParity.ctrl as ComboBox).SelectedIndexChanged += EditItem;
            (numDataBits.ctrl as NumericUpDown).ValueChanged += EditItem;

            //Active 버튼
            pnlPortBtn = new Panel();
            pnlPortBtn.Dock = DockStyle.Bottom;
            pnlPortBtn.Size = new Size(pnlPortBtn.Width, 30);

            BtnPortNew = new Button();
            BtnPortDel = new Button();

            BtnPortNew.Text = RuntimeData.String("F1000");
            BtnPortDel.Text = RuntimeData.String("F1002");

            BtnPortNew.Dock = DockStyle.Right;
            BtnPortDel.Dock = DockStyle.Right;

            BtnPortNew.Size = new Size(100, BtnPortNew.Height);
            BtnPortDel.Size = new Size(100, BtnPortDel.Height);

            pnlPortBtn.Controls.Add(BtnPortNew);
            pnlPortBtn.Controls.Add(BtnPortDel);
            pnlPort.Controls.Add(pnlPortBtn);

            BtnPortDel.Click += PortDelete;
            BtnPortNew.Click += PortNew;

            Label splitLine1 = CreateSplitLine(DockStyle.Bottom);
            pnlPort.Controls.Add(splitLine1);

            //Control 정렬
            pnlPortBtn.BringToFront();
            cboParity.BringToFront();
            cboStopBit.BringToFront();
            numDataBits.BringToFront();
            cboBaudRate.BringToFront();
            cboProtocolType.BringToFront();
            cboPortList.BringToFront();
            splitLine1.BringToFront();
            gbPort.BringToFront();

            this.Controls.Add(pnlPort);

        }

        private void CreateUnitInfo()
        {
            pnlUnit = new Panel();
            pnlUnit.Dock = DockStyle.Left;
            pnlUnit.Size = new Size(300, pnlUnit.Height);
            pnlUnit.MinimumSize = new Size(300, pnlUnit.Height);

            gbUnit = new GroupBox();
            gbUnit.Dock = DockStyle.Fill;
            gbUnit.Size = new Size(gbUnit.Width, 200);
            gbUnit.Text = RuntimeData.String("F0400");

            pnlUnit.Controls.Add(gbUnit);

            numUnitAddress = new ucControlBox(CtrlType.NumbericUpDown);
            cboUnitType = new ucControlBox(CtrlType.ComboBox);
            cboUnitModel = new ucControlBox(CtrlType.ComboBox);
            txtUnitName = new ucControlBox(CtrlType.TextBox);

            //Control 명(Control Type - Item구분 - 담당Property)
            numUnitAddress.ctrl.Name = "Numeric-Unit-SlaveAddr";
            cboUnitType   .ctrl.Name = "Combo-Unit-UnitType";
            cboUnitModel  .ctrl.Name = "Combo-Unit-UnitModel";
            txtUnitName   .ctrl.Name = "Txt-Unit-UnitName";

            //Label 표기 Text
            numUnitAddress.LblText = RuntimeData.String("F0300");
            cboUnitType.LblText = RuntimeData.String("F0301");
            cboUnitModel.LblText = RuntimeData.String("F0302");
            txtUnitName.LblText = RuntimeData.String("F0303");

            //Items
            (cboUnitType.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(UnitType)).OfType<object>().ToArray());
            (cboUnitModel.ctrl as ComboBox).Items.AddRange(Enum.GetValues(typeof(UnitModel)).OfType<object>().ToArray());
            (txtUnitName.ctrl as TextBox).Text = Convert.ToString((cboUnitModel.ctrl as ComboBox).Items[0]);

            //Defulat Value
            (cboUnitType.ctrl as ComboBox).SelectedIndex = 0;
            (cboUnitModel.ctrl as ComboBox).SelectedIndex = 0;

            (numUnitAddress.ctrl as NumericUpDown).Value = 1;
            (numUnitAddress.ctrl as NumericUpDown).Maximum = 128;
            (numUnitAddress.ctrl as NumericUpDown).Minimum = 1;

            numUnitAddress.Dock = DockStyle.Bottom;
            cboUnitType.Dock = DockStyle.Bottom;
            cboUnitModel.Dock = DockStyle.Bottom;
            txtUnitName.Dock = DockStyle.Bottom;

            //Label Width
            int unitLabelWIdth = 100;
            numUnitAddress.LblWidth = unitLabelWIdth;
            cboUnitType.LblWidth = unitLabelWIdth;
            cboUnitModel.LblWidth = unitLabelWIdth;
            txtUnitName.LblWidth = unitLabelWIdth;

            pnlUnit.Controls.Add(numUnitAddress);
            pnlUnit.Controls.Add(cboUnitType);
            pnlUnit.Controls.Add(cboUnitModel);
            pnlUnit.Controls.Add(txtUnitName);

            (cboUnitType.ctrl as ComboBox).SelectedIndexChanged += EditItem;
            (cboUnitModel.ctrl as ComboBox).SelectedIndexChanged += EditItem;
            (numUnitAddress.ctrl as NumericUpDown).ValueChanged += EditItem;
            (txtUnitName.ctrl as TextBox).TextChanged += EditItem;

            //Active 버튼
            pnlUnitBtn = new Panel();
            pnlUnitBtn.Size = new Size(pnlUnitBtn.Width, 30);

            BtnUnitNew = new Button();
            BtnUnitDel = new Button();

            BtnUnitNew.Text = RuntimeData.String("F1000");
            BtnUnitDel.Text = RuntimeData.String("F1002");

            pnlUnitBtn.Dock = DockStyle.Bottom;
            BtnUnitNew.Dock = DockStyle.Right;
            BtnUnitDel.Dock = DockStyle.Right;

            BtnUnitNew.Size = new Size(100, BtnUnitNew.Height);
            BtnUnitDel.Size = new Size(100, BtnUnitDel.Height);

            pnlUnitBtn.Controls.Add(BtnUnitNew);
            pnlUnitBtn.Controls.Add(BtnUnitDel);
            pnlUnit.Controls.Add(pnlUnitBtn);

            BtnUnitDel.Click += UnitDelete;
            BtnUnitNew.Click += UnitNew;

            Label splitLine1 = CreateSplitLine(DockStyle.Bottom);
            pnlUnit.Controls.Add(splitLine1);

            pnlUnitBtn.BringToFront();
            txtUnitName.BringToFront();
            cboUnitModel.BringToFront();
            cboUnitType.BringToFront();
            numUnitAddress.BringToFront();
            splitLine1.BringToFront();
            gbUnit.BringToFront();

            this.Controls.Add(pnlUnit);
        }

        private void CreateChannelnfo()
        {
            pnlCh = new Panel();
            pnlCh.Dock = DockStyle.Left;
            pnlCh.Size = new Size(300, pnlCh.Height);
            pnlCh.MinimumSize = new Size(300, pnlCh.Height);

            gbCh = new GroupBox();
            gbCh.Dock = DockStyle.Fill;
            gbCh.Size = new Size(gbCh.Width, 200);
            gbCh.Text = RuntimeData.String("F0700");

            int chLabelWIdth = 100;

            //사용여부
            chkChEnable = new ucControlBox(CtrlType.CheckBox);
            chkChEnable.Dock = DockStyle.Bottom;
            chkChEnable.LblWidth = chLabelWIdth;
            chkChEnable.LblText = RuntimeData.String("F0701");

            //Active 버튼
            pnlChBtn = new Panel();
            pnlChBtn.Dock = DockStyle.Bottom;
            pnlChBtn.Size = new Size(pnlChBtn.Width, 30);

            BtnChNew = new Button();
            BtnChNew.Dock = DockStyle.Right;
            BtnChNew.Size = new Size(100, BtnChNew.Height);
            BtnChNew.Text = RuntimeData.String("F1000");

            BtnChSave = new Button();
            BtnChSave.Dock = DockStyle.Right;
            BtnChSave.Size = new Size(100, BtnChSave.Height);
            BtnChSave.Text = RuntimeData.String("F1001");

            BtnChDel = new Button();
            BtnChDel.Dock = DockStyle.Right;
            BtnChDel.Size = new Size(100, BtnChDel.Height);
            BtnChDel.Text = RuntimeData.String("F1002");

            pnlCh.Controls.Add(gbCh);
            pnlCh.Controls.Add(CreateSplitLine(DockStyle.Bottom));
            pnlCh.Controls.Add(chkChEnable);
            pnlCh.Controls.Add(pnlChBtn);

            pnlChBtn.Controls.Add(BtnChNew);
            pnlChBtn.Controls.Add(BtnChSave);
            pnlChBtn.Controls.Add(BtnChDel);

            this.Controls.Add(pnlCh);
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
            foreach (Port port in imsiPorts.Values)
            {
                foreach (Unit unit in port.Units.Values)
                {
                    RadioButton rdoUnit = new RadioButton();
                    rdoUnit.Name = port.PortName + unit.SlaveAddr.ToString("D2");  //GroupBox Controls 탐색방식
                    rdoUnit.Text = unit.UnitModelUserName;
                    rdoUnit.Dock = DockStyle.Top;
                    rdoUnit.Visible = true;
                    rdoUnit.Tag = unit;

                    rdoUnit.CheckedChanged += (sender, e) => { RdoCheckedChanged(rdoUnit); };

                    gbUnit.Controls.Add(rdoUnit);
                }

                RadioButton rdoPort = new RadioButton();
                rdoPort.Name = port.PortName;
                rdoPort.Text = port.PortName;
                rdoPort.Dock = DockStyle.Top;
                rdoPort.Visible = true;
                rdoPort.Tag = port;

                rdoPort.CheckedChanged += (sender, e) => { RdoCheckedChanged(rdoPort); };

                gbPort.Controls.Add(rdoPort);
            }

            //Item 정렬
            SortGroupBox();
        }

        /// <summary>
        /// GroupBox 정렬
        /// </summary>
        private void SortGroupBox()
        {
            //Port Sort
            foreach (var dicPairPort in imsiPorts.OrderBy(x => x.Key))
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

        #endregion ControlInitialize End

        #region Port
        /// <summary>
        /// Port 신규 생성
        /// </summary>
        private void PortNew(object sender, EventArgs e)
        {
            try
            {
                string portName = "Empty Port";

                if (imsiPorts.ContainsKey(portName))
                {
                    //중복 포트 검사
                    return;
                }

                //신규생성
                Port port = null;
                uProtocolType getProtocolType = (uProtocolType)(cboProtocolType.ctrl as ComboBox).SelectedItem;

                //통신 규칙 구분
                if (getProtocolType == uProtocolType.ModBusRTU
                     || getProtocolType == uProtocolType.ModBusAscii)
                {
                    BaudRate baudRate = (BaudRate)(cboBaudRate.ctrl as ComboBox).SelectedItem;
                    int dataBits = Convert.ToInt32((numDataBits.ctrl as NumericUpDown).Value);
                    Parity pairty = (Parity)(cboParity.ctrl as ComboBox).SelectedItem;
                    StopBits stopBits = (StopBits)(cboStopBit.ctrl as ComboBox).SelectedItem;

                    port = new Port(portName, getProtocolType, baudRate, dataBits, pairty, stopBits);

                    imsiPorts.Add(portName, port);
                }

                //후처리
                if (port != null)
                {
                    InitGroupBox(); //GroupBox 재생성

                    (gbPort.Controls[port.PortName] as RadioButton).Checked = true; //Check 유지

                    
                }
            }
            catch(Exception ex)
            { 
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Port 삭제
        /// </summary>
        private void PortDelete(object sender, EventArgs e)
        {
            //선택중인 RadioButton 가져오기
            RadioButton rdo = GetSelectGroupBoxItem("Port");

            if (rdo != null)
            {
                Port port = rdo.Tag as Port;

                //등록된 Port 삭제
                imsiPorts.Remove(port.PortName);

                //삭제 후처리
                //GroupBox 조정
                InitGroupBox();
            }
        }

        #endregion Port End

        #region Unit

        private void UnitNew(object sender, EventArgs e)
        {
            //선택된 Port에 Unit 추가
            try
            {
                //선택된 Port없는지 검사
                Port port = GetSelectGroupBoxItem("Port").Tag as Port;
                if (port == null)
                {
                    return;
                }

                //사용중인 SlaveAddress 검색
                int slaveAddr = Convert.ToInt32((numUnitAddress.ctrl as NumericUpDown).Maximum);

                if (port.Units.ContainsKey(slaveAddr))
                {
                    return;
                }

                //신규 생성
                UnitType type = (UnitType)(cboUnitType.ctrl as ComboBox).SelectedItem;
                UnitModel model = (UnitModel)(cboUnitModel.ctrl as ComboBox).SelectedItem;
                string name = "Empty Unit";

                Unit unit = new Unit(port, slaveAddr, type, model, name);
                //Unit 등록
                port.Units.Add(slaveAddr, unit);

                //이하 Unit등록 후처리
                InitGroupBox();

                (gbPort.Controls[port.PortName] as RadioButton).Checked = true;
                (gbUnit.Controls[port.PortName + unit.SlaveAddr.ToString("D2")] as RadioButton).Checked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UnitDelete(object sender, EventArgs e)
        {
            RadioButton rdo = GetSelectGroupBoxItem("Unit");
            if (rdo != null)
            {
                Unit unit = rdo.Tag as Unit;

                //등록된 Unit 삭제
                imsiPorts[unit.ParentPort.PortName].Units.Remove(unit.SlaveAddr);

                //GroupBox 조정
                InitGroupBox();

                (gbPort.Controls[unit.ParentPort.PortName] as RadioButton).Checked = true;
            }
        }

        #endregion Unit End

        /// <summary>
        /// 정보 Control 데이터 수정 이벤트
        /// </summary>
        private void EditItem(object sender, EventArgs e)
        {
            //Radio 변경으로인한 Item 변경 Flag 확인
            if (!EditingFlag)
            {
                string ctrlName = (sender as Control).Name;
                string[] obj = ctrlName.Split('-'); //Name 부여 상단 규칙 참고

                RadioButton rdo = GetSelectGroupBoxItem(obj[1]);

                if (rdo != null)
                {
                    isEdit = true;  //화면 연 후 변경내역 확인용 - 저장, 초기화에 사용

                    //Item 구분
                    if (obj[1] == "Port")
                    {
                        Port port = rdo.Tag as Port;

                        //Property 구분
                        switch (obj[2])
                        {
                            case "PortName":
                                string afName = (cboPortList.ctrl as ComboBox).SelectedItem.ToString();
                                if (!Dnf.Utils.Controls.UtilCustom.DictKeyChange(RuntimeData.Ports, port.PortName, afName))
                                {
                                    MessageBox.Show(RuntimeData.String("A017"));
                                    return;
                                }
                                imsiPorts[afName].PortName = afName;

                                //후처리
                                InitGroupBox(); //GroupBox 재생성

                                (gbPort.Controls[afName] as RadioButton).Checked = true; //Check 유지
                                break;
                            case "ProtocolType": port.ProtocolType = (uProtocolType)(cboProtocolType.ctrl as ComboBox).SelectedItem; break;
                            case "BaudRate": port.BaudRate = (BaudRate)(cboBaudRate.ctrl as ComboBox).SelectedItem; break;
                            case "DataBits": port.DataBits = Convert.ToInt32((numDataBits.ctrl as NumericUpDown).Value); break;
                            case "StopBits": port.StopBIt = (StopBits)(cboStopBit.ctrl as ComboBox).SelectedItem; break;
                            case "Parity": port.Parity = (Parity)(cboParity.ctrl as ComboBox).SelectedItem; break;
                        }
                    }
                    else if (obj[1] == "Unit")
                    {
                        Unit unit = rdo.Tag as Unit;

                        //Property 구분
                        switch (obj[2])
                        {
                            case "SlaveAddr":
                                int afAddr = Convert.ToInt32((numUnitAddress.ctrl as NumericUpDown).Value);
                                if (!Dnf.Utils.Controls.UtilCustom.DictKeyChange(unit.ParentPort.Units, unit.SlaveAddr, afAddr))
                                {
                                    MessageBox.Show(RuntimeData.String("A012"));
                                    return;
                                }
                                imsiPorts[unit.ParentPort.PortName].Units[afAddr].SlaveAddr = afAddr;
                                imsiPorts[unit.ParentPort.PortName].Units[afAddr].UnitModelUserName = (txtUnitName.ctrl as TextBox).Text;

                                //후처리
                                InitGroupBox();

                                (gbPort.Controls[unit.ParentPort.PortName] as RadioButton).Checked = true;
                                (gbUnit.Controls[unit.ParentPort.PortName + afAddr.ToString("D2")] as RadioButton).Checked = true;
                                break;
                            case "UnitType": unit.UnitModelType = (UnitType)(cboUnitType.ctrl as ComboBox).SelectedItem; break;
                            case "UnitModel":
                                unit.UnitModelName = (UnitModel)(cboUnitModel.ctrl as ComboBox).SelectedItem;
                                //Model에 따른 Channel 변경
                                break;
                            case "UnitName":
                                unit.UnitModelUserName = (txtUnitName.ctrl as TextBox).Text;

                                InitGroupBox();

                                (gbPort.Controls[unit.ParentPort.PortName] as RadioButton).Checked = true;
                                (gbUnit.Controls[unit.ParentPort.PortName + unit.SlaveAddr.ToString("D2")] as RadioButton).Checked = true;
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GroupBox에서 선택된 RadioButton 변경 이벤트
        /// </summary>
        /// <param name="rdo">변경된 RadioButton</param>
        private void RdoCheckedChanged(RadioButton rdo)
        {
            //신규생성 상태에서 Item Radio 선택 시 신규선택 상태 해제
            if (rdo.Checked)
            {
                string rdoType = rdo.Tag.GetType().Name;

                //GroupBox Visible 조정
                if (rdoType == "Port")
                {
                    Port port = rdo.Tag as Port;

                    if (port != null)
                    {
                        EditingFlag = true;

                        //선택된 Radio값 지정
                        (cboPortList.ctrl as ComboBox).SelectedItem = port.PortName;
                        (cboProtocolType.ctrl as ComboBox).SelectedItem = port.ProtocolType;
                        (cboBaudRate.ctrl as ComboBox).SelectedItem = port.BaudRate;
                        (numDataBits.ctrl as NumericUpDown).Value = port.DataBits;
                        (cboStopBit.ctrl as ComboBox).SelectedItem = port.StopBIt;
                        (cboParity.ctrl as ComboBox).SelectedItem = port.Parity;

                        //Unit Visible 조정
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

                        //Channel Visible 조정

                        EditingFlag = false;
                    }
                }
                else if (rdoType == "Unit")
                {
                    Unit unit = rdo.Tag as Unit;

                    if (unit != null)
                    {
                        EditingFlag = true;

                        //선택된 Radio값 수정
                        (numUnitAddress.ctrl as NumericUpDown).Value = unit.SlaveAddr;
                        (cboUnitType.ctrl as ComboBox).SelectedItem = unit.UnitModelType;
                        (cboUnitModel.ctrl as ComboBox).SelectedItem = unit.UnitModelName;
                        (txtUnitName.ctrl as TextBox).Text = unit.UnitModelUserName;

                        //UnitModel에따른 Channel 설정

                        EditingFlag = false;
                    }
                }
            }
        }


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

        private void SaveEnd(object sender, EventArgs e)
        {
            if (isEdit)
            {
                RuntimeData.Ports = imsiPorts;
                mainForm.InitTreeItem();    //MainForm Tree 재구성

                MessageBox.Show(RuntimeData.String("A018"));
            }
        }

        private void CancleEvent(object sender, EventArgs e)
        {
            if (isEdit)
            {
                //취소 시 기존 데이터로 복구
                if(MessageBox.Show(RuntimeData.String("A019"), "", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    imsiPorts = RuntimeData.Ports;
                }
            }
        }
    }
}
