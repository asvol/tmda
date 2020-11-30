using System;
using System.Globalization;
using System.Windows.Data;

namespace Asv.Tmda.View
{
    public class AbsoluteDigitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return Math.Abs(int.Parse(value.ToString()));
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
