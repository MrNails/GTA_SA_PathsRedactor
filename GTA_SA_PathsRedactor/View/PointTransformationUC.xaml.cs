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
        private ViewModel.PathEditor m_path;

        public PointTransformationUC()
        {
            InitializeComponent();

            m_pointTransformVM = new ViewModel.PointTransformVM();

            var pointsTransfromData = new Services.PointTransformationData[SettingsForResolutionCB.Items.Count];

            for (int i = 0; i < SettingsForResolutionCB.Items.Count; i++)
            {
                var comboBoxItem = SettingsForResolutionCB.Items[i] as ComboBoxItem;
                pointsTransfromData[i] = new Services.PointTransformationData();

                if (comboBoxItem != null)
                {
                    pointsTransfromData[i].TransformName = comboBoxItem.Content.ToString();
                }
            }

            m_pointTransformVM.AddNewPointTransformationData(pointsTransfromData);
            m_pointTransformVM.PropertyChanged += PointTransformVM_PropertyChanged;

            this.DataContext = m_pointTransformVM;
        }

        private void PointTransformVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            m_path.PointTransformation = m_pointTransformVM.CurrentPointTransformData;
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Services.PointTransformationData> PointTransformationDatas
        {
            get
            {
                return m_pointTransformVM.PointTranformationDatas;
            }
        }

        public ViewModel.PathEditor EditablePath
        {
            get { return m_path; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_path = value;

                m_path.PointTransformation = m_pointTransformVM.CurrentPointTransformData;
            }
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
