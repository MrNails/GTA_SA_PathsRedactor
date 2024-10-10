using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Interaction logic for PointControllerUC.xaml
    /// </summary>
    public partial class PointControllerUC : UserControl
    {
        private ViewModel.PathViewModel _pathViewModel;

        public PointControllerUC()
        {
            InitializeComponent();
        }
        public PointControllerUC(ViewModel.PathViewModel pathViewModel)
        {
            InitializeComponent();

            PathViewModel = pathViewModel;

            if (pathViewModel.CurrentPath != null)
            {
                PathColorCP.SelectedColor = pathViewModel.CurrentPath.Color;
            }

            DataContext = PathViewModel;

            PathViewModel.PropertyChanged += PathViewModelPropertyChanged;

            PathColorCP.SelectedColorChagned += PathColorCP_SelectedColorChagned;
        }

        public ViewModel.PathViewModel PathViewModel
        {
            get { return _pathViewModel; }
            set
            {
                if (_pathViewModel != null)
                {
                    _pathViewModel.PathSelected -= ChangeSelection_PathSelected;
                }

                _pathViewModel = value;
                DataContext = value;

                _pathViewModel.PathSelected += ChangeSelection_PathSelected;
            }
        }

        private void PathViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPathIndex" && _pathViewModel.CurrentPathIndex != -1)
            {
                PathColorCP.SelectedColor = _pathViewModel.CurrentPath.Color;
            }
        }

        private void ChangeSelection_PathSelected(ViewModel.PathViewModel sender, Services.PathSelectionArgs e)
        {
            AvailablePathsListBox.UpdateLayout();

            if (sender.Paths.Count == 0)
            {
                return;
            }

            if (e.OldIndex != -1 && e.OldIndex < sender.Paths.Count)
            {
                sender.Paths[e.OldIndex].WorkField.Visibility = Visibility.Hidden;

                var oldItem = (ListBoxItem)AvailablePathsListBox.ItemContainerGenerator.ContainerFromIndex(e.OldIndex);
                var oldItemContentPresenter = oldItem.FindVisualChild<ContentPresenter>();

                if (oldItemContentPresenter == null)
                {
                    return;
                }

                var oldTextBlock = (TextBlock)oldItemContentPresenter.ContentTemplate.FindName("SelectionStarTextBlock", oldItemContentPresenter);

                oldTextBlock.Text = "";
            }
            if (e.NewIndex != -1 && e.NewIndex < sender.Paths.Count)
            {
                sender.Paths[e.NewIndex].WorkField.Visibility = Visibility.Visible;

                var newItem = (ListBoxItem)AvailablePathsListBox.ItemContainerGenerator.ContainerFromIndex(e.NewIndex);
                var newItemContentPresenter = newItem.FindVisualChild<ContentPresenter>();

                if (newItemContentPresenter == null)
                {
                    return;
                }

                var newTextBlock = (TextBlock)newItemContentPresenter.ContentTemplate.FindName("SelectionStarTextBlock", newItemContentPresenter);

                newTextBlock.Text = "*";
            }
        }

        private void PathColorCP_SelectedColorChagned(object sender, RoutedPropertyChangedEventArgs<SolidColorBrush> e)
        {
            if (_pathViewModel.CurrentPath != null)
                _pathViewModel.CurrentPath.Color.Color = PathColorCP.SelectedColor.Color;
        }
    }
}
