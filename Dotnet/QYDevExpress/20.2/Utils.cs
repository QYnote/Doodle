using DevExpress.XtraEditors.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYDevExpress._20._2
{
    public class Utils
    {
        public static void SetLookupComboBox(RepositoryItemLookUpEdit rpoLup, object dataSource, string displayMember = "", string valueMemeber = "")
        {
            rpoLup.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            rpoLup.DataSource = dataSource;
            rpoLup.DisplayMember = displayMember == "" ? null : displayMember;
            rpoLup.ValueMember = valueMemeber == "" ? null : valueMemeber;
            rpoLup.PopulateColumns();
        }
    }
}
