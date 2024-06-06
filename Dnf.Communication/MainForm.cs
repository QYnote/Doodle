using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dnf.Communication.Controls;
using Dnf.Communication.Data;
using Dnf.Communication.Frm;

//Button Resource 사이트

namespace Dnf.Communication
{
    public partial class MainForm : Dnf.Utils.Views.FrmBase
    {
        #region Control 모음
        private MenuStrip TextMenu = new MenuStrip();   
        private ToolStripMenuItem TextMenu_Basic = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_Basic_Unit = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_File = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_File_XmlSave = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_File_XmlLoad = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_Comm = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_Comm_CreatePort = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_Comm_PortOpen = new ToolStripMenuItem();
        private ToolStripMenuItem TextMenu_Comm_PortClose = new ToolStripMenuItem();
        private ToolStrip IconMenu = new ToolStrip();   //상단 아이콘 메뉴
        private ToolStripButton IconMenu_File_XmlSave = new ToolStripButton();
        private ToolStripButton IconMenu_File_XmlLoad = new ToolStripButton();
        private ToolStripButton IconMenu_Comm_CreatePort = new ToolStripButton();
        private ToolStripButton IconMenu_Comm_PortOpen = new ToolStripButton();
        private ToolStripButton IconMenu_Comm_PortClose = new ToolStripButton();
        private ToolStripButton IconMenu_Test = new ToolStripButton();

        private StatusStrip StatusBar = new StatusStrip();   //상태 바
        private ToolStripStatusLabel LblStatus = new ToolStripStatusLabel();

        public TabControl TabCtrl = new TabControl();  //Tab Page(주 화면)
        private Button BtnTabClose;                     //Tab Page 닫기 버튼

        private Panel pnlList;      //생성된 정보들모음
        private TreeView Tree;      //등록된 Port-Unit Tree
        private ImageList TreeImgList = new ImageList();
        private ContextMenuStrip TreeMenu = new ContextMenuStrip();  //Tree 우클릭 메뉴
        private ToolStripMenuItem TreeMenu_CreatePort = new ToolStripMenuItem();
        private ToolStripMenuItem TreeMenu_EditPort = new ToolStripMenuItem();
        private ToolStripSeparator TreeMenuLine1 = new ToolStripSeparator();
        private ToolStripMenuItem TreeMenu_CreateUnit = new ToolStripMenuItem();
        private ToolStripMenuItem TreeMenu_EditUnit = new ToolStripMenuItem();
        private ToolStripSeparator TreeMenuLine2 = new ToolStripSeparator();
        private ToolStripMenuItem TreeMenu_PortOpen = new ToolStripMenuItem();
        private ToolStripMenuItem TreeMenu_PortClose = new ToolStripMenuItem();

        private Panel pnlProperty = new Panel();
        private DataGridView gvPort = new DataGridView();
        private DataGridView gvUnit = new DataGridView();
        private DataGridViewColumn colPortPropertyName = new DataGridViewTextBoxColumn();
        private DataGridViewColumn colPortPropertyValue = new DataGridViewTextBoxColumn();
        private DataGridViewColumn colUnitPropertyName = new DataGridViewTextBoxColumn();
        private DataGridViewColumn colUnitPropertyValue = new DataGridViewTextBoxColumn();
        #endregion Control 모음 End

        private BackgroundWorker bgWorker;
        private Port SelectedPort;
        private Unit SelectedUnit;

        public MainForm()
        {
            InitializeComponent();
            InitControl();
        }

        private void InitControl()
        {
            this.FormClosed += FrmClosed;

            //Control Add
            InitializeControl_Base();
            InitializeControl_Info();
            Initialize_TreeMenu();
            InitializeDockIndex();

            SetImageList();
            SetText();

            this.Text = RuntimeData.String("F00");
            InitializeBackGroundWorker();
        }

        #region Control 설정

        /// <summary>
        /// 메뉴, TabControl 생성
        /// </summary>
        private void InitializeControl_Base()
        {
            InitializeControl_TextMenu();
            InitializeControl_IconMenu();

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
        }

        /// <summary>
        /// Text Menu 생성
        /// </summary>
        private void InitializeControl_TextMenu()
        {
            TextMenu_Basic.Name = "TextMenu_Basic";
            TextMenu_Basic_Unit.Name = "TextMenu_Basic_Unit";
            TextMenu_File.Name = "TextMenu_File";
            TextMenu_File_XmlSave.Name = "TextMenu_File_XmlSave";
            TextMenu_File_XmlLoad.Name = "TextMenu_File_XmlLoad";
            TextMenu_Comm.Name = "TextMenu_Comm";
            TextMenu_Comm_CreatePort.Name = "TextMenu_Comm_CreatePort";
            TextMenu_Comm_PortOpen.Name = "TextMenu_Comm_PortOpen";
            TextMenu_Comm_PortClose.Name = "TextMenu_Comm_PortClose";

            //TextMenu_File.DropDownItems.AddRange(new ToolStripItem[] { TextMenu_File_XmlSave, TextMenu_File_XmlLoad });
            TextMenu_Basic.DropDownItems.AddRange(new ToolStripItem[] { TextMenu_Basic_Unit });
            TextMenu_Comm.DropDownItems.AddRange(new ToolStripItem[] { TextMenu_Comm_CreatePort, TextMenu_Comm_PortOpen, TextMenu_Comm_PortClose });
            TextMenu.Items.AddRange(new ToolStripItem[] {
                TextMenu_Basic,
                new ToolStripSeparator(), 
                TextMenu_File, 
                new ToolStripSeparator(), 
                TextMenu_Comm
            });

            TextMenu_Basic_Unit.Click += (sender, e) => { OpenTabPage(new Frm_UnitSetting()); };
            TextMenu_Comm_CreatePort.Click += (sender, e) => { CreatePort(); };
            TextMenu_Comm_PortOpen.Click += (sender, e) => { ConnectPort(); };
            TextMenu_Comm_PortClose.Click += (sender, e) => { DisConnectPort(); };
        }

        /// <summary>
        /// Icon Menu 생성
        /// </summary>
        private void InitializeControl_IconMenu()
        {
            IconMenu.ImageScalingSize = new Size(32, 32);

            IconMenu_File_XmlSave.Name = "IconMenu_File_XmlSave";
            IconMenu_File_XmlLoad.Name = "IconMenu_File_XmlLoad";
            IconMenu_Comm_CreatePort.Name = "IconMenu_Comm_CreatePort";
            IconMenu_Comm_PortOpen.Name = "IconMenu_Comm_PortOpen";
            IconMenu_Comm_PortClose.Name = "IconMenu_Comm_PortClose";
            IconMenu_Test.Name = "IconMenu_Test";

            IconMenu_File_XmlSave.DisplayStyle = ToolStripItemDisplayStyle.Image;
            IconMenu_File_XmlLoad.DisplayStyle = ToolStripItemDisplayStyle.Image;
            IconMenu_Comm_CreatePort.DisplayStyle = ToolStripItemDisplayStyle.Image;
            IconMenu_Comm_PortOpen.DisplayStyle = ToolStripItemDisplayStyle.Image;
            IconMenu_Comm_PortClose.DisplayStyle = ToolStripItemDisplayStyle.Image;
            IconMenu_Test.DisplayStyle = ToolStripItemDisplayStyle.Image;

            //IconMenusms ImageList 사용해서 가져오면 Image 깨짐
            IconMenu_Comm_CreatePort.Image = Dnf.Utils.Properties.Resources.Plus_00_32x32;
            IconMenu_Comm_PortOpen.Image = Dnf.Utils.Properties.Resources.Connect_Green_32x32;
            IconMenu_Comm_PortClose.Image = Dnf.Utils.Properties.Resources.Connect_Red_32x32;

            IconMenu_Test.Image = Dnf.Utils.Properties.Resources.Test_32x32;
            IconMenu.Items.AddRange(new ToolStripItem[] {
                IconMenu_Comm_CreatePort,
                IconMenu_Comm_PortOpen,
                IconMenu_Comm_PortClose,
                IconMenu_Test });

            IconMenu_Comm_CreatePort.Click += (sender, e) => { CreatePort(); };
            IconMenu_Comm_PortOpen.Click += (sender, e) => { ConnectPort(); };
            IconMenu_Comm_PortClose.Click += (sender, e) => { DisConnectPort(); };
            IconMenu_Test.Click += (sender, e) => { TestFunction(); };
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
            Tree.ImageList = TreeImgList;
            Tree.Nodes.Add("Program Computer");
            Tree.Nodes[0].ImageIndex = 0;
            Tree.Nodes[0].SelectedImageIndex = Tree.Nodes[0].ImageIndex;
            Tree.SelectedImageIndex = 5;    //선택한 Node Image 기본값

            pnlList.Controls.Add(Tree);
            Initialize_PropertyGrid();

            this.Controls.Add(pnlList);

            //Tree.AfterSelect += (sender, e) => { InitItemInfo(Tree.SelectedNode); };
        }

        /// <summary>
        /// Tree에서 선택한 속성 DataGridView
        /// </summary>
        private void Initialize_PropertyGrid()
        {
            pnlProperty.Dock = DockStyle.Bottom;
            pnlProperty.Size = new Size(pnlProperty.Width, 200);
            pnlProperty.MinimumSize = new Size(200, 200);

            //Grid조정
            gvPort.Dock = DockStyle.Fill;
            gvUnit.Dock = DockStyle.Fill;
            //DataTable에 따른 Column 자동생성 숨기기
            gvPort.AutoGenerateColumns = false;
            gvUnit.AutoGenerateColumns = false;
            //Runtime 중 유저가 Row 추가하는 공간 숨기기
            gvPort.AllowUserToAddRows = false;
            gvUnit.AllowUserToAddRows = false;
            //유저가 Row높이 수정 막기
            gvPort.AllowUserToResizeRows = false;
            gvUnit.AllowUserToResizeRows = false;
            //현재 Row 화살표 숨기기
            gvPort.RowHeadersVisible = false;
            gvUnit.RowHeadersVisible = false;
            //초기 Visible값 숨기기
            gvPort.Visible = false;
            gvUnit.Visible = false;
            //여러개 선택
            gvPort.MultiSelect = false;
            gvUnit.MultiSelect = false;
            //Header Size 수정 막기
            gvPort.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gvUnit.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;


            //Column 조정
            colPortPropertyName.Name = "P";
            colPortPropertyValue.Name = "V";
            colUnitPropertyName.Name = "P";
            colUnitPropertyValue.Name = "V";
            //너비
            colPortPropertyName.Width = 90;
            colPortPropertyValue.Width = pnlList.Width - colPortPropertyName.Width - 3;
            colUnitPropertyName.Width = 90;
            colUnitPropertyValue.Width = pnlList.Width - colPortPropertyName.Width - 3;
            //DataTable 연동할 Column명
            colPortPropertyName.DataPropertyName = "P";
            colPortPropertyValue.DataPropertyName = "V";
            colUnitPropertyName.DataPropertyName = "P";
            colUnitPropertyValue.DataPropertyName = "V";
            //Header Text정렬
            colPortPropertyName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPortPropertyValue.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colUnitPropertyName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colUnitPropertyValue.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //Value Text 정렬
            colPortPropertyName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPortPropertyValue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colUnitPropertyName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colUnitPropertyValue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //읽기만 가능
            colPortPropertyName.ReadOnly = true;
            colPortPropertyValue.ReadOnly = true;
            colUnitPropertyName.ReadOnly = true;
            colUnitPropertyValue.ReadOnly = true;
            //정렬 불가능
            colPortPropertyName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colPortPropertyValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colUnitPropertyName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colUnitPropertyValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            //너비조절 불가능
            colPortPropertyName.Resizable = DataGridViewTriState.False;
            colPortPropertyValue.Resizable = DataGridViewTriState.False;
            colUnitPropertyName.Resizable = DataGridViewTriState.False;
            colUnitPropertyValue.Resizable = DataGridViewTriState.False;

            gvPort.Columns.AddRange(colPortPropertyName, colPortPropertyValue);
            gvUnit.Columns.AddRange(colUnitPropertyName, colUnitPropertyValue);

            pnlProperty.Controls.Add(gvPort);
            pnlProperty.Controls.Add(gvUnit);
            pnlList.Controls.Add(pnlProperty);
        }

        /// <summary>
        /// Tree 우클릭 Menu
        /// </summary>
        private void Initialize_TreeMenu()
        {
            TreeMenu_CreatePort.Name = "TreeMenu_CreatePort";
            TreeMenu_EditPort.Name = "TreeMenu_EditPort";
            TreeMenu_CreateUnit.Name = "TreeMenu_CreateUnit";
            TreeMenu_EditUnit.Name = "TreeMenu_EditUnit";

            TreeMenu.Items.Add(TreeMenu_PortOpen);
            TreeMenu.Items.Add(TreeMenu_PortClose);
            TreeMenu.Items.Add(TreeMenuLine1);
            TreeMenu.Items.Add(TreeMenu_CreatePort);
            TreeMenu.Items.Add(TreeMenu_EditPort);
            TreeMenu.Items.Add(TreeMenuLine2);
            TreeMenu.Items.Add(TreeMenu_CreateUnit);
            TreeMenu.Items.Add(TreeMenu_EditUnit);

            TreeMenu_CreatePort.Click += (sender, e) => { CreatePort(); };
            TreeMenu_EditPort.Click   += (sender, e) => { EditPort(); };
            TreeMenu_CreateUnit.Click += (sender, e) => { CreateUnit(); };
            TreeMenu_EditUnit.Click   += (sender, e) => { EditUnit(); };

            Tree.NodeMouseClick += Tree_NodeMouseClick; ;
            Tree.AfterSelect += Tree_AfterSelect;
        }

        /// <summary>
        /// Dock 순서 조정
        /// </summary>
        private void InitializeDockIndex()
        {
            TextMenu.BringToFront();
            IconMenu.BringToFront();
            pnlList.BringToFront();
            TabCtrl.BringToFront();
            BtnTabClose.BringToFront();
        }

        /// <summary>
        /// Main Form Tree 재지정
        /// </summary>
        internal void InitTreeItem()
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
            bgWorker.DoWork += BackgroundWorkder_DoWork;
            //bgWorker.RunWorkerAsync();
        }

        /// <summary>
        /// TreeView ImageList
        /// </summary>
        private void SetImageList()
        {
            //Tree
            TreeImgList.Images.Add("Root", Dnf.Utils.Properties.Resources.BlueCircleSetting_16x16);     //Program Computer
            TreeImgList.Images.Add("SerialPort", Dnf.Utils.Properties.Resources.Serial_Come_16x16);     //Serial Port
            TreeImgList.Images.Add("LANPort", Dnf.Utils.Properties.Resources.LAN_Come_16x16);           //LAN Port
            TreeImgList.Images.Add("DisConnect", Dnf.Utils.Properties.Resources.RedPower_16x16);        //빨강(미연결)
            TreeImgList.Images.Add("ConnectError", Dnf.Utils.Properties.Resources.YellowWarning_16x16); //노랑(연결 불량, 오류)
            TreeImgList.Images.Add("Connect", Dnf.Utils.Properties.Resources.GreenSync_16x16);          //초록(정상연결)
            TreeImgList.Images.Add("TreeChild", Dnf.Utils.Properties.Resources.TreeChild_16x16);        //Unit 미만 항목
            TreeImgList.Images.Add("Empty", Dnf.Utils.Properties.Resources.empty_16x16);                //Default 아이콘
        }

        /// <summary>
        /// 화면에 표기되는 Text들
        /// </summary>
        private void SetText()
        {
            //TextMenu
            TextMenu_Basic.Text        = RuntimeData.String("F000100");
            TextMenu_Basic_Unit.Text   = RuntimeData.String("F00010000");
            TextMenu_File.Text         = RuntimeData.String("F000101");
            TextMenu_File_XmlSave.Text = RuntimeData.String("F00010100");
            TextMenu_File_XmlLoad.Text = RuntimeData.String("F00010101");
            TextMenu_Comm.Text         = RuntimeData.String("F000102");
            TextMenu_Comm_CreatePort.Text     = RuntimeData.String("F00010200");
            TextMenu_Comm_PortOpen.Text    = RuntimeData.String("F00010201");
            TextMenu_Comm_PortClose.Text   = RuntimeData.String("F00010202");

            //IconMenu
            IconMenu_File_XmlSave.ToolTipText    = RuntimeData.String("F000200");
            IconMenu_File_XmlLoad.ToolTipText    = RuntimeData.String("F000201");
            IconMenu_Comm_CreatePort.ToolTipText = RuntimeData.String("F000202");
            IconMenu_Comm_PortOpen.ToolTipText   = RuntimeData.String("F000203");
            IconMenu_Comm_PortClose.ToolTipText  = RuntimeData.String("F000204");
            IconMenu_Test.ToolTipText = "테스트";

            //TreeMenu
            TreeMenu_CreatePort.Text = RuntimeData.String("F000300");
            TreeMenu_EditPort.Text   = RuntimeData.String("F000301");
            TreeMenu_CreateUnit.Text = RuntimeData.String("F000302");
            TreeMenu_EditUnit.Text   = RuntimeData.String("F000303");
            TreeMenu_PortOpen.Text   = RuntimeData.String("F000304");
            TreeMenu_PortClose.Text  = RuntimeData.String("F000305");

            //Property Grid
            colPortPropertyName.HeaderText  = RuntimeData.String("F000400");
            colPortPropertyValue.HeaderText = RuntimeData.String("F000401");
            colUnitPropertyName.HeaderText  = RuntimeData.String("F000400");
            colUnitPropertyValue.HeaderText = RuntimeData.String("F000401");
        }

        #endregion Control 설정

        #region Event

        #region Menu Function

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
            FrmPort frmPort = new FrmPort(FrmEditType.Edit, SelectedPort);

            if (frmPort.ShowDialog() == DialogResult.OK)
            {
                InitTreeItem();
            }
        }

        private void ConnectPort()
        {
            if (SelectedPort == null) return;

            SelectedPort.Open();
        }

        private void DisConnectPort()
        {
            if (SelectedPort == null) return;

            SelectedPort.Close();
        }

        /// <summary>
        /// Unit 생성
        /// </summary>
        private void CreateUnit()
        {
            TreeNode node = Tree.SelectedNode;
            string type = GetNodeTagType(Tree.SelectedNode);

            if (type != string.Empty)
            {
                FrmUnit frmUnit = null;

                if (type == "Port")
                {
                    frmUnit = new FrmUnit(FrmEditType.New, node.Tag as Port);
                }
                else if (type == "Unit")
                {
                    frmUnit = new FrmUnit(FrmEditType.New, node.Parent.Tag as Port);
                }

                //정상적으로 진행되면 수정 Form 열기
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
            string type = GetNodeTagType(Tree.SelectedNode);

            if(type == "Unit")
            {
                FrmUnit frmUnit = new FrmUnit(FrmEditType.Edit, node.Parent.Tag as Port, node.Tag as Unit);

                if (frmUnit.ShowDialog() == DialogResult.OK)
                {
                    InitTreeItem();
                }
            }
        }

        #endregion Menu Function End
        #region TreeList

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

                VisibleTreeStripMenu(e.Node.Level);
                e.Node.ContextMenuStrip = TreeMenu;
            }
        }

        /// <summary>
        /// TreeView선택한 Node에따른 우클릭 Menu Visible 처리
        /// </summary>
        /// <param name="nodeLvl"></param>
        private void VisibleTreeStripMenu(int nodeLvl)
        {
            //Root
            if (nodeLvl == 0)
            {
                //TreeMenu
                TreeMenu_CreatePort.Visible = true;
                TreeMenu_EditPort.Visible = false;
                TreeMenuLine1.Visible = false;
                TreeMenu_CreateUnit.Visible = false;
                TreeMenu_EditUnit.Visible = false;
                TreeMenuLine2.Visible = false;
                TreeMenu_PortOpen.Visible = false;
                TreeMenu_PortClose.Visible = false;
            }
            //Port
            else if (nodeLvl == 1)
            {
                //TreeMenu
                TreeMenu_CreatePort.Visible = false;
                TreeMenu_EditPort.Visible = true;
                TreeMenuLine1.Visible = true;
                TreeMenu_CreateUnit.Visible = true;
                TreeMenu_EditUnit.Visible = false;
                TreeMenuLine2.Visible = true;
                TreeMenu_PortOpen.Visible = true;
                TreeMenu_PortClose.Visible = true;
            }
            //Unit
            else if (nodeLvl == 2)
            {
                //TreeMenu
                TreeMenu_CreatePort.Visible = false;
                TreeMenu_EditPort.Visible = false;
                TreeMenuLine1.Visible = false;
                TreeMenu_CreateUnit.Visible = false;
                TreeMenu_EditUnit.Visible = true;
                TreeMenuLine2.Visible = false;
                TreeMenu_PortOpen.Visible = false;
                TreeMenu_PortClose.Visible = false;
            }
            //Channel
            else if (nodeLvl == 3) { }
            else if (nodeLvl == 4) { }
            else
            {
                //TreeMenu
                TreeMenu_CreatePort.Visible = false;
                TreeMenu_EditPort.Visible = false;
                TreeMenuLine1.Visible = false;
                TreeMenu_CreateUnit.Visible = false;
                TreeMenu_EditUnit.Visible = false;
                TreeMenuLine2.Visible = false;
                TreeMenu_PortOpen.Visible = false;
                TreeMenu_PortClose.Visible = false;
            }
        }

        /// <summary>
        /// Tree선택 Action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            if (node.Tag != null)
            {
                string type = node.Tag.GetType().Name;
                DataTable dt = null;

                if (type == "Custom_SerialPort" || type == "Custom_EthernetPort")
                {
                    //선택된 Port 설정
                    this.SelectedPort = e.Node.Tag as Port;

                    //Porperty 변경
                    if (type == "Custom_SerialPort")
                    {
                        dt = SerialPortProperty(e.Node.Tag as Custom_SerialPort);
                    }
                    else if (type == "Custom_EthernetPort")
                    {
                        dt = EthernetPortProperty(e.Node.Tag as Custom_EthernetPort);
                    }

                    if (dt != null)
                    {
                        gvPort.DataSource = dt;

                        gvPort.Visible = true;
                        gvUnit.Visible = false;
                    }

                    //메뉴 Visible
                    if (this.SelectedPort.IsOpen == true)
                    {
                        TextMenu_Comm_PortOpen.Visible = false;
                        TextMenu_Comm_PortClose.Visible = true;
                        IconMenu_Comm_PortOpen.Visible = false;
                        IconMenu_Comm_PortClose.Visible = true;
                    }
                    else if (this.SelectedPort.IsOpen == false)
                    {
                        TextMenu_Comm_PortOpen.Visible = true;
                        TextMenu_Comm_PortClose.Visible = false;
                        IconMenu_Comm_PortOpen.Visible = true;
                        IconMenu_Comm_PortClose.Visible = false;
                    }
                }
                else if (type == "Unit")
                {
                    //선택된 Unit, Port 설정
                    this.SelectedPort = e.Node.Parent.Tag as Port;
                    this.SelectedUnit = e.Node.Tag as Unit;

                    //Porperty 변경
                    dt = UnitProperty(e.Node.Tag as Unit);

                    if (dt != null)
                    {
                        gvUnit.DataSource = dt;

                        gvPort.Visible = false;
                        gvUnit.Visible = true;
                    }

                    //메뉴 Visible
                    if (this.SelectedPort.IsOpen == true)
                    {
                        TextMenu_Comm_PortOpen.Visible = false;
                        TextMenu_Comm_PortClose.Visible = true;
                        IconMenu_Comm_PortOpen.Visible = false;
                        IconMenu_Comm_PortClose.Visible = true;
                    }
                    else if (this.SelectedPort.IsOpen == false)
                    {
                        TextMenu_Comm_PortOpen.Visible = true;
                        TextMenu_Comm_PortClose.Visible = false;
                        IconMenu_Comm_PortOpen.Visible = true;
                        IconMenu_Comm_PortClose.Visible = false;
                    }
                }
            }
        }

        private delegate void InvokerSetUI(Port port);
        /// <summary>
        /// 그려진 UI 변경
        /// </summary>
        private void SetUI(Port port)
        {
            if (this.InvokeRequired)
            {
                //메인 Thred에서 사용중이면 해당 Thread에서 SetUI 실행해달라고 요청하기
                this.Invoke(new InvokerSetUI(SetUI), new object[] { port });
            }
            else
            {
                //UI 설정
                foreach (Unit unit in port.Units.Values)
                {
                    //Unit UI 설정
                    if(unit.State == UnitConnectionState.Open_DisConnect)
                    {
                        //미연결 상태
                        unit.Node.ImageKey = "DisConnect";
                        unit.Node.SelectedImageKey = unit.Node.ImageKey;
                    }
                }
            }
        }

        /// <summary>
        /// Node의 Tag Type 가져오기
        /// </summary>
        /// <param name="node">가져올 Node</param>
        /// <returns>Success : Port, Unit / Error : string.Empty</returns>
        private string GetNodeTagType(TreeNode node)
        {
            string type = string.Empty;

            if (node != null && node.Tag != null)
            {
                string typeName = node.Tag.GetType().Name;
                if (typeName == "Custom_SerialPort"
                    || typeName == "Custom_EthernetPort")
                {
                    return "Port";
                }
                else if (typeName == "Unit")
                {
                    return "Unit";
                }
            }

            return type;
        }

        #endregion TreeList End
        #region TabPage

        private void OpenTabPage(TabPage frm)
        {
            //이미 Form 열려있을 시
            if (TabCtrl.TabPages.ContainsKey(frm.Name))
            {
                TabCtrl.TabPages[frm.Name].Focus();
                return;
            }

            if (TabCtrl.TabPages.Count == 0)
            {
                BtnTabClose.Visible = true;
            }

            //열려있는 Form이 없을 시
            TabCtrl.TabPages.Add(frm);
            frm.Focus();
        }

        /// <summary>
        /// TabPage 종료
        /// </summary>
        /// <param name="pageName"></param>
        internal void RemoveTabPage(string pageName)
        {
            if (TabCtrl.TabPages.Count == 1)
            {
                BtnTabClose.Visible = false;
            }

            //종료이벤트
            if (TabCtrl.TabPages[pageName].GetType() == typeof(Frm_UnitSetting))
            {
                (TabCtrl.TabPages[pageName] as Frm_UnitSetting).UnitInfoSave();
            }

            TabCtrl.TabPages.RemoveByKey(pageName);
        }

        #endregion TabPage End
        #region Property

        private DataTable SerialPortProperty(Custom_SerialPort port)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("P", typeof(string));
            dt.Columns.Add("V", typeof(string));

            dt.Rows.Add("PortName", port.PortName);
            dt.Rows.Add("ProtocolType", port.ProtocolType);
            dt.Rows.Add("BaudRate", port.BaudRate);
            dt.Rows.Add("DataBits", port.DataBits);
            dt.Rows.Add("Parity", port.Parity);
            dt.Rows.Add("StopBit", port.StopBIt);

            return dt;
        }

        private DataTable EthernetPortProperty(Custom_EthernetPort port)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("P", typeof(string));
            dt.Columns.Add("V", typeof(string));

            dt.Rows.Add("PortNo", port.PortNo);
            dt.Rows.Add("IP", port.IPAddr);

            return dt;
        }

        private DataTable UnitProperty(Unit unit)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("P", typeof(string));
            dt.Columns.Add("V", typeof(string));

            dt.Rows.Add("Address", unit.SlaveAddr);
            dt.Rows.Add("Type", unit.UnitType);
            dt.Rows.Add("Model", unit.UnitModel);
            dt.Rows.Add("Name(User)", unit.UnitName);

            return dt;
        }


        #endregion Property End

        private void BackgroundWorkder_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (!bgWorker.CancellationPending)
                {
                    foreach (TreeNode portNode in Tree.Nodes[0].Nodes)
                    {
                        if (portNode.Tag == null) continue;
                        Port port = (Port)portNode.Tag;

                        //UI 변경
                        SetUI(port);
                    }
                }

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Form 닫기 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmClosed(object sender, FormClosedEventArgs e)
        {
            if (bgWorker != null && bgWorker.IsBusy) bgWorker.CancelAsync();
        }

        #endregion Event End

        private void TestFunction()
        {
            if(RuntimeData.TestPortUsed == true)
                { RuntimeData.TestPortUsed = false; }
            else{ RuntimeData.TestPortUsed = true; }
        }

    }
}
