using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Dnf.Communication.Frm
{
    public partial class Frm_UnitSetting : TabPage
    {
        public event EventHandler Evnet_PageClosed;
        #region Controls
        private int marginValue = 3;

        private Panel pnlUnitItem = new Panel();
        private Panel pnlUnitGroup = new Panel();
        private Label LblUnitGroup = new Label();
        private Panel pnlUnitGroupAction = new Panel();
        private TextBox TxtUnitGroup = new TextBox();
        private Button btnUnitGroupAdd = new Button();
        private Button btnUnitGroupDel = new Button();
        private ListBox LbxUnitGroup = new ListBox();
        private Panel pnlUnitModel = new Panel();
        private Label LblUnitModel = new Label();
        private Panel pnlUnitModelAction = new Panel();
        private TextBox TxtUnitModel = new TextBox();
        private Button btnUnitModelAdd = new Button();
        private Button btnUnitModelDel = new Button();
        private ListBox LbxUnitModel = new ListBox();

        #endregion Controls End

        private string SelectedGroup = string.Empty;
        private string SelectedModel = string.Empty;
        private Dictionary<string, Dictionary<string, int>> dicUnitGroups;

        private string InfoFilePath = RuntimeData.DataPath + "UnitInfo.xml";


        public Frm_UnitSetting()
        {
            InitializeComponent();

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
            pnlUnitItem.Dock = DockStyle.Left;
            pnlUnitItem.Size = new Size(150, 100);

            InitializeUnitGroupModel();
            SetPositionSize();
            //InitializeUnitGroup();
            //InitializeUnitModel();
            //InitializeDockIndex();
            SetText();

            this.Controls.Add(pnlUnitItem);
        }

        /// <summary>
        /// Unit Group Control 생성
        /// </summary>
        private void InitializeUnitGroupModel()
        {
            LblUnitGroup.AutoSize = false;
            LblUnitGroup.TextAlign = ContentAlignment.MiddleCenter;
            LblUnitGroup.BorderStyle = BorderStyle.FixedSingle;

            TxtUnitGroup.AutoSize = false;
            TxtUnitGroup.MaxLength = 10;

            btnUnitGroupAdd.Text = "+";
            btnUnitGroupDel.Text = "-";

            LbxUnitGroup.AutoSize = false;
            LbxUnitGroup.Sorted = true;

            //Unit 모델
            LblUnitModel.AutoSize = false;
            LblUnitModel.TextAlign = ContentAlignment.MiddleCenter;
            LblUnitModel.BorderStyle = BorderStyle.FixedSingle;

            TxtUnitModel.AutoSize = false;
            TxtUnitModel.MaxLength = 10;

            btnUnitModelAdd.Text = "+";
            btnUnitModelDel.Text = "-";

            LbxUnitModel.AutoSize = false;
            LbxUnitModel.Sorted = true;


            this.Controls.Add(TxtUnitGroup);
            this.Controls.Add(LblUnitGroup);
            this.Controls.Add(btnUnitGroupAdd);
            this.Controls.Add(btnUnitGroupDel);
            this.Controls.Add(LbxUnitGroup);

            this.Controls.Add(TxtUnitModel);
            this.Controls.Add(LblUnitModel);
            this.Controls.Add(btnUnitModelAdd);
            this.Controls.Add(btnUnitModelDel);
            this.Controls.Add(LbxUnitModel);


            btnUnitGroupAdd.Click += (sender, e) => { UnitGroupAdd(); };
            btnUnitModelAdd.Click += (sender, e) => { UnitModelAdd(); };
            LbxUnitGroup.SelectedIndexChanged += UnitGroupSelectedChanged;
            LbxUnitModel.SelectedIndexChanged += UnitModelSelectedChanged;
        }

        /// <summary>
        /// Dock 순서 조정
        /// </summary>
        private void InitializeDockIndex()
        {
            pnlUnitGroup.BringToFront();
            pnlUnitModel.BringToFront();
        }

        private void SetText()
        {
            LblUnitGroup.Text = RuntimeData.String("F030100");
            LblUnitModel.Text = RuntimeData.String("F030101");
        }

        private void SetPositionSize()
        {
            /* Form 사용하면 SizeChanged가 발동 안하고
             * TabPage 쓰면 Form Closed가 없고
             * 미쳐버리겠네 진짜
             */
            int margin = 3;

            LblUnitGroup.Location = new Point(margin, margin);
            LblUnitGroup.Size = new Size(160 + (margin * 2), 30);

            TxtUnitGroup.Location = new Point(margin,
                LblUnitGroup.Location.Y + LblUnitGroup.Height + margin);
            TxtUnitGroup.Size = new Size(100, 27);

            btnUnitGroupAdd.Location = new Point(TxtUnitGroup.Location.X + TxtUnitGroup.Width + margin,
                TxtUnitGroup.Location.Y - 2);
            btnUnitGroupAdd.Size = new Size(30, 30);

            btnUnitGroupDel.Location = new Point(btnUnitGroupAdd.Location.X + btnUnitGroupAdd.Width + margin,
                btnUnitGroupAdd.Location.Y);
            btnUnitGroupDel.Size = btnUnitGroupAdd.Size;

            LbxUnitGroup.Location = new Point(margin,
                TxtUnitGroup.Location.Y +  TxtUnitGroup.Height + margin);
            LbxUnitGroup.Size = new Size(LblUnitGroup.Width,
                (this.Size.Height / 2) - (margin + LblUnitGroup.Height + margin + TxtUnitGroup.Height + margin));
            // - (margin + LblUnitGroup.Height + margin + TxtUnitGroup.Height + margin)

            //Unit 모델
            LblUnitModel.Location = new Point(margin,
                (this.Size.Height / 2) + (margin / 2));
            LblUnitModel.Size = LblUnitGroup.Size;

            TxtUnitModel.Location = new Point(margin,
                LblUnitModel.Location.Y + LblUnitModel.Height + margin);
            TxtUnitModel.Size = TxtUnitGroup.Size;

            btnUnitModelAdd.Location = new Point(btnUnitGroupAdd.Location.X,
                TxtUnitModel.Location.Y);
            btnUnitModelAdd.Size = btnUnitGroupAdd.Size;

            btnUnitModelDel.Location = new Point(btnUnitGroupDel.Location.X,
                TxtUnitModel.Location.Y);
            btnUnitModelDel.Size = btnUnitGroupAdd.Size;

            LbxUnitModel.Location = new Point(margin,
                TxtUnitModel.Location.Y + TxtUnitModel.Height + margin);
            LbxUnitModel.Size = new Size(LbxUnitGroup.Width,
                LbxUnitGroup.Height);
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
            //this.pnlUnitGroup.Size = new Size(pnlUnitGroup.Width, this.Size.Height / 2);
        }

        /// <summary>
        /// Unit Group 선택Index 변경 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitGroupSelectedChanged(object sender, EventArgs e)
        {
            this.SelectedGroup = LbxUnitGroup.SelectedItem as string;
            LbxUnitModel.Items.Clear();

            //하위 모델이 있을경우 첫번째 Item 선택
            if (dicUnitGroups[SelectedGroup].Count > 0)
            {
                //해당Group의 Model리스트 조회
                foreach (string model in dicUnitGroups[SelectedGroup].Keys)
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
        }

        #endregion Event End

        /// <summary>
        /// Unit Group 생성
        /// </summary>
        private void UnitGroupAdd()
        {
            string unitGroupName = TxtUnitGroup.Text.Trim();

            //빈칸입력인지 검사
            if (unitGroupName == "")
            {
                MessageBox.Show(RuntimeData.String("F030000"));
                return;
            }
            //동일한 Group명 있는지 검사
            if (dicUnitGroups.Keys.Contains(unitGroupName))
            {
                MessageBox.Show(RuntimeData.String("F030001"));
                return;
            }

            LbxUnitGroup.Items.Add(unitGroupName);

            //후처리
            dicUnitGroups.Add(unitGroupName, new Dictionary<string, int>());
            TxtUnitGroup.Text = "";
            LbxUnitGroup.SelectedItem = unitGroupName;
        }

        /// <summary>
        /// Unit Model 생성
        /// </summary>
        private void UnitModelAdd()
        {
            string unitModelName = TxtUnitModel.Text.Trim();

            //Group이 선택된 상태인지 검사
            if(SelectedGroup == null || SelectedGroup == string.Empty || SelectedGroup == "")
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
            if (dicUnitGroups[SelectedGroup].Keys.Contains(unitModelName))
            {
                MessageBox.Show(RuntimeData.String("F030001"));
                return;
            }

            LbxUnitModel.Items.Add(unitModelName);

            //후처리
            dicUnitGroups[SelectedGroup].Add(unitModelName, 0);
            TxtUnitModel.Text = "";
            LbxUnitModel.SelectedItem = unitModelName;
        }

        public void UnitInfoSave()
        {
            //xmlNode 추가인데..... 수정하면 한줄만 추가하는 방식으로는 못만드나?
            //불러온상태에서 추가할떄마다 그 정보만 Update한다던가
            //없는 unit정보일때 Add하는방식
            XmlDocument xdoc = new XmlDocument();
            //xdoc.AppendChild(xdoc.CreateXmlDeclaration("1.0", "UTF-8", ""));
            XmlNode root = xdoc.CreateElement("UnitList");
            xdoc.AppendChild(root);

            foreach (string unitGroup in dicUnitGroups.Keys)
            {
                XmlNode xmlgroup = xdoc.CreateElement("UnitGroup");
                //그룹 이름 정의
                XmlAttribute groupName = xdoc.CreateAttribute("Name");
                groupName.Value = unitGroup;
                xmlgroup.Attributes.Append(groupName);

                foreach (string unitModel in dicUnitGroups[unitGroup].Keys)
                {
                    XmlNode xmlUnitModel = xdoc.CreateElement("Model");
                    //Model 이름 정의
                    XmlAttribute modelName = xdoc.CreateAttribute("Name");
                    modelName.Value = unitModel;
                    xmlUnitModel.Attributes.Append(modelName);

                    //그룹 하위로 추가
                    xmlgroup.AppendChild(xmlUnitModel);
                }

                root.AppendChild(xmlgroup);
            }

            //작성파일 저장
            xdoc.Save(InfoFilePath);
        }

        private void UnitInfoLoad()
        {
            if (File.Exists(InfoFilePath))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(InfoFilePath);

                if (xdoc.ChildNodes.Count > 0)
                {
                    XmlNode unitList = xdoc.SelectSingleNode("UnitList");
                    dicUnitGroups = new Dictionary<string, Dictionary<string, int>>();

                    //가져온 Node Dictionary에 추가
                    foreach (XmlNode groupNode in unitList.ChildNodes)
                    {
                        //Dictionary에 추가
                        string groupName = groupNode.Attributes["Name"].Value;
                        dicUnitGroups.Add(groupName, new Dictionary<string, int>());
                        //ListBox에 추가
                        LbxUnitGroup.Items.Add(groupName);

                        foreach (XmlNode unitModel in groupNode.ChildNodes)
                        {
                            string modelName = unitModel.Attributes["Name"].Value;
                            //Dictionary에 추가
                            dicUnitGroups[groupName].Add(modelName, 0);

                            //ListBox에 추가
                            LbxUnitModel.Items.Add(modelName);
                        }
                    }

                    //그룹항목이 있을경우 첫번째 항목 선택
                    if(LbxUnitGroup.Items.Count > 0)
                    {
                        LbxUnitGroup.SelectedIndex = 0;
                    }
                }
            }
        }
    }
}
