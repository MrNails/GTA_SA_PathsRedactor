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
            if (parameter == null)
                return value;

            var currentPTD = (PointTransformationData?)null; //GlobalSettings.GetInstance().GetCurrentTranfromationData();

            if (currentPTD == null)
                return value;

            double nValue = (double)value;

            switch (parameter.ToString().ToLower())
            {
                case "x":
                    int horizontallyInvert = currentPTD.InvertHorizontally ? -1 : 1;

                    return Math.Round(horizontallyInvert * currentPTD.PointScaleX * (nValue - currentPTD.OffsetX), 5);
                case "y":
                    int verticallyInvert = currentPTD.InvertVertically ? -1 : 1;

                    return Math.Round(verticallyInvert * currentPTD.PointScaleY * (nValue - currentPTD.OffsetY), 5);
                default:
                    break;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return value;

            var currentPTD = (PointTransformationData?)null; //GlobalSettings.GetInstance().GetCurrentTranfromationData();

            if (currentPTD == null)
                return value;

            if (double.TryParse(value.ToString(), NumberStyles.Float | NumberStyles.AllowTrailingSign, CultureInfo.InvariantCulture, out double nValue))
            {
                switch (parameter.ToString().ToLower())
                {
                    case "x":
                        int horizontallyInvert = currentPTD.InvertHorizontally ? -1 : 1;

                        return horizontallyInvert * nValue / currentPTD.PointScaleX + currentPTD.OffsetX;
                    case "y":
                        int verticallyInvert = currentPTD.InvertVertically ? -1 : 1;

                        return verticallyInvert * nValue / currentPTD.PointScaleY + currentPTD.OffsetY;
                    default:
                        break;
                }
            }

            return value;
        }
    }
}
