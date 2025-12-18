using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Utils.Controls.Utils
{
    public class QYMath
    {
        /// <summary>
        /// Token 후위 표기법 계산처리 Process
        /// </summary>
        /// <param name="token">처리할 Regex값</param>
        public delegate double CustomCalcNotaion_Postfix(string token);
        /// <summary>
        /// 중위 표기법 Custom 처리정보
        /// </summary>
        public class CustomInfixNotationArgs
        {
            private string _regex = string.Empty;
            private CustomCalcNotaion_Postfix _calc = null;

            /// <summary>
            /// 숫자, 연산기호, Math함수를 제외한 특수 Token 추출 규칙 Regex
            /// </summary>
            public string Regex { get => this._regex;  }
            /// <summary>
            /// Regex 항목 후위식 계산 처리
            /// </summary>
            public CustomCalcNotaion_Postfix Calc { get => this._calc;  }
            /// <summary>
            /// 중위표기법 Custom 처리정보
            /// </summary>
            /// <param name="regex">추출 Regex</param>
            /// <param name="calc">Regex 후위식 값 입력 Method</param>
            public CustomInfixNotationArgs(string regex, CustomCalcNotaion_Postfix calc)
            {
                this._regex = regex;
                this._calc = calc;
            }
        }

        private Dictionary<string, int> dicOper = new Dictionary<string, int>()
        {
            { "+", 1 }, { "-", 1 },
            { "*", 2 }, { "/", 2 }, { "%", 2 },
            { "^", 3 },     //제곱
            //Math 함수
            { "u-", 99 },   //음수
        };
        private List<string> _func = new List<string>()
        {
            "abs", "max", "min",
            "pow", "sqrt",  //거듭제곱(^), 제곱근(√)
            "sin", "cos", "tan",
            "asin", "acos", "atan",
            "log",
        };

        /// <summary>
        /// 중위식 string 계산
        /// </summary>
        /// <param name="calc">중위식 계산식</param>
        /// <param name="e">중위식 계산식</param>
        /// <returns>결과값</returns>
        public double CalcString_InfixNotation(string calc, CustomInfixNotationArgs e = null)
        {
            //1. Token 추출
            string[] tokens;
            Queue<string> postQueue;
            if (e == null)
            {
                //1. Token 추출
                tokens = this.CalcString_GetTokens(calc);
                if (tokens == null) return double.NaN;

                //2. 중위식 → 후위식 계산식 변환
                postQueue = this.CalcString_ConvertMidToPost(tokens);
            }
            else
            {
                //1. Token 추출
                tokens = this.CalcString_GetTokens(calc, e.Regex);
                if (tokens == null) return double.NaN;

                //2. 중위식 → 후위식 계산식 변환
                postQueue = this.CalcString_ConvertMidToPost(tokens, e.Regex);
            }
            if(postQueue == null) return double.NaN;

            //3. 후위식 계산
            return this.CalcString_CalcPost(postQueue, e.Calc);
        }
        /// <summary>
        /// 중위식 계산 - Token추출
        /// </summary>
        /// <param name="calc">중위식 계산식</param>
        /// <param name="customRegex">Token 추출 Custom Regex문</param>
        /// <returns>추출된 Token List</returns>
        private string[] CalcString_GetTokens(string calc, string customRegex = "")
        {
            List<string> tokens = new List<string>();
            string regexPattern =
                @"([0-9]*\.?[0-9]+)" + //숫자
                @"|(\+|-|\*|/|\^|\(|\))" +  //연산자
                @"|(\w+)|(,)";   //Math 함수

            //Token 추출 Custom 규칙 Regex 적용
            if (customRegex != "")
                regexPattern += $"|{customRegex}";

            //Regex에따른 Token들 추출
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(calc, regexPattern))
            {
                if(!string.IsNullOrEmpty(match.Value))
                        tokens.Add(match.Value);
            }

            if (tokens.Count == 0) return null;
            else return tokens.ToArray();
        }
        /// <summary>
        /// 중위식 계산 - 중위식 → 후위식 변환
        /// </summary>
        /// <param name="tokens">변환할 중위식 Tokens</param>
        /// <param name="customConvert">후위식 변환 Custom Action</param>
        /// <returns>변환된 후위식 계산순서 Queue</returns>
        /// <exception cref="ArgumentException">변환 오류</exception>
        private Queue<string> CalcString_ConvertMidToPost(string[] tokens, string customRegex = "")
        {
            Queue<string> postQueue = new Queue<string>();//후위식 작업순서 Queue
            Stack<string> operStack = new Stack<string>();//연산자 순서 임시보관용 Stack

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];

                if (double.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                    //일반 숫자형으로 변환 - 실패시 다음 if
                    postQueue.Enqueue(token);
                else if(customRegex != "" && System.Text.RegularExpressions.Regex.IsMatch(token, customRegex))
                {
                    //Custom Regex에 일치하는 항목 
                    postQueue.Enqueue(token);
                }
                else if (this._func.Contains(token.ToLowerInvariant()))
                {
                    //Math 기능 연산자 검사
                    operStack.Push(token.ToLowerInvariant());
                }
                else if(token == ",")
                {
                    //','가 사용된 수식의 괄호까지 후위식 입력
                    while (operStack.Count > 0 && operStack.Peek() != "(")
                        postQueue.Enqueue(operStack.Pop());

                    if (operStack.Count == 0 || operStack.Peek() != "(")
                        throw new ArgumentException("수식오류: 괄호 짝이 안맞거나 ','위치가 잘못됨");
                }
                else if (this.dicOper.ContainsKey(token))
                {
                    //일반적인 후위 Queue 입력처리
                    string curToken = token;

                    //뺄셈, 음수 구분처리
                    if (curToken == "-"
                        && (i == 0  //첫 수식
                            || tokens[i - 1] == "(" || tokens[i - 1] == "," //'-'앞이 괄호 or 수식의 ,
                            || dicOper.ContainsKey(tokens[i - 1]))  //'-'앞이 연산자
                    )
                    {
                        curToken = "u-"; //unary minus
                    }

                    //연산자일경우 Stack 처리
                    while (operStack.Count > 0 && operStack.Peek() != "(")
                    {
                        string topOper = operStack.Peek();

                        if (_func.Contains(curToken) || //우측 우선 연산인경우(기능적 Function)
                            (dicOper.ContainsKey(topOper) &&
                                (
                                    (curToken != "^" && dicOper[topOper] >= dicOper[curToken]) ||   //좌측 우선 연산자 && TopOper가 우선순위가 더 높음
                                    (curToken == "^" && dicOper[topOper] > dicOper[curToken])      //우측 우선 연산자 && TopOper가 우선순위가 더 높음
                                )
                            )
                        )
                            postQueue.Enqueue(operStack.Pop());
                        else break;
                    }

                    operStack.Push(curToken);
                }
                else if (token == "(")
                    operStack.Push(token);
                else if (token == ")")
                {
                    //괄호 닫기 처리
                    while (operStack.Count > 0 && operStack.Peek() != "(")
                        //괄호 안에 있던 연산식 Stack제거 및 후위식 입력
                        postQueue.Enqueue(operStack.Pop());

                    if (postQueue.Count == 0)
                        throw new ArgumentException("수식오류: 괄호 짝이 안맞음");

                    operStack.Pop();    //Stack '(' 제거

                    //괄호'('앞에 있던 연산자 후위식 입력
                    if (operStack.Count > 0 && operStack.Peek() != "(")
                        postQueue.Enqueue(operStack.Pop());
                }
            }

            //남은 연산자 뒤에 붙이기
            while(operStack.Count > 0)
            {
                string oper = operStack.Pop();
                if(oper == "(" || oper == ")")
                    throw new ArgumentException("수식오류: 괄호 짝이 안맞음");

                postQueue.Enqueue(oper);
            }

            if (postQueue.Count == 0) return null;
            else return postQueue;
        }
        /// <summary>
        /// 후위식 계산
        /// </summary>
        /// <param name="postQueue">후위식 Queue</param>
        /// <param name="customCalc">후위식 Custom 계산식</param>
        /// <returns>계산 결과값</returns>
        /// <exception cref="ArgumentException">계산 오류</exception>
        private double CalcString_CalcPost(Queue<string> postQueue, CustomCalcNotaion_Postfix customCalc = null)
        {
            Stack<double> postStack = new Stack<double>();

            while(postQueue.Count > 0)
            {
                string token = postQueue.Dequeue();

                if (double.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v))
                    //일반 숫자형으로 변환 - 실패시 다음 if
                    postStack.Push(v);
                else if (customCalc != null &&
                         this.dicOper.ContainsKey(token) == false &&
                         this._func.Contains(token) == false)
                {
                    //Regex에 속한 Token 처리
                    postStack.Push(customCalc.Invoke(token));
                }
                else if (this.dicOper.ContainsKey(token) || this._func.Contains(token))
                {
                    if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%" || token == "^" ||
                        token == "min" || token == "max" || token == "pow" || token == "log")
                    {
                        if (postStack.Count < 2)
                            throw new ArgumentException($"잘못된 수식: 수식부족 - '{token}'");

                        double v1 = postStack.Pop(),
                               v2 = postStack.Pop(),
                               rst;

                        switch (token)
                        {
                            case "+": rst = v2 + v1; break;
                            case "-": rst = v2 - v1; break;
                            case "*": rst = v2 * v1; break;
                            case "/": rst = v2 / v1; break;
                            case "%": rst = v2 % v1; break;
                            case "^": rst = Math.Pow(v2, v1); break;
                            case "min": rst = Math.Min(v2, v1); break;
                            case "max": rst = Math.Max(v2, v1); break;
                            case "pow": rst = Math.Pow(v2, v1); break;
                            /*Log(밑, 값)
                             * Excel과 C#에서는 (값, 밑)을 사용하지만 사용자 입력 편의성을 위해 뒤바꿈
                             */
                            case "log": rst = Math.Log(v1, v2); break;
                            default: throw new ArgumentException($"미설정 연산자 - {token}");
                        }

                        postStack.Push(rst);
                    }
                    else if (token == "u-" || token == "abs" || token == "sqrt" ||
                        token == "sin" || token == "cos" || token == "tan" ||
                        token == "asin" || token == "acos" || token == "atan")
                    {
                        if (postStack.Count < 1) throw new ArgumentException("수식오류: 단항 연산자");

                        double postValue = postStack.Pop(),
                               rst;

                        switch (token)
                        {
                            case "u-": rst = -postValue; break;
                            case "abs": rst = Math.Abs(postValue); break;
                            case "sqrt": rst = Math.Sqrt(postValue); break;
                            case "sin": rst = Math.Sin(postValue); break;
                            case "cos": rst = Math.Cos(postValue); break;
                            case "tan": rst = Math.Tan(postValue); break;
                            case "asin": rst = Math.Asin(postValue); break;
                            case "acos": rst = Math.Acos(postValue); break;
                            case "atan": rst = Math.Atan(postValue); break;
                            default: throw new ArgumentException($"미설정 연산자 - {token}");
                        }

                        postStack.Push(rst);
                    }
                    else
                        throw new ArgumentException("수식오류: 미확인 Token");
                }//End oper
            }//End while

            if(postStack.Count > 0) new ArgumentException("수식오류: 미완전 수식");

            return postStack.Pop();
        }

        /// <summary>
        /// 선형회귀 계산식
        /// </summary>
        /// <param name="ary">(x, y)데이터 Array</param>
        /// <returns>(기울기, 절편)</returns>
        /// <remarks>
        /// 기울기: 선의 기울기, 0에 가까울수록 수평<br/>
        /// 절편: 선의 시작점
        /// </remarks>
        public (double, double) LinearRegression((double, double)[] ary)
        {
            double xSum = 0, xAvg,
                    ySum = 0, yAvg;

            for (int i = 0; i < ary.Length; i++)
            {
                xSum += ary[i].Item1;
                ySum += ary[i].Item2;
            }

            xAvg = xSum / ary.Length;
            yAvg = ySum / ary.Length;

            double c = 0, p = 0;
            for (int i = 0; i < ary.Length; i++)
            {
                double xCal = ary[i].Item1 - xAvg,
                       yCal = ary[i].Item2 - yAvg;

                c += xCal * yCal;   //분자
                p += Math.Pow(yCal, 2); //분모
            }

            double slope = c / p,   //기울기
                   intercept = yAvg - (slope * xAvg);  //절편

            return (slope, intercept);
        }

        /// <summary>
        /// 2D 행렬 곱셈
        /// </summary>
        /// <param name="a">전위 2D 행렬</param>
        /// <param name="b">후위 2D 행렬</param>
        /// <returns>곱셈 결과 행렬</returns>
        /// <exception cref="InvalidOperationException">곱셈 불가처리</exception>
        public double[,] Multiply2DArray(double[,] a, double[,] b)
        {
            int aRow = a.GetLength(0),
                aCol = a.GetLength(1),
                bRow = b.GetLength(0),
                bCol = b.GetLength(1);

            if (aCol != bCol)
                throw new InvalidOperationException($"첫번째 행렬의 열({aCol})이 두번째 행열의 행({bRow})과 일치하지 않습니다.");

            double[,] result = new double[aRow, bCol];

            //i: 전위 Array Index
            for (int i = 0; i < aRow; i++)
            {
                //j: 후위 Array Index
                for (int j = 0; j < bCol; j++)
                {
                    double sum = 0;

                    //k: 결과 Index
                    for(int k = 0;k < aCol; k++)
                        sum += a[i, k] * b[k, j];

                    result[i, j] = sum;
                }
            }

            return result;
        }
        /// <summary>
        /// 전치(Pivot)행렬 전환
        /// </summary>
        /// <param name="ary">전환할 행렬</param>
        /// <returns>전환된 행렬</returns>
        public double[,] Pivot2DArray(double[,] ary)
        {
            int row = ary.GetLength(0),
                col = ary.GetLength(1);

            double[,] pivot = new double[col, row];
            for (int i = 0; i < row; i++)
                for (int j = 0; j < col; j++)
                    pivot[j, i] = ary[i, j];

            return pivot;
        }
    }
}
