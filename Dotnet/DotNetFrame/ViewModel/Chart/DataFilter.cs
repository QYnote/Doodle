using DotNet.Utils.Controls.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace DotNetFrame.ViewModel.Chart
{
    internal class DataFilter
    {
        private int _kernal_size = 3;
        private double _peak_threshold = 0;

        public int KernalSize { get => _kernal_size; set => _kernal_size = value; }
        public double Peak_Threshold { get => _peak_threshold; set => _peak_threshold = value; }

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
                List<double> list_in_kernal = List_In_kernal(ref ary, i);

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
            if (weights != null && weights.Length < this._kernal_size * 2 + 1)
                throw new ArgumentException($"Weigths Legnth not allow\r\nCurrent: {weights.Length} / Allow: {this._kernal_size * 2 + 1}");

            List<double> resultData = new List<double>();

            for (int i = 0; i < ary.Length; i++)
            {
                List<double> list_in_kernal = List_In_kernal(ref ary, i);

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

        private List<T> List_In_kernal<T>(ref T[] ary, int currentIndex)
        {
            int minIndex = currentIndex - this._kernal_size,
                    maxIndex = currentIndex + this._kernal_size,
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
            double[] weight = new double[this._kernal_size * 2 + 1];
            double weightSum = 0;

            //△모양의 가중값 입력
            for (int k = 0; k < weight.Length; k++)
            {
                if (k <= this._kernal_size)
                    weight[k] = k + 1;
                else if (k > this._kernal_size)
                    weight[k] = this._kernal_size - (k - this._kernal_size) + 1;
            }
            weightSum = weight.Sum();
            //전체대비 비율로 전환
            for (int k = 0; k < weight.Length; k++)
                weight[k] = weight[k] / weightSum;

            return weight;
        }

        public List<DataPoint> GetPeakList(DataPointCollection collection)
        {
            List<DataPoint> peakList = new List<DataPoint>();   //봉우리목록

            //1. Collection Array화
            DataPoint[] ary = new DataPoint[collection.Count];
            for (int i = 0; i < collection.Count; i++)
                ary[i] = collection[i];

            for (int p = this._kernal_size; p < collection.Count; p++)
            {
                //2. 검사에 사용할 데이터 추출
                List<DataPoint> list_in_kernal = this.List_In_kernal(ref ary, p);

                //3. 봉우리 등록
                bool isPeak = true;

                foreach (var item in list_in_kernal)
                {
                    if (collection[p] == item) continue;

                    //검사할 모든 값보다 크면 봉우리로 처리
                    if (collection[p].YValues[0] < item.YValues[0])
                    {
                        isPeak = false;
                        break;
                    }
                }

                if (isPeak)
                    peakList.Add(collection[p]);
            }

            return peakList;
        }

        public double Calc_Anomaly(double[] ary)
        {
            double peakAvg = ary.Sum() / ary.Length,
                   peakMin = ary.Min(),
                   peakMax = ary.Max(),
                   peakRange = peakMax - peakMin;
            //튀는값 판단값, 봉우리 평균값 + (봉우리 범위 * 보정치)보다 높으면 튀는 데이터로 판단
            return peakAvg + ((peakAvg - peakMin) * 2);// + (peakRange * this._peak_threshold);
        }
    }
}
