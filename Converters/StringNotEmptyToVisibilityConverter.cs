using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NashDom.Converters
{
    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                if (parameter?.ToString() == "Inverse")
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
            return parameter?.ToString() == "Inverse" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}