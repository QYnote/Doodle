using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Utils.Views
{
    public enum CtrlType
    {
        ComboBox,
        TextBox,
        MaskedTextBox,
        NumbericUpDown,
        CheckBox
    }

    /// <summary>
    /// Label + Control 합쳐진 ControlBox
    /// </summary>
    public partial class ucControlBox : UserControl
    {
        public Control ctrl;            //Control Type
        public Label lbl = new Label(); //Control 명
        
        /// <summary>
        /// Text Label Text
        /// </summary>
        public string LblText
        {
            get => lbl.Text;
            set => lbl.Text = value;
        }
        /// <summary>
        /// Text Label 너비
        /// </summary>
        public int LblWidth
        {
            get => lbl.Width;
            set => lbl.Width = value;
        }

        /// <summary>
        /// Control별 입력값
        /// </summary>
        public object Value
        {
            get
            {
                     if (ctrl.GetType() == typeof(TextBox))       { return (ctrl as TextBox).Text; }
                else if (ctrl.GetType() == typeof(MaskedTextBox)) { return (ctrl as MaskedTextBox).Text; }
                else if (ctrl.GetType() == typeof(NumericUpDown)) { return (ctrl as NumericUpDown).Value; }
                else if (ctrl.GetType() == typeof(ComboBox))      { return (ctrl as ComboBox).SelectedItem; }
                else if (ctrl.GetType() == typeof(CheckBox))      { return (ctrl as CheckBox).Checked; }
                else { return null; }
            }
            set
            {
                     if (ctrl.GetType() == typeof(TextBox))       { (ctrl as TextBox).Text = value.ToString(); }
                else if (ctrl.GetType() == typeof(MaskedTextBox)) { (ctrl as MaskedTextBox).Text = value.ToString(); }
                else if (ctrl.GetType() == typeof(NumericUpDown)) { (ctrl as NumericUpDown).Value = Convert.ToInt32(value); }
                else if (ctrl.GetType() == typeof(ComboBox))      { (ctrl as ComboBox).SelectedItem = value; }
                else if (ctrl.GetType() == typeof(CheckBox))      { (ctrl as CheckBox).Checked = bool.Parse(value.ToString()); }
            }
        }

        /// <summary>
        /// Label + Control 세트
        /// </summary>
        /// <param name="type"></param>
        public ucControlBox(CtrlType type)
        {
            InitializeComponent();

            switch (type)
            {
                case CtrlType.ComboBox:
                    SetComboBox();
                    break;
                case CtrlType.TextBox: ctrl = new TextBox(); break;
                case CtrlType.MaskedTextBox: ctrl = new MaskedTextBox(); break;
                case CtrlType.NumbericUpDown: ctrl = new NumericUpDown(); break;
                case CtrlType.CheckBox:
                    SetCheckBox();
                    break;

            }

            CreateUc();
        }

        private void CreateUc()
        {
            //기본 Height
            this.Height = 23;

            lbl.Dock = DockStyle.Left;
            lbl.Width = (int)(this.Width * 0.3);
            lbl.Text = "Empty Text";
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            ctrl.Dock = DockStyle.Fill;

            this.Controls.Add(ctrl);
            this.Controls.Add(lbl);

            //디버기용ㅇ
            //this.BorderStyle = BorderStyle.FixedSingle;
            //lbl.BorderStyle = BorderStyle.FixedSingle;
        }

        #region ComboBox

        private void SetComboBox()
        {
            ctrl = new ComboBox();
            ComboBox cbo = ctrl as ComboBox;
            cbo.DropDownStyle = ComboBoxStyle.DropDownList; //Combobox Text 수정 못하게 막기

            //ComboBox Height 조절
            cbo.SizeChanged += (sender, e) => { SetComboBoxHeight(cbo.Handle, this.Height - 8); };
            //ComboBox Text Align
            cbo.DrawMode = DrawMode.OwnerDrawVariable;
            cbo.DrawItem += SetCboTextAlign;
        }

        /// <summary>
        /// ComboBox Size에서 Height 변경 안되서 강제적용
        /// </summary>
        /// <!-- https://stackoverflow.com/questions/3158004/how-do-i-set-the-height-of-a-combobox -->
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnf, UInt32 Msg, Int32 wParam, Int32 lParam);
        private const Int32 CB_SETITEMHEIGHT = 0x153;   //user32.dll ComboBox 높이 셋팅 번호
        private void SetComboBoxHeight(IntPtr cboHandle, Int32 setHight)
        {
            SendMessage(cboHandle, CB_SETITEMHEIGHT, -1, setHight);
        }

        /// <summary>
        /// ComboBox Text Align / 
        /// </summary>
        /// <!-- https://stackoverflow.com/questions/11817062/align-text-in-combobox -->
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetCboTextAlign(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            ComboBox cbo = sender as ComboBox;

            e.DrawBackground();

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Near;

            Brush brush = new SolidBrush(cbo.ForeColor);
            if((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                brush = SystemBrushes.HighlightText;
            }

            e.Graphics.DrawString(cbo.Items[e.Index].ToString(), cbo.Font, brush, e.Bounds, sf);
        }

        #endregion ComboBox End

        private void SetCheckBox()
        {
            ctrl = new CheckBox();
            CheckBox chk = ctrl as CheckBox;
            chk.Checked = true;
        }
    }
}
