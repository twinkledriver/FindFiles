using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Wpf_FindFiles
{   
    class NumberConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long longValue = (long)value;

            double temp = 0;
            if ((longValue >= 1024) && (longValue < 1024 * 1024))
            {
                temp = (double)longValue / 1024;
                return temp.ToString("N") + "K";
            }

            if ((longValue >=1024*1024)&&(longValue<1024*1024*1024))
            {
                temp=(double)longValue/(1024*1024);
                return temp.ToString("N") + "M";
            }

            if((longValue>=1024*1024*1024))
            {
                temp=(double)longValue/(1024*1024*1024);
                return temp.ToString("N") + "G";
            }

            return longValue.ToString() + "字节";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
