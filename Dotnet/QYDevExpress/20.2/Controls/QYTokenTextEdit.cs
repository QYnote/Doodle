using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QYDevExpress._20._2.Controls
{
    internal class QYTokenTextEdit : DevExpress.XtraEditors.MemoEdit
    {
        private Form popupForm = null;
        private DevExpress.XtraEditors.ListBoxControl listBox = new DevExpress.XtraEditors.ListBoxControl();

        private Color _tokenForeColor = Color.DarkRed;

        public Color TokenForeColor { get => this._tokenForeColor; set => this._tokenForeColor = value; }
        public object DataSource { get => this.listBox.DataSource; set => this.listBox.DataSource = value; }

        public QYTokenTextEdit()
        {
            InitPopup();

            base.Properties.ScrollBars = ScrollBars.None;
            base.KeyPress += txt_KeyPress;//'[', ']'키 누름 처리
            base.KeyDown += txt_KeyDown_DelToken;//Token 삭제처리
            base.KeyDown += txt_KeyDown_FocusPopup;//'↓'키 누름 처리
            base.Leave += HYTokenTextEdit_Leave;
            base.Paint += HYTokenTextEdit_Paint;
        }

        private void HYTokenTextEdit_Paint(object sender, PaintEventArgs e)
        {
            if (base.ContainsFocus || this.popupForm.Visible) return;

            if (base.Text.Contains("[") && base.Text.Contains("]"))
            {
                int handle = 0,
                    lastIndex = 0;
                float px = 1,
                      py = 1,
                      maxFontHeight = 0;
                bool startOpen = false;//'[' or ']'열기 여부
                base.MaskBox.Visible = false;//안하면 TextBox가 Graphic로 그린거 위에 차지됨

                //Text 배경 설정
                Rectangle background = new Rectangle(base.ClientRectangle.X + 1, base.ClientRectangle.Y + 1,
                    base.ClientRectangle.Width - 2, base.ClientRectangle.Height - 2);
                e.Graphics.FillRectangle(new SolidBrush(base.BackColor), background);

                //Text Token 추출
                List<string> tokens = new List<string>();
                string text = string.Empty;
                while (handle < base.Text.Length)
                {
                    //문장 분호 확인
                    if ((char.IsPunctuation(base.Text[handle]) && base.Text[handle] != '.')
                        || base.Text[handle] == '+' || base.Text[handle] == '-'
                        || base.Text[handle] == '*' || base.Text[handle] == '/'
                        || base.Text[handle] == '%' || base.Text[handle] == '^'
                        )
                    {
                        if (startOpen == false && base.Text[handle] == '[')
                        {
                            startOpen = true;
                            handle++;
                            continue;
                        }
                        else if (startOpen)
                        {
                            if (base.Text[handle] != ']')
                            {
                                handle++;
                                continue;
                            }
                            else
                            {
                                startOpen = false;
                                text = base.Text.Substring(lastIndex, handle - lastIndex + 1);
                            }
                        }
                        else
                        {
                            text = base.Text.Substring(lastIndex, handle - lastIndex + 1);
                        }

                        tokens.Add(text);
                        handle++;
                        lastIndex = handle;
                    }
                    else
                    {
                        handle++;
                    }
                }
                text = base.Text.Substring(lastIndex, base.Text.Length - lastIndex);
                if (text.Contains("[") == false && text.Contains("]") == false)
                    tokens.Add(text);

                //Font 적용된 최대 높이 탐색
                foreach (var token in tokens)
                {
                    Font font = base.Font;
                    if (token.Contains("[") && token.Contains("]"))
                        font = new Font(base.Font.FontFamily, base.Font.Size, FontStyle.Bold);

                    SizeF fontSize = e.Graphics.MeasureString(token, font);
                    if (maxFontHeight < fontSize.Height)
                        maxFontHeight = fontSize.Height;
                }

                //Text 그리기
                foreach (var token in tokens)
                {
                    SolidBrush brush;
                    if (token.Contains("[") && token.Contains("]"))
                        brush = new SolidBrush(this._tokenForeColor);
                    else
                        brush = new SolidBrush(base.ForeColor);

                    Font font = base.Font;
                    if (token.Contains("[") && token.Contains("]"))
                        font = new Font(base.Font.FontFamily, base.Font.Size, FontStyle.Bold);

                    SizeF fontSize = e.Graphics.MeasureString(token, font);
                    if (px + fontSize.Width > base.Width)
                    {
                        //Text가 Control Width를 넘어갈 경우 줄바꿈
                        px = 0;
                        py += maxFontHeight;
                    }

                    using (Pen pen = new Pen(brush, 100))
                    {
                        e.Graphics.DrawString(token, font, brush, new PointF(px, py));
                    }

                    px += fontSize.Width;
                }

            }
            else
            {
                base.MaskBox.Visible = true;
            }
        }

        /// <summary>
        /// Token Popup 목록 초기화
        /// </summary>
        private void InitPopup()
        {
            this.popupForm = new Form();
            this.popupForm.FormBorderStyle = FormBorderStyle.None;
            this.popupForm.StartPosition = FormStartPosition.Manual;
            this.popupForm.ShowInTaskbar = false;
            this.popupForm.TopMost = true;
            this.popupForm.MinimumSize = new Size(50, 50);
            this.popupForm.MaximumSize = new Size(200, 150);

            this.listBox.Dock = DockStyle.Fill;
            this.listBox.Click += (s, e) =>
            {

                if (this.listBox.SelectedItem != null)
                {
                    this.InsertToken((string)this.listBox.SelectedItem);
                    this.HidePopup();
                }
            };//선택 Item 선택처리
            this.listBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up && this.listBox.SelectedIndex == 0)
                {
                    //기존 TextBox로 복귀
                    base.Focus();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter && this.listBox.SelectedItem != null)
                {
                    //Enter Key누를 시 Item 적용
                    this.InsertToken((string)this.listBox.SelectedItem);
                    this.HidePopup();

                    e.Handled = true;
                }
            };

            this.popupForm.Controls.Add(this.listBox);
        }

        /// <summary>
        /// Text에 선택된 Item Text Insert처리
        /// </summary>
        /// <param name="word"></param>
        private void InsertToken(string word)
        {
            int selStart = base.SelectionStart;
            string txt = $"[{word}]";

            base.Text = base.Text.Insert(selStart, txt);

            base.Focus();
            base.SelectionStart = selStart + txt.Length;
            base.SelectionLength = 0;
        }

        /// <summary>
        /// Control에서 Focus Out처리
        /// </summary>
        private void HYTokenTextEdit_Leave(object sender, EventArgs e)
        {
            if (this.popupForm.Visible)
            {
                this.popupForm.Visible = false;
                this.popupForm.Hide();
            }
        }

        /// <summary>
        /// Popup 보이기처리
        /// </summary>
        private void ShowPopup()
        {
            Point screenPoint = base.PointToScreen(new Point(0, base.Height));
            this.popupForm.Location = screenPoint;

            this.popupForm.Width = base.Width;
            this.popupForm.Height = 100;

            this.popupForm.Show();
            this.popupForm.Visible = true;
            this.popupForm.BringToFront();
        }
        /// <summary>
        /// Popup 숨기기처리
        /// </summary>
        private void HidePopup()
        {
            if (this.popupForm.Visible)
            {
                this.popupForm.SendToBack();
                this.popupForm.Visible = false;
                this.popupForm.Hide();

                base.Focus();
            }
        }

        private void txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '[' || e.KeyChar == ']')
            {
                this.ShowPopup();
                e.Handled = true;
            }
            else
            {
                this.HidePopup();
            }
        }
        private void txt_KeyDown_DelToken(object sender, KeyEventArgs e)
        {
            int startIdx = -1,
                endIdx = -1;

            //삭제처리
            if (e.KeyCode == Keys.Back && base.SelectionStart > 0)
            {
                //Backspace로 지울 Text가 ']'인지 검사
                if (base.Text[base.SelectionStart - 1] == ']')
                {
                    endIdx = base.SelectionStart - 1;
                    //'['위치 찾기
                    for (int i = base.SelectionStart - 1; i >= 0; i--)
                    {
                        if (base.Text[i] == '[')
                        {
                            startIdx = i;
                            break;
                        }
                    }
                }
                else
                {
                    //Text Cursor가 []안에 있는 Text인지 검사
                    //'['탐색
                    for (int i = base.SelectionStart - 1; i >= 0; i--)
                    {
                        if (base.Text[i] == '[')
                        {
                            startIdx = i;
                            break;
                        }
                    }
                    //']'탐색
                    if (startIdx >= 0)
                    {
                        for (int i = startIdx; i < base.Text.Length; i++)
                        {
                            if (base.Text[i] == ']')
                            {
                                endIdx = i;
                                break;
                            }
                        }
                    }

                    //일반적인 삭제인 경우
                    if (endIdx < base.SelectionStart)
                    {
                        startIdx = -1;
                        endIdx = -1;
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete && base.SelectionStart < base.Text.Length)
            {
                //Delete로 지울 Text가 '['인지 검사
                if (base.Text[base.SelectionStart] == '[')
                {
                    startIdx = base.SelectionStart;
                    //']'탐색
                    for (int i = base.SelectionStart + 1; i < base.Text.Length; i++)
                    {
                        if (base.Text[i] == ']')
                        {
                            endIdx = i;
                            break;
                        }
                    }
                }
                else
                {
                    //Text Cursor가 []안에 있는 Text인지 검사
                    //'['탐색
                    for (int i = base.SelectionStart; i >= 0; i--)
                    {
                        if (base.Text[i] == '[')
                        {
                            startIdx = i;
                            break;
                        }
                    }
                    //']'탐색
                    for (int i = startIdx + 1; i < base.Text.Length; i++)
                    {
                        if (base.Text[i] == ']')
                        {
                            endIdx = i;
                            break;
                        }
                    }

                    //일반적인 삭제인 경우
                    if (endIdx < base.SelectionStart)
                    {
                        startIdx = -1;
                        endIdx = -1;
                    }
                }
            }
            //화살표 로 []안으로 이동시 []앞,뒤로 이동하도록하는 기능 개발하기


            if (startIdx != -1 && endIdx != -1)
            {
                int textLength = endIdx - startIdx + 1;//[]가 포함된 Text 길이
                string tokenText = base.Text.Substring(startIdx + 1, textLength - 2);   //삭제할 []가 제외된 Text

                base.Text = base.Text.Remove(startIdx, textLength);
                base.SelectionStart = startIdx;

                e.SuppressKeyPress = true;
            }
        }
        private void txt_KeyDown_FocusPopup(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && this.popupForm != null && !this.popupForm.ContainsFocus)
            {
                //↓키 누를 시 Popup으로 Focus 이동
                if (this.popupForm.Visible)
                {
                    this.listBox.Focus();
                    if (this.listBox.Items.Count > 0) this.listBox.SelectedIndex = 0;

                    e.SuppressKeyPress = true;
                }
            }
        }
    }
}
