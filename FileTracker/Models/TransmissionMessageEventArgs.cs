using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTracker.Models
{
    /// <summary>
    /// Modelから送出されるメッセージを表します。
    /// </summary>
    public class TransmissionMessageEventArgs : EventArgs
    {
        /// <summary>
        /// メッセージを格納します。
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// メッセージが処理されたかを示す値を取得、設定します。
        /// </summary>
        public bool Handled { get; set; }

        public TransmissionMessageEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
