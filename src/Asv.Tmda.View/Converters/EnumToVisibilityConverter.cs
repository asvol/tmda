using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Asv.Tmda.View
{
    public class EnumToVisibilityConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null) return false;
            var par = parameter.ToString();
            if (par.StartsWith("!"))
            {
                par = par.Substring(1);
                var res = (par.Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase));
                return res ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                var res = (par.Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase));
                return res ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}