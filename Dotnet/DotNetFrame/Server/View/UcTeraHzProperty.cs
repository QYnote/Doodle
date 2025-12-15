using DotNet.Utils.Controls.Utils;
using DotNetFrame.Base.Model;
using DotNetFrame.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFrame.Server.View
{
    public partial class UcTeraHzProperty : UserControl
    {
        #region UI Controls

        private Label lbl_Server_IP = new Label();
        private TextBox txt_Server_IP = new TextBox();
        private Label lbl_Server_PortNo = new Label();
        private NumericUpDown num_Server_PortNo = new NumericUpDown();
        private Label lbl_Sensor_Count = new Label();
        private NumericUpDown num_Sensor_Count = new NumericUpDown();
        private CheckBox chk_Sensor_Offset_Object = new CheckBox();
        private NumericUpDown num_Sensor_Offset_Object = new NumericUpDown();
        private CheckBox chk_Sensor_Offset_Max = new CheckBox();
        private NumericUpDown num_Sensor_Offset_Max = new NumericUpDown();
        private CheckBox chk_Sensor_Offset_Random = new CheckBox();
        private Label lbl_Sensor_Offset_BoundScale = new Label();
        private NumericUpDown num_Sensor_Offset_BoundScale = new NumericUpDown();

        private Button btnConnection = new Button();

        private TextBox txtLog = null;

        #endregion UI Controls

        private VM_Server_HY_TeraHz _teraHz = new VM_Server_HY_TeraHz();

        internal UcTeraHzProperty(TextBox txtLog)
        {
            this.txtLog = txtLog;

            InitializeComponent();
            this.InitUI();
            this.InitText();

            this._teraHz.ServerLog += ServerLog;
            this.VisibleChanged += UcTeraHzProperty_VisibleChanged;
        }

        private void InitUI()
        {
            this.Dock = DockStyle.Top;

            this.lbl_Server_IP.Location = new Point(3, 3);
            this.lbl_Server_IP.TextAlign = ContentAlignment.MiddleLeft;
            this.txt_Server_IP.Left = this.lbl_Server_IP.Right + 3;
            this.txt_Server_IP.Top = this.lbl_Server_IP.Top;
            this.txt_Server_IP.Width = 80;
            this.txt_Server_IP.TextAlign = HorizontalAlignment.Center;
            this.txt_Server_IP.Text = this._teraHz.IP;
            this.txt_Server_IP.KeyPress += QYUtils.Event_KeyPress_IP;
            this.txt_Server_IP.TextChanged += TxtIP_TextChanged;

            this.lbl_Server_PortNo.Left = this.lbl_Server_IP.Left;
            this.lbl_Server_PortNo.Top = this.lbl_Server_IP.Bottom + 3;
            this.lbl_Server_PortNo.TextAlign = ContentAlignment.MiddleLeft;
            this.num_Server_PortNo.Left = this.lbl_Server_PortNo.Right + 3;
            this.num_Server_PortNo.Top = this.lbl_Server_PortNo.Top;
            this.num_Server_PortNo.Width = this.txt_Server_IP.Width;
            this.num_Server_PortNo.DecimalPlaces = 0;
            this.num_Server_PortNo.TextAlign = HorizontalAlignment.Right;
            this.num_Server_PortNo.Minimum = 0;
            this.num_Server_PortNo.Maximum = int.MaxValue;
            this.num_Server_PortNo.Value = this._teraHz.PortNo;
            this.num_Server_PortNo.ValueChanged += NumPortNo_ValueChanged;

            this.lbl_Sensor_Count.Left = this.lbl_Server_PortNo.Left;
            this.lbl_Sensor_Count.Top = this.lbl_Server_PortNo.Bottom + 3;
            this.lbl_Sensor_Count.TextAlign = ContentAlignment.MiddleLeft;
            this.num_Sensor_Count.Left = this.lbl_Sensor_Count.Right + 3;
            this.num_Sensor_Count.Top = this.lbl_Sensor_Count.Top;
            this.num_Sensor_Count.Width = this.txt_Server_IP.Width;
            this.num_Sensor_Count.DecimalPlaces = 0;
            this.num_Sensor_Count.TextAlign = HorizontalAlignment.Right;
            this.num_Sensor_Count.Minimum = 0;
            this.num_Sensor_Count.Maximum = UInt64.MaxValue;
            this.num_Sensor_Count.Value = VM_Server_HY_TeraHz.DEFAULT_SENSOR_COUNT;
            this.num_Sensor_Count.ValueChanged += NumSensor_ValueChanged;

            this.chk_Sensor_Offset_Object.Left = this.lbl_Sensor_Count.Left;
            this.chk_Sensor_Offset_Object.Top = this.lbl_Sensor_Count.Bottom + 3;
            this.chk_Sensor_Offset_Object.Width = this.lbl_Sensor_Count.Width;
            this.chk_Sensor_Offset_Object.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_Sensor_Offset_Object.Checked = false;
            this.chk_Sensor_Offset_Object.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_Sensor_Offset_Object.CheckedChanged += Chk_Sensor_Offset_Object_CheckedChanged;
            this.num_Sensor_Offset_Object.Left = this.chk_Sensor_Offset_Object.Right + 3;
            this.num_Sensor_Offset_Object.Top = this.chk_Sensor_Offset_Object.Top;
            this.num_Sensor_Offset_Object.Width = this.txt_Server_IP.Width;
            this.num_Sensor_Offset_Object.DecimalPlaces = 0;
            this.num_Sensor_Offset_Object.TextAlign = HorizontalAlignment.Right;
            this.num_Sensor_Offset_Object.Minimum = 0;
            this.num_Sensor_Offset_Object.Maximum = UInt16.MaxValue;
            this.num_Sensor_Offset_Object.Value = VM_Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_OBJECT;
            this.num_Sensor_Offset_Object.Enabled = false;
            this.num_Sensor_Offset_Object.ValueChanged += Num_sensor_offset_object_ValueChanged;

            this.chk_Sensor_Offset_Max.Left = this.chk_Sensor_Offset_Object.Left;
            this.chk_Sensor_Offset_Max.Top = this.chk_Sensor_Offset_Object.Bottom + 3;
            this.chk_Sensor_Offset_Max.Width = this.lbl_Sensor_Count.Width;
            this.chk_Sensor_Offset_Max.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_Sensor_Offset_Max.Checked = false;
            this.chk_Sensor_Offset_Max.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_Sensor_Offset_Max.CheckedChanged += Chk_Sensor_Offset_Max_CheckedChanged;
            this.num_Sensor_Offset_Max.Left = this.chk_Sensor_Offset_Max.Right + 3;
            this.num_Sensor_Offset_Max.Top = this.chk_Sensor_Offset_Max.Top;
            this.num_Sensor_Offset_Max.Width = this.txt_Server_IP.Width;
            this.num_Sensor_Offset_Max.DecimalPlaces = 0;
            this.num_Sensor_Offset_Max.TextAlign = HorizontalAlignment.Right;
            this.num_Sensor_Offset_Max.Minimum = 0;
            this.num_Sensor_Offset_Max.Maximum = UInt16.MaxValue;
            this.num_Sensor_Offset_Max.Value = VM_Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_MAX;
            this.num_Sensor_Offset_Max.Enabled = false;
            this.num_Sensor_Offset_Max.ValueChanged += Num_sensor_offset_max_ValueChanged;

            this.chk_Sensor_Offset_Random.Left = this.chk_Sensor_Offset_Max.Left;
            this.chk_Sensor_Offset_Random.Top = this.chk_Sensor_Offset_Max.Bottom + 3;
            this.chk_Sensor_Offset_Random.Width = this.lbl_Sensor_Count.Width;
            this.chk_Sensor_Offset_Random.CheckAlign = ContentAlignment.MiddleRight;
            this.chk_Sensor_Offset_Random.Checked = true;
            this.chk_Sensor_Offset_Random.TextAlign = ContentAlignment.MiddleLeft;
            this.chk_Sensor_Offset_Random.CheckedChanged += Chk_Sensor_Offset_Random_CheckedChanged;

            this.lbl_Sensor_Offset_BoundScale.Left = this.chk_Sensor_Offset_Random.Left;
            this.lbl_Sensor_Offset_BoundScale.Top = this.chk_Sensor_Offset_Random.Bottom + 3;
            this.lbl_Sensor_Offset_BoundScale.TextAlign = ContentAlignment.MiddleLeft;
            this.num_Sensor_Offset_BoundScale.Left = this.lbl_Sensor_Offset_BoundScale.Right + 3;
            this.num_Sensor_Offset_BoundScale.Top = this.lbl_Sensor_Offset_BoundScale.Top;
            this.num_Sensor_Offset_BoundScale.Width = this.txt_Server_IP.Width;
            this.num_Sensor_Offset_BoundScale.DecimalPlaces = 0;
            this.num_Sensor_Offset_BoundScale.TextAlign = HorizontalAlignment.Right;
            this.num_Sensor_Offset_BoundScale.Minimum = 0;
            this.num_Sensor_Offset_BoundScale.Maximum = UInt16.MaxValue;
            this.num_Sensor_Offset_BoundScale.Value = VM_Server_HY_TeraHz.DEFAULT_SENSOR_OFFSET_BOUNDSCALE;
            this.num_Sensor_Offset_BoundScale.ValueChanged += Num_sensor_offset_boundScale_ValueChanged;

            this.btnConnection.Left = this.lbl_Sensor_Offset_BoundScale.Left;
            this.btnConnection.Top = this.lbl_Sensor_Offset_BoundScale.Bottom + 3;
            this.btnConnection.TextAlign = ContentAlignment.MiddleCenter;
            this.btnConnection.Tag = false;
            this.btnConnection.Click += BtnConnection_Click;

            this.Height = this.btnConnection.Bottom + 3;

            this.Controls.Add(this.lbl_Server_IP);
            this.Controls.Add(this.txt_Server_IP);
            this.Controls.Add(this.lbl_Server_PortNo);
            this.Controls.Add(this.num_Server_PortNo);
            this.Controls.Add(this.lbl_Sensor_Count);
            this.Controls.Add(this.num_Sensor_Count);
            this.Controls.Add(this.chk_Sensor_Offset_Object);
            this.Controls.Add(this.num_Sensor_Offset_Object);
            this.Controls.Add(this.chk_Sensor_Offset_Max);
            this.Controls.Add(this.num_Sensor_Offset_Max);
            this.Controls.Add(this.chk_Sensor_Offset_Random);
            this.Controls.Add(this.lbl_Sensor_Offset_BoundScale);
            this.Controls.Add(this.num_Sensor_Offset_BoundScale);
            this.Controls.Add(this.btnConnection);
        }

        private void InitText()
        {
            this.lbl_Server_IP.Text = AppData.Lang("server.terahz.ip");
            this.lbl_Server_PortNo.Text = AppData.Lang("server.terahz.portno");
            this.lbl_Sensor_Count.Text = AppData.Lang("server.terahz.sensor.count");
            this.chk_Sensor_Offset_Object.Text = AppData.Lang("server.terahz.sensor.offset.object");
            this.chk_Sensor_Offset_Max.Text = AppData.Lang("server.terahz.sensor.offset.max");
            this.chk_Sensor_Offset_Random.Text = AppData.Lang("server.terahz.sensor.offset.random");
            this.lbl_Sensor_Offset_BoundScale.Text = AppData.Lang("server.terahz.sensor.offset.boundscale");
            this.btnConnection.Text = AppData.Lang("server.terahz.connection.connect");
        }

        private void TxtIP_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;

            this._teraHz.IP = txt.Text;
        }

        private void NumPortNo_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._teraHz.PortNo = Convert.ToInt32(num.Value);
        }

        private void Chk_Sensor_Offset_Object_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            this._teraHz.ApplyObject = chk.Checked;

            this.num_Sensor_Offset_Object.Enabled = chk.Checked;
        }

        private void NumSensor_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            try
            {
                this._teraHz.SensorCount = Convert.ToInt32(num.Value);
            }
            catch (ArgumentOutOfRangeException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Message: {ex.Message}\r\nTrace:{ex.StackTrace}");
            }
        }

        private void Num_sensor_offset_object_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._teraHz.OffsetObject = Convert.ToInt16(num.Value);
        }

        private void Chk_Sensor_Offset_Max_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            this._teraHz.ApplyMax = chk.Checked;

            this.num_Sensor_Offset_Max.Enabled = chk.Checked;
            this.chk_Sensor_Offset_Object.Enabled = !chk.Checked;
        }

        private void Num_sensor_offset_max_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._teraHz.OffsetMax = Convert.ToInt16(num.Value);
        }

        private void Chk_Sensor_Offset_Random_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            this._teraHz.ApplyRandom = chk.Checked;
            this.num_Sensor_Offset_BoundScale.Enabled = chk.Checked;
            this.chk_Sensor_Offset_Object.Enabled = !chk.Checked;
        }

        private void Num_sensor_offset_boundScale_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = sender as NumericUpDown;

            this._teraHz.OffsetBoundScale = Convert.ToInt16(num.Value);
        }

        private void BtnConnection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if((bool)btn.Tag)
            {
                this._teraHz.Close();

                this.txt_Server_IP.Enabled = true;
                this.num_Server_PortNo.Enabled = true;
                this.num_Sensor_Count.Enabled = true;
                btn.Text = AppData.Lang("server.terahz.connection.connect");

                btn.Tag = false;
            }
            else
            {
                this.txtLog.Text = string.Empty;

                this._teraHz.Open();

                this.txt_Server_IP.Enabled = false;
                this.num_Server_PortNo.Enabled = false;
                this.num_Sensor_Count.Enabled = false;
                btn.Text = AppData.Lang("server.terahz.connection.disconnect");

                btn.Tag = true;
            }
        }

        private void ServerLog(params object[] obj)
        {
            if (obj[0] is string == false || this.IsDisposed) return;

            if (this.InvokeRequired)
                this.BeginInvoke(new Update_WithParam(ServerLog), new object[] { obj });
            else
            {
                string txt = obj[0] as string;

                this.txtLog.AppendText(string.Format("{0}: {1}\r\n", DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff"), txt));
            }
        }

        private void UcTeraHzProperty_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == false)
            {
                this._teraHz.Close();
            }
        }
    }
}
