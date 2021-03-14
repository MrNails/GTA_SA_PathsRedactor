using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace GTA_SA_PathsRedactor.Services.Converters
{
    public class MousePositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BindingProxy proxy)
            {
                var point = Mouse.GetPosition(proxy.Data as IInputElement);

                return new Core.Models.GTA_SA_Point(point.X, point.Y, 0, false);
            }
            else if (value is IInputElement inputElement)
            {
                var point = Mouse.GetPosition(inputElement);

                return new Core.Models.GTA_SA_Point(point.X, point.Y, 0, false);
            }
            else 
            {
                return value;
            }
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
