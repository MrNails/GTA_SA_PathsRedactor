using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Models
{
    public class GTA_SA_Point : INotifyPropertyChanged, ICloneable, IEquatable<GTA_SA_Point>
    {
        private double m_x;
        private double m_y;
        private double m_z;
        private bool m_isStopPoint;

        public event PropertyChangedEventHandler PropertyChanged;

        public GTA_SA_Point() : this(0, 0, 0, false)
        { }
        public GTA_SA_Point(System.Windows.Point point, double z, bool isStopPoint) : this(point.X, point.Y, z, isStopPoint)
        { }
        public GTA_SA_Point(System.Windows.Media.Media3D.Point3D point3D, bool isStopPoint) : this(point3D.X, point3D.Y, point3D.Z, isStopPoint)
        { }
        public GTA_SA_Point(double x, double y, double z, bool isStopPoint)
        {
            X = x;
            Y = y;
            Z = z;
            IsStopPoint = isStopPoint;
        }

        public double X
        {
            get { return m_x; }
            set 
            { 
                m_x = value;
                OnPropertyChanged("X");
            }
        }
        public double Y
        {
            get { return m_y; }
            set
            {
                m_y = value;
                OnPropertyChanged("Y");
            }
        }
        public double Z
        {
            get { return m_z; }
            set
            {
                m_z = value;
                OnPropertyChanged("Z");
            }
        }
        public bool IsStopPoint
        {
            get { return m_isStopPoint; }
            set
            {
                m_isStopPoint = value;
                OnPropertyChanged("IsStopPoint");
            }
        }

        public System.Windows.Point GetAsPoint2D()
        {
            return new System.Windows.Point(m_x, m_y);
        }
        public System.Windows.Media.Media3D.Point3D GetAsPoint3D()
        {
            return new System.Windows.Media.Media3D.Point3D(m_x, m_y, m_z);
        }

        public object Clone()
        {
            return new GTA_SA_Point(m_x, m_y, m_z, m_isStopPoint);
        }

        public override bool Equals(object obj)
        {
            GTA_SA_Point gtaPoint = obj as GTA_SA_Point;

            return Equals(gtaPoint);
        }
        public override int GetHashCode()
        {
            return m_x.GetHashCode() ^ m_y.GetHashCode() ^ m_z.GetHashCode() ^ m_isStopPoint.GetHashCode();
        }
        public override string ToString()
        {
            return $"{{{m_x}, {m_y}, {m_z}}} IsStop = {m_isStopPoint}";
        }

        public bool Equals(GTA_SA_Point other)
        {
            if (other == null)
            {
                return false;
            }

            return other.m_x == m_x && other.m_y == m_y &&
                   other.m_z == m_z && other.m_isStopPoint == m_isStopPoint;
        }

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public static explicit operator System.Windows.Point(GTA_SA_Point gtaPoint)
        {
            return gtaPoint.GetAsPoint2D();
        }
        public static explicit operator System.Windows.Media.Media3D.Point3D(GTA_SA_Point gtaPoint)
        {
            return gtaPoint.GetAsPoint3D();
        }

        public static bool operator ==(GTA_SA_Point left, GTA_SA_Point right)
        {
            if (ReferenceEquals(left, null))
            {
                if (ReferenceEquals(right, null))
                    return true;
                else
                    return false;
            }
            else
            {
                return left.Equals(right);
            }
        }
        public static bool operator !=(GTA_SA_Point left, GTA_SA_Point right)
        {
            return !(left == right);
        }
    }
}
