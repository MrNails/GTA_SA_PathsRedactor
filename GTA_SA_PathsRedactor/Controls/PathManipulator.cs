using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Core.Models;

namespace GTA_SA_PathsRedactor.Controls;

public sealed class PathManipulator : FrameworkElement
{
    public static readonly DependencyProperty PointsToDisplayProperty = 
        DependencyProperty.Register(nameof(PointsToDisplay), 
                                    typeof(IReadOnlyList<WorldPoint>), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(Array.Empty<WorldPoint>(), PointsToDisplayPropertyChanged, CoercePointsToDisplay));

    public static readonly DependencyProperty PointsColorProperty = 
        DependencyProperty.Register(nameof(PointsColor), 
                                    typeof(Brush), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(Brushes.Black, InvalidateControlInternal, CoercePointsColor));

    public static readonly DependencyProperty StopPointPointsColorProperty = 
        DependencyProperty.Register(nameof(StopPointPointsColor), 
                                    typeof(Brush), 
                                    typeof(PathManipulator), 
                                    new PropertyMetadata(Brushes.Black, InvalidateControlInternal, CoercePointsColor));
    
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
    
    protected override void OnRender(DrawingContext drawingContext)
    {
        if (PointsToDisplay.Count == 0)
        {
            return;
        }
        
        var pointColor = PointsColor;
        var stopPointColor = StopPointPointsColor;
        var pointsToDisplay = PointsToDisplay;
        
        var pointPen = new Pen(pointColor, 1);
        var selectedPointPen = new Pen(Brushes.Black, 1);
        var linePen = new Pen(pointColor, 1);

        var firstPoint = pointsToDisplay[0];
        var lastPoint = pointsToDisplay[^1];
        
        drawingContext.DrawEllipse(firstPoint.IsStopPoint ? stopPointColor : pointColor, 
                                   firstPoint.IsSelected ? selectedPointPen : pointPen, 
                                   new Point(firstPoint.X, firstPoint.Y), 2, 2);
        
        for (int i = 1; i < pointsToDisplay.Count; i++)
        {
            var worldPoint = pointsToDisplay[i];
            var previousPoint = pointsToDisplay[i - 1];
            var tmpCenter = new Point(worldPoint.X, worldPoint.Y);
            
            drawingContext.DrawEllipse(worldPoint.IsStopPoint ? stopPointColor : pointColor, 
                                       worldPoint.IsSelected ? selectedPointPen : pointPen, 
                                       tmpCenter, 2, 2);
            
            drawingContext.DrawLine(linePen, new Point(previousPoint.X, previousPoint.Y), tmpCenter);
            
            lastPoint = worldPoint;
        }
        
        drawingContext.DrawLine(linePen, new Point(lastPoint.X, lastPoint.Y), new Point(firstPoint.X, firstPoint.Y));
        
        base.OnRender(drawingContext);
    }
    
    private void PointsToDisplayOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Remove)
        {
            InvalidateVisual();
        }
    }
    
    private static void InvalidateControlInternal(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((PathManipulator)d).InvalidateVisual();
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
        return baseValue is not Brush ? Brushes.Black : baseValue;
    }
    
    private static object CoercePointsToDisplay(DependencyObject d, object baseValue)
    {
        return baseValue is not IEnumerable<WorldPoint> ? Enumerable.Empty<WorldPoint>() : baseValue;
    }
}