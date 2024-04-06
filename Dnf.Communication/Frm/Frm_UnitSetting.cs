using Dnf.Communication.Data;
using Dnf.Utils.Controls;
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
        #endregion Controls End

        private string SelectedType = string.Empty;
        private string SelectedModel = string.Empty;

        private string InfoFilePath = RuntimeData.DataPath + "UnitInfo.xml";


        internal Frm_UnitSetting()
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
            InitializeUnitTypeModel();
            SetPositionSize();
            //InitializeUnitType();
            //InitializeUnitModel();
            //InitializeDockIndex();
            SetText();
        }

        /// <summary>
        /// Unit Type Control 생성
        /// </summary>
        private void InitializeUnitTypeModel()
        {
            LblUnitType.AutoSize = false;
            LblUnitType.TextAlign = ContentAlignment.MiddleCenter;
            LblUnitType.BorderStyle = BorderStyle.FixedSingle;

            TxtUnitType.AutoSize = false;
            TxtUnitType.MaxLength = 10;

            btnUnitTypeAdd.Text = "+";
            btnUnitTypeDel.Text = "-";

            LbxUnitType.AutoSize = false;
            LbxUnitType.Sorted = true;

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

            //지원 Protocl
            LblSupportProtocol.AutoSize = false;
            LblSupportProtocol.TextAlign = ContentAlignment.MiddleCenter;
            LblSupportProtocol.BorderStyle = BorderStyle.FixedSingle;

            CLbxSupportProtocol.AutoSize = false;
            CLbxSupportProtocol.Items.AddRange(UtilCustom.EnumToItems<uProtocolType>());
            CLbxSupportProtocol.CheckOnClick = true;    //항목 선택 시 Check 바로 변경

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


        private void SetText()
        {
            LblUnitType.Text = RuntimeData.String("F030100");
            LblUnitModel.Text = RuntimeData.String("F030101");
            LblSupportProtocol.Text = RuntimeData.String("F030102");
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
                (this.Size.Height / 2) - (margin + LblUnitType.Height + margin + TxtUnitType.Height + margin));
            // - (margin + LblUnitType.Height + margin + TxtUnitType.Height + margin)

            //Unit 모델
            LblUnitModel.Location = new Point(margin,
                (this.Size.Height / 2) + (margin / 2));
            LblUnitModel.Size = LblUnitType.Size;

            TxtUnitModel.Location = new Point(margin,
                LblUnitModel.Location.Y + LblUnitModel.Height + margin);
            TxtUnitModel.Size = TxtUnitType.Size;

            btnUnitModelAdd.Location = new Point(btnUnitTypeAdd.Location.X,
                TxtUnitModel.Location.Y);
            btnUnitModelAdd.Size = btnUnitTypeAdd.Size;

            btnUnitModelDel.Location = new Point(btnUnitTypeDel.Location.X,
                TxtUnitModel.Location.Y);
            btnUnitModelDel.Size = btnUnitTypeAdd.Size;

            LbxUnitModel.Location = new Point(margin,
                TxtUnitModel.Location.Y + TxtUnitModel.Height + margin);
            LbxUnitModel.Size = new Size(LbxUnitType.Width,
                LbxUnitType.Height);

            //지원 Protocol
            LblSupportProtocol.Location = new Point(LblUnitType.Location.X + LblUnitType.Width + margin,
                margin);
            LblSupportProtocol.Size = LblUnitType.Size;

            CLbxSupportProtocol.Location = new Point(LblSupportProtocol.Location.X,
                LblSupportProtocol.Location.Y + LblSupportProtocol.Height + margin);
            CLbxSupportProtocol.Size = new Size(LblSupportProtocol.Width,
                100);
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
            foreach (uProtocolType protocol in model.SupportProtocol.Keys)
            {
                int idx = CLbxSupportProtocol.Items.IndexOf(protocol);
                bool flag = model.SupportProtocol[protocol];

                CLbxSupportProtocol.SetItemChecked(idx, flag);
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
            if (SelectedType != string.Empty || SelectedType != "")
            {
                if (SelectedModel != string.Empty || SelectedModel != "")
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
