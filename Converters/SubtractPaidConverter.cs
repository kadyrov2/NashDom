using System;
using System.Globalization;
using System.Windows.Data;

namespace NashDom.Converters
{
    public class SubtractPaidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal totalAccrued && parameter != null)
            {
                if (decimal.TryParse(parameter.ToString(), NumberStyles.Any, culture, out decimal paid))
                {
                    return totalAccrued - paid;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}