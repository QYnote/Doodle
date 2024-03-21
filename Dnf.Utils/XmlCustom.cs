using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Dnf.Utils
{
    public static class XmlCustom
    {
        public static void DataTableToXML(DataTable dt, string fileName)
        {
            string path = string.Format("{0}\\{1}.xml", Data_Runtime.DataPath, fileName);

            dt.WriteXml(path);
        }

        public static DataTable XMLtoDataTable(string fileName)
        {
            string path = string.Format("{0}\\{1}.xml", Data_Runtime.DataPath, fileName);
            DataTable dt = null;

            if(File.Exists(path))
            {
                dt.ReadXml(path);
            }

            return dt;
        }


    }
}
