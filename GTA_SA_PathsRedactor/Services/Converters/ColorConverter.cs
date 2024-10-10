using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GTA_SA_PathsRedactor.Services.Converters
{
    public sealed class ColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush scb)
            {
                return scb.Color;
            }

            return value;
        }
    }
}
