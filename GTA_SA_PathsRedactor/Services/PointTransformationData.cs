using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA_SA_PathsRedactor.Services
{
    [Serializable]
    internal sealed class PointTransformationData : INotifyPropertyChanged
    {
        public static readonly PointTransformationData TransformatorForResolution800x600;
        public static readonly PointTransformationData TransformatorForResolution1080x850;
        public static readonly PointTransformationData TransformatorForResolution1280x1024;
        public static readonly PointTransformationData TransformatorForResolution1680x1050;
        public static readonly PointTransformationData TransformatorForResolution1920x1080;
        public static PointTransformationData CurrentTransformator;

        private double m_offsetX;
        private double m_offsetY;
        private double m_pointScaleX;
        private double m_pointScaleY;
        private double m_originalMapWidth;
        private double m_originalMapHeight;
        private string m_transformName;

        //TransformatorForResolution800x600 = new PointTransformationData(455, 395, 6.6, 7.6, 630, 540);
        //TransformatorForResolution1080x850 = new PointTransformationData(455, 395, 6.6, 7.6, 910, 810);
        //TransformatorForResolution1280x1024 = new PointTransformationData(355, 320, 5.4, 6.2, 1110, 963);
        //TransformatorForResolution1680x1050 = new PointTransformationData(455, 395, 6.6, 7.6, 1510, 990);
        //TransformatorForResolution1920x1080 = new PointTransformationData(455, 395, 6.6, 7.6, 1766, 1035);

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
                    throw new ArgumentException("Tranformation name can't be empty.");
                }

                m_transformName = value;
                OnPropertyChanged("TransformName");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
