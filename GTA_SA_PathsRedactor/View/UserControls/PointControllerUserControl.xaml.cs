using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GTA_SA_PathsRedactor.ViewModel;

namespace GTA_SA_PathsRedactor.View.UserControls
{
    /// <summary>
    /// Interaction logic for PointControllerUC.xaml
    /// </summary>
    public sealed partial class PointControllerUserControl : UserControl
    {
        private readonly PathViewModel? _pathViewModel;

        public PointControllerUserControl()
        {
            InitializeComponent();
            
            PathColorColorPicker.SelectedColorChagned += PathColorColorPickerSelectedColorChanged;
        }

        //TODO: Convert to DependencyProperty
        public PathViewModel? PathViewModel
        {
            get => _pathViewModel;
            init
            {
                if (_pathViewModel is not null)
                {
                    _pathViewModel.PropertyChanged -= PathViewModelPropertyChanged;
                }

                if (value is not null)
                {
                    if (value.CurrentPath != null)
                    {
                        PathColorColorPicker.SelectedColor = value.CurrentPath.Color;
                    }

                    value.PropertyChanged += PathViewModelPropertyChanged;
                }
                
                _pathViewModel = value;
                DataContext = value;
            }
        }

        private void PathViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PathViewModel.CurrentPathIndex) && 
                PathViewModel?.CurrentPath is not null)
            {
                PathColorColorPicker.SelectedColor = PathViewModel.CurrentPath.Color;
            }
        }
        

        private void PathColorColorPickerSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<SolidColorBrush> e)
        {
            //TODO: Try to move in xaml with converter
            if (PathViewModel?.CurrentPath is not null)
                PathViewModel.CurrentPath.Color = PathColorColorPicker.SelectedColor;
        }
    }
}
