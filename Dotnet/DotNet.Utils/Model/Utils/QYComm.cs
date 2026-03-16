using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Utils.Controls.Utils
{
    /// <summary>QY ViewModel Class</summary>
    /// <remarks>
    /// UI Process 및 데이터 다공<br/>
    /// Model 참조, UI간에만 진행되는 Process
    /// </remarks>
    public class QYViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<string> ErrorMessage;

        /// <summary>
        /// UI Thread
        /// </summary>
        private readonly SynchronizationContext _ui_thread = null;

        protected SynchronizationContext UIThread => this._ui_thread;

        public QYViewModel()
        {
            this._ui_thread = SynchronizationContext.Current;
        }

        protected void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void OnErrorMessage(string message) => this.ErrorMessage?.Invoke(message);
    }

}
