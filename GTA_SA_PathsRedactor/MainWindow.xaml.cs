using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTA_SA_PathsRedactor.IoC;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.View;
using GTA_SA_PathsRedactor.View.UserControls;
using GTA_SA_PathsRedactor.View.Windows;
using GTA_SA_PathsRedactor.ViewModel;

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IPageContainerService<UserControl> _pageContainer;
        
        private MainViewModel? _pathViewModel;

        public MainWindow()
        {
            InitializeComponent();

            _pageContainer = IoCServicesLocator.PageContainerService;
        }

        private void InitializeAdditionalComponent()
        {
            _pageContainer.AddPage(Constants.MainPageName_, new PointControllerUserControl { DataContext = _pathViewModel, VerticalAlignment = VerticalAlignment.Top });
            _pageContainer.AddPage(Constants.PathSettingsPageName_, new PointTransformationUserControl { VerticalAlignment = VerticalAlignment.Top });
        }

        private void SaveCurrentPath(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_pathViewModel!.SaveCurrentPathCommand.CanExecute(null))
                return;

            _pathViewModel.SaveCurrentPathCommand.Execute(null);
            
        }
        private void SaveCurrentPathAs(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_pathViewModel!.SaveCurrentPathCommand.CanExecute(null))
                return;

            _pathViewModel.SaveCurrentPathAsCommand.Execute(null);
        }

        private void Help(object sender, ExecutedRoutedEventArgs e)
        { 
            new HelpWindow().Show();
        }
        private void About(object sender, RoutedEventArgs e)
        { 
            new AboutWindow().Show();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_pathViewModel is null)
            {
                _pathViewModel = (MainViewModel)DataContext;
                InitializeAdditionalComponent();
            }
        }
    }
}
