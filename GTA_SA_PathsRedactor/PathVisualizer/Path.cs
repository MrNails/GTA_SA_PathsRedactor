using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using GTA_SA_PathsRedactor.Models;

namespace GTA_SA_PathsRedactor.PathVisualizer
{
    public class Path : FrameworkElement, INotifyPropertyChanged
    {
        private static readonly Brush s_defaultLinesColor;

        private string m_pathName;

        private List<VisualObject> m_dots;
        private List<LineVisual> m_lines;
        private VisualObject m_currentObject;
        private Pen m_linesColor;

        private bool m_isChangingTransform;

        protected VisualCollection children;

        public event PropertyChangedEventHandler PropertyChanged;

        static Path()
        {
            s_defaultLinesColor = new SolidColorBrush(Colors.Red);
        }

        public Path(string pathName)
            : this(pathName, s_defaultLinesColor)
        { }
        public Path(string pathName, Brush linesColor)
        {
            children = new VisualCollection(this);

            var defaultLine = new LineVisual(new GTA_SA_Point(0, 0, 0, false),
                                             new GTA_SA_Point(0, 0, 0, false));
            defaultLine.PropertyChanged += ObjectPropertyChanged;
            children.Add(defaultLine);

            m_isChangingTransform = false;

            m_dots = new List<VisualObject>();
            m_lines = new List<LineVisual> { defaultLine };
            m_linesColor = new Pen();

            LinesColor = linesColor;
            LinesThickness = 2;
            PathName = pathName;
        }

        public int PointCount
        {
            get { return m_dots.Count; }
        }


        public string PathName
        {
            get { return m_pathName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_pathName = value;
                OnPropertyChanged("PathName");
            }
        }
        public double LinesThickness
        {
            get { return m_linesColor.Thickness; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Path's lines thickness can't be less than zero.");
                }

                m_linesColor.Thickness = value;
                OnPropertyChanged("LinesThickness");
            }
        }
        public Brush LinesColor
        {
            get { return m_linesColor.Brush; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_linesColor.Brush = value;
                OnPropertyChanged("LinesColor");
            }
        }
        public VisualObject CurrentObject
        {
            get { return m_currentObject; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_currentObject = value;
                OnPropertyChanged("CurrentObject");
            }
        }

        public void AddPoint(GTA_SA_Point point)
        {
            AddPoint(new DotVisual(point));
        }
        public void AddPoint(GTA_SA_Point point, Brush brush)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }

            AddPoint(new DotVisual(point, brush));
        }
        public void AddPoint(VisualObject dot)
        {
            if (dot == null)
            {
                throw new ArgumentNullException("dot");
            }

            dot.MouseUp += ObecjctClicked_MouseDown;
            dot.PropertyChanged += ObjectPropertyChanged;

            m_dots.Add(dot);
            children.Add(dot);

            if (m_dots.Count > 1)
            {
                var line = new LineVisual(m_dots[m_dots.Count - 2].Point, dot.Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;

                line.PropertyChanged += ObjectPropertyChanged;

                m_lines.Add(line);
                children.Insert(0, line);
            }

            Draw();
        }

        public void AddRangePoint(IEnumerable<GTA_SA_Point> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            AddRangePoint(points.Select(p => new DotVisual(p)));
        }
        public void AddRangePoint(IEnumerable<VisualObject> dots)
        {
            if (dots == null)
            {
                throw new ArgumentNullException("dot");
            }

            foreach (var dot in dots)
            {
                dot.MouseUp += ObecjctClicked_MouseDown;
                dot.PropertyChanged += ObjectPropertyChanged;
            }

            int lastIndex = m_dots.Count - 1;
            m_dots.AddRange(dots);

            if (m_dots.Count > 1 && lastIndex != -1)
            {
                var line = new LineVisual(m_dots[lastIndex].Point, m_dots[lastIndex + 1].Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;
                line.PropertyChanged += ObjectPropertyChanged;


                children.Add(m_dots[lastIndex]);
                children.Insert(0, line);

                var dotEnumerator = dots.GetEnumerator();
                var previousDot = dotEnumerator.Current;
                children.Add(previousDot);

                while (dotEnumerator.MoveNext())
                {
                    if (previousDot == null || dotEnumerator.Current == null)
                    {
                        previousDot = dotEnumerator.Current;
                        continue;
                    }

                    children.Add(dotEnumerator.Current);

                    line = new LineVisual(previousDot.Point, dotEnumerator.Current.Point);
                    line.LineColor = m_linesColor.Brush;
                    line.LineThickness = m_linesColor.Thickness;

                    line.PropertyChanged += ObjectPropertyChanged;
                    previousDot.MouseDown += ObecjctClicked_MouseDown;
                    previousDot.PropertyChanged += ObjectPropertyChanged;

                    m_lines.Add(line);

                    children.Insert(0, line);

                    previousDot = dotEnumerator.Current;
                }

                dotEnumerator.Dispose();
            }
            else
            {
                var dotEnumerator = dots.GetEnumerator();

                var previousDot = dotEnumerator.Current;

                while (dotEnumerator.MoveNext())
                {
                    if (previousDot == null || dotEnumerator.Current == null)
                    {
                        previousDot = dotEnumerator.Current;
                        continue;
                    }

                    children.Add(dotEnumerator.Current);

                    var line = new LineVisual(previousDot.Point, dotEnumerator.Current.Point);
                    line.LineColor = m_linesColor.Brush;
                    line.LineThickness = m_linesColor.Thickness;

                    line.PropertyChanged += ObjectPropertyChanged;
                    previousDot.MouseDown += ObecjctClicked_MouseDown;
                    previousDot.PropertyChanged += ObjectPropertyChanged;

                    m_lines.Add(line);

                    children.Insert(0, line);

                    previousDot = dotEnumerator.Current;
                }

                dotEnumerator.Dispose();
            }

            Draw();
        }

        public void InsertPoint(int index, GTA_SA_Point point)
        {
            InsertPoint(index, new PathVisualizer.DotVisual(point));
        }
        public void InsertPoint(int index, GTA_SA_Point point, Brush brush)
        {
            if (brush == null)
            {
                throw new ArgumentNullException("brush");
            }

            InsertPoint(index, new PathVisualizer.DotVisual(point, brush));
        }
        public void InsertPoint(int index, VisualObject dot)
        {
            if (dot == null)
            {
                throw new ArgumentNullException("dot");
            }
            if (index < 0 || index > m_dots.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            dot.MouseUp += ObecjctClicked_MouseDown;
            dot.PropertyChanged += ObjectPropertyChanged;

            m_dots.Insert(index, dot);
            children.Insert(m_lines.Count + index, dot);

            if (m_dots.Count > 1)
            {
                if (index != m_lines.Count - 1)
                {
                    var oldLine = m_lines[index];

                    m_lines.Remove(oldLine);
                    children.Remove(oldLine);

                    var dividedLine = LineVisual.DivideLine(oldLine, dot.Point);
                    dividedLine.LeftLine.LineColor = m_linesColor.Brush;
                    dividedLine.RightLine.LineColor = m_linesColor.Brush;
                    dividedLine.LeftLine.LineThickness = m_linesColor.Thickness;
                    dividedLine.RightLine.LineThickness = m_linesColor.Thickness;

                    dividedLine.RightLine.PropertyChanged += ObjectPropertyChanged;
                    dividedLine.LeftLine.PropertyChanged += ObjectPropertyChanged;

                    m_lines.Insert(index, dividedLine.LeftLine);
                    m_lines.Insert(index + 1, dividedLine.RightLine);


                    children.Insert(m_lines.Count - index, dividedLine.LeftLine);
                    children.Insert(m_lines.Count - index - 1, dividedLine.RightLine);
                }
                else
                {
                    var line = new LineVisual(m_dots[m_dots.Count - 2].Point, dot.Point);
                    line.LineColor = m_linesColor.Brush;
                    line.LineThickness = m_linesColor.Thickness;
                    line.PropertyChanged += ObjectPropertyChanged;

                    m_lines.Add(line);
                    children.Insert(0, line);
                }
            }

            Draw();
        }

        public bool RemovePoint(GTA_SA_Point point)
        {
            var foundDot = m_dots.Where(dot => dot.InputHitTest(point.GetAsPoint2D()) != null).FirstOrDefault();

            return RemovePoint(foundDot);
        }
        public bool RemovePoint(VisualObject dot)
        {
            bool res = m_dots.Remove(dot);
            int oldLineIndex = m_lines.FindIndex(line => line.Start == dot.Point || line.End == dot.Point);

            children.Remove(dot);

            if (oldLineIndex != -1)
            {
                children.RemoveAt(oldLineIndex);
                children.RemoveAt(oldLineIndex);
            }

            Draw();
            return res;
        }
        public bool RemovePointAt(int index)
        {
            if (index < 0 || index > m_dots.Count)
            {
                return false;
            }

            return RemovePoint(m_dots[index]);
        }

        public void Clear()
        {
            var lastLine = m_lines[0];

            m_dots.Clear();
            m_lines.Clear();

            lastLine.LineThickness = 0;
            m_lines.Add(lastLine);
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
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void ObecjctClicked_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is VisualObject vOject)
            {
                if (e.Handled)
                {
                    vOject.MouseUp -= ObecjctClicked_MouseDown;
                    return;
                }

                this.CurrentObject = vOject;

                vOject.IsSelected = !vOject.IsSelected;
            }
        }
        private void ObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (m_isChangingTransform)
            {
                return;
            }

            var visual = sender as Visual;

            if (visual != null)
            {
                int oldIndex = children.IndexOf(visual);

                if (oldIndex != -1)
                {
                    children.RemoveAt(oldIndex);
                    children.Insert(oldIndex, visual);
                }
            }
        }

        private void Draw()
        {
            if (m_dots.Count == 0)
            {
                return;
            }

            var lastLine = m_lines[0];
            lastLine.Start = m_dots[0].Point;
            lastLine.End = m_dots[m_dots.Count - 1].Point;
            lastLine.LineColor = m_linesColor.Brush;
            lastLine.LineThickness = m_linesColor.Thickness;
        }

        public void DrawScale(double offsetX, double offsetY, double scaleX, double scaleY)
        {
            children.Clear();

            m_isChangingTransform = true;

            var dotEnumerator = m_dots.GetEnumerator();
            var lineEnumerator = m_lines.GetEnumerator();

            while (dotEnumerator.MoveNext() && lineEnumerator.MoveNext())
            {
                var currentDot = dotEnumerator.Current;
                currentDot.Point.X = currentDot.OriginPoint.X / scaleX + offsetX;
                currentDot.Point.Y = currentDot.OriginPoint.Y / scaleY + offsetY;

                children.Add(dotEnumerator.Current);
                children.Insert(0, lineEnumerator.Current);
            }

            dotEnumerator.Dispose();
            lineEnumerator.Dispose();

            m_isChangingTransform = false;

        }
    }
}
