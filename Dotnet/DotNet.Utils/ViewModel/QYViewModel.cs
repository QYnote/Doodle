using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Utils.ViewModel
{
    public class QYViewModel : INotifyPropertyChanged
    {
        private readonly SynchronizationContext _syncContext;

        public event PropertyChangedEventHandler PropertyChanged;

        protected SynchronizationContext SyncContext => this._syncContext;

        protected QYViewModel()
        {
            this._syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        protected void OnPropertyChanged(string name) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected void RunUI(Action action)
        {
            if(this._syncContext != null)
            {
                this.SyncContext.Post(_ => action(), null);
            }
            else
            {
                action();
            }
        }
    }
}
