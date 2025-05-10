using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dnf.Comm.Data
{
    internal static class ucXML
    {
        private static string DefaultPath = Environment.CurrentDirectory + "\\Data\\";
        private static string UnitInfoFileName = "UnitInfo";

        /// <summary>
        /// UnitType List 호출
        /// </summary>
        /// <returns></returns>
        internal static string[] GetUnitTypeList()
        {
            string filePath = DefaultPath + UnitInfoFileName + ".xml";

            if (System.IO.File.Exists(filePath))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filePath);

                if (xdoc.ChildNodes.Count > 0)
                {
                    XmlNode unitList = xdoc.SelectSingleNode("UnitList");
                    List<string> typeList = new List<string>();

                    //type List에 추가
                    foreach (XmlNode typeNode in unitList.SelectNodes("UnitType"))
                    {
                        typeList.Add(typeNode.Attributes["Name"].Value);
                    }

                    return typeList.ToArray();
                }
            }

            return null;
        }

        /// <summary>
        /// UnitModel List 호출
        /// </summary>
        /// <param name="unitType">호출 할 UnitType</param>
        /// <returns></returns>
        internal static string[] GetUnitModelList(string unitType)
        {
            if (unitType == string.Empty || unitType == "") return null;

            string filePath = DefaultPath + UnitInfoFileName + ".xml";

            if (System.IO.File.Exists(filePath))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filePath);

                if (xdoc.ChildNodes.Count > 0)
                {
                    //Root
                    XmlNode unitList = xdoc.SelectSingleNode("UnitList");

                    //unitType에 해당하는 Node가져오기
                    string nodePath = string.Format("/UnitList/UnitType[@Name='{0}']", unitType);
                    XmlNode typeList = unitList.SelectSingleNode(nodePath);

                    if(typeList != null)
                    {
                        List<string> modelList = new List<string>();
                        foreach (XmlNode modelNode in typeList.SelectNodes("Model"))
                        {
                            modelList.Add(modelNode.Attributes["Name"].Value);
                        }

                        return modelList.ToArray();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// UnitModel이 지원하는 Protocol List 호출
        /// </summary>
        /// <param name="unitType"></param>
        /// <param name="unitModel"></param>
        /// <returns></returns>
        internal static uProtocolType[] GetSupportProtocolList(string unitType, string unitModel)
        {
            if (unitType == string.Empty || unitType == "") return null;

            string filePath = DefaultPath + UnitInfoFileName + ".xml";

            if (System.IO.File.Exists(filePath))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(filePath);

                if (xdoc.ChildNodes.Count > 0)
                {
                    //Root
                    XmlNode unitList = xdoc.SelectSingleNode("UnitList");

                    //unitType에 해당하는 Node가져오기
                    string nodePath = string.Format("UnitType[@Name='{0}']/Model[@Name='{1}']/SupportProtocol", unitType, unitModel);
                    XmlNode nodeProtocolList = unitList.SelectSingleNode(nodePath);

                    if (nodeProtocolList != null)
                    {
                        List<uProtocolType> protocolList = new List<uProtocolType>();
                        foreach (XmlNode protocolNode in nodeProtocolList.ChildNodes)
                        {
                            if(protocolNode.InnerText == "1")
                            {
                                protocolList.Add(protocolNode.Name.ToEnum<uProtocolType>());
                            }
                        }

                        return protocolList.ToArray();
                    }
                }
            }

            return null;
        }
    }
}
