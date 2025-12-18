using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Utils.Controls.Utils
{
    public class QYViewModelHandler : INotifyPropertyChanged
    {
        public event Action<string> ErrorMessage;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPopertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void OnErrorMessage(string str) => this.ErrorMessage?.Invoke(str);
    }
}
