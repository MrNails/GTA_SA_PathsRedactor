using System;
using System.Globalization;
using System.Windows.Data;

namespace GTA_SA_PathsRedactor.Services.Converters
{
    public sealed class HistoryHasChangedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool changed)
            {
                return changed ? "Current path have changes" : string.Empty;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
