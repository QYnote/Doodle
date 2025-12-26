using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Utils.Controls.Utils
{
    public class QYComm
    {
        /// <summary>
        /// Byte Array 합치기
        /// </summary>
        /// <param name="baseBytes">앞에위치할 Byte Array</param>
        /// <param name="backBytes">뒤에 위치할 Byte Array</param>
        /// <returns></returns>
        public byte[] BytesAppend(byte[] baseBytes, byte[] backBytes)
        {
            if (backBytes == null) return baseBytes;
            byte[] containByts = new byte[baseBytes.Length + backBytes.Length];

            //옮길 Array, 옮길 Array 시작 index, 넘겨받은 Array, 넘겨받을 Array index, 옮길 Array 수
            Buffer.BlockCopy(baseBytes, 0, containByts, 0, baseBytes.Length);
            Buffer.BlockCopy(backBytes, 0, containByts, baseBytes.Length, backBytes.Length);

            baseBytes = containByts;

            //메모리 초기화
            containByts = null;
            return baseBytes;
        }
    }

    public class QYBindingBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<string> ErrorMessage;

        protected void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void OnErrorMessage(string message) => this.ErrorMessage?.Invoke(message);
    }

}
