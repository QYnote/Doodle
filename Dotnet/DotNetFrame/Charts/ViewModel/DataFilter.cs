using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetFrame.Chart.ViewModel
{
    internal class DataFilter
    {
        internal const int DEFAULT_FILTER_KERNAL_SIZE = 10;
        internal const int DEFAULT_PEAK_KERNAL_SIZE = 10;

        private int _filter_kernal_size = DEFAULT_FILTER_KERNAL_SIZE;
        private int _peak_kernal_size = DEFAULT_PEAK_KERNAL_SIZE;

        public int Filter_KernalSize { get => _filter_kernal_size; set => _filter_kernal_size = value; }
        public int Peak_KernalSize { get => _peak_kernal_size; set => _peak_kernal_size = value; }

        /// <summary>
        /// 단순 이동 평균
        /// </summary>
        /// <param name="ary">원본 Data Array</param>
        /// <returns>필터링이 적용된 Data</returns>
        public double[] MAF(double[] ary)
        {
            if (ary == null || ary.Length == 0) return null;
            List<double> resultData = new List<double>();

            for (int i = 0; i < ary.Length; i++)
            {
                List<double> list_in_kernal = List_In_kernal(ref ary, i, this._filter_kernal_size);

                double v = list_in_kernal.Sum() / list_in_kernal.Count;

                resultData.Add(v);
            }

            return resultData.ToArray();
        }
        /// <summary>
        /// 가중 이동 평균
        /// </summary>
        /// <param name="ary">원본 Data Array</param>
        /// <param name="weights">가중치 Array</param>
        /// <returns>필터링이 적용된 Data</returns>
        /// <exception cref="ArgumentException">가중치 Array 길이 미일치 에러</exception>
        public double[] WAF(double[] ary, double[] weights = null)
        {
            if (ary == null || ary.Length == 0) return null;
            if (weights != null && weights.Length < this._filter_kernal_size * 2 + 1)
                throw new ArgumentException($"Weigths Legnth not allow\r\nCurrent: {weights.Length} / Allow: {this._filter_kernal_size * 2 + 1}");

            List<double> resultData = new List<double>();

            for (int i = 0; i < ary.Length; i++)
            {
                List<double> list_in_kernal = List_In_kernal(ref ary, i, this._filter_kernal_size);

                if (weights == null)
                    weights = this.Default_WAF_Weights();

                //Data에 가중치 부여
                double v = 0;
                for (int k = 0; k < list_in_kernal.Count; k++)
                    v += (list_in_kernal[k] * weights[k]);

                resultData.Add(v);
            }

            return resultData.ToArray();
        }

        private List<T> List_In_kernal<T>(ref T[] ary, int currentIndex, int kernalSize)
        {
            int minIndex = currentIndex - kernalSize,
                maxIndex = currentIndex + kernalSize,
                handle = 0,
                offset = minIndex + handle;
            List<T> result = new List<T>();

            //1. Kernal 내에 있는 데이터 추출
            while (offset < currentIndex)
            {
                //1-1. 현재 Index보다 작은 Index Data Kernal 수만큼 추출
                offset = minIndex + handle;

                if (offset >= 0)
                    result.Add(ary[offset]);

                handle++;
            }

            //1-2. 중앙값 데이터 등록
            result.Add(ary[currentIndex]);
            handle++;

            while (offset < maxIndex)
            {
                //1-3. 현재 Index보다 큰 Index Data Kernal 수만큼 추출
                offset = minIndex + handle;
                if (offset < ary.Length - 1)
                    result.Add(ary[offset]);

                handle++;
            }

            return result;
        }

        private double[] Default_WAF_Weights()
        {
            double[] weight = new double[this._filter_kernal_size * 2 + 1];
            double weightSum = 0;

            //△모양의 가중값 입력
            for (int k = 0; k < weight.Length; k++)
            {
                if (k <= this._filter_kernal_size)
                    weight[k] = k + 1;
                else if (k > this._filter_kernal_size)
                    weight[k] = this._filter_kernal_size - (k - this._filter_kernal_size) + 1;
            }
            weightSum = weight.Sum();
            //전체대비 비율로 전환
            for (int k = 0; k < weight.Length; k++)
                weight[k] = weight[k] / weightSum;

            return weight;
        }

        public List<int> GetPeakIndexList(double[] ary)
        {
            List<int> peakList = new List<int>();   //봉우리목록

            for (int p = this._peak_kernal_size; p < ary.Length; p++)
            {
                //2. 검사에 사용할 데이터 추출
                List<double> list_in_kernal = this.List_In_kernal(ref ary, p, this._peak_kernal_size);

                //3. 봉우리 등록
                bool isPeak = true;

                foreach (var item in list_in_kernal)
                {
                    if (ary[p] == item) continue;

                    //검사할 모든 값보다 크면 봉우리로 처리
                    if (ary[p] <= item)
                    {
                        isPeak = false;
                        break;
                    }
                }

                if (isPeak)
                    peakList.Add(p);
            }

            return peakList;
        }

        public double Calc_Anomaly(double[] dataAry, double[] peakAry)
        {
            double peakAvg = peakAry.Sum() / peakAry.Length,
                   dataMin = dataAry.Min();
                   //peakMax = ary.Max(),
                   //peakRange = peakMax - peakMin;
            //튀는값 판단값 = 봉우리 평균값 + (봉우리 평균값 - 데이터 최소값)
            return peakAvg + (peakAvg - dataMin);
        }
    }
}
