using System;
using System.Linq;
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

    private bool _mouseDown;
    private bool _mapContainerMouseDown;
    private Point _mouseDownPosition;
    private Point _mapContainerMouseDownPosition;
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
        if (e.LeftButton == MouseButtonState.Released)
        {
            _mapContainerMouseDownPosition = e.GetPosition(MapContainer);
            e.Handled = false;
            return;
        }
        
        _mouseDown = true;
        _mouseDownPosition = e.GetPosition(this);
        
        var translateTransform = GetMapTranslateTransform();
        
        if (translateTransform is not null)
            _mapTranslation = new Point(translateTransform.X, translateTransform.Y);

        e.Handled = true;
    }
    
    private void PathManipulatorUserControl_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mouseDown)
            return;

        var translateTransform = GetMapTranslateTransform();
        if (translateTransform is null)
            return;
        
        var scaleTransform = GetMapScaleTransform();
        
        if ((scaleTransform?.ScaleX ?? StandardZoom_).Equals(1) && 
            (scaleTransform?.ScaleY ?? StandardZoom_).Equals(1))
            return;
        
        var position = e.GetPosition(this);
        var newX = _mapTranslation.X + position.X - _mouseDownPosition.X;
        var newY = _mapTranslation.Y + position.Y - _mouseDownPosition.Y;
        
        ClampMapToScreen(translateTransform, scaleTransform!, newX, newY);

        e.Handled = true;
    }

    private void PathManipulatorUserControl_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _mouseDown = false;
    }

    private void PathManipulatorUserControl_OnMouseLeave(object sender, MouseEventArgs e)
    {
        _mouseDown = false;
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
        _mapContainerMouseDown = false;
    }
}