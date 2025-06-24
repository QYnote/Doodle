using Dnf.Comm.Controls;
using Dnf.Comm.Controls.PCPorts;
using Dnf.Utils.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Communication.Frm
{
    internal delegate void DataLogHandler(string type, byte[] data);

    public partial class FrmTester : Form
    {
        #region UI Controls

        private ComboBox cboPortList = new ComboBox();
        private Button btnConn = new Button();
        private Label lblConnStatus = new Label();

        private GroupBox gbxBaudrate = new GroupBox();
        private GroupBox gbxParity = new GroupBox();
        private GroupBox gbxStopBits = new GroupBox();
        private GroupBox gbxDataBits = new GroupBox();

        private CheckBox chkRewrite = new CheckBox();
        private NumericUpDown numRewrite = new NumericUpDown();
        private CheckBox chkRewriteInfi = new CheckBox();

        private Label lblWriteDesc = new Label();
        private TextBox txtWrite = new TextBox();
        private Button btnWrite = new Button();

        private DataGridView gvLogData = new DataGridView();

        #endregion UI Controls

        private int[] _baudrateList = new int[] { 9600, 19200, 38400, 57600, 115200 };
        private byte[] _dataBits = new byte[] { 7, 8 };
        private DataTable _dtLogData = new DataTable();

        private ProgramPort _port;

        public FrmTester()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            this.cboPortList.Location = new Point(3, 3);
            this.cboPortList.Items.AddRange(SerialPort.GetPortNames());
            this.cboPortList.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboPortList.Width = 100;

            this.btnConn.Location = new Point(this.cboPortList.Location.X, this.cboPortList.Location.Y + this.cboPortList.Height);
            this.btnConn.Width = this.cboPortList.Width - 21;
            this.btnConn.Text = "Connect";
            this.btnConn.Click += (s, e) =>
            {
                if(this.btnConn.Text == "Connect")
                {
                    this.btnConn.Text = "Disconnect";
                    this.lblConnStatus.BackColor = Color.Green;
                }
                else
                {
                    this.btnConn.Text = "Connect";
                    this.lblConnStatus.BackColor = Color.Red;
                }
            };

            this.lblConnStatus.Size = new Size(19, 19);
            this.lblConnStatus.Location = new Point(this.btnConn.Location.X + this.btnConn.Width + 1, this.btnConn.Location.Y + 2);
            this.lblConnStatus.BackColor = Color.Red;

            //Baudrate
            this.gbxBaudrate.Location = new Point(this.cboPortList.Location.X + this.cboPortList.Width + 3, this.cboPortList.Location.Y);
            this.gbxBaudrate.Text = "Baudrate";
            foreach (var baud in this._baudrateList)
            {
                RadioButton rdo = this.CreateRdo(baud.ToString("#,#"));
                rdo.TextAlign = ContentAlignment.MiddleRight;

                this.gbxBaudrate.Width = rdo.Width + 20;
                this.gbxBaudrate.Controls.Add(rdo);

                rdo.BringToFront();
            }
            this.gbxBaudrate.Height = this.gbxBaudrate.Controls.Count * 22 + 6;
            (this.gbxBaudrate.Controls[this.gbxBaudrate.Controls.Count - 1] as RadioButton).Checked = true;

            //Parity
            this.gbxParity.Location = new Point(this.gbxBaudrate.Location.X + this.gbxBaudrate.Width + 3, this.gbxBaudrate.Location.Y);
            this.gbxParity.Text = "Parity";
            foreach (Parity pairty in QYUtils.EnumToItems<Parity>())
            {
                RadioButton rdo = this.CreateRdo(pairty);

                this.gbxParity.Width = rdo.Width + 20;
                this.gbxParity.Controls.Add(rdo);

                rdo.BringToFront();
            }
            this.gbxParity.Height = this.gbxBaudrate.Height;
            (this.gbxParity.Controls[this.gbxParity.Controls.Count - 1] as RadioButton).Checked = true;

            //StopBits
            this.gbxStopBits.Location = new Point(this.gbxParity.Location.X + this.gbxParity.Width + 3, this.gbxParity.Location.Y);
            this.gbxStopBits.Text = "StopBits";
            foreach (StopBits stop in QYUtils.EnumToItems<StopBits>())
            {
                RadioButton rdo = this.CreateRdo(stop);

                this.gbxStopBits.Width = rdo.Width + 20;
                this.gbxStopBits.Controls.Add(rdo);

                rdo.BringToFront();
            }
            this.gbxStopBits.Height = this.gbxBaudrate.Height;
            (this.gbxStopBits.Controls[this.gbxStopBits.Controls.Count - 1] as RadioButton).Checked = true;

            //DataBits
            this.gbxDataBits.Location = new Point(this.gbxStopBits.Location.X + this.gbxStopBits.Width + 3, this.gbxStopBits.Location.Y);
            this.gbxDataBits.Text = "DataBits";
            foreach (byte data in this._dataBits)
            {
                RadioButton rdo = this.CreateRdo(data);

                this.gbxDataBits.Controls.Add(rdo);

                rdo.BringToFront();
            }
            this.gbxDataBits.Height = this.gbxBaudrate.Height;
            this.gbxDataBits.Width = 65;
            (this.gbxDataBits.Controls[this.gbxDataBits.Controls.Count - 1] as RadioButton).Checked = true;

            //반복전송
            this.chkRewrite.Location = new Point(this.gbxDataBits.Location.X + this.gbxDataBits.Width + 3, this.gbxDataBits.Location.Y);
            this.chkRewrite.Width = 73;
            this.chkRewrite.Text = "반복전송";
            this.chkRewrite.CheckAlign = ContentAlignment.MiddleLeft;
            this.chkRewrite.Checked = false;
            this.chkRewrite.CheckedChanged += (s, e) =>
            {
                this.chkRewriteInfi.Enabled = this.chkRewrite.Checked;
                this.chkRewriteInfi.Checked = false;
                this.numRewrite.Enabled = this.chkRewrite.Checked;
            };

            this.numRewrite.Location = new Point(this.chkRewrite.Location.X + this.chkRewrite.Width + 3, this.chkRewrite.Location.Y + 2);
            this.numRewrite.Width = 60;
            this.numRewrite.Value = 100;
            this.numRewrite.Minimum = 2;
            this.numRewrite.Enabled = false;

            this.chkRewriteInfi.Location = new Point(this.chkRewrite.Location.X, this.chkRewrite.Location.Y + this.chkRewrite.Height + 3);
            this.chkRewriteInfi.Width = 73;
            this.chkRewriteInfi.Text = "무한반복";
            this.chkRewriteInfi.Checked = false;
            this.chkRewriteInfi.CheckAlign = ContentAlignment.MiddleLeft;
            this.chkRewriteInfi.CheckedChanged += (s, e) =>
            {
                this.numRewrite.Enabled = !this.chkRewriteInfi.Checked;
            };
            this.chkRewriteInfi.Enabled = false;

            //전송값
            this.btnWrite.Width = 60;
            this.btnWrite.Location = new Point(this.ClientSize.Width - (this.btnWrite.Width + 3), this.gbxDataBits.Location.Y + this.gbxDataBits.Height - this.btnWrite.Height);
            this.btnWrite.Text = "Write";
            this.btnWrite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnWrite.Click += BtnWrite_Click;

            this.txtWrite.Location = new Point(this.gbxDataBits.Location.X + this.gbxDataBits.Width + 3, this.btnWrite.Location.Y);
            this.txtWrite.Width = this.btnWrite.Location.X - (this.txtWrite.Location.X + 3);
            this.txtWrite.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.lblWriteDesc.Location = new Point(this.txtWrite.Location.X, this.txtWrite.Location.Y - (this.txtWrite.Height - 3));
            this.lblWriteDesc.AutoSize = false;
            this.lblWriteDesc.Height = 18;
            this.lblWriteDesc.Width = 140;
            this.lblWriteDesc.Text = "Dec: @000 / Hex: #00";
            this.lblWriteDesc.TextAlign = ContentAlignment.MiddleLeft;

            //데이터 로그 Grid
            this.gvLogData.Location = new Point(this.cboPortList.Location.X, this.gbxBaudrate.Location.Y + this.gbxBaudrate.Height + 3);
            this.gvLogData.Size = new Size(600, this.ClientSize.Height - (this.gvLogData.Location.Y + 3));
            this.gvLogData.DataSource = this._dtLogData;
            this.gvLogData.AllowUserToResizeColumns = false;
            this.gvLogData.AllowUserToResizeRows = false;
            this.gvLogData.AllowUserToAddRows = false;
            this.gvLogData.RowHeadersVisible = false;
            this.gvLogData.AutoGenerateColumns = false;
            this.gvLogData.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.gvLogData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;


            this.Controls.Add(this.cboPortList);
            this.Controls.Add(this.btnConn);
            this.Controls.Add(this.lblConnStatus);
            this.Controls.Add(this.gbxBaudrate);
            this.Controls.Add(this.gbxParity);
            this.Controls.Add(this.gbxStopBits);
            this.Controls.Add(this.gbxDataBits);
            this.Controls.Add(this.chkRewrite);
            this.Controls.Add(this.numRewrite);
            this.Controls.Add(this.chkRewriteInfi);
            this.Controls.Add(this.lblWriteDesc);
            this.Controls.Add(this.txtWrite);
            this.Controls.Add(this.btnWrite);
            this.Controls.Add(this.gvLogData);

            this._port = new ProgramPort("COM5", 9600, 8, Parity.None, StopBits.One);
        }


        private RadioButton CreateRdo(object data)
        {
            RadioButton rdo = new RadioButton();
            rdo.Height = 19;
            rdo.Checked = false;
            rdo.Dock = DockStyle.Top;
            rdo.Width = (int)this.CreateGraphics().MeasureString(data.ToString(), rdo.Font).Width;
            rdo.TextAlign = ContentAlignment.MiddleLeft;
            rdo.Text = data.ToString();

            return rdo;
        }

        private void InitLogGrid()
        {
            this._dtLogData.Rows.Clear();
            this._dtLogData.Columns.Clear();
            this.gvLogData.Columns.Clear();

            this._dtLogData.Columns.Add(new DataColumn("Type", typeof(string)) { DefaultValue = string.Empty });
            this._dtLogData.Columns.Add(new DataColumn("Time", typeof(DateTime)));

            DataGridViewTextBoxColumn colType = new DataGridViewTextBoxColumn();
            colType.HeaderText = "Type";
            colType.DataPropertyName = "Type";
            colType.ReadOnly = true;
            colType.Width = 35;
            colType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn colTime = new DataGridViewTextBoxColumn();
            colTime.HeaderText = "Time";
            colTime.DataPropertyName = "Time";
            colTime.ReadOnly = true;
            colTime.Width = 130;
            colTime.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.gvLogData.Columns.Add(colType);
            this.gvLogData.Columns.Add(colTime);

            this.gvLogData.EndEdit();
        }

        private void BtnWrite_Click(object sender, EventArgs e)
        {
            InitLogGrid();

            List<byte> dataList = new List<byte>();
            byte[] data = null;

            string txt = this.txtWrite.Text;
            int txtHandle = 0;

            try
            {
                while (txt.Length > txtHandle)
                {
                    if (txt[txtHandle] == '@')
                    {
                        if (txt.Length <= txtHandle + 3 + 1) break;

                        string value = txt.Substring(++txtHandle, 3);
                        dataList.Add(Convert.ToByte(value));

                        txtHandle += 3;
                    }
                    else if (txt[txtHandle] == '#')
                    {
                        if (txt.Length <= txtHandle + 2 + 1) break;

                        string value = txt.Substring(++txtHandle, 2);
                        dataList.Add(Convert.ToByte(value, 16));

                        txtHandle += 2;
                    }
                    else
                        txtHandle++;
                }

                if (dataList.Count > 0)
                    data = dataList.ToArray();

                WriteData(data);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format("[Error]Write Error\nMessage:{0}\n\nTrace:{1}", ex.Message, ex.StackTrace));
            }
        }

        private void WriteData(byte[] data)
        {
            WriteDataLog("Req", data);
        }

        private void WriteDataLog(string type, byte[] data)
        {
            if (data == null) return;

            DataRow dr = this._dtLogData.NewRow();
            dr["Type"] = type;
            dr["Time"] = DateTime.Now.ToString();

            for (int i = 0; i < data.Length; i++)
            {
                string colName = string.Format("Col{0}", i);

                if(this._dtLogData.Columns.Contains(colName) == false)
                {
                    this._dtLogData.Columns.Add(colName, typeof(string));

                    DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                    col.HeaderText = (i + 1).ToString();
                    col.DataPropertyName = colName;
                    col.ReadOnly = true;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.Width = 24;
                    col.FillWeight = 1;

                    this.gvLogData.Columns.Add(col);
                }

                dr[colName] = data[i].ToString("X2");
            }

            this._dtLogData.Rows.Add(dr);
        }

    }
}
