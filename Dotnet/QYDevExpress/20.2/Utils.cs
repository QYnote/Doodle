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
        public static DevExpress.Skins.Skin CurrentSkin => DevExpress.Skins.SkinManager.Default.GetSkin(DevExpress.Skins.SkinProductId.Grid, DevExpress.LookAndFeel.UserLookAndFeel.Default.ActiveLookAndFeel);

        public static void SetLookupComboBox(DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit rpoLup, object dataSource, string displayMember = "", string valueMemeber = "")
        {
            rpoLup.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            rpoLup.DataSource = dataSource;
            rpoLup.DisplayMember = displayMember == "" ? null : displayMember;
            rpoLup.ValueMember = valueMemeber == "" ? null : valueMemeber;
            rpoLup.PopulateColumns();
        }

        /// <summary>
        /// TreelistNode 
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<DevExpress.XtraTreeList.Nodes.TreeListNode> GetNodeList(DevExpress.XtraTreeList.Nodes.TreeListNodes nodes)
        {
            foreach (DevExpress.XtraTreeList.Nodes.TreeListNode node in nodes)
            {
                yield return node;
                foreach (var child in GetNodeList(node.Nodes))
                {
                    yield return child;
                }
            }
        }
    }
}
