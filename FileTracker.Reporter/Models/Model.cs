using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;

namespace FileTracker.Reporter.Models
{
    public class Model : NotificationObject
    {
        private Model()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                IsInfoGiven = true;
                InfoPath = System.IO.Path.GetFullPath(args[1]);
                ExceptionInfo = ExceptionInfo.ReadInfo(args[1]);
            }
        }

        static Model _instance;
        public static Model Instance
        {
            get
            {
                if (_instance == null) _instance = new Model();
                return _instance;
            }
        }

        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        /// <summary>
        /// 例外情報が渡されたかどうかを格納します。
        /// </summary>
        public bool IsInfoGiven { get; private set; }

        /// <summary>
        /// 渡された例外情報のパスを格納します。
        /// </summary>
        public string InfoPath { get; private set; }

        /// <summary>
        /// 渡された例外情報を格納します。
        /// </summary>
        public ExceptionInfo ExceptionInfo { get; private set; }


        public void SaveInfo()
        {
            ExceptionInfo.WriteInfo(ExceptionInfo, InfoPath);
        }
    }
}
