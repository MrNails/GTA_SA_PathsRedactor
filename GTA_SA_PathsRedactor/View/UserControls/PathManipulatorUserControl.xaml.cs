using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Models.Dto;
using GTA_SA_PathsRedactor.Services.Extensions;
using GTA_SA_PathsRedactor.ViewModel;

namespace GTA_SA_PathsRedactor.View.UserControls;

public partial class PathManipulatorUserControl : UserControl
{
    public const int MaxMapZoomFactor_ = 7;
    public const int StandardZoom_ = 1;
    
    public static readonly DependencyProperty ImagePathProperty = 
        DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(PathManipulatorUserControl));
    
    private bool _mapContainerMouseDown;
    private Point _mouseDownPosition;
    private Point _mapContainerMouseDownPosition;
    private Point _lastMapContainerMousePosition;
    private Point _mapTranslation;
    
    public PathManipulatorUserControl()
    {
        InitializeComponent();
    }
    
    public string ImagePath
    {
        get => (string)GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }

    private ScaleTransform? GetMapScaleTransform()
    {
        var renderGrid = RenderGrid;
        if (renderGrid.RenderTransform is not TransformGroup transformGroup)
            return null;

        return transformGroup.Children.FirstOrDefault(child => child is ScaleTransform) as ScaleTransform;
    }
    
    private TranslateTransform? GetMapTranslateTransform()
    {
        var renderGrid = RenderGrid;
        if (renderGrid.RenderTransform is not TransformGroup transformGroup)
            return null;

        return transformGroup.Children.FirstOrDefault(child => child is TranslateTransform) as TranslateTransform;
    }

    private Point TransformScreenToMapPoint(Point screenPoint)
    {
        var resultPoint = screenPoint;
        var scale = GetMapScaleTransform();
        var translate = GetMapTranslateTransform();
        
        if (scale is null ||
            translate is null ||
            scale.ScaleX.Equals(StandardZoom_) ||
            scale.ScaleY.Equals(StandardZoom_)) 
            return screenPoint;
        
        //Transform mouse click position as offset from center of map
        resultPoint.X = RenderGrid.ActualWidth / 2 - resultPoint.X;
        resultPoint.Y = RenderGrid.ActualHeight / 2 - resultPoint.Y;

        resultPoint.X = RenderGrid.ActualWidth / 2 - (resultPoint.X + translate.X) / scale.ScaleX;
        resultPoint.Y = RenderGrid.ActualHeight / 2 - (resultPoint.Y + translate.Y) / scale.ScaleY;

        return screenPoint;
    }
    
    private void ClampMapToScreen(TranslateTransform translateTransform, ScaleTransform scaleTransform, double newX, double newY)
    {
        var signX = Math.Sign(newX);
        var signY = Math.Sign(newY);

        //We take scale transform, subtract 1 and get actual zoom value.
        //Then we divide it by half and multiply result by map size - we receive actual max map offset.
        //Then we need to restrict current offset (need to make absolute to right restriction) by max offset.
        translateTransform.X = signX * Math.Min(RenderGrid.ActualWidth * ((scaleTransform.ScaleX - StandardZoom_) / 2), Math.Abs(newX));
        translateTransform.Y = signY * Math.Min(RenderGrid.ActualHeight * ((scaleTransform.ScaleY - StandardZoom_) / 2), Math.Abs(newY));
    }
    
    private void HandleMouseMoveOnLeftButtonPressed(Point mapMousePosition)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
            _lastMapContainerMousePosition != default &&
            DataContext is PathEditorViewModel viewModel)
        {
            viewModel.SelectedPoints.ForEach(point => point.Position -= new Vector3((float)(_lastMapContainerMousePosition.X - mapMousePosition.X),
                (float)(_lastMapContainerMousePosition.Y - mapMousePosition.Y), 
                0));
            
            MapContainer.InvalidateVisual();
            return;
        }

        var rectangleTranslate = (TranslateTransform)SelectionRectangle.RenderTransform;
        var newWidth = mapMousePosition.X - _mapContainerMouseDownPosition.X;
        var newHeight = mapMousePosition.Y - _mapContainerMouseDownPosition.Y;

        if (newWidth < 0)
            rectangleTranslate.X = mapMousePosition.X;
        if (newHeight < 0)
            rectangleTranslate.Y = mapMousePosition.Y;

        SelectionRectangle.Width = Math.Abs(newWidth);
        SelectionRectangle.Height = Math.Abs(newHeight);
    }
    
    private void PathManipulatorUserControl_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scaleTransform = GetMapScaleTransform();
        if (scaleTransform is null) 
            return;

        var delta = e.Delta > 0 ? 0.1 : -0.1;
        
        scaleTransform.ScaleX = Math.Clamp(scaleTransform.ScaleX + delta, 1, MaxMapZoomFactor_);
        scaleTransform.ScaleY = Math.Clamp(scaleTransform.ScaleY + delta, 1, MaxMapZoomFactor_);
        
        var translateTransform = GetMapTranslateTransform();

        //We take scale transform, subtract 1 and get actual zoom value.
        //Then we divide it by half and multiply result by map size - we receive actual max map offset.
        if (translateTransform is not null)
        {
            var mousePosition = e.MouseDevice.GetPosition(this);
            
            //Transform mouse position to center based position
            translateTransform.X += (RenderGrid.ActualWidth / 2 - mousePosition.X) / scaleTransform.ScaleX / 2;
            translateTransform.Y += (RenderGrid.ActualHeight / 2 - mousePosition.Y) / scaleTransform.ScaleY / 2;
            
            ClampMapToScreen(translateTransform, scaleTransform, translateTransform.X, translateTransform.Y);
        }

        e.Handled = true;
    }

    private void PathManipulatorUserControl_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _mapContainerMouseDownPosition = e.GetPosition(MapContainer);
        e.Handled = true;

        _mouseDownPosition = e.GetPosition(this);
        
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var rectangleTransform = (TranslateTransform)SelectionRectangle.RenderTransform;
            rectangleTransform.X = _mapContainerMouseDownPosition.X;
            rectangleTransform.Y = _mapContainerMouseDownPosition.Y;
        }
        
        var translateTransform = GetMapTranslateTransform();
        
        if (translateTransform is not null)
            _mapTranslation = new Point(translateTransform.X, translateTransform.Y);
    }
    
    private void PathManipulatorUserControl_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_mapContainerMouseDown)
            return;

        var position = e.GetPosition(this);
        var mapMousePosition = e.GetPosition(MapContainer);

        if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
        {
            HandleMouseMoveOnLeftButtonPressed(mapMousePosition);
        } 
        else if (e.MiddleButton == MouseButtonState.Pressed)
        {
            var translateTransform = GetMapTranslateTransform();
            if (translateTransform is null)
                return;
        
            var scaleTransform = GetMapScaleTransform();
        
            if ((scaleTransform?.ScaleX ?? StandardZoom_).Equals(1) && 
                (scaleTransform?.ScaleY ?? StandardZoom_).Equals(1))
                return;
        
            var newX = _mapTranslation.X + position.X - _mouseDownPosition.X;
            var newY = _mapTranslation.Y + position.Y - _mouseDownPosition.Y;
        
            ClampMapToScreen(translateTransform, scaleTransform!, newX, newY);
            
            e.Handled = true;
        }
        
        _lastMapContainerMousePosition = mapMousePosition;
    }
    
    private void PathManipulatorUserControl_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _lastMapContainerMousePosition = default;
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
    }

    private void PathManipulatorUserControl_OnMouseLeave(object sender, MouseEventArgs e)
    {
        _mapContainerMouseDown = false;
    }
    
    private void PathManipulatorUserControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var translateTransform = GetMapTranslateTransform();

        if (translateTransform is null)
            return;

        var scaleTransform = GetMapScaleTransform();
        
        if (scaleTransform is null ||
            (scaleTransform.ScaleX.Equals(1) &&
            scaleTransform.ScaleY.Equals(1)))
            return;

        ClampMapToScreen(translateTransform, scaleTransform, translateTransform.X, translateTransform.Y);
        e.Handled = true;
    }

    private void AddPointMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PathEditorViewModel viewModel)
            return;
        
        var point = new WorldPoint((float)_mapContainerMouseDownPosition.X, (float)_mapContainerMouseDownPosition.Y, 0, false);
        if (viewModel.AddPointCommand.CanExecute(point))
            viewModel.AddPointCommand.Execute(point);
    }

    private void InsertPointMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PathEditorViewModel viewModel)
            return;

        var nearestPoints = MapContainer.FindConnectedPointsBetweenWhichLiesGiven(_mapContainerMouseDownPosition);

        if (nearestPoints.Length == 0)
            return;
        
        var point = new WorldPoint((float)_mapContainerMouseDownPosition.X, (float)_mapContainerMouseDownPosition.Y, 0, false);
        var insertPointDto = new InsertPointDto(point, (viewModel.Points.IndexOf(nearestPoints[0]) + 1) % viewModel.Points.Count);
        
        if (viewModel.InsertPointCommand.CanExecute(insertPointDto))
            viewModel.InsertPointCommand.Execute(insertPointDto);
    }

    private void PathManipulatorUserControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _mapContainerMouseDown = e.MouseDevice.DirectlyOver.Equals(MapContainer);
    }
    
    private void PathManipulatorUserControl_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mapContainerMouseDown ||
            DataContext is not PathEditorViewModel viewModel ||
            viewModel.SelectedPoint is null)
            return;

        var mousePosition = e.GetPosition(MapContainer);
        var selectedPoint = viewModel.SelectedPoint;

        selectedPoint.X = (float)mousePosition.X;
        selectedPoint.Y = (float)mousePosition.Y;
        MapContainer.InvalidateVisual();
    }

    private void PathManipulatorUserControl_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (SelectionRectangle.ActualWidth > 3 &&
            SelectionRectangle.ActualHeight > 3 &&
            DataContext is PathEditorViewModel viewModel)
        {
            var selectionRectPosition = (TranslateTransform)SelectionRectangle.RenderTransform;
            var topLeftPoint = new Point(selectionRectPosition.X, selectionRectPosition.Y);
            var bottomRightPoint = new Point(selectionRectPosition.X + SelectionRectangle.ActualWidth, selectionRectPosition.Y + SelectionRectangle.ActualHeight);
            
            viewModel.SelectPointsCommand.Execute(new Rect(TransformScreenToMapPoint(topLeftPoint), TransformScreenToMapPoint(bottomRightPoint)));
            MapContainer.InvalidateVisual();
        }
        
        _mapContainerMouseDown = false;
    }
}