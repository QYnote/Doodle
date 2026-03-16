using System.ComponentModel;

namespace DotNet.Utils.Model
{
    /// <summary>
    /// QY Model Class
    /// </summary>
    /// <remarks>
    /// 데이터 소스 및 HW 제어<br/>
    /// App 전용 자원이 아닌 순수자원(Raw Data), Status, Process, 통신 관리
    /// </remarks>
    /// Model 규모가 작을경우 ViewModel과 거의 동일 할 수 있음
    public abstract class QYModel : INotifyPropertyChanged
    {
        //Model
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
