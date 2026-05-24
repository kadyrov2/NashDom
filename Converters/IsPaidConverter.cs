using System;
using System.Globalization;
using System.Windows.Data;

namespace NashDom.Converters
{
    public class IsPaidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPaid)
            {
                return isPaid ? "Оплачено" : "Не оплачено";
            }
            return "Не известно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == "Оплачено";
        }
    }
}