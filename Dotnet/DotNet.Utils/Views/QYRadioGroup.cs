using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet.Utils.Views
{
    public class QYRadioGroup : UserControl
    {
        public event EventHandler SelectedValueChanged;

        private GroupBox _groupbox = new GroupBox();
        private FlowLayoutPanel _layout = new FlowLayoutPanel();
        private RadioButtonCollection _items = new RadioButtonCollection();

        private object _datasource = null;

        public object DataSource
        {
            get => this._datasource;
            set
            {
                if (this.DataSource != value)
                {
                    _datasource = value;
                    this.RefreshGroup();
                }
            }
        }
        public string DisplayMember { get; set; }
        public string ValueMember { get; set; }
        public RadioButtonCollection Items => this._items;

        private object _selectedValue;
        public object SelectedValue
        {
            get => this._selectedValue;
            set
            {
                if(this.SelectedValue != value)
                {
                    this._selectedValue = value;

                    this.SelectedValueChanged.Invoke(this, EventArgs.Empty);
                    this.UpdateCheckState();
                }
            }
        }
        public string Caption
        {
            get => this._groupbox.Text;
            set
            {
                if(this.Caption != value)
                {
                    this._groupbox.Text = value;
                }
            }
        }

        public QYRadioGroup()
        {
            this.InitUI();
        }

        private void InitUI()
        {
            this._groupbox.Dock = DockStyle.Fill;
            this._layout.Dock = DockStyle.Fill;

            this.Controls.Add(this._groupbox);
            this._groupbox.Controls.Add(this._layout);
        }

        public void RefreshGroup()
        {
            //Group 재표기
            this._layout.Controls.Clear();
            this.Items.Clear();

            if(this.DataSource is IEnumerable list)
            {
                foreach (var item in list)
                {
                    string display = item.GetType().GetProperty(this.DisplayMember)?.GetValue(item).ToString();
                    object val = item.GetType().GetProperty(this.ValueMember)?.GetValue(item);

                    RadioButton rdo = new RadioButton();
                    rdo.Text = display;
                    rdo.Tag = val;
                    rdo.AutoSize = true;

                    rdo.Click += (s, e) =>
                    {
                        this.SelectedValue = rdo.Tag;
                    };
                    
                    this.Items.Add(rdo);
                    this._layout.Controls.Add(rdo);
                }
            }
        }

        private void UpdateCheckState()
        {
            //값이 바뀌면 RadioButton Check 재설정
            foreach (RadioButton rdo in this._layout.Controls)
            {
                rdo.Checked = rdo.Tag.Equals(this.SelectedValue);
            }
        }
    }

    public class RadioButtonCollection : IEnumerable
    {
        List<RadioButton> _items = new List<RadioButton>();
        public int Count => this._items.Count;

        internal void Add(RadioButton rdo) => this._items.Add(rdo);
        internal void Remove(RadioButton rdo) => this._items.Remove(rdo);
        internal void Remove(int idx) => this._items.RemoveAt(idx);
        internal void Clear() => this._items.Clear();
        internal bool Contains(RadioButton rdo) => this.Contains(rdo);

        public RadioButton this[int index] => this._items[index];
        public IEnumerator GetEnumerator() => _items.GetEnumerator();

    }
}
