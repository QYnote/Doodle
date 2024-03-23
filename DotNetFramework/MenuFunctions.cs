using Dnf.Communication;
using Dnf.Utils.Controls;
using DotNetFramework.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DotNetFramework
{
    internal class MenuFunctions
    {
        private MainForm form;

        internal MenuFunctions(MainForm mainForm) { 
            form = mainForm;
        }

        /// <summary>
        /// Item 생성 Page Create
        /// </summary>
        /// <param name="pageName">생성될 Page 명 및 코드</param>
        public void ItemCre(string pageName)
        {
            TabPage page = null;
            //TabPgae명 검색
            int tabIdx = form.TabCtrl.TabPages.IndexOfKey(pageName);

            if (tabIdx == -1)
            {
                //TapPage 신규 생성
                page = new FrmItemEdit(form);
                page.Padding = new Padding(3);
                page.UseVisualStyleBackColor = true;
                page.Name = pageName;
                page.Text = pageName;

                form.TabCtrl.TabPages.Add(page);
                page.Focus();
            }
            else
            {
                //해당 Tab 이동
                page = form.TabCtrl.TabPages[tabIdx];
                page.Focus();
            }
        }

        /// <summary>
        /// GroupBox에서 선택된 Port 연결Open
        /// </summary>
        public void PortOpen(TreeNode node)
        {
            if (node != null && node.Tag.GetType() == typeof(Port))
            {
                Port port = node.Tag as Port;

                //포트열기
                if (port.PortOpen())
                {
                    form.LblStatus.Text = RuntimeData.String("A007");
                }
                else
                {
                    form.LblStatus.Text = RuntimeData.String("A008");
                }
            }
            else
            {
                form.LblStatus.Text = RuntimeData.String("A002");
                return;
            }
        }

        /// <summary>
        /// GroupBox에서 선택된 Port 연결Close
        /// </summary>
        public void PortClose(TreeNode node)
        {
            if (node != null && node.Tag.GetType() == typeof(Port))
            {
                Port port = node.Tag as Port;

                //포트닫기
                if (port.PortClose())
                {
                    form.LblStatus.Text = RuntimeData.String("A009");
                }
                else
                {
                    form.LblStatus.Text = RuntimeData.String("A010");
                }
            }
            else
            {
                form.LblStatus.Text = RuntimeData.String("A002");
                return;
            }
        }

        #region XML

        public void XmlSave()
        {
            //Port 정보 저장
            if (RuntimeData.Ports == null || RuntimeData.Ports.Count == 0) return;

            XmlDocument xdoc = new XmlDocument();
            XmlNode rootNode = xdoc.CreateElement("Root");

            foreach (Port port in RuntimeData.Ports.Values)
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

                XmlAttribute attrParity = xdoc.CreateAttribute("Parity");
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

            xdoc.Save(string.Format("{0}\\{1}.xml", RuntimeData.DataPath, "PortInfo"));

            form.LblStatus.Text = RuntimeData.String("A014");
        }

        public void XmlLoad()
        {
            XmlDocument xdoc = new XmlDocument();
            try
            {
                string path = string.Format("{0}\\{1}.xml", RuntimeData.DataPath, "PortInfo");

                if (!System.IO.File.Exists(path)) { throw new Exception(RuntimeData.String("A016")); }

                xdoc.Load(path);
                XmlElement root = xdoc.DocumentElement;


                foreach (XmlNode nodePort in root.ChildNodes)
                {
                    Port port = new Port(
                        nodePort.Attributes["PortName"].Value,
                        (uProtocolType)Enum.Parse(typeof(uProtocolType), nodePort.Attributes["Protocol"].Value),
                        (BaudRate)Enum.Parse(typeof(BaudRate), nodePort.Attributes["BaudRate"].Value),
                        Convert.ToInt16(nodePort.Attributes["DataBits"].Value),
                        (Parity)Enum.Parse(typeof(Parity), nodePort.Attributes["Parity"].Value),
                        (StopBits)Enum.Parse(typeof(StopBits), nodePort.Attributes["StopBit"].Value)
                        );

                    RuntimeData.Ports.Add(port.PortName, port);

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

                form.InitTreeItem();
                form.LblStatus.Text = RuntimeData.String("A015");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion XML End
    }
}
