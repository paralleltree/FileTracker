using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FileTracker.Views.Converters
{
    class ByteSizeConverter : IValueConverter
    {
        static readonly string[] Suffix = new string[] { "", "K", "M", "G", "T" };
        static readonly double Unit = 1024;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is long)) throw new ArgumentException("サイズを表すバイト数はlong型でなければいけません。", "value");

            double size = (double)((long)value);

            int i;
            for (i = 0; i < Suffix.Length - 1; i++)
            {
                if (size < Unit) break;
                size /= Unit;
            }

            return string.Format("{0:" + (i == 0 ? "0" : "0.0") + "} {1}B", size, Suffix[i]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
