using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Asv.Tmda.View
{
    public class ResourceByStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null)
                return DependencyProperty.UnsetValue;
            return Application.Current.FindResource(value.ToString().Replace('-','_'));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}