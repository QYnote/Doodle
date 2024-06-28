using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dnf.Comm.Frm
{
    public partial class TabPageBase : TabPage
    {
        public delegate void PageRemoveDelegate();
        public PageRemoveDelegate BeforeRemovePageHandler;

        public TabPageBase(string PageName, string Caption)
        {
            base.Name = PageName;
            base.Text = Caption;

            InitializeComponent();
        }

        /// <summary>
        /// 현재 TabPage 삭제
        /// </summary>
        public void Remove()
        {
            this.BeforeRemovePageHandler?.Invoke();

            Thread.Sleep(10);
            this.Dispose();
        }
    }
}
