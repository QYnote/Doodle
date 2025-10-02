using DotNet.Utils.Controls.Utils;
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

namespace DotNet.Utils.Controls
{
    public partial class QYLeftMenu : UserControl
    {
        #region UI Controls

        private Panel pnlTop = new Panel();
        private Button btnExpand = new Button();
        private Button btnMain = new Button();

        internal Panel pnlBody = new Panel();

        #endregion UI Controls


        private bool _isExpand = true;
        private int _expandWidth = 150;

        public int ExpandWidth
        {
            get
            {
                return this._expandWidth;
            }
            set
            {
                this._expandWidth = value;

                if (this._isExpand)
                {
                    //즉시 적용
                    base.Width = this._expandWidth;
                }
            }
        }
        public int CollapsedWidth { get; set; } = 32;
        public Image ExpandImage
        {
            get
            {
                return this.btnMain.Image;
            }
            set
            {
                this.btnMain.Image = value;
            }
        }

        public QYLeftMenuItemCollection Items { get; }
        public EventHandler TopMainClick { set { this.btnMain.Click += value; } }
        /// <summary>
        /// 아이템 클릭 이벤트
        /// </summary>
        /// <remarks>
        /// 이벤트가 설정된 후에 Item이 추가되어야함
        /// </remarks>
        public EventHandler ItemClick;


        public QYLeftMenu()
        {
            InitializeComponent();
            InitUI();

            this.Items = new QYLeftMenuItemCollection(this);
        }

        private void InitUI()
        {
            base.Dock = DockStyle.Left;
            base.Width = this.ExpandWidth;
            base.BackColorChanged += (s, e) =>
            {
                //BackColor 수정 시 Item의 BackColor도 수정
                this.btnExpand.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, this.btnExpand.BackColor);
                this.btnExpand.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, this.btnExpand.BackColor);

                this.btnMain.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, this.btnMain.BackColor);
                this.btnMain.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, this.btnMain.BackColor);
            };

            this.pnlTop.BackColor = Color.Transparent;
            this.pnlTop.BorderStyle = BorderStyle.None;
            this.pnlTop.Dock = DockStyle.Top;
            this.pnlTop.Height = 38;
            this.pnlTop.Padding = new Padding(0);
            this.pnlTop.Margin = new Padding(0);

            this.btnExpand.Dock = DockStyle.Left;
            this.btnExpand.FlatAppearance.BorderSize = 0;
            this.btnExpand.Width = 41;
            this.btnExpand.FlatStyle = FlatStyle.Flat;
            this.btnExpand.Image = Properties.Resources.Menu_Hamburgar_32x32;
            this.btnExpand.ImageAlign = ContentAlignment.MiddleCenter;
            this.btnExpand.Click += Animation_Expands;

            this.btnMain.Dock = DockStyle.Fill;
            this.btnMain.FlatAppearance.BorderSize = 0;
            this.btnMain.FlatStyle = FlatStyle.Flat;
            this.btnMain.ImageAlign = ContentAlignment.MiddleCenter;

            this.pnlBody.BackColor = Color.Transparent;
            this.pnlBody.Dock = DockStyle.Fill;

            base.Controls.Add(this.pnlBody);
            base.Controls.Add(this.pnlTop);
            this.pnlTop.Controls.Add(this.btnMain);
            this.pnlTop.Controls.Add(this.btnExpand);
        }

        private async void Animation_Expands(object sender, EventArgs e)
        {
            //4. 상태값 반전
            this._isExpand = !this._isExpand;


            if (this._isExpand)
            {
                //1. 확장 애니메이션
                for (int w = this.CollapsedWidth; w <= this.ExpandWidth; w += 10)
                {
                    base.Width = w;
                    await Task.Delay(10);
                }

                //Item
                foreach (var item in this.Items)
                    item.ExpandChanged(this._isExpand);

                //3. 메인 이미지 보이기
                this.btnMain.Show();
            }
            else
            {
                //Item
                foreach (var item in this.Items)
                    item.ExpandChanged(this._isExpand);

                //1. 축소 애니메이션
                for (int w = this.ExpandWidth; w >= this.CollapsedWidth; w -= 10)
                {
                    base.Width = w;
                    await Task.Delay(10);
                }

                //2. 메인 이미지 숨김
                this.btnMain.Hide();
            }
        }
    }

    public class QYLeftMenuItemCollection : QYUtils.Collection<Panel, QYLeftMenuItem>
    {

        internal QYLeftMenuItemCollection(QYLeftMenu owner) : base(owner.pnlBody)
        {

        }

        public override void Add(QYLeftMenuItem item)
        {
            item.Click += (base.Owner.Parent as QYLeftMenu).ItemClick;
            base.Add(item);
        }
    }

    public class QYLeftMenuItem : Button
    {
        private string _text = string.Empty;
        public QYLeftMenuItem()
        {
            InitUI();
        }

        private void InitUI()
        {
            base.BackColorChanged += (s, e) =>
            {
                base.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, base.BackColor);
                base.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, base.BackColor);
            };
            base.Dock = DockStyle.Top;
            base.FlatAppearance.BorderSize = 0;
            base.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, base.BackColor);
            base.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, base.BackColor);
            base.FlatStyle = FlatStyle.Flat;
            base.ImageAlign = ContentAlignment.MiddleLeft;
            base.Height = 40;
            base.Text = "QYLeftMenuItem";
            base.TextAlign = ContentAlignment.MiddleLeft;
            base.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        internal void ExpandChanged(bool isExpand)
        {
            if (isExpand)
            {
                base.Text = _text;
            }
            else
            {
                this._text = base.Text;
                base.Text = string.Empty;
            }
        }
    }
}
