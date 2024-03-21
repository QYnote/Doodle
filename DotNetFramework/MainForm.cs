using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Dnf.Communication;
using Dnf.Utils;
using DotNetFramework.Communication;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace DotNetFramework
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 생성된 Port List Dic(Port명, Port)
        /// </summary>
        public Dictionary<string, Port> ports = new Dictionary<string, Port>();
        /// <summary>
        /// GroupBox에서 선택된 Port
        /// </summary>
        Port SelectedPort;

        MdiClient MdiClient;

        #region Control 모음
        MenuStrip TextMenu = new MenuStrip();   //상단 글자메뉴
        ToolStrip IconMenu = new ToolStrip();   //상단 아이콘 메뉴
        TabControl TabCtrl;                     //Tab Page(주 화면)
        Label lblStatus;    //상태

        Panel pnlList;      //생성된 정보들모음
        TreeView Tree;      //등록된 Port-Unit Tree
        DataGridView gv;    //Tree에서 선택됨 Item 정보
        #endregion Control 모음 End

        Button btnSendData;

        public MainForm()
        {
            InitializeComponent();
            InitControl();
        }

        private void InitControl()
        {
            //버튼작동 상태
            lblStatus = new Label();
            lblStatus.Size = new Size(lblStatus.Width, 60);
            lblStatus.Dock = DockStyle.Top;
            lblStatus.Font = new Font(lblStatus.Font.FontFamily, (float)14.0, FontStyle.Bold);
            lblStatus.Text = "None Action";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;


            TabCtrl = new TabControl();
            TabCtrl.Dock = DockStyle.Fill;

            //Control Add
            //this.Controls.Add(TabCtrl);
            this.IsMdiContainer = true;
            this.Controls.Add(MdiClient);
            //IsMdiContainer = true;
            CreateControl_Info();
            this.Controls.Add(lblStatus);
            CreateControl_Base();

            this.Size = new Size(1400, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void CreateControl_Base()
        {
            //Text 메뉴
            //파일
            ToolStripMenuItem tmFile = new ToolStripMenuItem() { Text = Data_Runtime.String("F0600") }; //파일
            ToolStripMenuItem tmFileXmlSave = new ToolStripMenuItem() { Name = "XmlSave", Text = Data_Runtime.String("F0000") };//XML 저장
            ToolStripMenuItem tmFileXmlLoad = new ToolStripMenuItem() { Name = "XmlLoad", Text = Data_Runtime.String("F0001") };//XML 불러오기
            tmFile.DropDownItems.AddRange(new ToolStripItem[] { tmFileXmlSave, tmFileXmlLoad });
            //통신
            ToolStripMenuItem tmComm = new ToolStripMenuItem() { Text = Data_Runtime.String("F0601") }; //통신
            ToolStripMenuItem tmCommCre   = new ToolStripMenuItem() { Name = "PortCre" , Text = Data_Runtime.String("F0204") };//Model 생성
            ToolStripMenuItem tmCommOpen  = new ToolStripMenuItem() { Name = "PortOpen" , Text = Data_Runtime.String("F0201") };//Port 열기
            ToolStripMenuItem tmCommClose = new ToolStripMenuItem() { Name = "PortClose", Text = Data_Runtime.String("F0202") };//Port 닫기
            
            ToolStripMenuItem test = new ToolStripMenuItem() { Text = "Test" };
            tmComm.DropDownItems.AddRange(new ToolStripItem[] { tmCommCre, tmCommOpen, tmCommClose });

            TextMenu.Items.AddRange(new ToolStripItem[] { tmFile, tmComm, test });

            //Icon 메뉴
            //XML 저장
            ToolStripButton imFileXmlSave = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image };
            imFileXmlSave.Image = Dnf.Utils.Properties.Resources.UpLoad_32x32;
            //XML 불러오기
            ToolStripButton imFileXmlLoad = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image };
            imFileXmlLoad.Image = Dnf.Utils.Properties.Resources.DownLoad_32x32;
            //Port, Unit 생성
            ToolStripButton imCommCre = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image };
            imCommCre.Image = Dnf.Utils.Properties.Resources.Plus_00_32x32;
            //포트열기
            ToolStripButton imCommOpen = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image };
            imCommOpen.Image = Dnf.Utils.Properties.Resources.Play_00_32x32;
            //포트닫기
            ToolStripButton imCommClose = new ToolStripButton() { DisplayStyle = ToolStripItemDisplayStyle.Image };
            imCommClose.Image = Dnf.Utils.Properties.Resources.Stop_00_32x32;

            IconMenu.ImageScalingSize = new Size(32,32);
            IconMenu.Items.AddRange(new ToolStripItem[] { 
                //파일
                imFileXmlSave, imFileXmlLoad, 
                new ToolStripSeparator(),
                //통신
                imCommCre, imCommOpen, imCommClose,
                new ToolStripSeparator(),
            });

            //

            this.Controls.Add(IconMenu);
            this.Controls.Add(TextMenu);

            tmCommCre    .Click += (sender, e) => { CreateControl_TabPage(Data_Runtime.String("F0204")); };
            tmFileXmlSave.Click += (sender, e) => { InfoSave();  };
            tmFileXmlLoad.Click += (sender, e) => { InfoLoad();  };
            imFileXmlSave.Click += (sender, e) => { InfoSave();  };
            imFileXmlLoad.Click += (sender, e) => { InfoLoad();  };
            imCommCre    .Click += (sender, e) => { CreateControl_TabPage(Data_Runtime.String("F0204")); };
            imCommOpen   .Click += (sender, e) => { PortOpen();  };
            imCommClose  .Click += (sender, e) => { PortClose(); };

            test  .Click += (sender, e) => {};
        }

        /// <summary>
        /// TabPage 생성
        /// </summary>
        /// <param name="pageName">TabPage 명칭</param>
        private void CreateControl_TabPage(string pageName)
        {
            TabPage page;
            //TabPgae명 검색
            int tabIdx = TabCtrl.TabPages.IndexOfKey(pageName);

            if (tabIdx == -1)
            {
                //TapPage 신규 생성
                page = new FrmItemEdit(this);
                page.Padding = new Padding(3);
                page.UseVisualStyleBackColor = true;
                page.Name = pageName;
                page.Text = pageName;

                TabCtrl.TabPages.Add(page);
                page.Focus();
            }
            else
            {
                //해당 Tab 이동
                page = TabCtrl.TabPages[tabIdx];
                page.Focus();
            }
        }

        /// <summary>
        /// Port, Unit 정보창 Panel
        /// </summary>
        private void CreateControl_Info()
        {
            //정보 Panel
            pnlList = new Panel();
            pnlList.Dock = DockStyle.Left;
            pnlList.Size = new Size(200, pnlList.Height);

            //생성된 Port, Unit Tree
            Tree = new TreeView();
            Tree.Dock = DockStyle.Fill;
            Tree.Size = new Size(Tree.Width, 100);
            Tree.MinimumSize = new Size(200, 100);
            Tree.Nodes.Add("Program Computer");

            //선택한 Port, Unit 정보 Grid
            gv = new DataGridView();
            gv.Dock = DockStyle.Bottom;
            gv.AutoGenerateColumns = false;
            gv.Size = new Size(gv.Width, 200);
            gv.MinimumSize = new Size(200, 200);
            gv.AutoGenerateColumns = false;     //DataTable에 따른 Column 자동생성 숨기기
            gv.AllowUserToAddRows = false;      //Runtime 중 유저가 Row 추가하는 공간 숨기기
            gv.AllowUserToResizeRows = false;   //유저가 Row높이 수정 막기
            gv.RowHeadersVisible = false;       //현재 Row 화살표 숨기기

            //속성 Column
            DataGridViewColumn colName = new DataGridViewTextBoxColumn();
            colName.HeaderText = Data_Runtime.String("F0500");
            colName.DataPropertyName = "P";
            colName.Width = 90;
            colName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colName.ReadOnly = true;
            colName.SortMode = DataGridViewColumnSortMode.NotSortable;  //정렬 불가능
            colName.Resizable = DataGridViewTriState.False;             //너비조절 불가능

            //Value Column
            DataGridViewColumn colValue = new DataGridViewTextBoxColumn();
            colValue.HeaderText = Data_Runtime.String("F0501");
            colValue.DataPropertyName = "V";
            colValue.Width = pnlList.Width - colName.Width - 3;
            colValue.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colValue.ReadOnly = true;
            colValue.SortMode = DataGridViewColumnSortMode.NotSortable; //정렬 불가능
            colValue.Resizable = DataGridViewTriState.False;            //너비조절 불가능

            gv.Columns.Add(colName);
            gv.Columns.Add(colValue);

            pnlList.Controls.Add(Tree);
            pnlList.Controls.Add(gv);
            this.Controls.Add(pnlList);

            Tree.AfterSelect += (sender, e) => { InitItemInfo(Tree.SelectedNode); };
        }

        /// <summary>
        /// Main Form Tree 재지정
        /// </summary>
        internal void InitTreeItem()
        {
            Tree.Nodes[0].Nodes.Clear();    //Program Computer Node 하위항목 삭제

            foreach (Port port in this.ports.Values)
            {
                TreeNode portNode = new TreeNode();
                portNode.Name = port.PortName;
                portNode.Text = port.PortName;
                portNode.Tag = port;

                foreach (Unit unit in port.Units.Values)
                {
                    TreeNode unitNode = new TreeNode();
                    unitNode.Name = unit.UnitModelUserName + unit.SlaveAddr;
                    unitNode.Text = unit.UnitModelUserName;
                    unitNode.Tag = unit;

                    portNode.Nodes.Add(unitNode);
                }

                Tree.Nodes[0].Nodes.Add(portNode);
            }

            Tree.ExpandAll();
        }

        /// <summary>
        /// Tree에서 Item 선택 시 정보창
        /// </summary>
        private void InitItemInfo(TreeNode node)
        {
            Type tagType = node.Tag?.GetType();

            if(tagType == typeof(Port))
            {
                //Port Node 선택 시
                SetPortInfoTable(node);
            }
            else if(tagType == typeof(Unit))
            {
                //Unit Node 선택 시
                SetUnitInfotable(node);
            }
            else
            {
                //지정된 Tag 없거나 미사용일 시
                gv.DataSource = null;   
            }
        }

        private void SetPortInfoTable(TreeNode node)
        {
            DataTable dt = gv.DataSource == null ? new DataTable() : gv.DataSource as DataTable;
            Port port = node.Tag as Port;

            if (dt.TableName != "Port")
            {
                //Port Table이 아니였을 시 데이터 수정
                dt.TableName = "Port";
                dt.Columns.Clear();
                dt.Columns.Add("P", typeof(string));
                dt.Columns.Add("V", typeof(object));

                dt.Rows.Clear();
                dt.Rows.Add(new object[] { Data_Runtime.String("F0100"), null });
                dt.Rows.Add(new object[] { Data_Runtime.String("F0101"), null });
                dt.Rows.Add(new object[] { Data_Runtime.String("F0102"), null });
                dt.Rows.Add(new object[] { Data_Runtime.String("F0103"), null });
                dt.Rows.Add(new object[] { Data_Runtime.String("F0104"), null });
                dt.Rows.Add(new object[] { Data_Runtime.String("F0105"), null });
            }

            dt.Rows[0][1] = port.PortName;
            dt.Rows[1][1] = port.ProtocolType;
            dt.Rows[2][1] = port.BaudRate;
            dt.Rows[3][1] = port.DataBits;
            dt.Rows[4][1] = port.StopBIt;
            dt.Rows[5][1] = port.Parity;

            gv.DataSource = dt; 
        }

        private void SetUnitInfotable(TreeNode node)
        {
            DataTable dt = gv.DataSource == null ? new DataTable() : gv.DataSource as DataTable;
            Unit unit = node.Tag as Unit;

            if (dt.TableName != "Unit")
            {
                //Port Table이 아니였을 시 데이터 수정
                dt.TableName = "Unit";
                dt.Columns.Clear();
                dt.Columns.Add("P", typeof(string));
                dt.Columns.Add("V", typeof(object));

                dt.Rows.Clear();
                dt.Rows.Add(new object[] { Data_Runtime.String("F0300"), null });   //Slave Addr
                dt.Rows.Add(new object[] { Data_Runtime.String("F0301"), null });   //모델 구분
                dt.Rows.Add(new object[] { Data_Runtime.String("F0302"), null });   //모델
                dt.Rows.Add(new object[] { Data_Runtime.String("F0303"), null });   //모델명(사용자지정)
            }

            dt.Rows[0][1] = unit.SlaveAddr;
            dt.Rows[1][1] = unit.UnitModelType;
            dt.Rows[2][1] = unit.UnitModelName;
            dt.Rows[3][1] = unit.UnitModelUserName;

            gv.DataSource = dt;
        }

        #region Port Active

        /// <summary>
        /// GroupBox에서 선택된 Port 연결Open
        /// </summary>
        private void PortOpen()
        {
            if (SelectedPort == null)
            {
                lblStatus.Text = Data_Runtime.String("A002");
                return;
            }

            //포트열기
            if (SelectedPort.PortOpen())
            {
                lblStatus.Text = Data_Runtime.String("A007");
            }
            else
            {
                lblStatus.Text = Data_Runtime.String("A008");
            }
        }

        /// <summary>
        /// GroupBox에서 선택된 Port 연결Close
        /// </summary>
        private void PortClose()
        {
            if (SelectedPort == null)
            {
                lblStatus.Text = Data_Runtime.String("A002");
                return;
            }

            //포트열기
            if (SelectedPort.PortClose())
            {
                lblStatus.Text = Data_Runtime.String("A009");
            }
            else
            {
                lblStatus.Text = Data_Runtime.String("A010");
            }
        }


        /// <summary>
        /// GroupBox에서 선택된 Port에 데이터 전송
        /// </summary>
        private void SendData()
        {
            if (SelectedPort == null)
            {
                lblStatus.Text = Data_Runtime.String("A002");
                return;
            }

            SelectedPort.PortSend();
        }

        #endregion Port Active End

        #region XML

        private void InfoSave()
        {
            //Port 정보 저장
            if (ports == null || ports.Count == 0) return;

            XmlDocument xdoc = new XmlDocument();
            XmlNode rootNode = xdoc.CreateElement("Root");

            foreach (Port port in ports.Values)
            {
                XmlNode portNode = xdoc.CreateElement("Port");

                XmlAttribute attrPortName = xdoc.CreateAttribute("PortName");
                attrPortName.Value = port.PortName;

                XmlAttribute attrProtocol = xdoc.CreateAttribute("Protocol");
                attrProtocol.Value = ((int)port.ProtocolType).ToString();

                XmlAttribute attrBaudRate = xdoc.CreateAttribute("BaudRate");
                attrBaudRate.Value = ((int)port.BaudRate).ToString();

                XmlAttribute attrDataBits = xdoc.CreateAttribute("DataBits");
                attrDataBits.Value = port.DataBits.ToString();

                XmlAttribute attrParity= xdoc.CreateAttribute("Parity");
                attrParity.Value = ((int)port.Parity).ToString();

                XmlAttribute attrStopBIts = xdoc.CreateAttribute("StopBit");
                attrStopBIts.Value = ((int)port.StopBIt).ToString();

                portNode.Attributes.Append(attrPortName);
                portNode.Attributes.Append(attrProtocol);
                portNode.Attributes.Append(attrBaudRate);
                portNode.Attributes.Append(attrDataBits);
                portNode.Attributes.Append(attrParity);
                portNode.Attributes.Append(attrStopBIts);

                foreach (Unit unit in port.Units.Values)
                {
                    XmlNode unitNode = xdoc.CreateElement("Unit");

                    XmlAttribute attrAddr = xdoc.CreateAttribute("SlaveAddr");
                    attrAddr.Value = unit.SlaveAddr.ToString();

                    XmlAttribute attrType = xdoc.CreateAttribute("UnitType");
                    attrType.Value = ((int)unit.UnitModelType).ToString();

                    XmlAttribute attrModel = xdoc.CreateAttribute("UnitModel");
                    attrModel.Value = ((int)unit.UnitModelName).ToString();

                    XmlAttribute attrUserName = xdoc.CreateAttribute("UserName");
                    attrUserName.Value = unit.UnitModelUserName.ToString();

                    unitNode.Attributes.Append(attrAddr);
                    unitNode.Attributes.Append(attrType);
                    unitNode.Attributes.Append(attrModel);
                    unitNode.Attributes.Append(attrUserName);

                    portNode.AppendChild(unitNode);
                }

                rootNode.AppendChild(portNode);
            }

            xdoc.AppendChild(rootNode);

            xdoc.Save(string.Format("{0}\\{1}.xml", Data_Runtime.DataPath, "PortInfo"));

            lblStatus.Text = Data_Runtime.String("A014");
        }

        private void InfoLoad()
        {
            XmlDocument xdoc = new XmlDocument();
            try
            {
                string path = string.Format("{0}\\{1}.xml", Data_Runtime.DataPath, "PortInfo");

                if(!File.Exists(path)) { throw new Exception(Data_Runtime.String("A016")); }

                xdoc.Load(path);
                XmlElement root = xdoc.DocumentElement;


                foreach (XmlNode nodePort in root.ChildNodes)
                {
                    Port port = new Port(
                        nodePort.Attributes["PortName"].Value,
                        (ProtocolType)Enum.Parse(typeof(ProtocolType), nodePort.Attributes["Protocol"].Value),
                        (BaudRate)Enum.Parse(typeof(BaudRate), nodePort.Attributes["BaudRate"].Value),
                        Convert.ToInt16(nodePort.Attributes["DataBits"].Value),
                        (Parity)Enum.Parse(typeof(Parity), nodePort.Attributes["Parity"].Value),
                        (StopBits)Enum.Parse(typeof(StopBits), nodePort.Attributes["StopBit"].Value)
                        );

                    ports.Add(port.PortName, port);

                    foreach (XmlNode nodeUnit in nodePort.ChildNodes)
                    {
                        int addr = Convert.ToInt16(nodeUnit.Attributes["SlaveAddr"].Value);

                        Unit unit = new Unit(
                            port,
                            addr,
                            (UnitType)Enum.Parse(typeof(UnitType), nodeUnit.Attributes["UnitType"].Value),
                            (UnitModel)Enum.Parse(typeof(UnitModel), nodeUnit.Attributes["UnitModel"].Value),
                            nodeUnit.Attributes["UserName"].Value
                            );

                        port.Units.Add(addr, unit);
                    }
                }

                InitTreeItem();
                lblStatus.Text = Data_Runtime.String("A015");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion XML End
    }
}
