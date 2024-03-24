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

        public string LblText
        {
            get => lbl.Text;
            set => lbl.Text = value;
        }
        public int LblWidth
        {
            get => lbl.Width;
            set => lbl.Width = value;
        }
        
        public ucControlBox(CtrlType type)
        {
            InitializeComponent();

            switch (type)
            {
                case CtrlType.ComboBox:
                    SetComboBox();
                    break;
                case CtrlType.TextBox:
                    ctrl = new TextBox();
                    break;
                case CtrlType.NumbericUpDown:
                    ctrl = new NumericUpDown();
                    break;
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
            lbl.Text = "None";
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
