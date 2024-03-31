using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Dnf.Communication.Controls;
using Dnf.Communication.Data;
using Dnf.Communication.Frm;
using Dnf.Utils.Controls;
using Dnf.Utils.Views;

//Button Resource 사이트

namespace Dnf.Communication
{
    public partial class MainForm : Dnf.Utils.Views.FrmBase
    {
        #region Control 모음
        public MenuStrip TextMenu = new MenuStrip();   //상단 글자메뉴
        public ToolStrip IconMenu = new ToolStrip();   //상단 아이콘 메뉴
        private StatusStrip StatusBar = new StatusStrip();   //상태 바
        public ToolStripStatusLabel LblStatus = new ToolStripStatusLabel();
        public TabControl TabCtrl = new TabControl();  //Tab Page(주 화면)
        public Button BtnTabClose;                     //Tab Page 닫기 버튼

        public Panel pnlList;      //생성된 정보들모음
        public TreeView Tree;      //등록된 Port-Unit Tree
        private ImageList ImgList = new ImageList();
        public ContextMenuStrip TreeMenu = new ContextMenuStrip();  //Tree 우클릭 메뉴
        private ToolStripMenuItem menu_CreatePort = new ToolStripMenuItem();
        private ToolStripMenuItem menu_EditPort = new ToolStripMenuItem();
        private ToolStripMenuItem menu_CreateUnit = new ToolStripMenuItem();
        private ToolStripMenuItem menu_EditUnit = new ToolStripMenuItem();

        ToolStripSeparator MenuLine1 = new ToolStripSeparator();

        public DataGridView gv;    //Tree에서 선택됨 Item 정보

        private BackgroundWorker bgWorker;
        #endregion Control 모음 End

        private bool isAction = false;

        public MainForm()
        {
            InitializeComponent();
            InitControl();
        }

        private void InitControl()
        {
            CheckForIllegalCrossThreadCalls = false;
            this.FormClosed += FrmClosed;

            //Control Add
            InitializeControl_Base();
            InitializeControl_Info();
            Initialize_TreeMenu_RightClick();

            SortControl();
            SetImageList();
            InitializeBackGroundWorker();

            this.Text = "통신";
        }

        /// <summary>
        /// 메뉴, TabControl 생성
        /// </summary>
        private void InitializeControl_Base()
        {
            //Text 메뉴
            //파일
            ToolStripMenuItem tmFile = new ToolStripMenuItem() { Text = RuntimeData.String("F0600") }; //파일
            ToolStripMenuItem tmFileXmlSave = new ToolStripMenuItem() { Name = "TxtMenuXmlSave", Text = RuntimeData.String("F0000") };//XML 저장
            ToolStripMenuItem tmFileXmlLoad = new ToolStripMenuItem() { Name = "TxtMenuXmlLoad", Text = RuntimeData.String("F0001") };//XML 불러오기
            //통신
            ToolStripMenuItem tmComm = new ToolStripMenuItem() { Text = RuntimeData.String("F0601") }; //통신
            ToolStripMenuItem tmCommCre   = new ToolStripMenuItem() { Name = "TxtMenuItemCre"  , Text = RuntimeData.String("F0204") };//Model 생성
            ToolStripMenuItem tmCommOpen  = new ToolStripMenuItem() { Name = "TxtMenuPortOpen" , Text = RuntimeData.String("F0201") };//Port 열기
            ToolStripMenuItem tmCommClose = new ToolStripMenuItem() { Name = "TxtMenuPortClose", Text = RuntimeData.String("F0202") };//Port 닫기
            
            
            //Icon 메뉴
            IconMenu.ImageScalingSize = new Size(32, 32);

            ToolStripButton imFileXmlSave = new ToolStripButton() { Name = "IconMenuXmlSave"  , DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = RuntimeData.String("F0000") };//XML 저장
            ToolStripButton imFileXmlLoad = new ToolStripButton() { Name = "IconMenuXmlLoad"  , DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = RuntimeData.String("F0001") };//XML 불러오기
            ToolStripButton imCommCre     = new ToolStripButton() { Name = "IconMenuItemCre"  , DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = RuntimeData.String("F0204") };//Port, Unit 생성
            ToolStripButton imCommOpen    = new ToolStripButton() { Name = "IconMenuPortOpen" , DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = RuntimeData.String("F0201") };//포트열기
            ToolStripButton imCommClose   = new ToolStripButton() { Name = "IconMenuPortClose", DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = RuntimeData.String("F0202") };//포트닫기
            ToolStripButton test          = new ToolStripButton() { Name = "IconMenuTest"     , DisplayStyle = ToolStripItemDisplayStyle.Image, ToolTipText = "테스트" }; //테스트

            imFileXmlSave.Image = Dnf.Utils.Properties.Resources.UpLoad_32x32;
            imFileXmlLoad.Image = Dnf.Utils.Properties.Resources.DownLoad_32x32;
            imCommCre.Image = Dnf.Utils.Properties.Resources.Plus_00_32x32;
            imCommOpen.Image = Dnf.Utils.Properties.Resources.Play_00_32x32;
            imCommClose.Image = Dnf.Utils.Properties.Resources.Stop_00_32x32;
            test.Image = Dnf.Utils.Properties.Resources.Test_32x32;

            tmFileXmlSave.Click += ActMethod;
            tmFileXmlLoad.Click += ActMethod;
            tmCommCre.Click += ActMethod;
            tmCommOpen.Click += ActMethod;
            tmCommClose.Click += ActMethod;
            imFileXmlSave.Click += ActMethod;
            imFileXmlLoad.Click += ActMethod;
            imCommCre.Click += ActMethod;
            imCommOpen.Click += ActMethod;
            imCommClose.Click += ActMethod;
            test.Click += ActMethod;

            tmFile.DropDownItems.AddRange(new ToolStripItem[] { tmFileXmlSave, tmFileXmlLoad });
            tmComm.DropDownItems.AddRange(new ToolStripItem[] { tmCommCre, tmCommOpen, tmCommClose });
            TextMenu.Items.AddRange(new ToolStripItem[] { tmFile, tmComm });
            IconMenu.Items.AddRange(new ToolStripItem[] { 
                //파일
                imFileXmlSave, imFileXmlLoad, 
                new ToolStripSeparator(),
                //통신
                imCommCre, imCommOpen, imCommClose,
                new ToolStripSeparator(),
                test
            });

            //상태바
            StatusBar.Dock = DockStyle.Bottom;
            LblStatus.Text = "Status";
            StatusBar.Items.Add(LblStatus);

            //Tab Control
            TabCtrl.Dock = DockStyle.Fill;

            //Tab 닫기 Button
            BtnTabClose = new Button();
            BtnTabClose.Size = new Size(20, 20);
            BtnTabClose.Image = Dnf.Utils.Properties.Resources.Close_16x16;
            BtnTabClose.Location = new Point(this.Size.Width - BtnTabClose.Width - 17,  //너비 - 17 : 보정값
                TextMenu.Height + IconMenu.Height + BtnTabClose.Height - 6);            //높이 - 6  : 보정값
            BtnTabClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            BtnTabClose.Visible = false;

            this.Controls.Add(IconMenu);
            this.Controls.Add(TextMenu);
            this.Controls.Add(StatusBar);
            this.Controls.Add(TabCtrl);
            this.Controls.Add(BtnTabClose);

            BtnTabClose  .Click += (sender, e) => { RemoveTabPage(TabCtrl.SelectedTab.Name); };
            TabCtrl.ControlAdded += (sender, e) => { if (TabCtrl.TabPages.Count == 1) BtnTabClose.Visible = true; };    

            test  .Click += (sender, e) => { TestFunction(); };
        }

        /// <summary>
        /// Port, Unit 정보창 Panel
        /// </summary>
        private void InitializeControl_Info()
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
            Tree.ImageList = ImgList;
            Tree.Nodes.Add("Program Computer");
            Tree.Nodes[0].ImageIndex = 0;
            Tree.Nodes[0].SelectedImageIndex = Tree.Nodes[0].ImageIndex;
            Tree.SelectedImageIndex = 5;    //선택한 Node Image 기본값

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
            colName.HeaderText = RuntimeData.String("F0500");
            colName.Name = "P";
            colName.DataPropertyName = "P";
            colName.Width = 90;
            colName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colName.ReadOnly = true;
            colName.SortMode = DataGridViewColumnSortMode.NotSortable;  //정렬 불가능
            colName.Resizable = DataGridViewTriState.False;             //너비조절 불가능

            //Value Column
            DataGridViewColumn colValue = new DataGridViewTextBoxColumn();
            colValue.HeaderText = RuntimeData.String("F0501");
            colValue.Name = "V";
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

            //Tree.AfterSelect += (sender, e) => { InitItemInfo(Tree.SelectedNode); };
        }

        private void Initialize_TreeMenu_RightClick()
        {
            menu_CreatePort.Name = "TreeMenu_CreatePort";
            menu_EditPort.Name = "TreeMenu_EditPort";
            menu_CreateUnit.Name = "TreeMenu_CreateUnit";
            menu_EditUnit.Name = "TreeMenu_EditUnit";

            menu_CreatePort.Text = RuntimeData.String("F0106");
            menu_EditPort.Text = RuntimeData.String("F0205");
            menu_CreateUnit.Text = RuntimeData.String("F0304");
            menu_EditUnit.Text = RuntimeData.String("F0308");

            TreeMenu.Items.Add(menu_CreatePort);
            TreeMenu.Items.Add(menu_EditPort);
            TreeMenu.Items.Add(MenuLine1);
            TreeMenu.Items.Add(menu_CreateUnit);
            TreeMenu.Items.Add(menu_EditUnit);

            menu_CreatePort.Click += (sender, e) => { CreatePort(); };
            menu_EditPort.Click   += (sender, e) => { EditPort(); };
            menu_CreateUnit.Click += (sender, e) => { CreateUnit(); };
            menu_EditUnit.Click   += (sender, e) => { EditUnit(); };

            Tree.NodeMouseClick += Tree_NodeMouseClick; ;
        }

        /// <summary>
        /// Main Form Tree 재지정
        /// </summary>
        public void InitTreeItem()
        {
            Tree.Nodes[0].Nodes.Clear();    //Program Computer Node 하위항목 삭제

            //Port
            foreach (Port port in RuntimeData.Ports.Values)
            {
                TreeNode portNode = new TreeNode();
                portNode.Name = port.PortName;
                portNode.Text = port.PortName;
                portNode.Tag = port;

                if (port.ProtocolType == uProtocolType.ModBusTcpIp)
                {
                    portNode.ImageKey = "LANPort";
                }
                else
                {
                    portNode.ImageKey = "SerialPort";
                }
                portNode.SelectedImageKey = portNode.ImageKey;

                //Unit
                foreach (Unit unit in port.Units.Values)
                {
                    TreeNode unitNode = new TreeNode();
                    unitNode.Name = unit.UnitName + unit.SlaveAddr;
                    unitNode.Text = unit.UnitName;
                    unitNode.ImageKey = "DisConnect";
                    unitNode.SelectedImageKey = unitNode.ImageKey;
                    unitNode.Tag = unit;

                    //Channel
                    //foreach (UnitChannel channel in unit.Channel)
                    //{
                    //    TreeNode channelNode = new TreeNode();
                    //    channelNode.Name = string.Format("Ch{0:D2}",channel.ChannelNumber);
                    //    channelNode.Text = channelNode.Name;
                    //    channelNode.ImageKey = "TreeChild";
                    //}

                    portNode.Nodes.Add(unitNode);
                }

                Tree.Nodes[0].Nodes.Add(portNode);
            }

            Tree.ExpandAll();
        }

        /// <summary>
        /// BackGroundWorker 생성 및 시작
        /// </summary>
        private void InitializeBackGroundWorker()
        {
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = false;
            //bgWorker.DoWork += BackgroundWorkder_DoWork;
            //bgWorker.RunWorkerAsync();
        }

        #region Event

        /// <summary>
        /// Tree에서 Item 선택 시 정보창
        /// </summary>
        private void SelectTreeItem(TreeNode node)
        {
            //선택 Item 정보 표시
            Type tagType = node.Tag?.GetType();

            if (tagType == typeof(Port))
            {
                //Port Node 선택 시
                SetPortInfoTable(node);
            }
            else if (tagType == typeof(Unit))
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
                dt.Rows.Add(new object[] { RuntimeData.String("F0100"), null });
                dt.Rows.Add(new object[] { RuntimeData.String("F0101"), null });
                dt.Rows.Add(new object[] { RuntimeData.String("F0102"), null });
                dt.Rows.Add(new object[] { RuntimeData.String("F0103"), null });
                dt.Rows.Add(new object[] { RuntimeData.String("F0104"), null });
                dt.Rows.Add(new object[] { RuntimeData.String("F0105"), null });
            }

            //dt.Rows[0][1] = port.PortName;
            //dt.Rows[1][1] = port.ProtocolType;
            //dt.Rows[2][1] = port.BaudRate;
            //dt.Rows[3][1] = port.DataBits;
            //dt.Rows[4][1] = port.StopBIt;
            //dt.Rows[5][1] = port.Parity;

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
                dt.Rows.Add(new object[] { RuntimeData.String("F0300"), null });   //Slave Addr
                dt.Rows.Add(new object[] { RuntimeData.String("F0301"), null });   //모델 구분
                dt.Rows.Add(new object[] { RuntimeData.String("F0302"), null });   //모델
                dt.Rows.Add(new object[] { RuntimeData.String("F0303"), null });   //모델명(사용자지정)
            }

            //dt.Rows[0][1] = unit.SlaveAddr;
            //dt.Rows[1][1] = unit.UnitType;
            //dt.Rows[2][1] = unit.UnitModel;
            //dt.Rows[3][1] = unit.UnitName;

            gv.DataSource = dt;
        }

        /// <summary>
        /// TreeView 우클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Tree.SelectedNode = e.Node;

                VisibleTreeMenu(e.Node.Level);
                e.Node.ContextMenuStrip = TreeMenu;
            }
        }

        /// <summary>
        /// TreeView 우클릭 시 나오는 Menu Visible 처리
        /// </summary>
        /// <param name="nodeLvl"></param>
        private void VisibleTreeMenu(int nodeLvl)
        {
            //Root
            if (nodeLvl == 0)
            {
                menu_CreatePort.Visible = true;
                menu_EditPort.Visible = false;
                MenuLine1.Visible = false;
                menu_CreateUnit.Visible = false;
                menu_EditUnit.Visible = false;
            }
            //Port
            else if (nodeLvl == 1)
            {
                menu_CreatePort.Visible = false;
                menu_EditPort.Visible = true;
                MenuLine1.Visible = true;
                menu_CreateUnit.Visible = true;
                menu_EditUnit.Visible = false;
            }
            //Unit
            else if (nodeLvl == 2)
            {
                menu_CreatePort.Visible = false;
                menu_EditPort.Visible = false;
                MenuLine1.Visible = false;
                menu_CreateUnit.Visible = false;
                menu_EditUnit.Visible = true;
            }
            //Channel
            else if (nodeLvl == 3) { }
            else if (nodeLvl == 4) { }
        }

        #endregion Event End

        /// <summary>
        /// Port 생성
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreatePort()
        {
            //Port 생성
            FrmPort frmPort = new FrmPort(FrmEditType.New);

            if (frmPort.ShowDialog() == DialogResult.OK)
            {
                InitTreeItem();
            }
        }

        /// <summary>
        /// Port 수정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditPort()
        {
            //Port 수정
            TreeNode node = Tree.SelectedNode;

            if(node != null || node.Tag != null)
            {
                if(node.Tag.GetType().Name == "Custom_SerialPort"
                    || node.Tag.GetType().Name == "Custom_EthernetPort")
                {
                    FrmPort frmPort = new FrmPort(FrmEditType.Edit, node.Tag as Port);

                    if (frmPort.ShowDialog() == DialogResult.OK)
                    {
                        InitTreeItem();
                    }
                }
            }
        }

        /// <summary>
        /// Unit 생성
        /// </summary>
        private void CreateUnit()
        {
            TreeNode node = Tree.SelectedNode;

            if (node != null || node.Tag != null)
            {
                FrmUnit frmUnit = null;
                if (node.Tag.GetType().Name == "Custom_SerialPort"
                    || node.Tag.GetType().Name == "Custom_EthernetPort")
                {
                    frmUnit = new FrmUnit(FrmEditType.New, node.Tag as Port);
                }
                else if (node.Tag.GetType().Name == "Unit")
                {
                    frmUnit = new FrmUnit(FrmEditType.New, node.Parent.Tag as Port);
                }

                if (frmUnit != null)
                {
                    if (frmUnit.ShowDialog() == DialogResult.OK)
                    {
                        InitTreeItem();
                    }
                }
            }
        }

        /// <summary>
        /// Unit 수정
        /// </summary>
        private void EditUnit()
        {
            TreeNode node = Tree.SelectedNode;

            if (node != null || node.Tag != null)
            {
                if (node.Tag.GetType().Name == "Unit")
                {
                    FrmUnit frmUnit = new FrmUnit(FrmEditType.Edit, node.Parent.Tag as Port, node.Tag as Unit);

                    if (frmUnit.ShowDialog() == DialogResult.OK)
                    {
                        InitTreeItem();
                    }
                }
            }
        }




        private void SetImageList()
        {
            //Image 보관
            ImgList.Images.Add("Root", Dnf.Utils.Properties.Resources.BlueCircleSetting_16x16);     //Program Computer
            ImgList.Images.Add("SerialPort", Dnf.Utils.Properties.Resources.Serial_Come_16x16);     //Serial Port
            ImgList.Images.Add("LANPort", Dnf.Utils.Properties.Resources.LAN_Come_16x16);           //LAN Port
            ImgList.Images.Add("DisConnect", Dnf.Utils.Properties.Resources.RedPower_16x16);        //빨강(미연결)
            ImgList.Images.Add("ConnectError", Dnf.Utils.Properties.Resources.YellowWarning_16x16); //노랑(연결 불량, 오류)
            ImgList.Images.Add("Connect", Dnf.Utils.Properties.Resources.GreenSync_16x16);          //초록(정상연결)
            ImgList.Images.Add("TreeChild", Dnf.Utils.Properties.Resources.TreeChild_16x16);        //Unit 미만 항목
            ImgList.Images.Add("Empty", Dnf.Utils.Properties.Resources.empty_16x16);                //Default 아이콘
        }

        private void SortControl()
        {
            TextMenu.BringToFront();
            IconMenu.BringToFront();
            pnlList.BringToFront();
            TabCtrl.BringToFront();
            BtnTabClose.BringToFront();
        }

        /// <summary>
        /// 함수(Method, Function) 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActMethod(object sender, EventArgs e)
        {
            string itemName = (sender as ToolStripItem).Name;
            if (itemName == null || itemName == "") return;

            //함수만 뽑기위해 삭제할 텍스트
            string[] removeTxt = new string[] { "TxtMenu", "IconMenu" };

            foreach (string txt in removeTxt)
            {
                itemName = itemName.Replace(txt, "");
            }

            try
            {
                MenuFunctions func = new MenuFunctions(this);   //기능 저장된 Class
                MethodInfo method = typeof(MenuFunctions).GetMethod(itemName);   //함수 가져오기

                if (method != null)
                {
                    //함수 실행(Class, Input Parameter)
                    switch (itemName)
                    {
                        case "PortOpen": method.Invoke(func, new object[] { Tree.SelectedNode }); break;        //포트열기
                        case "PortClose": method.Invoke(func, new object[] { Tree.SelectedNode }); break;       //포트닫기
                        case "ItemCre": method.Invoke(func, new object[] { RuntimeData.String("F0204") }); break;//Item 생성하기
                        case "XmlSave": method.Invoke(func, null); break;   //Xml 저장
                        case "XmlLoad": method.Invoke(func, null); break;   //Xml 불러오기
                        case "Test": method.Invoke(func, new object[] { UnitType.UnitType2 }); break;   //Xml 불러오기
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BackgroundWorkder_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (!bgWorker.CancellationPending)
                {

                }
                else
                {
                    //Background Error 날경우 잠깐 휴식
                    Thread.Sleep(500);
                }
            }
        }

        private void SetNodeImage(TreeNode node, ConnectionState state)
        {
            if (state == ConnectionState.Closed) { node.ImageKey = "DisConnect"; }      //미연결
            else if (state == ConnectionState.Executing) { node.ImageKey = "ConnectError"; } //연결중
            else if (state == ConnectionState.Open) { node.ImageKey = "Connect"; }  //연결됨
            node.SelectedImageKey = node.ImageKey;
        }

        private void FrmClosed(object sender, FormClosedEventArgs e)
        {
            if (bgWorker != null&& bgWorker.IsBusy) bgWorker.CancelAsync();
            RuntimeData.Ports = new Dictionary<string, Port>();
        }

        public void RemoveTabPage(string pageName)
        {
            if (TabCtrl.TabPages.Count == 1)
            {
                TabCtrl.TabPages.RemoveByKey(pageName);
                BtnTabClose.Visible = false;
            }
            else
            {
                TabCtrl.TabPages.RemoveByKey(pageName);
            }
        }

        private void TestFunction()
        {
            FrmPort frmPort = new FrmPort(FrmEditType.New);

            if (frmPort.ShowDialog() == DialogResult.OK)
            {
                InitTreeItem();
            }
        }
    }
}
