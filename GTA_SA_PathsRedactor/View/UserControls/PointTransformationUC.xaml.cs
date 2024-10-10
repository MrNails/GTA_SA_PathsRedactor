using System;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Models;

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Логика взаимодействия для PointTransformationUC.xaml
    /// </summary>
    public partial class PointTransformationUC : UserControl
    {
        private ViewModel.PointTransformViewModel _pointTransformViewModel;

        public PointTransformationUC()
        {
            InitializeComponent();

            _pointTransformViewModel = new ViewModel.PointTransformViewModel();

            // m_pointTransformVM.AddNewPointTransformationData(GlobalSettings.GetInstance().PTD);

            this.DataContext = _pointTransformViewModel;
        }

        public void AddGoToHomeCommand(RelayCommand goToMainMenu)
        {
            if (goToMainMenu == null)
            {
                throw new ArgumentNullException("goToMainMenu");
            }

            _pointTransformViewModel.GoToMainMenu = goToMainMenu;
        }
    }
}
