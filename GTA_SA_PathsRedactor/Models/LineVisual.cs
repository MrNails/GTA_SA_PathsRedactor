using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.Models
{
    public record DividedLine(LineVisual LeftLine, LineVisual RightLine);

    public class LineVisual : FrameworkElement, INotifyPropertyChanged
    {
        private static readonly Brush s_defaultLineColor;

        private GTA_SA_Point m_startPoint;
        private GTA_SA_Point m_endPoint;

        private Pen m_color;
        private VisualCollection children;

        public event PropertyChangedEventHandler? PropertyChanged;

        static LineVisual()
        {
            s_defaultLineColor = new SolidColorBrush(Colors.Red);
        }

        public LineVisual(GTA_SA_Point start, GTA_SA_Point end)
            : this (start, end, s_defaultLineColor)
        {}
        public LineVisual(GTA_SA_Point start, GTA_SA_Point end, Brush lineColor)
        {
            children = new VisualCollection(this);
            m_color = new Pen();

            Start = start;
            End = end;
            LineColor = lineColor;
        }

        public GTA_SA_Point Start
        {
            get { return m_startPoint; }
            set 
            {
                m_startPoint = value;
                m_startPoint.PropertyChanged -= ObjectPropertyChanged;
                m_startPoint.PropertyChanged += ObjectPropertyChanged;

                if (m_endPoint != null)
                {
                    Draw();
                }

                OnPropertyChanged("Start");
            }
        }
        public GTA_SA_Point End
        {
            get { return m_endPoint; }
            set 
            { 
                m_endPoint = value;
                m_endPoint.PropertyChanged -= ObjectPropertyChanged;
                m_endPoint.PropertyChanged += ObjectPropertyChanged;

                if (m_startPoint != null)
                {
                    Draw();
                }

                OnPropertyChanged("End");
            }
        }
        public Brush LineColor
        {
            get { return m_color.Brush; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_color.Brush = value;
                Draw();
                OnPropertyChanged("LineColor");
            }
        }
        public double LineThickness
        {
            get { return m_color.Thickness; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Line thickness can't be less than zero.");
                }

                m_color.Thickness = value;
                Draw();
                OnPropertyChanged("LineThickness");
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
        protected override void OnRender(DrawingContext drawingContext)
        {
            Draw();
            base.OnRender(drawingContext);
        }

        protected void ObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Draw();
        }
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public virtual void Draw()
        {
            children.Clear();

            DrawingVisual body = new DrawingVisual();

            using (var context = body.RenderOpen())
            {
                context.DrawLine(m_color, m_startPoint.As2DPoint(), m_endPoint.As2DPoint());
            }

            children.Add(body);
        }

        public static DividedLine DivideLine(LineVisual oldLine, GTA_SA_Point newPoint)
        {
            LineVisual left = new LineVisual(oldLine.m_startPoint, newPoint);
            LineVisual right = new LineVisual(newPoint, oldLine.m_endPoint);

            return new DividedLine(left, right);
        }
    }
}
