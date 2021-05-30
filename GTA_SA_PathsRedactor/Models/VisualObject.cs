using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.Models
{
    public abstract class VisualObject : FrameworkElement, INotifyPropertyChanged, ITransformable
    {
        private GTA_SA_Point m_point;
        private GTA_SA_Point m_originPoint;
        private bool m_isSelected;

        protected VisualCollection children;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected VisualObject(GTA_SA_Point point)
        {
            children = new VisualCollection(this);

            Point = point;
            m_originPoint = (GTA_SA_Point)point.Clone();
        }

        public GTA_SA_Point OriginPoint
        {
            get => m_originPoint;
        }
        public GTA_SA_Point Point
        {
            get => m_point;
            set
            {
                m_point = value;
                m_point.PropertyChanged -= ObjectPropertyChanged;
                m_point.PropertyChanged += ObjectPropertyChanged;

                Draw();

                OnPropertyChanged("Point");
            }
        }

        public bool IsSelected
        {
            get => m_isSelected;
            set
            {
                m_isSelected = value;
                Draw();
                OnPropertyChanged("IsSelected");
            }
        }

        protected override int VisualChildrenCount
        {
            get { return children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return children[index];
        }

        protected void ObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Draw();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Draw();
            base.OnRender(drawingContext);
        }

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public abstract void Draw();

        public void Transform(PointTransformationData? pointTransformationData)
        {
            int horizontallyInvert = pointTransformationData.InvertHorizontally ? -1 : 1;
            int verticallyInvert = pointTransformationData.InvertVertically ? -1 : 1;

            Point.X = horizontallyInvert * OriginPoint.X / pointTransformationData.PointScaleX + pointTransformationData.OffsetX;
            Point.Y = verticallyInvert * OriginPoint.Y / pointTransformationData.PointScaleY + pointTransformationData.OffsetY;
        }

        public void TransformBack(PointTransformationData? pointTransformationData)
        {
            int horizontallyInvert = pointTransformationData.InvertHorizontally ? -1 : 1;
            int verticallyInvert = pointTransformationData.InvertVertically ? -1 : 1;

            m_originPoint.X = horizontallyInvert * pointTransformationData.PointScaleX * (m_point.X - pointTransformationData.OffsetX);
            m_originPoint.Y = verticallyInvert * pointTransformationData.PointScaleY * (m_point.Y - pointTransformationData.OffsetY);

            m_point.Z = m_originPoint.Z;
            m_point.IsStopPoint = m_originPoint.IsStopPoint;
        }
    }
}
