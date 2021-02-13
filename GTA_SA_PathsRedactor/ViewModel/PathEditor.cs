using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using GTA_SA_PathsRedactor.Models;
using System.Collections.ObjectModel;
using GTA_SA_PathsRedactor.PathVisualizer;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PathEditor : INotifyPropertyChanged
    {
        private static readonly Brush s_defaultLinesColor;

        private string m_pathName;

        private ObservableCollection<VisualObject> m_dots;
        private List<LineVisual> m_lines;
        private VisualObjectsCollection m_selectedDots;
        private VisualObject m_currentObject;
        private Pen m_linesColor;

        private bool m_isChangingTransform;
        private bool m_multipleSelectionMode;

        private MouseButtonEventHandler m_dotHanler;
        private MouseButtonEventHandler m_lineHanler;

        protected Canvas m_workField;

        public event PropertyChangedEventHandler PropertyChanged;

        static PathEditor()
        {
            s_defaultLinesColor = new SolidColorBrush(Colors.Red);
        }

        public PathEditor(string pathName)
            : this(pathName, s_defaultLinesColor)
        { }
        public PathEditor(string pathName, Brush linesColor)
        {
            m_workField = new Canvas();

            var defaultLine = new LineVisual(new GTA_SA_Point(0, 0, 0, false),
                                             new GTA_SA_Point(0, 0, 0, false));
            m_workField.Children.Add(defaultLine);

            m_isChangingTransform = false;

            m_dots = new ObservableCollection<VisualObject>();
            m_lines = new List<LineVisual> { defaultLine };
            m_selectedDots = new VisualObjectsCollection();
            m_linesColor = new Pen();

            LinesColor = linesColor;
            LinesThickness = 2;
            PathName = pathName;
        }

        public int PointCount
        {
            get { return m_dots.Count; }
        }
        public Canvas WorkField
        {
            get { return m_workField; }
        }
        public VisualObjectsCollection SelectedDots
        {
            get { return m_selectedDots; }
        }

        public bool MultipleSelectionMode
        {
            get { return m_multipleSelectionMode; }
            set
            {
                m_multipleSelectionMode = value;
                m_selectedDots.Clear();
                SelectionClear();
                OnPropertyChanged("MultipleSelectionMode");
            }
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
                m_currentObject = value;
                OnPropertyChanged("CurrentObject");
            }
        }

        public event MouseButtonEventHandler DotsMouseUp
        {
            add
            {
                m_dotHanler += value;

                foreach (var dot in m_dots)
                {
                    dot.MouseDown += value;
                }
            }
            remove
            {
                m_dotHanler -= value;

                foreach (var dot in m_dots)
                {
                    dot.MouseDown -= value;
                }
            }
        }
        public event MouseButtonEventHandler LinesMouseUp
        {
            add
            {
                m_lineHanler += value;

                foreach (var line in m_lines)
                {
                    line.MouseUp += value;
                }
            }
            remove
            {
                m_lineHanler += value;

                foreach (var line in m_lines)
                {
                    line.MouseUp -= value;
                }
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

            dot.MouseDown += m_dotHanler;
            dot.PropertyChanged += ObjectPropertyChanged;

            m_dots.Add(dot);
            m_workField.Children.Add(dot);

            if (m_dots.Count > 1)
            {
                var line = new LineVisual(m_dots[m_dots.Count - 2].Point, dot.Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;

                m_lines.Add(line);
                m_workField.Children.Insert(0, line);
            }

            OnPropertyChanged("PointCount");
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
                throw new ArgumentNullException("dots");
            }

            int lastIndex = m_dots.Count - 1;

            if (m_dots.Count > 0 && lastIndex != -1)
            {
                var line = new LineVisual(m_dots[lastIndex].Point, dots.First().Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;

                m_lines.Add(line);
                m_workField.Children.Insert(0, line);
            }

            var dotEnumerator = dots.GetEnumerator();
            var previousDot = dotEnumerator.Current;
            var currentDot = previousDot;

            while (dotEnumerator.MoveNext())
            {
                if (previousDot == null || dotEnumerator.Current == null)
                {
                    previousDot = dotEnumerator.Current;
                    continue;
                }

                currentDot = dotEnumerator.Current;

                m_dots.Add(previousDot);
                m_workField.Children.Add(previousDot);

                var line = new LineVisual(previousDot.Point, currentDot.Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;

                previousDot.MouseDown += m_dotHanler;
                previousDot.PropertyChanged += ObjectPropertyChanged;

                m_lines.Add(line);

                m_workField.Children.Insert(0, line);

                previousDot = dotEnumerator.Current;
            }

            currentDot.MouseDown += m_dotHanler;
            currentDot.PropertyChanged += ObjectPropertyChanged;
            m_dots.Add(currentDot);
            m_workField.Children.Add(currentDot);

            dotEnumerator.Dispose();

            OnPropertyChanged("PointCount");
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

            dot.MouseDown += m_dotHanler;
            dot.PropertyChanged += ObjectPropertyChanged;

            m_dots.Insert(index, dot);
            m_workField.Children.Insert(m_lines.Count + index, dot);

            if (m_dots.Count > 1)
            {
                if (index != m_lines.Count - 1)
                {
                    var oldLine = m_lines[index];

                    m_lines.Remove(oldLine);
                    m_workField.Children.Remove(oldLine);

                    var dividedLine = LineVisual.DivideLine(oldLine, dot.Point);
                    dividedLine.LeftLine.LineColor = m_linesColor.Brush;
                    dividedLine.RightLine.LineColor = m_linesColor.Brush;
                    dividedLine.LeftLine.LineThickness = m_linesColor.Thickness;
                    dividedLine.RightLine.LineThickness = m_linesColor.Thickness;

                    m_lines.Insert(index, dividedLine.LeftLine);
                    m_lines.Insert(index + 1, dividedLine.RightLine);


                    m_workField.Children.Insert(m_lines.Count - index, dividedLine.LeftLine);
                    m_workField.Children.Insert(m_lines.Count - index - 1, dividedLine.RightLine);
                }
                else
                {
                    var line = new LineVisual(m_dots[m_dots.Count - 2].Point, dot.Point);
                    line.LineColor = m_linesColor.Brush;
                    line.LineThickness = m_linesColor.Thickness;

                    m_lines.Add(line);
                    m_workField.Children.Insert(0, line);
                }
            }

            OnPropertyChanged("PointCount");
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

            m_workField.Children.Remove(dot);

            if (oldLineIndex != -1)
            {
                m_workField.Children.RemoveAt(oldLineIndex);
                m_workField.Children.RemoveAt(oldLineIndex);
            }

            OnPropertyChanged("PointCount");
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

            OnPropertyChanged("PointCount");
        }

        public void SelectionClear()
        {
            foreach (var selectedDot in m_selectedDots)
            {
                selectedDot.IsSelected = false;
            }

            m_selectedDots.Clear();
            CurrentObject = null;
        }

        public void SelectPoints(Rect rect)
        {
            if (m_multipleSelectionMode)
            {
                foreach (var dot in m_dots)
                {
                    if (dot.Point.X >= rect.TopLeft.X && dot.Point.X <= rect.TopRight.X &&
                        dot.Point.Y >= rect.TopLeft.Y && dot.Point.Y <= rect.BottomLeft.Y)
                    {
                        dot.IsSelected = true;
                    }
                }
            }
        }

        public void MoveSelectedPoints(Point offset)
        {
            MoveSelectedPoints(offset.X, offset.Y);
        }
        public void MoveSelectedPoints(double offsetX, double offsetY)
        {
            foreach (var selectedDot in SelectedDots)
            {
                selectedDot.Point.X += offsetX;
                selectedDot.Point.Y += offsetY;
            }
        }

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void ObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var visualObj = sender as VisualObject;

            if (e.PropertyName == "IsSelected" && visualObj != null && visualObj.IsSelected)
            {
                if (!m_multipleSelectionMode)
                {
                    m_selectedDots.Clear();
                }

                m_selectedDots.AddVisualObject(visualObj);
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

            m_workField.Children.Remove(lastLine);
            m_workField.Children.Insert(m_lines.Count - 1, lastLine);
        }

        internal void DrawScale(Services.PointTransformationData pointTransformationData)
        {
            if (pointTransformationData == null)
            {
                return;
            }

            m_isChangingTransform = true;

            int horizontallyInvert = pointTransformationData.InvertHorizontally ? -1 : 1;
            int verticallyInvert = pointTransformationData.InvertVertically ? -1 : 1;

            foreach (var dot in m_dots)
            {
                dot.Point.X = horizontallyInvert * dot.OriginPoint.X / pointTransformationData.PointScaleX + pointTransformationData.OffsetX;
                dot.Point.Y = verticallyInvert * dot.OriginPoint.Y / pointTransformationData.PointScaleY + pointTransformationData.OffsetY;
            }

            m_isChangingTransform = false;

        }

        public sealed class VisualObjectsCollection : IEnumerable<VisualObject>
        {
            private bool m_isDeleting;

            private List<VisualObject> m_visualObjects;

            internal VisualObjectsCollection()
            {
                m_visualObjects = new List<VisualObject>();
            }

            public int Count { get { return m_visualObjects.Count; } }

            public IEnumerator<VisualObject> GetEnumerator()
            {
                return m_visualObjects.GetEnumerator();
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public int IndexOf(VisualObject visualObject)
            {
                return m_visualObjects.IndexOf(visualObject);
            }

            internal void AddVisualObject(VisualObject visualObject)
            {
                visualObject.PropertyChanged += DotPropertyChanged;

                m_visualObjects.Add(visualObject);
            }

            internal bool RemoveVisualObject(VisualObject visualObject)
            {
                m_isDeleting = true;
                bool res = m_visualObjects.Remove(visualObject);

                if (res)
                {
                    visualObject.PropertyChanged -= DotPropertyChanged;
                    visualObject.IsSelected = false;
                }

                m_isDeleting = false;

                return res;
            }

            internal void Clear()
            {
                m_isDeleting = true;

                foreach (var visualObject in m_visualObjects)
                {
                    visualObject.IsSelected = false;
                }

                m_isDeleting = false;

                m_visualObjects.Clear();
            }

            private void DotPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (m_isDeleting)
                {
                    return;
                }

                var dot = sender as VisualObject;

                if (e.PropertyName == "IsSelected" && !dot.IsSelected)
                {
                    RemoveVisualObject(dot);
                }
            }
        }
    }
}
