using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNetFramework
{
    public enum CtrlType
    {
        ComboBox,
        TextBox,
        NumbericUpDown
    }

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
            }

            CreateUc();
        }

        private void CreateUc()
        {
            lbl.Dock = DockStyle.Left;
            lbl.Width = (int)(this.Width * 0.3);
            lbl.Text = "None";
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            ctrl.Dock = DockStyle.Fill;

            this.Controls.Add(ctrl);
            this.Controls.Add(lbl);

            this.Height = lbl.Height > ctrl.Height ? ctrl.Height : lbl.Height;
        }

        private void SetComboBox()
        {
            ctrl = new ComboBox();
            ComboBox cbo = ctrl as ComboBox;
            cbo.DropDownStyle = ComboBoxStyle.DropDownList; //Combobox Text 수정 못하게 막기
        }
    }
}
