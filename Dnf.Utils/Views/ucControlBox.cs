using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
            //Text칸 높이가 더 크면 높이를 Control 높이로 맞추기
            this.Height = lbl.Height > ctrl.Height ? ctrl.Height : lbl.Height;

            lbl.Dock = DockStyle.Left;
            lbl.Width = (int)(this.Width * 0.3);
            lbl.Text = "Empty Text";
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            ctrl.Dock = DockStyle.Fill;

            this.Controls.Add(ctrl);
            this.Controls.Add(lbl);

        }

        private void SetComboBox()
        {
            ctrl = new ComboBox();
            ComboBox cbo = ctrl as ComboBox;
            cbo.DropDownStyle = ComboBoxStyle.DropDownList; //Combobox Text 수정 못하게 막기
        }

        private void SetCheckBox()
        {
            ctrl = new CheckBox();
            CheckBox chk = ctrl as CheckBox;
            chk.Checked = true;
        }
    }
}
