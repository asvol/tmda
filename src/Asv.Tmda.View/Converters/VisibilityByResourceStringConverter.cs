using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Asv.Tmda.View
{
    public class VisibilityByResourceStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null || string.IsNullOrWhiteSpace(value as string))
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    

    
}