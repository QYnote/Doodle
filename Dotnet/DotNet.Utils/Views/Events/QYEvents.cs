using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotNet.Utils.Views.Events
{
    public class QYEvents
    {
        #region 마우스 선택 영역 Rectangle 가져오기

        private Form _sampleForm = null;
        private bool _isMouseDown = false;
        private Point _selectionMouseDownStart = new Point(0, 0);
        private Point _selectionMouseDownEnd = new Point(0, 0);
        private Rectangle _selectionRect = Rectangle.Empty;

        private Rectangle GetSelection()
        {
            this._sampleForm = new Form();
            this._sampleForm.MouseDown += MouseDown_SelectionStart;
            this._sampleForm.MouseMove += _sampleForm_MouseMove;
            this._sampleForm.MouseUp += _sampleForm_MouseUp;

            return this._selectionRect;
        }

        private void MouseDown_SelectionStart(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            this._isMouseDown = true;
            this._selectionMouseDownStart = e.Location;
            this._selectionMouseDownEnd = this._selectionMouseDownStart;
        }

        private void _sampleForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (this._isMouseDown == false) return;

            this._selectionMouseDownEnd = e.Location;
        }

        private void _sampleForm_MouseUp(object sender, MouseEventArgs e)
        {
            this._isMouseDown = false;

            this._selectionRect = new Rectangle(
                Math.Min(this._selectionMouseDownStart.X, this._selectionMouseDownEnd.X),
                Math.Min(this._selectionMouseDownStart.Y, this._selectionMouseDownEnd.Y),
                Math.Abs(this._selectionMouseDownStart.X - this._selectionMouseDownEnd.X),
                Math.Abs(this._selectionMouseDownStart.Y - this._selectionMouseDownEnd.Y)
                );

        }

        #endregion 마우스 선택 영역  Rectangle 가져오기
    }
}
