using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Lab2.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        // Преобразование из bool в Brush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Если значение true, возвращаем LightCoral, если false — Green
                return boolValue ? Brushes.LightCoral : Brushes.LawnGreen;
            }
            // Возвращаем стандартный цвет (например, Transparent), если значение не является bool
            return Brushes.Transparent;
        }

        // Обратное преобразование, если нужно (обычно не используется для цветов)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
