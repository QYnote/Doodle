using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Dnf.Utils.Controls
{
    public partial class ucNumeric : UserControl
    {
        /* 그냥 ucNumeric 쓰면 Control Height 조정이 안되서 내가 직접 만드는 Numeric
         */
        #region public Property

        /// <summary>
        /// 최솟값
        /// </summary>
        public int MinValue = int.MinValue;
        /// <summary>
        /// 최대값
        /// </summary>
        public int MaxValue = int.MaxValue;
        /// <summary>
        /// 현재값[Dec]
        /// </summary>
        public int Value
        { 
            get
            {
                return Convert.ToInt32(txtValue.Rows[0].Cells[0].Value);
            }
            set
            {
                txtValue.Rows[0].Cells[0].Value = value.ToString();
            }
        }
        /// <summary>
        /// 현재값[Hex]
        /// </summary>
        public string HexValue { get { return Convert.ToInt32(txtValue.Rows[0].Cells[0].Value).ToString("X"); } }
        /// <summary>
        /// 버튼 보이기 여부
        /// </summary>
        public bool VisibleButton
        {
            get
            {
                return pnlButton.Visible;
            }
            set
            {
                pnlButton.Visible = value;
            }
        }
        /// <summary>
        /// Text 위치
        /// </summary>
        public DataGridViewContentAlignment TextAlignment
        {
            get
            {
                return txtValue.DefaultCellStyle.Alignment;
            }
            set
            {
                txtValue.DefaultCellStyle.Alignment = value;
            }
        }
        /// <summary>
        /// Up, Down 버튼 or 스크롤 변화값
        /// </summary>
        public int UpdownValue = 1;

        #endregion public Property End


        /*그냥 TextBox는 상하 정렬이 안되서 Gridview의 1개 Cell만 나타내는 것으로 대체*/
        private DataGridView txtValue = new DataGridView();
        private Panel pnlButton = new Panel();
        private Button btnUp = new Button();
        private Button btnDown = new Button();

        public ucNumeric()
        {
            InitializeComponent();
            InitializeControl();

            this.Size = new Size(120, 30);
        }

        private void InitializeControl()
        {
            pnlButton.Dock = DockStyle.Right;
            pnlButton.Width = 20;

            btnUp.Dock = DockStyle.Top;
            btnUp.Height = pnlButton.Height / 2;
            btnUp.TextAlign = ContentAlignment.MiddleCenter;
            btnUp.Text = "▲";
            btnDown.Dock = DockStyle.Fill;
            btnDown.TextAlign = ContentAlignment.MiddleCenter;
            btnDown.Text = "▼";

            txtValue.Dock = DockStyle.Fill;
            txtValue.AutoSize = false;                                      //자동사이즈
            txtValue.AllowUserToAddRows = false;                            //User Row추가
            txtValue.AllowUserToDeleteRows = false;                         //User Row삭제
            txtValue.AllowUserToResizeColumns = false;                      //User Column 너비 조절
            txtValue.AllowUserToResizeRows = false;                         //User Row 높이 조절
            txtValue.RowHeadersVisible = false;                             //Focuse 된 Row 화살표
            txtValue.ColumnHeadersVisible = false;                          //Column 머리말
            txtValue.MultiSelect = false;                                   //여러 셀 선택
            txtValue.EditMode = DataGridViewEditMode.EditOnEnter;           //선택 시 바로 Edit에 들어가는지
            txtValue.ScrollBars = ScrollBars.None;                          //Scroll바 표기
            txtValue.CellBorderStyle = DataGridViewCellBorderStyle.None;  //셀 Border처리
            txtValue.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;        //선택시 Cell Text Color
            txtValue.DefaultCellStyle.SelectionBackColor = SystemColors.Window;             //선택시 Cell 배경 Color
            txtValue.DefaultCellStyle.ForeColor = SystemColors.ControlText;                 //Cell Text Color
            txtValue.DefaultCellStyle.BackColor = SystemColors.Window;                      //Cell 배경 Color
            txtValue.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; //기본 정렬

            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            txtValue.Columns.Add(col);
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            col.DefaultCellStyle.Format = String.Format("n0");
            col.ValueType = typeof(int);
            txtValue.Rows.Add("0");

            pnlButton.Controls.Add(btnDown);
            pnlButton.Controls.Add(btnUp);
            this.Controls.Add(txtValue);
            this.Controls.Add(pnlButton);

            this.SizeChanged += SizeChanged_SetButtonSize;
            txtValue.EditingControlShowing += TxtValue_EditingControlShowing_SetValueOnlyText;
            txtValue.CellValueChanged += Value_SetFrontMultiZero;
            btnUp.Click += (sender, e) => { txtValue.CurrentCell.Value = Convert.ToInt32(txtValue.CurrentCell.Value) + UpdownValue; };
            btnDown.Click += (sender, e) => { txtValue.CurrentCell.Value = Convert.ToInt32(txtValue.CurrentCell.Value) - UpdownValue; };

            txtValue.CellValueChanged += UserCustomEvent_Start;
        }


        /// <summary>
        /// 앞자리에 0을 사용 시 중복된 0값 제거
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Value_SetFrontMultiZero(object sender, DataGridViewCellEventArgs e)
        {
            //Validating으로 할지 TextChanged로 할지 실사용 봐야할듯
            DataGridView txt = sender as DataGridView;
            DataGridViewCell Cell = txt.CurrentCell;
            if (Cell == null) return;

            //자리수구분 쉼표(,)제거
            string value = Cell.Value.ToString().Replace(",", "");
            if (value.Length == 1) return;

            //0으로 시작하는지 점검
            if (value.StartsWith("0"))
            {
                //앞자리 0개수 검사
                int zeroLength = 0;
                foreach (char c in value)
                {
                    if (c == '0')
                    {
                        zeroLength++;
                    }
                    else
                    {
                        break;
                    }
                }

                //0만 계속해서 입력중인거면 0값으로 표기
                if (zeroLength == value.Length)
                {
                    value = "0";
                }
                else
                {
                    //앞자리 수 0 삭제
                    value = value.Substring(zeroLength, value.Length - zeroLength);
                }

                //앞자리 0인값들 제거후 해당 Text값 지정
                Cell.Value = value;
            }

            txt.RefreshEdit();  //변경값 적용시키기
            txt.Focus();    //적용시킨 후 Cell 선택 벗어나기
        }

        /// <summary>
        /// Cell에 KeypRess Event 넣기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtValue_EditingControlShowing_SetValueOnlyText(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(KeyPress_CheckDecimal);
            TextBox txt = e.Control as TextBox;
            if (txt != null)
            {
                e.Control.KeyPress += new KeyPressEventHandler(KeyPress_CheckDecimal);
            }
        }

        /// <summary>
        /// Text에 입력한 키보드 Key값 점검(숫자랑, Backspace같은거만 쓸수 있도록)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyPress_CheckDecimal(object sender, KeyPressEventArgs e)
        {
            /*작업내역
             * IsControl : Ender, Backsapce같은 명령어 확인(이런건 먹어야하니까)
             * IsDigit : 10진수 검사
             * e.Handled : true - 막기, false - 가능
             */
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Control Size 변경 시 Button이 Height Size 절반을 차지하도록 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeChanged_SetButtonSize(object sender, EventArgs e)
        {
            //GridCell 조절
            txtValue.Rows[0].Height = this.Height - 2;

            //Button 높이 조절
            btnUp.Height = pnlButton.Height / 2;
            if (btnUp.Height - 10 >= 40)
            {
                btnUp.Font = new Font(btnUp.Font.FontFamily, 15);
                btnDown.Font = new Font(btnDown.Font.FontFamily, 15);
            }
            else
            {
                btnUp.Font = new Font(btnUp.Font.FontFamily, btnUp.Height - 10);
                btnDown.Font = new Font(btnDown.Font.FontFamily, btnDown.Height - 10);
            }
        }

        /// <summary>
        /// 유저가 지정한 Event 실행시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserCustomEvent_Start(object sender, EventArgs e)
        {
            UserCustom_CellValueChanged();
        }

        /// <summary>
        /// User Custom ValueChanged Event
        /// </summary>
        public event EventHandler ValueChanged;
        /// <summary>
        /// Value Changed Event 발생
        /// </summary>
        protected virtual void UserCustom_CellValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
