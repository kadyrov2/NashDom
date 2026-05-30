using System;
using System.Globalization;
using System.Windows.Data;

namespace NashDom.Converters
{
    public class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPaid)
            {
                return isPaid ? "✅ Оплачено" : "⏳ Ожидает оплаты";
            }
            return "❓ Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString()?.Contains("Оплачено") == true;
        }
    }
}