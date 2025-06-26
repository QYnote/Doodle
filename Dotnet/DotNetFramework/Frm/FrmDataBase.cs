using DotNet.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFramework.Frm
{
    public partial class FrmDataBase : Form
    {
        private SQLCe _SQL = new SQLCe(
            "C:\\WorkerFile\\업무자료\\TCS\\TCS\\01_source\\01_TCS\\01_TCS\\TCS\\TCS.Data\\Products\\tcsdr.sdf",
            "admin123");

        #region UI Controls

        private ComboBox cboTables = new ComboBox();
        private DataGridView gvTable = new DataGridView();

        #endregion UI Controls

        public FrmDataBase()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            this.cboTables.Location = new Point(3, 3);
            this.cboTables.Width = 200;
            this.cboTables.DropDownStyle = ComboBoxStyle.DropDownList;
            UpdateTableList();
            this.cboTables.SelectedIndexChanged += (s, e) =>
            {
                UpdateGridColumns(this.cboTables.SelectedItem.ToString());
            };
            if (this.cboTables.Items.Count > 0) this.cboTables.SelectedIndex = 0;

            this.gvTable.Location = new Point(this.cboTables.Location.X, this.cboTables.Location.Y + this.cboTables.Height + 3);
            this.gvTable.Width = this.ClientSize.Width - 3;
            this.gvTable.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            this.Controls.Add(this.cboTables);
            this.Controls.Add(this.gvTable);
        }

        private void UpdateTableList()
        {
            string query = @"
            SELECT TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'TABLE';
            ";

            DataTable dt = this._SQL.ExcuteQuery<DataTable>(query);

            this.cboTables.Items.Clear();
            foreach (DataRow dr in dt.Rows)
                this.cboTables.Items.Add(dr["TABLE_NAME"].ToString());
        }

        private void UpdateGridColumns(string tableName)
        {
            string query = $@"
            SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,
            	CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = '{tableName}';
            SELECT * FROM {tableName};
            ";

            DataSet ds = this._SQL.ExcuteQuery<DataSet>(query);
            this.gvTable.Columns.Clear();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.HeaderText = dr["COLUMN_NAME"].ToString();
                col.DataPropertyName = col.HeaderText;
                switch (dr["DATA_TYPE"].ToString())
                {
                    case "nvarchar": col.MaxInputLength = dr["CHARACTER_MAXIMUM_LENGTH"] == null ? 100 : Convert.ToInt32(dr["CHARACTER_MAXIMUM_LENGTH"].ToString()); break;
                    case "numeric": col.ValueType = typeof(Int64); break;
                    case "int": col.ValueType = typeof(Int32); break;
                    case "smallint": col.ValueType = typeof(Int16); break;
                    case "tinyint": col.ValueType = typeof(byte); break;
                    case "bit": col.ValueType = typeof(bool); break;
                }
                
                this.gvTable.Columns.Add(col);
            }

            this.gvTable.DataSource = ds.Tables[1];
        }
    }
}
