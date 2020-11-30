using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Asv.Tmda.View
{
    public class NullNotVisibleConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ("!".Equals(parameter))
            {
                return value == null ? Visibility.Visible:Visibility.Collapsed;
            }
            else
            {
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
