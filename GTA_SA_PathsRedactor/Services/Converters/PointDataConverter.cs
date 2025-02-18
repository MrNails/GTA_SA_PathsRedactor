using System;
using System.Globalization;
using System.Windows.Data;
using GTA_SA_PathsRedactor.Models;

namespace GTA_SA_PathsRedactor.Services.Converters
{
    public sealed class PointDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
            
            if (parameter == null)
                return value;
            

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value;

            var currentPTD = (PointTransformationData?)null; //GlobalSettings.GetInstance().GetCurrentTranfromationData();

            if (currentPTD == null)
                return value;

            return value;
        }
    }
}
