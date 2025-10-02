using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DotNet.Utils.Controls.Utils
{
    public static class QYUtils
    {
        public static QYMath Math = new QYMath();

        public static QYComm Comm = new QYComm();

        /// <summary>
        /// 원본에 손상이 가지 않도록 복제품 생성
        /// </summary>
        /// <typeparam name="T">복사 Source Type</typeparam>
        /// <param name="source">복사 원본</param>
        /// <returns>복사 결과물</returns>
        public static T CopyFrom<T>(this T source)
        {
            if (source == null) return default(T);

            Type type = typeof(T);

            //값, 문자열
            if (type.IsValueType || type == typeof(string))
                return source;

            //리스트 인경우
            else if (typeof(System.Collections.IList).IsAssignableFrom(type))
            {
                System.Collections.IList originList = (System.Collections.IList)source;
                System.Collections.IList copyList = (System.Collections.IList)Activator.CreateInstance(type);

                foreach (var item in originList)
                {
                    copyList.Add(CopyFrom(item));
                }

                return (T)copyList;
            }

            //Class 복사
            else
            {
                object copy = Activator.CreateInstance(type);

                //get;set; Property 복사
                foreach (var info in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if(info.CanRead && info.CanWrite)
                    {
                        var value = info.GetValue(source, null);
                        info.SetValue(copy, CopyFrom(value), null);
                    }
                }

                //get;set; 없는 Property 복사
                foreach (var info in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    var value = info.GetValue(source);
                    info.SetValue(copy, CopyFrom(value));
                }

                return (T)copy;
            }
        }
        /// <summary>
        /// Array에서 Array의 시작 Index 찾기
        /// </summary>
        /// <param name="source">찾을 Array</param>
        /// <param name="pattern">검사할 Array</param>
        /// <param name="startIdx">시작 Index</param>
        /// <returns></returns>
        public static int Find<T>(this T[] source, T[] pattern, int startIdx = 0) where T : IEquatable<T>
        {
            if (source == null || pattern == null) return -1;
            if (source.Length == 0 || source.Length < pattern.Length) return -1;

            int idx = -1;

            for (int i = startIdx; i <= source.Length - pattern.Length; i++)
            {
                bool isMatch = true;

                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j].Equals(pattern[j]) == false)
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch) return i;
            }

            return idx;
        }

        /// <summary>
        /// Enum List -> object[]값으로 변경
        /// </summary>
        /// <typeparam name="T">변환할 Enum</typeparam>
        /// <returns>ItemList</returns>
        static public object[] EnumToItems<Tenum>()
        {
            return Enum.GetValues(typeof(Tenum)).OfType<object>().ToArray();
        }

        #region Event

        /// <summary>
        /// ColumnOnlyNumeric의 Key검사용 16진수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static private void Event_KeyPress_Hex(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)
                && !(e.KeyChar == 'a' || e.KeyChar == 'A'
                    || e.KeyChar == 'b' || e.KeyChar == 'B'
                    || e.KeyChar == 'c' || e.KeyChar == 'C'
                    || e.KeyChar == 'd' || e.KeyChar == 'D'
                    || e.KeyChar == 'e' || e.KeyChar == 'E'
                    || e.KeyChar == 'f' || e.KeyChar == 'F'))
            {
                e.Handled = true;
            }
            else if(e.KeyChar == 'a'
                    || e.KeyChar == 'b'
                    || e.KeyChar == 'c'
                    || e.KeyChar == 'd'
                    || e.KeyChar == 'e'
                    || e.KeyChar == 'f')
            {
                if(e.KeyChar == 'a') { e.KeyChar = 'A'; }
                else if(e.KeyChar == 'b') { e.KeyChar = 'B'; }
                else if(e.KeyChar == 'c') { e.KeyChar = 'C'; }
                else if(e.KeyChar == 'd') { e.KeyChar = 'D'; }
                else if(e.KeyChar == 'e') { e.KeyChar = 'E'; }
                else if(e.KeyChar == 'f') { e.KeyChar = 'F'; }
            }
        }

        public const string Regex_IP = @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";
        /// <summary>
        /// .NET Winform TextBox IP만 입력가능하도록 변경
        /// </summary>
        public static void Event_KeyPress_IP(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            Control ctrl = sender as Control;
            //1. 숫자 or . 을 입력한 것인지 검사
            //제어값 확인(Backspace, Delete 등)
            if (char.IsControl(e.KeyChar))
                return; //제어키면 그냥 진행시키기
            else
            {
                //숫자 or . 인지 확인
                if (!char.IsDigit(e.KeyChar) && !(e.KeyChar == '.'))
                {
                    e.Handled = true;
                    return;
                }
            }

            //2. IP주소에 맞게 입력하는지 검사
            string text = ctrl.Text + e.KeyChar;
            int dotCnt = text.Count(c => c == '.'); //점 입력 개수
            /*Regex문자 해석
             * ^ : 시작문자
             * ( : 그룹 시작
             *   \d{1,2}    : 1 or 2자리 숫자
             *  |1\d\d      : 100 ~ 199
             *  |2[0-4]\d   : 200 ~ 249
             *  |25[0-5]    : 250 ~ 255
             * ) : 그룹 종료
             * $ : 종료문자
             */
            string strRegex = "(\\d{1,2}|1\\d\\d|2[0-4]\\d|25[0-5])";
            string pattern = string.Empty;

            //구분 점에따라 확인 문자열 변경
            switch (dotCnt)
            {
                case 0:
                    pattern = string.Format("^{0}$", strRegex);
                    break;
                case 1:
                    if (e.KeyChar == '.') pattern = string.Format("^{0}.$", strRegex);
                    else pattern = string.Format("^{0}.{0}$", strRegex);
                    break;
                case 2:
                    if (e.KeyChar == '.') pattern = string.Format("^{0}.{0}.$", strRegex);
                    else pattern = string.Format("^{0}.{0}.{0}$", strRegex);
                    break;
                case 3:
                    if (e.KeyChar == '.') pattern = string.Format("^{0}.{0}.{0}.$", strRegex);
                    else pattern = string.Format("^{0}.{0}.{0}.{0}$", strRegex);
                    break;
            }

            if (pattern == string.Empty || System.Text.RegularExpressions.Regex.IsMatch(text, pattern) == false)
            {
                e.Handled = true;
                return;
            }
        }

        #endregion Event End

        #region 공통Type Control생성

        /// <summary>
        /// Panel 경계선 그리기
        /// </summary>
        /// <param name="dock">Dock 방향</param>
        /// <returns></returns>
        public static System.Windows.Forms.Label CreateSplitLine(System.Windows.Forms.DockStyle dock, int thickness = 4)
        {
            System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
            lbl.Dock = dock;
            lbl.Margin = new System.Windows.Forms.Padding(3);
            lbl.BackColor = System.Drawing.Color.DarkGray;
            lbl.Text = "";
            lbl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lbl.AutoSize = false;
            if (dock == System.Windows.Forms.DockStyle.Left || dock == System.Windows.Forms.DockStyle.Right)
            {
                lbl.Size = new System.Drawing.Size(thickness, lbl.Height);
            }
            else if (dock == System.Windows.Forms.DockStyle.Top || dock == System.Windows.Forms.DockStyle.Bottom)
            {
                lbl.Size = new System.Drawing.Size(lbl.Width, thickness);
            }

            return lbl;
        }

        #endregion 공통Type Control생성 End

        public class Collection<TOwner, TItem> where TOwner : Control
                                               where TItem : Control
        {
            public TOwner Owner { get; }
            internal Collection(TOwner owner)
            {
                this.Owner = owner;
            }

            public TItem this[int idx]
            {
                get { return this.Owner.Controls[idx] as TItem; }
            }
            /// <summary>
            /// Item 추가
            /// </summary>
            /// <param name="item">추가할 item</param>
            public virtual void Add(TItem item) => this.Owner.Controls.Add(item);
            /// <summary>
            /// Item 수
            /// </summary>
            public int Count => this.Owner.Controls.Count;
            /// <summary>
            /// foreach 지원용 Enumratable
            /// </summary>
            /// <returns>이번차례 Item</returns>
            public IEnumerator<TItem> GetEnumerator()
            {
                foreach (Control ctrl in this.Owner.Controls)
                {
                    if (ctrl is TItem item)
                        yield return item;
                }
            }
        }
    }
    /// <summary>
    /// Thread 동시접근 처리기
    /// </summary>
    public class OpenToken : IDisposable
    {
        /// <summary>
        /// 읽기 Method 중단용
        /// </summary>
        private readonly System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
        /// <summary>
        /// 읽기 Method Pause용
        /// </summary>
        private readonly System.Threading.ManualResetEventSlim _pauseEvent = new System.Threading.ManualResetEventSlim(true);
        /// <summary>
        /// 읽기 Method Multi Thread 방지용
        /// </summary>
        private readonly object _lock = new object();

        public System.Threading.CancellationToken Token => this._cts.Token;
        /// <summary>
        /// Token 사용중 여부
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// 사용중 설정
        /// </summary>
        /// <param name="running">사용중 여부</param>
        public void SetRunning(bool running) => this.IsRunning = running;
        /// <summary>
        /// Token 중단
        /// </summary>
        /// <remarks>
        /// Token.ThrowIfCancellationRequiested()가 들어간 Thread가 멈춰야 할 때 사용
        /// </remarks>
        /// _cts Cancle을 요청해서
        /// _cts.Token.ThrowIfCancellationRequiested()를 통해
        /// Cancle 요청을 받았으면 throw아니면 마저 진행함
        public void Cancle() => this._cts.Cancel();
        /// <summary>
        /// Token Thread 일시정지
        /// </summary>
        /// <remarks>
        /// WaitIfPaused가 들어간 Thread가 멈춰야 하는 곳에서 사용
        /// </remarks>
        public void Pause() => this._pauseEvent.Reset();
        /// <summary>
        /// Token Thread 재진행
        /// </summary>
        /// <remarks>
        /// WaitIfPaused가 들어간 Thread가 재시작 하는 곳에서 사용
        /// </remarks>
        public void Resume() => this._pauseEvent.Set();
        /// <summary>
        /// Token Thread 대기
        /// </summary>
        /// _pauseEvent가 Set상태가 될때까지 대기
        public void WaitIfPaused() => this._pauseEvent.Wait(this.Token);
        /// <summary>
        /// Token Multi Thread 접근 방지용 Lock
        /// </summary>
        /// <param name="action">Lock동안 진행할 Action</param>
        /// <remarks>
        /// 여러 Thread에서 동시 접근할 Object를 사용할 때
        /// Lock(() => { Method });를 사용
        /// </remarks>
        public void Lock(Action action) { lock (this._lock) { action(); } }

        public void Dispose()
        {
            this._cts.Dispose();
            this._pauseEvent.Dispose();
        }
    }
}
