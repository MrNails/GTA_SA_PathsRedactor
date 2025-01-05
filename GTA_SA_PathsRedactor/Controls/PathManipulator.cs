using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Services.Extensions;

namespace GTA_SA_PathsRedactor.Controls;

public sealed class PathManipulator : FrameworkElement
{
    private static readonly Brush DefaultBrush_ = Brushes.Black;
    private static readonly Pen TransparentPen_ = new Pen(Brushes.Transparent, 0);
    
    public static readonly DependencyProperty PointsToDisplayProperty = 
        DependencyProperty.Register(nameof(PointsToDisplay), 
                                    typeof(IReadOnlyList<WorldPoint>), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(Array.Empty<WorldPoint>(), PointsToDisplayPropertyChanged, CoercePointsToDisplay));

    public static readonly DependencyProperty PointsColorProperty = 
        DependencyProperty.Register(nameof(PointsColor), 
                                    typeof(Brush), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(DefaultBrush_, PointsColorPropertyChanged, CoercePointsColor));

    public static readonly DependencyProperty StopPointPointsColorProperty = 
        DependencyProperty.Register(nameof(StopPointPointsColor), 
                                    typeof(Brush), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(DefaultBrush_, StopPointPointsColorPropertyChanged, CoercePointsColor));

    public static readonly DependencyProperty PointRadiusProperty = 
        DependencyProperty.Register(nameof(PointRadius), 
                                    typeof(double), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(1.0, PointRadiusPropertyChanged, CoercePointsRadius));

    public static readonly DependencyProperty SelectedPointProperty = 
        DependencyProperty.Register(nameof(SelectedPoint), 
                                    typeof(WorldPoint), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(null, InvalidateControlInternal));
    
    private Pen _pathPen;
    private Pen _selectedPointPen;
    private Pen _stopPointPen;

    private bool _isMouseDown;

    public PathManipulator()
    {
        _pathPen = new Pen(DefaultBrush_, PointRadius);
        _selectedPointPen = new Pen(DefaultBrush_, PointRadius);
        _stopPointPen = new Pen(DefaultBrush_, PointRadius);
    }

    public IReadOnlyList<WorldPoint> PointsToDisplay
    {
        get => (IReadOnlyList<WorldPoint>)GetValue(PointsToDisplayProperty);
        set => SetValue(PointsToDisplayProperty, value);
    }

    public Brush PointsColor
    {
        get => (Brush)GetValue(PointsColorProperty);
        set => SetValue(PointsColorProperty, value);
    }
    
    public Brush StopPointPointsColor
    {
        get => (Brush)GetValue(StopPointPointsColorProperty);
        set => SetValue(StopPointPointsColorProperty, value);
    }

    public double PointRadius
    {
        get => (double)GetValue(PointRadiusProperty);
        set => SetValue(PointRadiusProperty, value);
    }

    public WorldPoint? SelectedPoint
    {
        get => GetValue(SelectedPointProperty) as WorldPoint;
        set => SetValue(SelectedPointProperty, value);
    }
    
    protected override void OnRender(DrawingContext drawingContext)
    {
        if (PointsToDisplay.Count == 0)
        {
            return;
        }
        
        var pointColor = PointsColor;
        var stopPointColor = StopPointPointsColor;
        var pointsToDisplay = PointsToDisplay;

        var firstPoint = pointsToDisplay[0];
        var lastPoint = pointsToDisplay[^1];
        var pointRadius = PointRadius;

        var linesToDisplayAmount = pointsToDisplay.Count - 1;
        for (int i = 0; i < linesToDisplayAmount; i++)
        {
            var point = pointsToDisplay[i];
            var nextPoint = pointsToDisplay[i + 1];
            drawingContext.DrawLine(_pathPen, new Point(point.X, point.Y),  new Point(nextPoint.X, nextPoint.Y));
        }
        
        if (linesToDisplayAmount > 1)
            drawingContext.DrawLine(_pathPen, new Point(lastPoint.X, lastPoint.Y), new Point(firstPoint.X, firstPoint.Y));
        
        for (int i = 0; i < pointsToDisplay.Count; i++)
        {
            var worldPoint = pointsToDisplay[i];

            drawingContext.DrawEllipse(worldPoint.IsStopPoint ? stopPointColor : pointColor, 
                                       worldPoint.IsSelected ? _selectedPointPen : TransparentPen_, 
                                       new Point(worldPoint.X, worldPoint.Y), pointRadius, pointRadius);
        }
        
        base.OnRender(drawingContext);
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        var position = e.GetPosition(this);
        var points = PointsToDisplay;
        var pointRadius = PointRadius;
        
        WorldPoint? foundPoint = null; 
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var tmpDistance = point.DistanceTo(position);

            if (tmpDistance < pointRadius)
            {
                foundPoint = point;
            }
        }

        if (SelectedPoint is not null)
            SelectedPoint.IsSelected = false;
        
        SelectedPoint = foundPoint;

        if (SelectedPoint is not null)
            e.Handled = SelectedPoint.IsSelected = true;

        _isMouseDown = true;
    }

    //TODO: Move in other place where mouse won't lose focus on point
    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        var selectedPoint = SelectedPoint;
        if (!_isMouseDown || selectedPoint is null)
            return;
        
        var newMousePosition = e.GetPosition(this);
        selectedPoint.X = (float)newMousePosition.X;
        selectedPoint.Y = (float)newMousePosition.Y;
        
        InvalidateVisual();
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _isMouseDown = false;
    }

    private void PointsToDisplayOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
        {
            InvalidateVisual();
        }
    }
    
    private static void InvalidateControlInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PathManipulator)d).InvalidateVisual();
    }
    
    private static void PointsColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (PathManipulator)d;
        
        self._pathPen = new Pen(e.NewValue as Brush ?? DefaultBrush_, self.PointRadius);
        
        InvalidateControlInternal(d, e);
    }
    
    private static void StopPointPointsColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (PathManipulator)d;
        
        self._stopPointPen = new Pen(e.NewValue as Brush ?? DefaultBrush_, self.PointRadius);
        
        InvalidateControlInternal(d, e);
    }
    
    private static void PointRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (PathManipulator)d;
        
        self._pathPen = new Pen(self._pathPen.Brush, self.PointRadius);
        self._stopPointPen = new Pen(self._stopPointPen.Brush, self.PointRadius);
        self._selectedPointPen = new Pen(self._selectedPointPen.Brush, self.PointRadius);

        InvalidateControlInternal(d, e);
    }
    
    private static void PointsToDisplayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (PathManipulator)d;
        
        if (e.NewValue is INotifyCollectionChanged newNotifyCollectionChanged)
        {
            newNotifyCollectionChanged.CollectionChanged += self.PointsToDisplayOnCollectionChanged;
        }

        if (e.OldValue is INotifyCollectionChanged oldNotifyCollectionChanged)
        {
            oldNotifyCollectionChanged.CollectionChanged -= self.PointsToDisplayOnCollectionChanged;
        }
        
        self.InvalidateVisual();
    }

    private static object CoercePointsColor(DependencyObject d, object baseValue)
    {
        return baseValue is not Brush ? DefaultBrush_ : baseValue;
    }
    
    private static object CoercePointsToDisplay(DependencyObject d, object baseValue)
    {
        return baseValue is not IEnumerable<WorldPoint> ? Enumerable.Empty<WorldPoint>() : baseValue;
    }
    
    private static object CoercePointsRadius(DependencyObject d, object baseValue)
    {
        if (baseValue is not double radius)
            return 1;

        return radius <= 0 ? 1 : radius;
    }
}