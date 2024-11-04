using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
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
        private PathViewModel? _pathViewModel;
        private UserControl[] _userControls;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeAdditionalComponent()
        {
            var mainUserControl = new PointControllerUserControl {  PathViewModel = _pathViewModel };
            var pathSettingUc = new PointTransformationUC();

            mainUserControl.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.AddGoToHomeCommand(new RelayCommand(() =>
            {
                UserContentContainer.Child = _userControls[0];
            }));

            UserContentContainer.Child = mainUserControl;

            _userControls = [mainUserControl, pathSettingUc];
        }

        private void SaveCurrentPath(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_pathViewModel.SaveCurrentPath.CanExecute(null))
                return;

            _pathViewModel.SaveCurrentPath.Execute(null);
            
        }
        private void SaveCurrentPathAs(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_pathViewModel.SaveCurrentPath.CanExecute(null))
                return;

            _pathViewModel.SaveCurrentPathAs.Execute(null);
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
                _pathViewModel = (PathViewModel)DataContext;
                InitializeAdditionalComponent();
            }
        }
    }
}
