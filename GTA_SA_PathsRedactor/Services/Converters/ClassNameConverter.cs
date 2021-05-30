using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace GTA_SA_PathsRedactor.Services.Converters
{
    public class ClassNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sValue = value.ToString().AsSpan();

            if (sValue.Contains('.'))
            {
                var name = sValue.Slice(sValue.LastIndexOf('.') + 1);

                return name.ToString();
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
