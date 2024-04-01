using Dnf.Communication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dnf.Communication.Controls
{
    //외부 데이터 연동
    public class ExternalData
    {
        public static string FilePath = RuntimeData.DataPath;

        #region XML

        /// <summary>
        /// Model정보 가져오기 ver.XML
        /// </summary>
        /// <param name="model">가져올 Model</param>
        /// <param name="attribute">가져올 Attribute</param>
        /// <returns></returns>
        public static string XmlLoad_ModelStruct(UnitModel model, string attribute)
        {
            XmlDocument xdoc = new XmlDocument();
            try
            {
                string path = string.Format("{0}\\UnitData\\{1}.xml", FilePath, "ModelStruct");

                if (!System.IO.File.Exists(path)) { throw new Exception(RuntimeData.String("XmlLoad ModelStruct Error")); }

                xdoc.Load(path);
                XmlElement root = xdoc.DocumentElement;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Attributes["Name"].Value != model.ToString()) continue;

                    return node.Attributes[attribute].Value;
                }

                return null;
            }
            catch {
                return null;
            }
        }

        #endregion XML End

        #region JSON

        #endregion
    }
}
