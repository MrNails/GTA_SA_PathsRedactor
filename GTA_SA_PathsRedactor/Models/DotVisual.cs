using System;
using System.Windows;
using System.Windows.Media;

namespace GTA_SA_PathsRedactor.Models
{
    public class DotVisual : VisualObject
    {
        private static readonly Pen s_defaultPen;
        private static readonly Pen s_selectedPen;

        private Brush m_color;

        static DotVisual()
        {
            s_defaultPen = new Pen(new SolidColorBrush(Colors.Transparent), 0);
            s_selectedPen = new Pen(new SolidColorBrush(Colors.Black), 1);
        }

        public DotVisual() : this (new GTA_SA_Point())
        { }
        public DotVisual(GTA_SA_Point point) : this(point, new SolidColorBrush(Colors.Red))
        { }
        public DotVisual(GTA_SA_Point point, Brush brush)
            : base(point)
        {
            Point = point;
            Color = brush;
        }

        public Brush Color
        {
            get => m_color;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_color = value;
                Draw();
                OnPropertyChanged("Color");
            }
        }

        public override void Draw()
        {
            children.Clear();

            DrawingVisual body = new DrawingVisual();

            Pen pen = null;

            if (IsSelected)
            {
                pen = s_selectedPen;
            }
            else
            {
                pen = s_defaultPen;
            }

            using (var context = body.RenderOpen())
            {
                context.DrawEllipse(m_color, pen, (Point)Point, 2, 2);
            }

            children.Add(body);
        }
    }
}
