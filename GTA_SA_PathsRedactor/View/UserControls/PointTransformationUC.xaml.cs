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
        private ViewModel.PointTransformVM m_pointTransformVM;

        public PointTransformationUC()
        {
            InitializeComponent();

            m_pointTransformVM = new ViewModel.PointTransformVM();

            // m_pointTransformVM.AddNewPointTransformationData(GlobalSettings.GetInstance().PTD);

            this.DataContext = m_pointTransformVM;
        }

        public void AddGoToHomeCommand(RelayCommand goToMainMenu)
        {
            if (goToMainMenu == null)
            {
                throw new ArgumentNullException("goToMainMenu");
            }

            m_pointTransformVM.GoToMainMenu = goToMainMenu;
        }
    }
}
