using GalaSoft.MvvmLight.Command;
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
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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
    //public class MousePositionConverter : IEventArgsConverter
    //{
    //    public object? Convert(object? value, object parameter)
    //    {
    //        var args = value as MouseEventArgs;

    //        System.Diagnostics.Debug.WriteLine(value);

    //        if (args == null)
    //        {
    //            return value;
    //        }

    //        if (parameter is BindingProxy proxy)
    //        {
    //            var point = args.GetPosition(proxy.Data as IInputElement);

    //            return new Core.Models.GTA_SA_Point(point.X, point.Y, 0, false);
    //        }
    //        else if (parameter is IInputElement inputElement)
    //        {
    //            var point = args.GetPosition(inputElement);

    //            return new Core.Models.GTA_SA_Point(point.X, point.Y, 0, false);
    //        }
    //        else
    //        {
    //            return value;
    //        }
    //    }
    //}
}
