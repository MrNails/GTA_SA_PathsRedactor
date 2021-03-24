using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            m_pointTransformVM.AddNewPointTransformationData(GlobalSettings.GetInstance().PTD);

            this.DataContext = m_pointTransformVM;
        }

        public void AddGoToHomeCommand(Services.RelayCommand goToMainMenu)
        {
            if (goToMainMenu == null)
            {
                throw new ArgumentNullException("goToMainMenu");
            }

            m_pointTransformVM.GoToMainMenu = goToMainMenu;
        }
    }
}
