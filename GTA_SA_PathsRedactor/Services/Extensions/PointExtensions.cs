using System.Windows;
using System.Windows.Media.Media3D;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Services
{
    public static class PointExtensions
    {
        public static Point As2DPoint(this GTA_SA_Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point3D As3DPoint(this GTA_SA_Point point)
        {
            return new Point3D(point.X, point.Y, point.Z);
        }
    }
}
