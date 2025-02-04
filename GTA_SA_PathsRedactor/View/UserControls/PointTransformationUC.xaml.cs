using System.Windows.Controls;

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
    }
}
