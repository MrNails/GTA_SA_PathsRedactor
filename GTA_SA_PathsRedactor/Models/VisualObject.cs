using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Models
{
    public abstract class VisualObject : FrameworkElement, INotifyPropertyChanged
    {
        private GTA_SA_Point m_point;
        private bool m_isSelected;

        protected VisualCollection children;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected VisualObject(GTA_SA_Point point)
        {
            children = new VisualCollection(this);

            Point = point;
            OriginPoint = (GTA_SA_Point)point.Clone();
        }

        public GTA_SA_Point OriginPoint { get; init; }
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
    }
}
