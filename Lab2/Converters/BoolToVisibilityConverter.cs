using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace Lab2.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        // Преобразование из bool в Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, является ли входное значение bool
            if (value is bool boolValue)
            {
                // Если параметр передан (true), то будет обратное поведение
                bool invert = parameter != null && System.Convert.ToBoolean(parameter);

                if (invert)
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            // Возвращаем Collapsed, если значение не является bool
            return Visibility.Collapsed;
        }

        // Обратное преобразование из Visibility в bool
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }
}
