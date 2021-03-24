using System;
using System.ComponentModel;

namespace GTA_SA_PathsRedactor.Services
{
    [Serializable]
    public sealed class PointTransformationData : Core.Entity
    {
        private bool m_invertHorizontally;
        private bool m_invertVertically;
        private double m_offsetX;
        private double m_offsetY;
        private double m_pointScaleX;
        private double m_pointScaleY;
        private double m_originalMapWidth;
        private double m_originalMapHeight;
        private string m_transformName;

        public PointTransformationData() 
            : this (0, 0, 0, 0, 0, 0, "empty")
        {}
        public PointTransformationData(double offsetX, double offsetY, double pointScaleX,
                                       double pointScaleY, double originalMapWidth,
                                       double originalMapHeight, string transformName)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            PointScaleX = pointScaleX;
            PointScaleY = pointScaleY;
            OriginalMapWidth = originalMapWidth;
            OriginalMapHeight = originalMapHeight;
            TransformName = transformName;
        }

        public bool InvertHorizontally
        {
            get { return m_invertHorizontally; }
            set
            {
                m_invertHorizontally = value;
                OnPropertyChanged("InvertHorizontally");
            }
        }
        public bool InvertVertically
        {
            get { return m_invertVertically; }
            set
            {
                m_invertVertically = value;
                OnPropertyChanged("InvertVertically");
            }
        }
        public double OffsetX
        {
            get { return m_offsetX; }
            set
            {
                m_offsetX = value;
                OnPropertyChanged("OffsetX");
            }
        }
        public double OffsetY
        {
            get { return m_offsetY; }
            set
            {
                m_offsetY = value;
                OnPropertyChanged("OffsetY");
            }
        }
        public double PointScaleX
        {
            get { return m_pointScaleX; }
            set
            {
                m_pointScaleX = value;
                OnPropertyChanged("PointScaleX");
            }
        }
        public double PointScaleY
        {
            get { return m_pointScaleY; }
            set
            {
                m_pointScaleY = value;
                OnPropertyChanged("PointScaleY");
            }
        }
        public double OriginalMapWidth
        {
            get { return m_originalMapWidth; }
            set
            {
                m_originalMapWidth = value;
                OnPropertyChanged("OriginalMapWidth");
            }
        }
        public double OriginalMapHeight
        {
            get { return m_originalMapHeight; }
            set
            {
                m_originalMapHeight = value;
                OnPropertyChanged("OriginalHeight");
            }
        }
        public string TransformName
        {
            get { return m_transformName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    m_errors["TransformName"] = "Transformation name can't be empty.";
                }
                else
                {
                    m_errors["TransformName"] = "";
                }

                m_transformName = value;
                OnPropertyChanged("TransformName");
            }
        }
    }
}
