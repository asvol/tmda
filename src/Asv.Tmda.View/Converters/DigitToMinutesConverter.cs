using System;
using System.Globalization;
using System.Windows.Data;

namespace Asv.Tmda.View
{
    public class DigitToMinutesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var decimal_degrees = (double)value;

            // set decimal_degrees value here

            var minutes = (decimal_degrees - Math.Floor(decimal_degrees)) * 60.0;
            var seconds = (minutes - Math.Floor(minutes)) * 60.0;
            var tenths = (seconds - Math.Floor(seconds));
            // get rid of fractional part
            minutes = Math.Floor(minutes);
            seconds = Math.Floor(seconds);

            return string.Format(CultureInfo.InvariantCulture, "{0:F0}° {1:F0}' {2:F0}\"{3:.000}", decimal_degrees, minutes, seconds, tenths);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
