using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Utils.Views
{
    public abstract class QYUserControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.BindingSource _bindingsource = new System.Windows.Forms.BindingSource();

        /// <summary>
        /// UserControl 최상위 BindingSource
        /// </summary>
        protected System.Windows.Forms.BindingSource BindingSource => this._bindingsource;

        public void BindViewModel<T>(T viewmodel)
        {
            this.BindingSource.DataSource = viewmodel;
        }
    }
}
