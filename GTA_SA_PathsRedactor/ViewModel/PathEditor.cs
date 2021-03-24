using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.ViewModel
{
    public class PathEditor : Core.Entity
    {
        private static readonly Color s_defaultLinesColor;

        private string m_pathName;
        private string m_pathFileName;

        private bool m_multipleSelectionMode;

        private ObservableCollection<VisualObject> m_dots;
        private List<LineVisual> m_lines;
        private VisualObjectsCollection m_selectedDots;
        private VisualObject? m_currentObject;
        private Pen m_linesColor;

        private MouseButtonEventHandler? m_dotHanler;
        private MouseButtonEventHandler? m_lineHandler;

        protected Canvas m_workField;

        static PathEditor()
        {
            s_defaultLinesColor = Colors.Red;
        }

        public PathEditor(string pathName)
            : this(pathName, new SolidColorBrush(s_defaultLinesColor))
        { }
        public PathEditor(string pathName, SolidColorBrush linesColor)
        {
            m_workField = new Canvas();

            var defaultLine = new LineVisual(new GTA_SA_Point(0, 0, 0, false),
                                             new GTA_SA_Point(0, 0, 0, false));
            m_workField.Children.Add(defaultLine);

            m_dots = new ObservableCollection<VisualObject>();
            m_lines = new List<LineVisual> { defaultLine };
            m_selectedDots = new VisualObjectsCollection();
            m_linesColor = new Pen();

            Color = linesColor;
            LinesThickness = 2;
            PathName = pathName;
            PathFileName = "";

            var gSettings = GlobalSettings.GetInstance();

            gSettings.PropertyChanged += GlobalSettings_PropertyChanged;
            gSettings.PTD.PropertyChanged += TransformationDataPropertyChanged;
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
        public ReadOnlyCollection<VisualObject> Dots => new ReadOnlyCollection<VisualObject>(m_dots);

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
                if (value.Length == 0)
                {
                    m_errors["PathName"] = "Path name can't be empty.";
                }
                else
                {
                    m_errors["PathName"] = "";
                }

                m_pathName = value;
                OnPropertyChanged("PathName");
            }
        }
        public string PathFileName
        {
            get { return m_pathFileName; }
            set
            {
                if (value.Length == 0)
                {
                    m_errors["PathFileName"] = "File path can't be empty.";
                }
                else
                {
                    m_errors["PathFileName"] = "";
                }

                m_pathFileName = value;
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
                    m_errors["LinesThickness"] = "Path's lines thickness can't be less than zero.";
                    throw new ArgumentOutOfRangeException("value", "Path's lines thickness can't be less than zero.");
                }

                m_errors["LinesThickness"] = "";
                m_linesColor.Thickness = value;
                OnPropertyChanged("LinesThickness");
            }
        }
        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)m_linesColor.Brush; }
            set
            {
                m_linesColor.Brush = value;
                OnPropertyChanged("LinesColor");
            }
        }
        public VisualObject? CurrentObject
        {
            get { return m_currentObject; }
            set
            {
                m_currentObject = value;
                OnPropertyChanged("CurrentObject");
            }
        }

        public event MouseButtonEventHandler? DotsMouseDown
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
        public event MouseButtonEventHandler? LinesMouseDown
        {
            add
            {
                m_lineHandler += value;

                foreach (var line in m_lines)
                {
                    line.MouseDown += value;
                }
            }
            remove
            {
                m_lineHandler += value;

                foreach (var line in m_lines)
                {
                    line.MouseDown -= value;
                }
            }
        }

        public int IndexOf(Func<VisualObject, bool> comparator)
        {
            int index = -1;

            if (comparator == null)
            {
                return index;
            }

            foreach (var dot in m_dots)
            {
                ++index;
                if (comparator(dot))
                {
                    return index;
                }
            }

            return index;
        }

        public void AddPoint(GTA_SA_Point point)
        {
            AddPoint(new DotVisual(point, m_linesColor.Brush));
        }
        public void AddPoint(GTA_SA_Point point, Brush brush)
        {
            AddPoint(new DotVisual(point, brush));
        }
        public void AddPoint(VisualObject dot)
        {
            dot.MouseDown += m_dotHanler;
            dot.PropertyChanged += ObjectPropertyChanged;

            m_dots.Add(dot);
            m_workField.Children.Add(dot);

            if (m_dots.Count > 1)
            {
                var line = new LineVisual(m_dots[m_dots.Count - 2].Point, dot.Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;
                line.MouseDown += m_lineHandler;

                m_lines.Add(line);
                m_workField.Children.Insert(0, line);
            }

            TransormPoint(dot);

            OnPropertyChanged("PointCount");
            Draw();
        }

        public void AddRangePoint(IEnumerable<GTA_SA_Point> points)
        {
            AddRangePoint(points.Select(p => new DotVisual(p, m_linesColor.Brush)));
        }
        public void AddRangePoint(IEnumerable<VisualObject> dots)
        {
            int lastIndex = m_dots.Count - 1;

            if (m_dots.Count > 0 && lastIndex != -1)
            {
                var line = new LineVisual(m_dots[lastIndex].Point, dots.First().Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;
                line.MouseDown += m_lineHandler;

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

                TransormPoint(previousDot);

                m_dots.Add(previousDot);
                m_workField.Children.Add(previousDot);

                var line = new LineVisual(previousDot.Point, currentDot.Point);
                line.LineColor = m_linesColor.Brush;
                line.LineThickness = m_linesColor.Thickness;
                line.MouseDown += m_lineHandler;

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

            TransormPoint(currentDot);

            dotEnumerator.Dispose();

            OnPropertyChanged("PointCount");
            Draw();
        }

        public void InsertPoint(int index, GTA_SA_Point point)
        {
            InsertPoint(index, new DotVisual(point));
        }
        public void InsertPoint(int index, GTA_SA_Point point, Brush brush)
        {
            InsertPoint(index, new Models.DotVisual(point, brush));
        }
        public void InsertPoint(int index, VisualObject dot)
        {
            if (index < 0 || index >= m_dots.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            dot.MouseDown += m_dotHanler;
            dot.PropertyChanged += ObjectPropertyChanged;

            if (index + 1 == m_dots.Count)
                index = 0;
            else
                index++;

            if (m_dots.Count > 1)
            {
                LineVisual oldLine = m_lines[index];

                m_lines.Remove(oldLine);
                m_workField.Children.Remove(oldLine);

                var dividedLine = LineVisual.DivideLine(oldLine, dot.Point);
                dividedLine.LeftLine.LineColor = m_linesColor.Brush;
                dividedLine.RightLine.LineColor = m_linesColor.Brush;
                dividedLine.LeftLine.LineThickness = m_linesColor.Thickness;
                dividedLine.RightLine.LineThickness = m_linesColor.Thickness;

                dividedLine.RightLine.MouseDown += m_lineHandler;
                dividedLine.LeftLine.MouseDown += m_lineHandler;

                m_lines.Insert(index, dividedLine.LeftLine);
                m_lines.Insert(index + 1, dividedLine.RightLine);

                m_workField.Children.Insert(m_lines.Count - index - 1, dividedLine.LeftLine);
                m_workField.Children.Insert(m_lines.Count - index - 2, dividedLine.RightLine);
            }

            m_dots.Insert(index, dot);
            m_workField.Children.Insert(m_lines.Count + index + 1, dot);

            TransormPoint(dot);

            OnPropertyChanged("PointCount");
            Draw();
        }


#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
        public bool RemovePoint(GTA_SA_Point? point)
        {
            var foundDot = m_dots.Where(dot => dot.InputHitTest(point.As2DPoint()) != null).FirstOrDefault();

            return RemovePoint(foundDot);
        }
        public bool RemovePoint(VisualObject? dot)
        {
            return RemovePointHelper(dot);
        }
        public bool RemovePointAt(int index)
        {
            if (index < 0 || index > m_dots.Count)
            {
                return false;
            }

            return RemovePoint(m_dots[index]);
        }
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.

        public void RemoveSelectedPoint()
        {
            foreach (var dot in SelectedDots)
            {
                RemovePointHelper(dot, true);
            }

            SelectedDots.Clear();
        }

        public void Clear()
        {
            var lastLine = m_lines[0];

            m_dots.Clear();
            m_lines.Clear();

            lastLine.LineThickness = 0;
            lastLine.ToolTip = 0;
            m_lines.Add(lastLine);

            m_workField.Children.Clear();

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

        private bool RemovePointHelper(VisualObject dot, bool isSelectionClear = false)
        {
            int dotIndex = m_dots.IndexOf(dot);
            bool res = m_dots.Remove(dot);

            if (!res)
            {
                return res;
            }

            if (m_lines.Count == 1)
            {
                m_workField.Children.Remove(dot);
                return res;
            }

            var dotLines = m_lines.Where(line => line.Start == dot.Point || line.End == dot.Point);
            var lineStart = dotLines.ElementAt(0);
            var lineEnd = dotLines.ElementAt(1);

            if (lineStart.Start != dot.Point)
            {
                var temp = lineStart;

                lineStart = lineEnd;
                lineEnd = temp;
            }

            m_workField.Children.Remove(dot);

            if (!isSelectionClear)
            {
                SelectedDots.RemoveVisualObject(dot);
            }

            if (dotIndex == 0)
                dotIndex = m_dots.Count - 1;
            else if (dotIndex == m_dots.Count)
                dotIndex = 0;

            lineEnd.End = m_dots[dotIndex].Point;

            m_workField.Children.Remove(lineStart);
            m_lines.Remove(lineStart);

            
            OnPropertyChanged("PointCount");
            Draw();

            return res;
        }

        private void ObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
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
        private void TransformationDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            DrawScale();
        }
        private void GlobalSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Resolution" ||
                e.PropertyName == "OriginalPTD")
            {
                if (e.PropertyName == "OriginalPTD")
                {
                    var gSettings = GlobalSettings.GetInstance();

                    if (gSettings.PTD != null)
                    {
                        gSettings.PTD.PropertyChanged -= TransformationDataPropertyChanged;
                        gSettings.PTD.PropertyChanged += TransformationDataPropertyChanged;
                    }
                }
                DrawScale();
            }
        }

        private void TransormPoint(VisualObject dot)
        {
            var currentPTD = GlobalSettings.GetInstance().GetCurrentTranfromationData();

            if (currentPTD == null ||
                currentPTD.PointScaleX == 0 ||
                currentPTD.PointScaleY == 0)
            {
                return;
            }

            int horizontallyInvert = currentPTD.InvertHorizontally ? -1 : 1;
            int verticallyInvert = currentPTD.InvertVertically ? -1 : 1;

            dot.Point.X = horizontallyInvert * dot.OriginPoint.X / currentPTD.PointScaleX + currentPTD.OffsetX;
            dot.Point.Y = verticallyInvert * dot.OriginPoint.Y / currentPTD.PointScaleY + currentPTD.OffsetY;
        }

        private void Draw()
        {
            if (m_dots.Count == 0)
            {
                return;
            }

            var lastLine = m_lines[0];
            lastLine.Start = m_dots[m_dots.Count - 1].Point;
            lastLine.End = m_dots[0].Point;
            lastLine.LineColor = m_linesColor.Brush;
            lastLine.LineThickness = m_linesColor.Thickness;

            m_workField.Children.Remove(lastLine);
            m_workField.Children.Insert(m_lines.Count - 1, lastLine);

#if DEBUG
            for (int i = 0; i < m_dots.Count; i++)
            {
                var line = m_lines[i];
                var dot = m_dots[i];

                line.ToolTip = $"Index: {i};\nStart = {line.Start};\nEnd = {line.End}";
                dot.ToolTip = $"Index: {i};\nPoint = {dot.Point}";
            }
#endif
        }
        private void DrawScale()
        {
            if (WorkField.Visibility != Visibility.Visible)
            {
                return;
            }

            var currentPTD = GlobalSettings.GetInstance().GetCurrentTranfromationData();

            if (currentPTD == null)
            {
                foreach (var dot in m_dots)
                {
                    dot.Point.X = dot.OriginPoint.X;
                    dot.Point.Y = dot.OriginPoint.Y;
                }
            }
            else
            {
                int horizontallyInvert = currentPTD.InvertHorizontally ? -1 : 1;
                int verticallyInvert = currentPTD.InvertVertically ? -1 : 1;

                foreach (var dot in m_dots)
                {
                    dot.Point.X = horizontallyInvert * dot.OriginPoint.X / currentPTD.PointScaleX + currentPTD.OffsetX;
                    dot.Point.Y = verticallyInvert * dot.OriginPoint.Y / currentPTD.PointScaleY + currentPTD.OffsetY;
                }
            }
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

            private void DotPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (m_isDeleting)
                {
                    return;
                }

                var dot = sender as VisualObject;

                if (e.PropertyName == "IsSelected" && dot?.IsSelected == false)
                {
                    RemoveVisualObject(dot);
                }
            }
        }
    }
}
