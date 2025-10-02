using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet.Utils.Views
{
    public class QYTokenTextBox : RichTextBox
    {
        private Form popup = new Form();
        private ListBox listBox = new ListBox();

        private Color _tokenForeColor = Color.DarkRed;

        public Color TokenForeColor { get => this._tokenForeColor; set => this._tokenForeColor = value;}
        public object DataSource { get => this.listBox.DataSource; set => this.listBox.DataSource = value; }

        private int changedStart = -1;

        public QYTokenTextBox()
        {
            base.Multiline = true;
            base.KeyPress += txt_KeyPress_Token;
            base.KeyDown += txt_KeyDown_DelToken;
            base.KeyDown += txt_KeyDown_ArrowFocusPopup;
            base.Leave += txt_LeaveFocus;
            base.TextChanged += QYTokenTextBox_TextChanged;

            InitPopup();
        }

        /// <summary>
        /// Token호출 키('[', ']')누름 처리
        /// </summary>
        private void txt_KeyPress_Token(object sender, KeyPressEventArgs e)
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
        /// <summary>
        /// 'Backspace' or 'Delete'키를 통한 Token 삭제처리
        /// </summary>
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

            //Token 삭제
            if (startIdx != -1 && endIdx != -1)
            {
                int textLength = endIdx - startIdx + 1;//[]가 포함된 Text 길이
                string tokenText = base.Text.Substring(startIdx + 1, textLength - 2);   //삭제할 []가 제외된 Text

                base.Text = base.Text.Remove(startIdx, textLength);
                base.SelectionStart = startIdx;

                e.SuppressKeyPress = true;
            }
        }
        /// <summary>
        /// Popup이 열려있을때 Text Focus상태에서 ↓키 누를 시 Popup으로 Focus 이동
        /// </summary>
        private void txt_KeyDown_ArrowFocusPopup(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && !this.popup.ContainsFocus)
            {
                //↓키 누를 시 Popup으로 Focus 이동
                if (this.popup.Visible)
                {
                    this.listBox.Focus();
                    if (this.listBox.Items.Count > 0) this.listBox.SelectedIndex = 0;

                    e.SuppressKeyPress = true;
                }
            }
        }
        /// <summary>
        /// Popup이 열려있는 상태일 때 다른 Control로 Focus가 나가질경우 Popup닫기
        /// </summary>
        private void txt_LeaveFocus(object sender, EventArgs e)
        {
            if (this.popup.Visible)
            {
                this.popup.Visible = false;
                this.popup.Hide();
            }
        }
        /// <summary>
        /// Text의 Token 색상 지정
        /// </summary>
        private void QYTokenTextBox_TextChanged(object sender, EventArgs e)
        {
            //Token추가가 아닌 일반적인 Changed일 경우 selection위치 가져오기
            if (this.changedStart == -1)
                this.changedStart = base.SelectionStart;

            SetTokenColor();

            this.changedStart = -1;
        }

        /// <summary>
        /// Popup 보이기 처리
        /// </summary>
        private void ShowPopup()
        {
            Point screenPoint = base.PointToScreen(new Point(0, base.Height));
            this.popup.Location = screenPoint;
            this.popup.Width = base.Width;
            this.popup.Height = 100;

            this.popup.Show();
            this.popup.Visible = true;
            this.popup.BringToFront();
        }
        /// <summary>
        /// Popup 숨기기 처리
        /// </summary>
        private void HidePopup()
        {
            if (this.popup.Visible)
            {
                this.popup.SendToBack();
                this.popup.Visible = false;
                this.popup.Hide();

                base.Focus();
            }
        }

        private void InitPopup()
        {
            this.popup.FormBorderStyle = FormBorderStyle.None;
            this.popup.StartPosition = FormStartPosition.Manual;
            this.popup.ShowInTaskbar = false;
            this.popup.TopMost = true;
            this.popup.MinimumSize = new Size(50, 50);
            this.popup.MaximumSize = new Size(200, 150);

            this.listBox.Dock = DockStyle.Fill;
            this.listBox.Click += ListBox_Click;
            this.listBox.KeyDown += ListBox_KeyDown;

            this.popup.Controls.Add(this.listBox);
        }

        /// <summary>
        /// Popup에서 Token 선택처리
        /// </summary>
        private void ListBox_Click(object sender, EventArgs e)
        {
            if(this.listBox.SelectedItem != null && this.listBox.SelectedItem is string item)
            {
                this.InsertToken(item);
                this.HidePopup();
            }
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Up && this.listBox.SelectedIndex == 0)
            {
                //최상단에 Item선택상태일 때 ↑키 누르면 TextBox로가기
                base.Focus();
                e.SuppressKeyPress = true;
                this.HidePopup();
            }
            else if(e.KeyCode == Keys.Enter && this.listBox.SelectedItem != null &&  this.listBox.SelectedItem is string item)
            {
                this.InsertToken(item);
                this.HidePopup();

                e.SuppressKeyPress = true;
            }
        }
        /// <summary>
        /// popup에서 선택된 Token 입력 처리
        /// </summary>
        /// <param name="word">입력할 Token</param>
        private void InsertToken(string word)
        {
            int selStart = base.SelectionStart;
            string txt = $"[{word}]";

            this.changedStart = selStart + txt.Length;
            base.Text = base.Text.Insert(selStart, txt);

            base.Focus();
        }

        private void SetTokenColor()
        {
            string regexPattern =
                @"(\[\w+\])" +
                @"|(\[\(\w+\)\w+\])" +
                @"|(\[\w+\(\w+\)\])" +
                @"|(\[\w+\(\w+\)\w+\])";

            foreach (Match match in Regex.Matches(base.Text, regexPattern))
            {
                base.Select(match.Index, match.Length);
                base.SelectionColor = this._tokenForeColor;
                base.SelectionFont = new Font(base.Font.FontFamily, base.Font.Size, FontStyle.Bold);
            }

            base.Select(this.changedStart, 0);

            base.SelectionColor = base.ForeColor;
            base.SelectionFont = base.Font;
        }
    }
}
